using System;
using System.Drawing;
using System.Threading.Tasks;

public class MainMenu
{
    static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult(); //this allows the main method to run asynchronously (the Main method itself is not allowed to be async -> see https://stackoverflow.com/questions/9208921/cant-specify-the-async-modifier-on-the-main-method-of-a-console-app for more information)
    }

    public static async Task MainAsync(string[] args) //the actual main method which will run asynchronously
    {
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Title = "Programmierpraktikum";

        while (true)
        {
            Console.WriteLine("Welcome to the main menu! Please choose between the following menu points by entering that number:\n1: Play Connect Four\n2: Play Chomp\n3: Do a server test\n4: Do a client test\nAnything else: Exit the program.");
            bool validInput = false;
            int input = 0;

            do
            {
                try
                { input = int.Parse(Console.ReadLine()); }
                catch (Exception)
                { return; } //not a number -> other input -> exit program

                validInput = true;
            } while (!validInput);

            switch (input)
            {
                case 1:
                    playConnectFour(); //no need to make this asyncronous so far
                    break;
                case 2: //play chomp
                    playChomp(); //no need to make this asyncronous so far
                    break;
                case 3: //server test
                    await startServer(); //runs asyncronously
                    break;
                case 4: //client test
                    await startClient(); //runs asyncronously
                    break;
                default: //exit program
                    return;
            }

            Console.WriteLine("Press any key to return to the main menu.");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private static void playConnectFour()
    {
        Console.Clear();
        //ConnectFour.ConnectFour cf = new ConnectFour.ConnectFour(); cf.startGame();
        try
        { ConnectFour.ConnectFour cf = new ConnectFour.ConnectFour(); cf.startGame(); }
        catch (Exception e)
        {
            Console.WriteLine("An error has occurred: " + e.Message);
            Console.WriteLine("");
        }

        Console.ReadKey();
        Console.Clear();
    }

    private static void playChomp()
    {
        bool validInput = false;
        Size boardSize = new Size(0, 0); int width = 0; int height = 0;
        Player.playerType opponentType = Player.playerType.Computer;
        string[] playerNames = new string[2];
        int firstPlayer = -1;

        validInput = false;
        do
        {
            Console.WriteLine("Please enter the desired width of the board:");

            try
            { width = int.Parse(Console.ReadLine()); }
            catch (Exception e)
            { Console.WriteLine("Error: " + e.Message); continue; }

            validInput = true;
        } while (!validInput);

        validInput = false;
        do
        {
            Console.WriteLine("Please enter the desired height of the board:");

            try
            { height = int.Parse(Console.ReadLine()); }
            catch (Exception e)
            { Console.WriteLine("Error: " + e.Message); continue; }

            validInput = true;
        } while (!validInput);

        boardSize = new Size(width, height);

        validInput = false;
        do
        {
            Console.WriteLine("Enter 1 to play against the computer or 2 to play against another person:");

            try
            {
                int inputNumber = int.Parse(Console.ReadLine());
                switch (inputNumber)
                {
                    case 1:
                        opponentType = Player.playerType.Computer;
                        break;
                    case 2:
                        opponentType = Player.playerType.Human;
                        break;
                    default:
                        throw new Exception("Invalid number.");
                }
            }
            catch (Exception e)
            { Console.WriteLine("Error: " + e.Message); continue; }

            validInput = true;
        } while (!validInput);

        if (opponentType == Player.playerType.Computer)
        {
            Console.WriteLine("Enter your name:");
            playerNames[0] = Console.ReadLine();
            playerNames[1] = "Computer";
        }
        else
        {
            Console.WriteLine("Enter the first player's name:");
            playerNames[0] = Console.ReadLine();
            Console.WriteLine("Enter the second player's name:");
            playerNames[1] = Console.ReadLine();
        }

        validInput = false;
        do
        {
            if (opponentType == Player.playerType.Computer) Console.WriteLine("Do you want to make the first turn (press 1) or should the computer start (press 2)?");
            else Console.WriteLine("Which player should start (press 1 or 2 respectively)?");

            try
            {
                int inputNumber = int.Parse(Console.ReadLine());
                switch (inputNumber)
                {
                    case 1:
                    case 2:
                        firstPlayer = inputNumber - 1;
                        break;
                    default:
                        throw new Exception("Invalid number.");
                }
            }
            catch (Exception e)
            { Console.WriteLine("Error: " + e.Message); continue; }

            validInput = true;
        } while (!validInput);

        Console.Clear();
        //Chomp newGame = new Chomp(boardSize, opponentType, playerNames, firstPlayer);                    
        try
        { Chomp newGame = new Chomp(boardSize, opponentType, playerNames, firstPlayer); }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            Console.WriteLine("");
        }

        Console.ReadKey();
        Console.Clear();
    }

    private static async Task startServer()
    {
        Console.Clear();
        using (Server s = new Server())
        { await s.startServer(); }
    }

    private static async Task startClient()
    {
        Console.Clear();
        using (Client c = new Client())
        { await c.startClient(); }
    }
}
