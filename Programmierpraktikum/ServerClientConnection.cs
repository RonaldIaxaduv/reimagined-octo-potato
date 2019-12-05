using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using Communication;
using System.Windows.Forms;


public class ServerClientConnection
{
    private const int bufferSize = 1000; //size of the buffer for the network stream
    private Socket handler = null;
    private NetworkStream netS = null;
    private BufferedStream bufS = null;
    public Server parentServer = null;
    //private List<Commands.ClientCommands> clientMsgs = new List<Commands.ClientCommands>();
    private object receivedData;

    public string username = "";
    public bool online = false;


    //basic behaviour
    public ServerClientConnection(Socket handler, Server parentServer)
    {
        this.handler = handler;
        this.parentServer = parentServer;
        netS = new NetworkStream(this.handler);
        bufS = new BufferedStream(netS, bufferSize);
        StartConnection();
    }

    public async void StartConnection()
    {
        try
        { await listen(); }
        catch (ObjectDisposedException)
        { }
        catch (Exception e)
        { Console.WriteLine("Error: {0}", e.Message); }

        if (username != "")
        { Console.WriteLine("Shutting down ServerClientConnection of {0}.", username); }
        else
        { Console.WriteLine("Shutting down ServerClientConnection of pending user."); }

        try
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            Shutdown().GetAwaiter().GetResult();
        }
        catch (ObjectDisposedException)
        { }
        catch (Exception e)
        { Console.WriteLine("Error: {0}", e.Message); }

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
        while (Connection.isConnected(handler))
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
                                                                  //Console.WriteLine("Received command number: {0}", command);
                                                                  //Console.WriteLine("Received data size: {0}", dataSize);

                await handleMessage(Commands.getClientCommand(command), dataSize);

