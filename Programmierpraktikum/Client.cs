using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

class Client : IDisposable
{
    public bool active = true;
    private Socket sender;
    private NetworkStream netS; private BufferedStream bufS;
    private const int bufferSize = 1000;
    private string username = "";

    private List<Communication.ServerCommands> serverMsgs = new List<Communication.ServerCommands>();
    private object receivedData;

    public Client()
    { }

    public async Task startClient()
    {
        bool validInput = false;
        IPAddress ipAddress = null;
        IPEndPoint remoteEP = null;

        //set up local end point
        Console.WriteLine("Enter the server's IP adress:");
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
            sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sender.Connect(remoteEP);
            Console.WriteLine("Connection successful.");

            //prepare remaining tools needed for execution of client
            netS = new NetworkStream(sender);
            bufS = new BufferedStream(netS, bufferSize);
            //active = true;

            listen(); //runs asynchronously
            await ClientMenu(); //runs asynchronously, ends when the client disconnects

            //release socket  
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e.Message);
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        Dispose();
    }

    private async void listen()
    {
        /*this method can receive ANY type of data through the stream. it is based on: https://stackoverflow.com/questions/2316397/sending-and-receiving-custom-objects-using-tcpclient-class-in-c-sharp
        how it works:
        - the stream first receives 8 byte (2x32 bit) containing a command code which tells the program what type of object will be received and the size of the data that will be received
        - the task which will handle the received data will be determined beforehand, so that the reading process doesn't need to be executed in separate methods
        - finally, the object data is read from the stream and can be cast on a variable of the expected type
        */

        byte[] comBlock = new byte[8];
        int bytesRec = 0;

        //continuously read incoming confirmation until clients shuts down
        while (active)
        {
            await Task.Delay(1000); //check for new messages every second
            if (netS.DataAvailable)
            {
                while (bytesRec < comBlock.Length)
                { bytesRec += await netS.ReadAsync(comBlock, bytesRec, comBlock.Length - bytesRec); }

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(comBlock, 0, 4);
                    Array.Reverse(comBlock, 4, 4);
                }
                uint command = BitConverter.ToUInt32(comBlock, 0); //first 4 bytes (more isn't read): command, i.e. what kind of object will be received
                int dataSize = BitConverter.ToInt32(comBlock, 4); //second 4 bytes: length of the upcoming message (arrays can only have a size of int, so sending a uint wouldn't work)
                //Console.WriteLine("Received command number: {0}", command);
                //Console.WriteLine("Received data size: {0}", dataSize);

                await handleMessage(Communication.getServerCommand(command), dataSize);

                //reset
                bytesRec = 0;
            }
        }
    }
    private async Task handleMessage(Communication.ServerCommands sCom, int dataSize)
    {
        //note: this method isn't async, but it wouldn't be able to return async methods if it wasn't declared as one itself
        switch (sCom)
        {
            case Communication.ServerCommands.PingReturn:
            case Communication.ServerCommands.UserNameFound:
            case Communication.ServerCommands.UserNameNotFound:
            case Communication.ServerCommands.PasswordIncorrect:
            case Communication.ServerCommands.LoginSuccessful:
            case Communication.ServerCommands.AccountCreatedSuccessfully:
            case Communication.ServerCommands.ServerError:
                await saveServerCommand(sCom);
                return;
            case Communication.ServerCommands.ReceiveString:
                await receiveString(sCom, dataSize);
                return;
            case Communication.ServerCommands.ReceiveList_String:
                await receiveList(sCom, dataSize);
                return;

            default:
                Console.WriteLine("Unknown server command.");
                return;
        }
    }

    private async Task saveServerCommand(Communication.ServerCommands sCom)
    { serverMsgs.Add(sCom); }
    private async Task awaitServerResponse(Communication.ServerCommands sCom)
    {
        while (serverMsgs.Count == 0 || !checkServerResponse(sCom))
        { await Task.Delay(1000); }
    }
    private bool checkServerResponse(Communication.ServerCommands sCom)
    {
        return serverMsgs.Contains(sCom);
    }
    private void removeServerResponse(Communication.ServerCommands sCom)
    {
        serverMsgs.Remove(sCom);
    }

    private async Task<byte[]> receiveData(int dataSize)
    {
        int bytesRec = 0;
        byte[] infBlock = new byte[dataSize];
        while (bytesRec < dataSize)
        { bytesRec += await netS.ReadAsync(infBlock, bytesRec, infBlock.Length - bytesRec); }
        return infBlock;
    }
    private async Task receiveString(Communication.ServerCommands sCom, int dataSize)
    {
        receivedData = Encoding.Unicode.GetString(await receiveData(dataSize));
        await saveServerCommand(sCom);
    }
    private async Task receiveList(Communication.ServerCommands sCom, int dataSize)
    {
        MemoryStream ms = new MemoryStream(await receiveData(dataSize));
        BinaryFormatter bf = new BinaryFormatter();
        ms.Position = 0;
        receivedData = bf.Deserialize(ms);
        Console.WriteLine("Done. Notifying program that the list has been received.");
        await saveServerCommand(sCom);
    }

    private async Task sendCommand(Communication.ClientCommands cCom)
    {
        await sendObject(cCom, null);
    }
    private async Task sendString(Communication.ClientCommands cCom, string str)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            //convert object to byte array
            byte[] data;
            if (str != "")
            { data = Encoding.Unicode.GetBytes(str); } //get data from string slightly differently than for objects because of encoding
            else
            { data = new byte[0]; }

            //send command number
            //Console.WriteLine("Sent command number: {0}", Communication.getServerCommandUInt(sCom));
            byte[] prep = BitConverter.GetBytes(Communication.getClientCommandUInt(cCom));
            if (BitConverter.IsLittleEndian) //target computer might use different endian -> send and receive as big endian and if necessary, restore to little endian in client
                Array.Reverse(prep);
            await netS.WriteAsync(prep, 0, prep.Length); //write command (uint) to stream

            //send data size
            //Console.WriteLine("Sent data size: {0}", data.Length);
            prep = BitConverter.GetBytes(data.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(prep);
            await netS.WriteAsync(prep, 0, prep.Length); //write length of data (int) to stream

            //send object
            await netS.WriteAsync(data, 0, data.Length); //write object to stream
        }
    }
    private async Task sendObject(Communication.ClientCommands cCom, object obj)
    {
        //see e.g.: https://stackoverflow.com/questions/2316397/sending-and-receiving-custom-objects-using-tcpclient-class-in-c-sharp

        using (MemoryStream ms = new MemoryStream())
        {
            //convert object to byte array
            byte[] data;
            if (obj != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj); //serialise object to memory stream first -> can provide size of data in byte
                data = ms.ToArray(); //the object converted to a byte array
            }
            else
            { data = new byte[0]; }

            //send command number
            //Console.WriteLine("Sent command number: {0}", Communication.getServerCommandUInt(sCom));
            byte[] prep = BitConverter.GetBytes(Communication.getClientCommandUInt(cCom));
            if (BitConverter.IsLittleEndian) //target computer might use different endian -> send and receive as big endian and if necessary, restore to little endian in client
                Array.Reverse(prep);
            await netS.WriteAsync(prep, 0, prep.Length); //write command (uint) to stream

            //send data size
            //Console.WriteLine("Sent data size: {0}", data.Length);
            prep = BitConverter.GetBytes(data.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(prep);
            await netS.WriteAsync(prep, 0, prep.Length); //write length of data (int) to stream

            //send object
            await netS.WriteAsync(data, 0, data.Length); //write object to stream
        }
    }
    


    private async Task ClientMenu()
    {
        //test ping
        await requestPing();
        
        //WIP test: receive list of registered (later: online) users
        await requestOnlineUserList();
        List<string> onlineUsers = (List<string>)receivedData;
        Console.WriteLine("List of online users:");
        if (onlineUsers.Count == 0)
        { Console.WriteLine("List is empty."); }
        else
        {
            foreach (string name in onlineUsers)
            { Console.WriteLine(name); }
        }
        //Console.ReadKey();
        
        //log in or sign up
        if (!(await requestLogin())) //login or account creation unsuccessful?
        { return; }

        //user has been logged in. provide interface (WIP):

        Console.WriteLine("WIP from here. Press any key to return to the main menu.");
        Console.ReadKey();
    }

    private async Task requestPing()
    {
        //await sendObject(Communication.ClientCommands.Ping, "PING");
        await sendCommand(Communication.ClientCommands.Ping);
        await awaitServerResponse(Communication.ServerCommands.PingReturn);
        removeServerResponse(Communication.ServerCommands.PingReturn);
    }

    private async Task<bool> requestLogin()
    {
        try
        {
            while (true) //keep trying to log in
            {
                await submitUserName();

                //wait for response
                await Task.WhenAny(awaitServerResponse(Communication.ServerCommands.UserNameFound), awaitServerResponse(Communication.ServerCommands.UserNameNotFound));

                if (checkServerResponse(Communication.ServerCommands.UserNameFound))
                {
                    removeServerResponse(Communication.ServerCommands.UserNameFound);

                    if (await submitUserPassword())
                    { break; }
                    else
                    { continue; }
                }
                else
                {
                    removeServerResponse(Communication.ServerCommands.UserNameNotFound);

                    await submitNewPassword();
                    break;
                }
            }

            //login or account creation complete
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("An error has occurred: {0}" + e.Message);
            return false;
        }
    }
    private async Task submitUserName()
    {
        bool validInput = false;
        string input = "";

        Console.WriteLine("Enter your username:");
        do
        {
            try
            { input = Console.ReadLine(); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            validInput = true;
        } while (!validInput);

        //submit user name
        await sendString(Communication.ClientCommands.SubmitUserName, input);

        username = input;
    }
    private async Task submitNewPassword()
    {
        bool validInput = false;
        string input = "";

        Console.WriteLine("Username not found. Please enter a password to create a new account:");
        do
        {
            try
            { input = Console.ReadLine(); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            validInput = true;
        } while (!validInput);

        //submit password
        await sendString(Communication.ClientCommands.SubmitPassword_NewUser, input);

        //wait for response
        await awaitServerResponse(Communication.ServerCommands.AccountCreatedSuccessfully);

        Console.WriteLine("Your account has been created successfully. Welcome, {0}.", username);
    }
    private async Task<bool> submitUserPassword()
    {
        bool validInput = false;
        string input = "";

        Console.Write("Username found. ");
        while (true)
        {
            Console.WriteLine("Please enter your password:");
            do
            {
                try
                {
                    input = Console.ReadLine();
                    if (input.Contains("<") || input.Contains(">") || input.Contains("|")) //<,> used for commands, | used for transmitting non-string variables
                    { throw new Exception("The entered password contains invalid characters."); }
                }
                catch (Exception e)
                { Console.WriteLine("Error: {0}", e.Message); continue; }

                validInput = true;
            } while (!validInput);

            //submit password
            await sendString(Communication.ClientCommands.SubmitPassword_ExistingUser, input);

            //wait for response
            await Task.WhenAny(awaitServerResponse(Communication.ServerCommands.PasswordIncorrect), awaitServerResponse(Communication.ServerCommands.LoginSuccessful));
            if (checkServerResponse(Communication.ServerCommands.PasswordIncorrect))
            {
                removeServerResponse(Communication.ServerCommands.PasswordIncorrect);

                Console.WriteLine("The entered password didn't match the username. Enter 0 to retry or anything else to enter a different user name.");
                //WIP:the following part doesn't work yet
                do
                {
                    try
                    { input = Console.ReadLine(); }
                    catch (Exception e)
                    { Console.WriteLine("Error: {0}", e.Message); continue; }

                    validInput = true;
                } while (!validInput);

                try
                {
                    byte selection = byte.Parse(input);
                    if (selection == 0) continue;
                }
                catch (Exception)
                { } //not a number -> other input -> enter different user name
                return false;
            }
            else
            {
                removeServerResponse(Communication.ServerCommands.LoginSuccessful);
                Console.WriteLine("Login successful. Welcome, {0}.", username);
                break;
            }
        }
        return true;
    }

    private async Task requestOnlineUserList()
    {
        await sendCommand(Communication.ClientCommands.RequestOnlineUserList);
        await awaitServerResponse(Communication.ServerCommands.ReceiveList_String);
    }

    private void requestSendChatMessage(string msg)
    {

    }

    public void Dispose()
    {
        active = false;

        //dispose of socket, streams etc.
        try
        { bufS.Close(); bufS.Dispose(); }
        catch (Exception)
        { }

        try
        { netS.Close(); netS.Dispose(); }
        catch (Exception)
        { }

        try
        { sender.Dispose(); }
        catch (Exception)
        { }
    }
}