using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
//using System.Runtime.Serialization.Formatters; //for xml formatter
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Communication;

/*
USAGE NOTES:
The client and server function of this application are intended to be used with a VPN tunnel, e.g. through Hamachi.
To start a server, choose the server option in the main menu and enter that PC's IP address as displayed in Hamachi and a port number.
To connect a client to a server, choose the client option in the main menu and enter the server's IP address (can be viewed in Hamachi) and the same port number as the server. The client and the server need to be in a Hamachi network for this to work.
The Computers' firewalls may cause connection issues (see https://help.logmein.com/articles/en_US/FAQ/Resolving-Hamachi-Request-Timed-Out -> only needs to be applied to the *public* firewall, but for *both* computers).
*/

public class Server : IDisposable
{
    public bool active = true;
    private const int bufferSize = 1000; //size of the buffer for the network stream
    private const int backlogSize = 10; //length of the pending connections queue
    private Socket clientListener = null;
    private NetworkStream netS = null;
    private BufferedStream bufS = null;

    private Dictionary<string, string> userInf; //contains information about all users (names and passwords)
    public List<ServerClientConnection> activeConnections = new List<ServerClientConnection>(); //list of users that are currently online

    
    //basic behaviour
    public async Task startServer()
    {
        bool validInput = false;

        //load userInf
        loadUserInf();

        IPAddress ipAddress = null;
        IPEndPoint remoteEP = null;

        //set up local end point
        Console.WriteLine("Enter the server's designated IP adress:");
        do
        {
            try
            { ipAddress = IPAddress.Parse(Console.ReadLine()); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            validInput = true;
        } while (!validInput);
        Console.WriteLine("IP address set up successfully: {0}", ipAddress);

        validInput = false;
        Console.WriteLine("Enter the desired port:");
        do
        {
            try
            { remoteEP = new IPEndPoint(ipAddress, int.Parse(Console.ReadLine())); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            validInput = true;
        } while (!validInput);
        Console.WriteLine("End point set up successfully: {0}", remoteEP.ToString());

        //set up new socket
        try
        {
            clientListener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            clientListener.Bind(remoteEP);
            //active = true;
            clientListener.Listen(backlogSize);

            Console.Clear();

            //start listening for connections and provide server interface
            Console.WriteLine("Server version number: {0}", Application.ProductVersion);
            await Task.WhenAny(listen(), ServerMenu());
            active = false;
        }
        catch (ObjectDisposedException)
        { }
        catch (Exception e)
        { Console.WriteLine("Error: {0}", e.Message); }

        //release socket
        try
        {
            clientListener.Shutdown(SocketShutdown.Both);
            clientListener.Close();
        }
        catch (Exception)
        { }

        Dispose();
    }
    private void loadUserInf()
    {
        //binary serialisation (unsafe)
        try
        {
            using (Stream stream = File.Open("userInf.bin", FileMode.Open))
            {
                BinaryFormatter bin = new BinaryFormatter();
                userInf = (Dictionary<string, string>)bin.Deserialize(stream);
            }
        }
        catch (FileNotFoundException fnfe)
        {
            Console.WriteLine("User information could not be found ({0}). Creating new instance...", fnfe.Message);
            userInf = new Dictionary<string, string>();
            SaveUserInf();
        }
        catch (IOException ioe)
        { Console.WriteLine("An IOException has occurred during deserialisation: {0}", ioe.Message); }
        catch (Exception e)
        { Console.WriteLine("An error has occurred during deserialisation: {0}", e.Message); }


        /*//xml serialisation (doesn't work with dictionaries apparently...)
        try
        {
            var reader = new System.Xml.Serialization.XmlSerializer(typeof(Dictionary<string, string>));
            using (StreamReader file = new StreamReader("userInf.xml"))
            {
                userInf = (Dictionary<string, string>)reader.Deserialize(file);
                file.Close();
            }
        }
        catch (FileNotFoundException fnfe)
        {
            Console.WriteLine("User information could not be found ({0}). Creating new instance...", fnfe.Message);
            userInf = new Dictionary<string, string>();
        }
        catch (IOException ioe)
        { Console.WriteLine("An IOException has occurred during deserialisation: {0}", ioe.Message); }
        catch (Exception e)
        { Console.WriteLine("An error has occurred during deserialisation: {0}", e.Message); }
        */
    }
    private void SaveUserInf()
    {
        //binary serialisation (unsafe)
        try
        {
            using (Stream stream = File.Open("userInf.bin", FileMode.Create))
            {
                BinaryFormatter bin = new BinaryFormatter();
                bin.Serialize(stream, userInf);
            }
        }
        catch (IOException ioe)
        { Console.WriteLine("An IOException has occurred during serialisation: {0}", ioe.Message); }
        catch (Exception e)
        { Console.WriteLine("An error has occurred during serialisation: {0}", e.Message); }


        /*//xml serialisation (doesn't work with dictionaries apparently...)
        var writer = new System.Xml.Serialization.XmlSerializer(typeof(Dictionary<string, string>));
        try
        {
            using (StreamWriter wfile = new StreamWriter("userInf.xml"))
            {
                writer.Serialize(wfile, userInf);
                wfile.Close();
            }                
        }
        catch (IOException ioe)
        { Console.WriteLine("An IOException has occurred during serialisation: {0}", ioe.Message); }
        catch (Exception e)
        { Console.WriteLine("An error has occurred during serialisation: {0}", e.Message); }
        */
    }

    private async Task listen()
    {
        while (active)
        {
            await Task.Delay(2500); //this needs to come before the accepting, otherwise the server menu task is blocked
            
            //execution is suspended while waiting for an incoming connection.
            ServerClientConnection newConnection = new ServerClientConnection(clientListener.Accept(), this);
        }
    }


    //user-related
    public bool tryLogin(string username, string password)
    {
        try
        {
            foreach (KeyValuePair<string, string> kvp in userInf) //this could throw an exception if a new user registers while the loop is executed
            {
                if (username == kvp.Key)
                {
                    if (password == kvp.Value)
                    { return true; }
                }
            }
        }
        catch (Exception)
        { }
        return false;
    }
    public bool tryRegister(string username, string password)
    {
        if (!userInf.ContainsKey(username))
        {
            if (username == "Server") return false;

            userInf.Add(username, password);
            SaveUserInf();
            return true;
        }
        return false;
    }


    //user interface
    private async Task ServerMenu()
    {
        //opens options for showing all users and their passwords, deleting all users, broadcasting a message (username: Server) and shutting down the server
        int selection;
        bool validInput = false;

        while (true)
        {
            Console.WriteLine("What would you like to do? (enter the according number)\n0: Get a list of all online users\n1: Get a list of all registered users (without passwords)\n2: Get a list of all registered users (including passwords)\n3: Send a message to all users\n4: Whisper a user\n5: Delete a user (will be kicked if online)\n6: Delete all users\nAnything else: Go offline");
            do
            {
                try
                { selection = int.Parse(Console.ReadLine()); }
                catch (Exception)
                { return; } //other input -> go offline

                validInput = true;
            } while (!validInput);

            switch (selection)
            {
                case 0:
                    displayOnlineUsers();
                    break;
                case 1:
                    displayRegisteredUsers();
                    break;
                case 2:
                    if (userInf.Count > 0)
                    {
                        Console.WriteLine("Full information about all registered users:");
                        try
                        {
                            foreach (KeyValuePair<string, string> kvp in userInf) //could throw an exception if a new user registers while th users are listed
                            { Console.WriteLine(kvp.Key + "\t\t" + kvp.Value); }
                        }
                        catch (Exception e)
                        { Console.WriteLine("Listing has been aborted. Reason: {0}", e.Message); }
                    }
                    else
                    { Console.WriteLine("There are no registered users yet."); }
                    break;
                case 3:
                    await writeGlobalMessagePrompt();
                    break;
                case 4:
                    await whisperUserPrompt();
                    break;
                case 5:
                    await deleteUserPrompt();
                    break;
                case 6:
                    await deleteAllUsersPromp();
                    break;

                default:
                    return;
            }
            Console.WriteLine("Press any key to return to the user menu."); Console.ReadKey();
            Console.Clear();
        }
    }

    public List<string> getRegisteredUsers()
    {
        List<string> output = new List<string>();
        while (true)
        {
            try
            {
                foreach (string name in userInf.Keys) //this could throw an exception if a new user registers while the loop is executed
                { output.Add(name); }
            }
            catch (Exception e)
            {
                output.Clear();
                Console.WriteLine("An exception has occurred while trying to list all registered users: {0}\nRetrying...", e.Message);
                continue;
            }
            break;
        }
        return output;
    }
    private void displayRegisteredUsers()
    {
        List<string> onlineUsers = getRegisteredUsers();

        if (onlineUsers.Count > 0)
        {
            Console.WriteLine("List of registered users:");
            for (int i = 0; i < onlineUsers.Count; i++)
            {
                Console.Write(onlineUsers[i]);
                if (i < onlineUsers.Count - 1)
                { Console.Write(", "); }
            }
            Console.WriteLine();
        }
        else
        { Console.WriteLine("There are no registered users yet."); }
    }
    public List<string> getOnlineUsers()
    {
        List<string> output = new List<string>();
        for (int i = 0; i < activeConnections.Count; i++)
        {
            if (activeConnections[i].online)
            { output.Add(activeConnections[i].username); }
        }
        return output;
    }
    private void displayOnlineUsers()
    {
        List<string> onlineUsers = getOnlineUsers();

        if (onlineUsers.Count > 0)
        {
            Console.WriteLine("List of currently online users:");
            for (int i = 0; i < onlineUsers.Count; i++)
            {
                Console.Write(onlineUsers[i]);
                if (i < onlineUsers.Count - 1)
                { Console.Write(", "); }
            }
            Console.WriteLine();
        }
        else
        { Console.WriteLine("There are no online users at the moment."); }
    }

    private async Task writeGlobalMessagePrompt()
    {
        string msg = "";
        bool validInput = false;

        Console.WriteLine("Please enter the message that you would like to send to all users:");
        do
        {
            try
            { msg = Console.ReadLine(); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            validInput = true;
        } while (!validInput);

        ChatMessage newMessage = new ChatMessage("Server", msg);
        if (msg != "") await sendMessage(newMessage);
    }
    private async Task whisperUserPrompt()
    {
        string recipient = "", msg = "";
        bool validInput = false;

        var onlineUsers = getOnlineUsers();
        if (onlineUsers.Count == 0)
        { Console.WriteLine("There are no online users at the moment."); return; }

        Console.WriteLine("Please enter the user that you would like to message:");
        do
        {
            try
            { recipient = Console.ReadLine(); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            if (!onlineUsers.Contains(recipient))
            {
                Console.WriteLine("The entered user is either offline or doesn't exist. Press 0 to retry or anything else to abort.");
                try
                { int selection = int.Parse(Console.ReadLine()); if (selection != 0) return; }
                catch (Exception)
                { return; }
                continue;
            }

            validInput = true;
        } while (!validInput);

        Console.WriteLine("Please enter the message that you would like to send to {0}:", recipient);
        do
        {
            try
            { msg = Console.ReadLine(); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            validInput = true;
        } while (!validInput);

        ChatMessage newMessage = new ChatMessage("Server", msg, recipient);
        if (msg != "") await sendMessage(newMessage);
    }
    public async Task<bool> sendMessage(ChatMessage msg)
    {
        if (msg.global)
        {
            for (int i = 0; i < activeConnections.Count; i++)
            { if (activeConnections[i].username != msg.sender) await activeConnections[i].sendMessage(msg); }
            Console.WriteLine("Global message from {0}: {1}", msg.sender, msg.msg);
            return true;
        }
        else
        {
            Console.WriteLine("Private message from {0} to {1}: {2}", msg.sender, msg.recipient, msg.msg);
            for (int i = 0; i < activeConnections.Count; i++)
            {
                if (activeConnections[i].username == msg.recipient)
                { await activeConnections[i].sendMessage(msg); return true; }
            }
            Console.WriteLine("Forwarding failed.");
            return false;
        }
    }

    private async Task deleteUserPrompt()
    {
        string username = "";
        bool validInput = false;

        var registeredUsers = getRegisteredUsers();
        if (registeredUsers.Count == 0)
        { Console.WriteLine("There are no registered users at the moment."); return; }

        Console.WriteLine("Please enter the name of the user that you want to delete:");
        do
        {
            try
            { username = Console.ReadLine(); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            if (!getRegisteredUsers().Contains(username))
            {
                Console.WriteLine("The entered user doesn't exist. Press 0 to continue or anything else to abort.");
                try
                { int selection = int.Parse(Console.ReadLine()); if (selection != 0) return; }
                catch (Exception)
                { return; }
                continue;
            }

            validInput = true;
        } while (!validInput);

        Console.WriteLine("Are you sure that you want to delete {0}'s account?. Press 0 to continue or anything else to abort.", username);
        try
        { int selection = int.Parse(Console.ReadLine()); if (selection != 0) return; }
        catch (Exception)
        { return; }

        await deleteUser(username);
    }
    private async Task deleteUser(string username)
    {
        //kick if online
        if (getOnlineUsers().Contains(username))
        {
            for (int i = 0; i < activeConnections.Count; i++) //not a foreach because it would throw an exception if a new user came online in the meantime
            {
                if (activeConnections[i].username == username)
                { await activeConnections[i].Shutdown(); break; }
            }
        }

        //delete user
        userInf.Remove(username);
        SaveUserInf();

        Console.WriteLine("The user {0} has been deleted.", username);
    }
    private async Task deleteAllUsersPromp()
    {
        int selection = -1;
        bool validInput = false;

        Console.WriteLine("Are you sure that you want to delete all users? (0: Yes, Other: No)");
        do
        {
            try
            { selection = int.Parse(Console.ReadLine()); }
            catch (Exception)
            { return; } //other input -> No -> abort

            validInput = true;
        } while (!validInput);

        switch (selection)
        {
            case 0:
                var regUsers = getRegisteredUsers();
                for (int i = 0; i < regUsers.Count; i++)
                { await deleteUser(regUsers[i]); }
                return;

            default:
                return;
        }
    }


    //shutdown
    public void Dispose()
    {
        active = false;

        if (activeConnections.Count > 0)
        {
            try
            {
                for (int i = 0; i < activeConnections.Count; i++)
                { activeConnections[i].Dispose(); }
            }
            catch (Exception)
            { }
        }
        
        try
        { bufS.Close(); bufS.Dispose(); }
        catch (Exception)
        { }

        try
        { netS.Close(); netS.Dispose(); }
        catch (Exception)
        { }

        try
        { clientListener.Dispose(); }
        catch (Exception)
        { }
    }
}