                //reset
                bytesRec = 0;
            }
            await Task.Delay(500); //check for new messages after a short delay
        }
    }


    //information processing, server-client communication
    private async Task handleMessage(Commands.ClientCommands cCom, int dataSize)
    {
        switch (cCom)
        {
            case Commands.ClientCommands.RequestCompareVersionNumber:
                await receiveString(cCom, dataSize);
                if (Application.ProductVersion == (string)receivedData)
                { await sendCommand(Commands.ServerCommands.ValidVersionNumber); }
                else
                { await sendCommand(Commands.ServerCommands.InvalidVersionNumber); }
                return;

            case Commands.ClientCommands.SubmitUserName:
                await receiveString(cCom, dataSize);
                if (parentServer.getOnlineUsers().Contains((string)receivedData)) //the user going by this name is already logged in -> choose different name
                { await sendCommand(Commands.ServerCommands.UserAlreadyOnline); }
                else if (parentServer.getRegisteredUsers().Contains((string)receivedData)) //submitted name contained in the list of registered users?
                { await sendCommand(Commands.ServerCommands.UserNameFound); }
                else
                { await sendCommand(Commands.ServerCommands.UserNameNotFound); }
                username = (string)receivedData;
                return;

            case Commands.ClientCommands.SubmitPassword_ExistingUser:
                await receiveString(cCom, dataSize);
                if (parentServer.tryLogin(username, (string)receivedData))
                {
                    await sendCommand(Commands.ServerCommands.LoginSuccessful);
                    await goOnline(); //sets online to true and notifies all users
                }
                else
                { await sendCommand(Commands.ServerCommands.PasswordIncorrect); }
                break;

            case Commands.ClientCommands.SubmitPassword_NewUser:
                await receiveString(cCom, dataSize);
                if (parentServer.tryRegister(username, (string)receivedData))
                {
                    await sendCommand(Commands.ServerCommands.AccountCreatedSuccessfully);
                    await goOnline(); //sets online to true and notifies all users
                }
                else
                { await sendCommand(Commands.ServerCommands.ServerError); }
                break;

            case Commands.ClientCommands.RequestOnlineUserList:
                await sendObject(Commands.ServerCommands.ReceiveList_String, parentServer.getOnlineUsers());
                break;

            case Commands.ClientCommands.RequestSendMessage:
                await receiveObject(Commands.ClientCommands.RequestSendMessage, dataSize);
                ChatMessage recMsg = (ChatMessage)receivedData;
                if (await parentServer.sendMessage(recMsg))
                { await sendCommand(Commands.ServerCommands.MessageSent); }
                else
                { await sendCommand(Commands.ServerCommands.ServerError); }
                break;


            default:
                Console.WriteLine("Unhandled client command ({0}).", cCom);
                break;
        }
        return;
    }

    //private async Task saveClientCommand(Commands.ClientCommands cCom)
    //{ clientMsgs.Add(cCom); }
    //private async Task awaitClientResponse(string commandString)
    //{
    //    while (clientMsgs.Count == 0 || !checkClientResponse(commandString))
    //    { await Task.Delay(1000); }
    //}
    //private async Task awaitClientResponse(Commands.ClientCommands command)
    //{
    //    await awaitClientResponse(getClientCommandString(command));
    //}
    //private bool checkClientResponse(string commandString)
    //{
    //    foreach (string msg in clientMsgs)
    //    {
    //        if (msg.Contains(commandString)) return true;
    //    }
    //    return false;
    //}
    //private bool checkClientResponse(Commands.ClientCommands command)
    //{
    //    return checkClientResponse(getClientCommandString(command));
    //}
    //private void removeClientResponse(string commandString)
    //{
    //    foreach (string msg in clientMsgs)
    //    {
    //        if (msg.Contains(commandString))
    //        {
    //            clientMsgs.Remove(msg);
    //            return;
    //        }
    //    }
    //}
    //private void removeClientResponse(Commands.ClientCommands command)
    //{
    //    removeClientResponse(getClientCommandString(command));
    //}

    private async Task<byte[]> receiveData(int dataSize)
    {
        int bytesRec = 0;
        byte[] infBlock = new byte[dataSize];
        while (bytesRec < dataSize)
        { bytesRec += await netS.ReadAsync(infBlock, bytesRec, infBlock.Length - bytesRec); }
        return infBlock;
    }
    private async Task receiveString(Commands.ClientCommands cCom, int dataSize)
    {
        receivedData = Encoding.Unicode.GetString(await receiveData(dataSize));
        //await saveClientCommand(cCom);
    }
    private async Task receiveObject(Commands.ClientCommands cCom, int dataSize)
    {
        MemoryStream ms = new MemoryStream(await receiveData(dataSize));
        BinaryFormatter bf = new BinaryFormatter();
        ms.Position = 0;
        receivedData = bf.Deserialize(ms);
        //await saveClientCommand(cCom);
    }

    private async Task sendCommand(Commands.ServerCommands sCom)
    {
        await sendObject(sCom, null);
    }
    private async Task sendString(Commands.ServerCommands sCom, string str)
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
            byte[] prep = BitConverter.GetBytes(Commands.getServerCommandUInt(sCom));
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
    private async Task sendObject(Commands.ServerCommands sCom, object obj)
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
            byte[] prep = BitConverter.GetBytes(Commands.getServerCommandUInt(sCom));
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
    public async Task sendMessage(ChatMessage msg)
    {
        await sendObject(Commands.ServerCommands.MessageForwarded, msg);
    }


    //user-related
    private async Task goOnline()
    {
        online = true;
        parentServer.activeConnections.Add(this);
        var newMsg = new ChatMessage("Server", username + " has entered the chat room.");
        await parentServer.sendMessage(newMsg);
        Console.WriteLine("{0} has come online.", username);
    }


    //shutdown
    public async Task Shutdown()
    {
        if (parentServer.activeConnections.Contains(this))
        {
            online = false;
            parentServer.activeConnections.Remove(this);
            var newMsg = new ChatMessage("Server", username + " has left the chat room.");
            await parentServer.sendMessage(newMsg);
            Console.WriteLine("{0} has gone offline.", username);
        }
        Dispose();
    }
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
        { handler.Dispose(); }
        catch (Exception)
        { }

        receivedData = null;
    }
}
