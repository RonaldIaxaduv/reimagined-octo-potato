using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

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
    private const int backlogSize = 2; //length of the pending connections queue
    private Dictionary<string, string> userInf; //contains information about all users (names and passwords)
    public List<ServerClientConnection> activeConnections = new List<ServerClientConnection>(); //list of users that are currently online
    private Socket clientListener = null;
    private NetworkStream netS = null;
    private BufferedStream bufS = null;

    public Server()
    { }
    
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

            // Start listening for connections.  
            while (active)
            {
                await Task.Delay(500);
                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.                
                ServerClientConnection newConnection = new ServerClientConnection(clientListener.Accept(), this); //WIP: this needs to be async
                //ServerClientConnection newConnection = new ServerClientConnection(await clientListener.EndAccept(await clientListener.BeginAccept(AsyncCallback(), new object())), this);
                activeConnections.Add(newConnection);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Dispose();
    }

    private async Task listen()
    {

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

    private void ManageClients()
    {
        //provides a menu for listing all clients in userInf as well as the means to add new entries and edit/delete existing ones

    }

    public List<string> getRegisteredUsers()
    {
        List<string> output = new List<string>();
        foreach (string name in userInf.Keys)
        { output.Add(name); }
        return output;
    }
    public List<string> getOnlineUsers()
    {
        List<string> output = new List<string>();
        foreach (ServerClientConnection user in activeConnections)
        {
            if (user.online)
            { output.Add(user.username); }
        }
        return output;
    }

    public bool tryLogin(string username, string password)
    {
        foreach (KeyValuePair<string, string> kvp in userInf)
        {
            if (username == kvp.Key)
            {
                if (password == kvp.Value)
                { return true; }
            }
        }
        return false;
    }
    public bool tryRegister(string username, string password)
    {
        if (!userInf.ContainsKey(username))
        {
            userInf.Add(username, password);
            SaveUserInf();
            return true;
        }
        return false;
    }

    private async Task broadcastMessage(string msg, string username)
    {
        //await Task.WhenAll()

    }

    private async Task ServerMenu()
    {
        //opens options for showing all users and their passwords, deleting all users, broadcasting a message (username: Server) and shutting down the server

    }

    public void Dispose()
    {
        active = false;

        foreach (ServerClientConnection user in activeConnections)
        { user.Shutdown(); }

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
