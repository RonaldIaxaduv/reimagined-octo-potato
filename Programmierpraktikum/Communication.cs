using System;
using System.Net.Sockets;

namespace Communication
{

    public static class Commands
    {

        public enum ClientCommands
        {
            RequestCompareVersionNumber,
            SubmitUserName,
            SubmitPassword_ExistingUser,
            SubmitPassword_NewUser,
            ClientError,

            RequestOnlineUserList,
            RequestSendMessage
        }
        public static uint getClientCommandUInt(ClientCommands cCom)
        {
            switch (cCom)
            {
                case ClientCommands.RequestCompareVersionNumber: return 0;
                case ClientCommands.SubmitUserName: return 1;
                case ClientCommands.SubmitPassword_ExistingUser: return 2;
                case ClientCommands.SubmitPassword_NewUser: return 3;
                case ClientCommands.ClientError: return 4;

                case ClientCommands.RequestOnlineUserList: return 5;
                case ClientCommands.RequestSendMessage: return 6;

                default: throw new Exception("Unknown client command (" + cCom + ").");
            }
        }
        public static ClientCommands getClientCommand(uint cCom)
        {
            switch (cCom)
            {
                case 0: return ClientCommands.RequestCompareVersionNumber;
                case 1: return ClientCommands.SubmitUserName;
                case 2: return ClientCommands.SubmitPassword_ExistingUser;
                case 3: return ClientCommands.SubmitPassword_NewUser;
                case 4: return ClientCommands.ClientError;

                case 5: return ClientCommands.RequestOnlineUserList;
                case 6: return ClientCommands.RequestSendMessage;

                default: throw new Exception("Unknown client command number (" + cCom + ").");
            }
        }


        public enum ServerCommands
        {
            ValidVersionNumber,
            InvalidVersionNumber,
            InvalidUserName,
            UserAlreadyOnline,
            UserNameFound,
            UserNameNotFound,
            PasswordIncorrect,
            LoginSuccessful,
            AccountCreatedSuccessfully,
            ServerError,

            MessageSent,
            MessageForwarded,
            ReceiveString,
            ReceiveList_String

        }
        public static uint getServerCommandUInt(ServerCommands sCom)
        {
            switch (sCom)
            {
                case ServerCommands.ValidVersionNumber: return 0;
                case ServerCommands.InvalidVersionNumber: return 1;
                case ServerCommands.InvalidUserName: return 2;
                case ServerCommands.UserAlreadyOnline: return 3;
                case ServerCommands.UserNameFound: return 4;
                case ServerCommands.UserNameNotFound: return 5;
                case ServerCommands.PasswordIncorrect: return 6;
                case ServerCommands.LoginSuccessful: return 7;
                case ServerCommands.AccountCreatedSuccessfully: return 8;
                case ServerCommands.ServerError: return 9;

                case ServerCommands.MessageSent: return 10;
                case ServerCommands.MessageForwarded: return 11;
                case ServerCommands.ReceiveString: return 12;
                case ServerCommands.ReceiveList_String: return 13;

                default: throw new Exception("Unknown server command (" + sCom + ").");
            }
        }
        public static ServerCommands getServerCommand(uint sCom)
        {
            switch (sCom)
            {
                case 0: return ServerCommands.ValidVersionNumber;
                case 1: return ServerCommands.InvalidVersionNumber;
                case 2: return ServerCommands.InvalidUserName;
                case 3: return ServerCommands.UserAlreadyOnline;
                case 4: return ServerCommands.UserNameFound;
                case 5: return ServerCommands.UserNameNotFound;
                case 6: return ServerCommands.PasswordIncorrect;
                case 7: return ServerCommands.LoginSuccessful;
                case 8: return ServerCommands.AccountCreatedSuccessfully;
                case 9: return ServerCommands.ServerError;

                case 10: return ServerCommands.MessageSent;
                case 11: return ServerCommands.MessageForwarded;
                case 12: return ServerCommands.ReceiveString;
                case 13: return ServerCommands.ReceiveList_String;

                default: throw new Exception("Unknown server command number (" + sCom + ").");
            }
        }
    }
    

    [Serializable()]
    public class ChatMessage
    {
        public string sender, msg, recipient;
        public bool global; //determines whether message is broadcast (to all online users) or only sent to one specific user

        public ChatMessage(string sender, string msg, string recipient)
        {
            this.sender = sender; this.msg = msg; this.recipient = recipient;
            global = false;
        }
        public ChatMessage(string sender, string msg) : this(sender, msg, "")
        { global = true; }
    }


    public static class Connection
    {
        public static bool isConnected(Socket socket)
        {
            //there is no direct method to check whether a client has disconnected, but this works (see https://stackoverflow.com/questions/722240/instantly-detect-client-disconnection-from-server-socket )
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0); //socket isn't readable or has data available to be read
            }
            catch (SocketException) { return false; }
        }

    }

}