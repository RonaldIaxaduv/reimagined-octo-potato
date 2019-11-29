using System;

public static class Communication
{
    public enum ClientCommands
    {
        Ping,
        SubmitUserName,
        SubmitPassword_ExistingUser,
        SubmitPassword_NewUser,
        ClientError,
        RequestOnlineUserList

    }

    public enum ServerCommands
    {
        PingReturn,
        UserNameFound,
        UserNameNotFound,
        PasswordIncorrect,
        LoginSuccessful,
        AccountCreatedSuccessfully,
        ServerError,

        ReceiveString,
        ReceiveList_String

    }

    public enum Commands //would this be easier? might be at the cost of some functionality, dunno
    {
        ReceiveString,
        ReceiveList
    }



    public static uint getClientCommandUInt(ClientCommands cCom)
    {
        switch (cCom)
        {
            case ClientCommands.Ping: return 0;
            case ClientCommands.SubmitUserName: return 1;
            case ClientCommands.SubmitPassword_ExistingUser: return 2;
            case ClientCommands.SubmitPassword_NewUser: return 3;
            case ClientCommands.ClientError: return 4;
            case ClientCommands.RequestOnlineUserList: return 5;

            default: throw new Exception("Unknown client command.");
        }
    }

    public static ClientCommands getClientCommand(uint cCom)
    {
        switch (cCom)
        {
            case 0: return ClientCommands.Ping;
            case 1: return ClientCommands.SubmitUserName;
            case 2: return ClientCommands.SubmitPassword_ExistingUser;
            case 3: return ClientCommands.SubmitPassword_NewUser;
            case 4: return ClientCommands.ClientError;
            case 5: return ClientCommands.RequestOnlineUserList;

            default: throw new Exception("Unknown client command number.");
        }
    }



    public static uint getServerCommandUInt(ServerCommands sCom)
    {
        switch (sCom)
        {
            case ServerCommands.PingReturn: return 0;
            case ServerCommands.UserNameFound: return 1;
            case ServerCommands.UserNameNotFound: return 2;
            case ServerCommands.PasswordIncorrect: return 3;
            case ServerCommands.LoginSuccessful: return 4;
            case ServerCommands.AccountCreatedSuccessfully: return 5;
            case ServerCommands.ServerError: return 6;
            case ServerCommands.ReceiveString: return 7;
            case ServerCommands.ReceiveList_String: return 8;

            default: throw new Exception("Unknown server command.");
        }
    }

    public static ServerCommands getServerCommand(uint sCom)
    {
        switch (sCom)
        {
            case 0: return ServerCommands.PingReturn;
            case 1: return ServerCommands.UserNameFound;
            case 2: return ServerCommands.UserNameNotFound;
            case 3: return ServerCommands.PasswordIncorrect;
            case 4: return ServerCommands.LoginSuccessful;
            case 5: return ServerCommands.AccountCreatedSuccessfully;
            case 6: return ServerCommands.ServerError;
            case 7: return ServerCommands.ReceiveString;
            case 8: return ServerCommands.ReceiveList_String;

            default: throw new Exception("Unknown server command number.");
        }
    }
}
