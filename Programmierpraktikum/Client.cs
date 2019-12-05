using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Communication;

class Client : IDisposable
{
    private Socket sender;
    private NetworkStream netS; private BufferedStream bufS;
    private const int bufferSize = 1000;

    private List<Commands.ServerCommands> serverMsgs = new List<Commands.ServerCommands>();
    private object receivedData;

    private string username = "";


    //basic behaviour
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

            await Task.WhenAny(listen(), ClientMenu()); //shut down when either user exits or server connection is severed
        }
        catch (Exception e)
        { Console.WriteLine("Error: {0}", e.Message); }

        //release socket
        try
        {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
        catch (Exception)
        { }

        Dispose();
    }
    private async Task listen()
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
        while (Connection.isConnected(sender))
        {
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

                await handleMessage(Commands.getServerCommand(command), dataSize);

                //reset
                bytesRec = 0;
            }
            await Task.Delay(500); //check for new messages after a short delay
        }
        Console.WriteLine("The connection with the server has been severed.");
    }


    //information processing, client-server communication
    private async Task handleMessage(Commands.ServerCommands sCom, int dataSize)
    {
        //note: this method isn't async, but it wouldn't be able to return async methods if it wasn't declared as one itself
        switch (sCom)
        {
            case Commands.ServerCommands.ValidVersionNumber:
            case Commands.ServerCommands.InvalidVersionNumber:
            case Commands.ServerCommands.UserAlreadyOnline:
            case Commands.ServerCommands.UserNameFound:
            case Commands.ServerCommands.UserNameNotFound:
            case Commands.ServerCommands.PasswordIncorrect:
            case Commands.ServerCommands.LoginSuccessful:
            case Commands.ServerCommands.AccountCreatedSuccessfully:
            case Commands.ServerCommands.ServerError:
            case Commands.ServerCommands.MessageSent:
                await saveServerCommand(sCom);
                return;
            case Commands.ServerCommands.ReceiveString:
                await receiveString(sCom, dataSize);
                return;
            case Commands.ServerCommands.ReceiveList_String:
                await receiveObject(sCom, dataSize);
                return;
            case Commands.ServerCommands.MessageForwarded:
                await receiveObject(sCom, dataSize);
                removeServerResponse(sCom);
                ChatMessage recMsg = (ChatMessage)receivedData;
                Console.WriteLine("Received a {0} message from {1}:\n{2}", recMsg.recipient == username ? "private" : "global", recMsg.sender, recMsg.msg);
                break;

            default:
                Console.WriteLine("Unhandled server command ({0}).", sCom);
                return;
        }
    }

    private async Task saveServerCommand(Commands.ServerCommands sCom)
    { serverMsgs.Add(sCom); }
    private async Task awaitServerResponse(Commands.ServerCommands sCom)
    {
        while (serverMsgs.Count == 0 || !checkServerResponse(sCom))
        { await Task.Delay(1000); }
    }
    private bool checkServerResponse(Commands.ServerCommands sCom)
    {
        return serverMsgs.Contains(sCom);
    }
    private void removeServerResponse(Commands.ServerCommands sCom)
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
    private async Task receiveString(Commands.ServerCommands sCom, int dataSize)
    {
        receivedData = Encoding.Unicode.GetString(await receiveData(dataSize));
        await saveServerCommand(sCom);
    }
    private async Task receiveObject(Commands.ServerCommands sCom, int dataSize)
    {
        MemoryStream ms = new MemoryStream(await receiveData(dataSize));
        BinaryFormatter bf = new BinaryFormatter();
        ms.Position = 0;
        receivedData = bf.Deserialize(ms);
        await saveServerCommand(sCom);
    }

    private async Task sendCommand(Commands.ClientCommands cCom)
    {
        await sendObject(cCom, null);
    }
    private async Task sendString(Commands.ClientCommands cCom, string str)
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
            byte[] prep = BitConverter.GetBytes(Commands.getClientCommandUInt(cCom));
            if (BitConverter.IsLittleEndian) //target computer might use different endian -> send and receive as big endian and if necessary, restore to little endian in client
                Array.Reverse(prep);
            await netS.WriteAsync(prep, 0, prep.Length); //write command (uint) to stream

            //send data size
            prep = BitConverter.GetBytes(data.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(prep);
            await netS.WriteAsync(prep, 0, prep.Length); //write length of data (int) to stream

            //send object
            await netS.WriteAsync(data, 0, data.Length); //write object to stream
        }
    }
    private async Task sendObject(Commands.ClientCommands cCom, object obj)
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
            byte[] prep = BitConverter.GetBytes(Commands.getClientCommandUInt(cCom));
            if (BitConverter.IsLittleEndian) //target computer might use different endian -> send and receive as big endian and if necessary, restore to little endian in client
                Array.Reverse(prep);
            await netS.WriteAsync(prep, 0, prep.Length); //write command (uint) to stream

            //send data size
            prep = BitConverter.GetBytes(data.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(prep);
            await netS.WriteAsync(prep, 0, prep.Length); //write length of data (int) to stream

            //send object
            await netS.WriteAsync(data, 0, data.Length); //write object to stream
        }
    }
    

    //user interface
    private async Task ClientMenu()
    {
        //compare version numbers of client and server
        if (!await requestCompareVersionNumber())
        { Console.WriteLine("Wrong version number. Please update your client or server."); return; }
                
        //log in or sign up
        if (!(await requestLogin())) //login or account creation unsuccessful?
        { return; }

        //user has been logged in. provide list of other currently online users
        await displayOnlineUserList();

        //provide online interface
        await OnlineUserMenu(); //finishes when user goes offline
    }
    private async Task OnlineUserMenu()
    {
        int selection;
        bool validInput = false;

        while (true)
        {
            Console.WriteLine("What would you like to do? (enter the according number)\n0: Get a list of all online users\n1: Write a global message\n2: Whisper another user\nAnything else: Go offline");
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
                    await displayOnlineUserList();
                    break;
                case 1:
                    await writeGlobalMessagePrompt();
                    break;
                case 2:
                    await whisperUserPrompt();
                    break;

                default:
                    return;
            }
            Console.WriteLine("Press any key to return to the user menu."); Console.ReadKey();
            Console.Clear();
        }
    }
    private async Task writeGlobalMessagePrompt()
    {
        string msg = "";
        bool validInput = false;

        Console.WriteLine("Please enter the message that you would like to send to all other users:");
        do
        {
            try
            { msg = Console.ReadLine(); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            validInput = true;
        } while (!validInput);

        if (msg != "") await requestBroadcastChatMessage(msg);
    }
    private async Task whisperUserPrompt()
    {
        string recipient = "", msg = "";
        bool validInput = false;

        await requestOnlineUserList();
        var onlineUsers = (List<string>)receivedData;

        Console.WriteLine("Please enter the user that you would like to message:");
        do
        {
            try
            { recipient = Console.ReadLine(); }
            catch (Exception e)
            { Console.WriteLine("Error: {0}", e.Message); continue; }

            if (!onlineUsers.Contains(recipient))
            { Console.WriteLine("The entered user is either offline or doesn't exist."); continue; }

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
        if (msg != "") await requestWhisperChatMessage(recipient, msg);
    }

    private async Task<bool> requestCompareVersionNumber()
    {
        Console.WriteLine("Client version number: {0}", Application.ProductVersion);
        await sendString(Commands.ClientCommands.RequestCompareVersionNumber, Application.ProductVersion);
        await Task.WhenAny(awaitServerResponse(Commands.ServerCommands.ValidVersionNumber), awaitServerResponse(Commands.ServerCommands.InvalidVersionNumber));

        if (checkServerResponse(Commands.ServerCommands.ValidVersionNumber))
        {
            removeServerResponse(Commands.ServerCommands.ValidVersionNumber);
            return true;
        }
        else
        {
            removeServerResponse(Commands.ServerCommands.InvalidVersionNumber);
            return false;
        }
    }

    private async Task<bool> requestLogin()
    {
        try
        {
            while (true) //keep trying to log in
            {
                await submitUserName();

                //wait for response
                await Task.WhenAny(awaitServerResponse(Commands.ServerCommands.UserNameFound), awaitServerResponse(Commands.ServerCommands.UserNameNotFound), awaitServerResponse(Commands.ServerCommands.InvalidUserName), awaitServerResponse(Commands.ServerCommands.UserAlreadyOnline));
                if (checkServerResponse(Commands.ServerCommands.InvalidUserName))
                {
                    removeServerResponse(Commands.ServerCommands.InvalidUserName);
                    Console.WriteLine("Invalid user name. Please enter a different one.");
                    continue;
                }
                else if (checkServerResponse(Commands.ServerCommands.UserAlreadyOnline))
                {
                    removeServerResponse(Commands.ServerCommands.UserAlreadyOnline);
                    Console.WriteLine("This user is already online. Please use a different name.");
                    continue;
                }
                else if (checkServerResponse(Commands.ServerCommands.UserNameFound))
                {
                    removeServerResponse(Commands.ServerCommands.UserNameFound);
                    if (await submitUserPassword()) break;
                    else continue;
                }
                else
                {
                    removeServerResponse(Commands.ServerCommands.UserNameNotFound);
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
        await sendString(Commands.ClientCommands.SubmitUserName, input);

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
        await sendString(Commands.ClientCommands.SubmitPassword_NewUser, input);

        //wait for response
        await awaitServerResponse(Commands.ServerCommands.AccountCreatedSuccessfully);

        Console.Clear();
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
            await sendString(Commands.ClientCommands.SubmitPassword_ExistingUser, input);

            //wait for response
            await Task.WhenAny(awaitServerResponse(Commands.ServerCommands.PasswordIncorrect), awaitServerResponse(Commands.ServerCommands.LoginSuccessful));
            if (checkServerResponse(Commands.ServerCommands.PasswordIncorrect))
            {
                removeServerResponse(Commands.ServerCommands.PasswordIncorrect);
                Console.WriteLine("The entered password didn't match the username. Enter 0 to retry or anything else to enter a different user name.");
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
                removeServerResponse(Commands.ServerCommands.LoginSuccessful);
                Console.Clear();
                Console.WriteLine("Login successful. Welcome, {0}.", username);
                break;
            }
        }
        return true;
    }

    private async Task displayOnlineUserList()
    {
        await requestOnlineUserList();
        List<string> onlineUsers = (List<string>)receivedData;
        Console.WriteLine("List of online users:");
        if (onlineUsers.Count == 0)
        { Console.WriteLine("/"); }
        else
        {
            for (int i = 0; i < onlineUsers.Count; i++)
            {
                Console.Write(onlineUsers[i]);
                if (i < onlineUsers.Count - 1)
                { Console.Write(", "); }
            }
            Console.WriteLine();
        }
    }
    private async Task requestOnlineUserList()
    {
        await sendCommand(Commands.ClientCommands.RequestOnlineUserList);
        await awaitServerResponse(Commands.ServerCommands.ReceiveList_String);
        removeServerResponse(Commands.ServerCommands.ReceiveList_String);
    }

    private async Task requestBroadcastChatMessage(string msg)
    {
        ChatMessage newMsg = new ChatMessage(username, msg);
        await sendObject(Commands.ClientCommands.RequestSendMessage, newMsg);
        await Task.WhenAny(awaitServerResponse(Commands.ServerCommands.MessageSent), awaitServerResponse(Commands.ServerCommands.ServerError));
        if (checkServerResponse(Commands.ServerCommands.MessageSent))
        {
            removeServerResponse(Commands.ServerCommands.MessageSent);
            Console.WriteLine("Your message has been sent.");
        }
        else
        {
            removeServerResponse(Commands.ServerCommands.ServerError);
            Console.WriteLine("An error has occurred while trying to send the message.");
        }
    }
    private async Task requestWhisperChatMessage(string recipient, string msg)
    {
        ChatMessage newMsg = new ChatMessage(username, msg, recipient);
        await sendObject(Commands.ClientCommands.RequestSendMessage, newMsg);
        await Task.WhenAny(awaitServerResponse(Commands.ServerCommands.MessageSent), awaitServerResponse(Commands.ServerCommands.ServerError));
        if (checkServerResponse(Commands.ServerCommands.MessageSent))
        {
            removeServerResponse(Commands.ServerCommands.MessageSent);
            Console.WriteLine("Your message has been sent.");
        }
        else
        {
            removeServerResponse(Commands.ServerCommands.ServerError);
            Console.WriteLine("An error has occurred while trying to send the message.");
        }
    }


    //shutdown
    public void Dispose()
    {
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

        receivedData = null;
    }
}