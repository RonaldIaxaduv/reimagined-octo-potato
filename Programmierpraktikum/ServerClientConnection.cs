using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;


public class ServerClientConnection
{
    private bool active = false;
    private const int bufferSize = 1000; //size of the buffer for the network stream
    private Socket handler = null;
    private NetworkStream netS = null;
    private BufferedStream bufS = null;
    public Server parentServer = null;
    //private List<Communication.ClientCommands> clientMsgs = new List<Communication.ClientCommands>();
    private object receivedData;

    public string username = "";
    public bool online = false;

    public ServerClientConnection(Socket handler, Server parentServer)
    {
        this.handler = handler;
        this.parentServer = parentServer;
        netS = new NetworkStream(this.handler);
        bufS = new BufferedStream(netS, bufferSize);
        active = true;
        StartConnection();
    }

    public async void StartConnection()
    {
        try
        { await listen(); }
        catch (Exception e)
        { Console.WriteLine("Error: {0}", e.Message); }

        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
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
                Console.WriteLine("Received command number: {0}", command);
                Console.WriteLine("Received data size: {0}", dataSize);

                await handleMessage(Communication.getClientCommand(command), dataSize);

                //reset
                bytesRec = 0;
            }
        }
    }
    private async Task handleMessage(Communication.ClientCommands cCom, int dataSize)
    {
        switch (cCom)
        {
            case Communication.ClientCommands.Ping:
                await sendCommand(Communication.ServerCommands.PingReturn);
                return;

            case Communication.ClientCommands.SubmitUserName:
                await receiveString(cCom, dataSize);
                if (parentServer.getRegisteredUsers().Contains((string)receivedData)) //submitted name contained in the list of registered users?
                { await sendCommand(Communication.ServerCommands.UserNameFound); }
                else
                { await sendCommand(Communication.ServerCommands.UserNameNotFound); }
                username = (string)receivedData;
                return;

            case Communication.ClientCommands.SubmitPassword_ExistingUser:
                await receiveString(cCom, dataSize);
                if (parentServer.tryLogin(username, (string)receivedData))
                {
                    online = true;
                    //WIP: notify all other online users
                    await sendCommand(Communication.ServerCommands.LoginSuccessful);
                    parentServer.activeConnections.Add(this);
                }
                else
                { await sendCommand(Communication.ServerCommands.PasswordIncorrect); }
                break;

            case Communication.ClientCommands.SubmitPassword_NewUser:
                await receiveString(cCom, dataSize);
                if (parentServer.tryRegister(username, (string)receivedData))
                {
                    online = true;
                    //WIP: notify all other online users
                    await sendCommand(Communication.ServerCommands.AccountCreatedSuccessfully);
                    parentServer.activeConnections.Add(this);
                }
                else
                { await sendCommand(Communication.ServerCommands.ServerError); }
                break;

            case Communication.ClientCommands.RequestOnlineUserList:
                await sendObject(Communication.ServerCommands.ReceiveList_String, parentServer.getRegisteredUsers()); //WIP (use getOnlineUsers)
                break;


            default:
                Console.WriteLine("Unknown client command.");
                break;
        }
        return;
    }

    //private async Task saveClientCommand(Communication.ClientCommands cCom)
    //{ clientMsgs.Add(cCom); }
    //private async Task awaitClientResponse(string commandString)
    //{
    //    while (clientMsgs.Count == 0 || !checkClientResponse(commandString))
    //    { await Task.Delay(1000); }
    //}
    //private async Task awaitClientResponse(Communication.ClientCommands command)
    //{
    //    await awaitClientResponse(Communication.getClientCommandString(command));
    //}
    //private bool checkClientResponse(string commandString)
    //{
    //    foreach (string msg in clientMsgs)
    //    {
    //        if (msg.Contains(commandString)) return true;
    //    }
    //    return false;
    //}
    //private bool checkClientResponse(Communication.ClientCommands command)
    //{
    //    return checkClientResponse(Communication.getClientCommandString(command));
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
    //private void removeClientResponse(Communication.ClientCommands command)
    //{
    //    removeClientResponse(Communication.getClientCommandString(command));
    //}

    private async Task<byte[]> receiveData(int dataSize)
    {
        int bytesRec = 0;
        byte[] infBlock = new byte[dataSize];
        while (bytesRec < dataSize)
        { bytesRec += await netS.ReadAsync(infBlock, bytesRec, infBlock.Length - bytesRec); }
        //WIP: if little endian is used by the system, infBlock might have to be reversed here
        return infBlock;
    }
    private async Task receiveString(Communication.ClientCommands cCom, int dataSize)
    {
        receivedData = Encoding.Unicode.GetString(await receiveData(dataSize));
        //await saveClientCommand(cCom);
    }

    private async Task sendCommand(Communication.ServerCommands sCom)
    {
        await sendObject(sCom, null);
    }
    private async Task sendString(Communication.ServerCommands sCom, string str)
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
            byte[] prep = BitConverter.GetBytes(Communication.getServerCommandUInt(sCom));
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
    private async Task sendObject(Communication.ServerCommands sCom, object obj)
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
            byte[] prep = BitConverter.GetBytes(Communication.getServerCommandUInt(sCom));
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

    public void Shutdown()
    {
        active = false; //stop listening -> automatically calls Dispose shortly afterwards
        parentServer.activeConnections.Remove(this);
        Dispose();
    }

    private void Dispose()
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
    }
}
