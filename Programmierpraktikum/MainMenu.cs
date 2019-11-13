using System;
using System.Drawing;

namespace Programmierpraktikum
{
    class MainMenu
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the main menu! Enter 1 to play Connect Four, 2 to play Chomp or anything else to exit the program.");
            bool validInput = false;
            int input = 0;

            do
            {
                try
                { input = int.Parse(Console.ReadLine()); }
                catch (Exception e)
                { Console.WriteLine("Error: " + e.Message); continue; }

                validInput = true;
            } while (!validInput);

            switch (input)
            {
                case 1: //play Connect 4
                    //ConnectFour.ConnectFour cf = new ConnectFour.ConnectFour(); cf.startGame();
                    
                    try
                    { ConnectFour.ConnectFour cf = new ConnectFour.ConnectFour(); cf.startGame(); }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error has occurred: " + e.Message);
                        Console.WriteLine("");
                    }
                    
                    break;



                case 2: //play chomp
                    Size boardSize = new Size(0,0); int width = 0; int height = 0;
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

                    //Chomp newGame = new Chomp(boardSize, opponentType, playerNames, firstPlayer);                    
                    try
                    { Chomp newGame = new Chomp(boardSize, opponentType, playerNames, firstPlayer); }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message);
                        Console.WriteLine("");
                    }   
                                   
                    break;



                default: //exit program
                    return;
            }
        }
    }
}
