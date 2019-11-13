using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ConnectFour
{
    public class ConnectFour : Game, Loggable
    {
        public Stack<TurnData> stack;
        public Stack<TurnData> turnStackProp
        {
            get{ return stack; }
            set{ stack = value; }
        }

        ConnectFourBoard newGame;
        Player player1 = new Player();
        Player player2 = new Player();
        public int Height;
        public int Length;
        private int currentPlayer; //either 1 or 2



        public void startGame()
        {
            turnStackProp = new Stack<TurnData>();
            string type = "";
            player1.type = Player.playerType.Human;
            //player1.colour = "Blue";
            //player2.colour = "DarkYellow";
            currentPlayer = 1;
            while (!(type.Equals("s") | type.Equals("m")))
            {
                Console.WriteLine("Press (s) for single player or (m) for multi-player ");
                type = Console.ReadLine();
                if (type == "s")
                {
                    player2.type = Player.playerType.Computer;
                    player2.name = "Roboto-san";
                    Console.WriteLine("Please enter your name:");
                    player1.name = Console.ReadLine();
                }

                else if (type.Equals("m"))
                {
                    player2.type = Player.playerType.Human;
                    Console.WriteLine("Player 1: Please enter your name:");
                    player1.name = Console.ReadLine();
                    Console.WriteLine("Player 2: Please enter your name:");
                    player2.name = Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Invalid character. Press s or m.");
                }
            }


            Console.WriteLine("Please choose the size of the board, starting with the length.\n Please enter an integer value:");
            Length = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Now enter the desired height of the board:");
            Height = Convert.ToInt32(Console.ReadLine());
            newGame = new ConnectFourBoard(Length, Height);
            newGame.display();

            while (newGame.FreeSpaces() == true && newGame.AndTheWinnerIs() == null)
            {
                round();
            }

            //player2 won
            if (newGame.AndTheWinnerIs() == "player2")
            {
                announceWinner();
            }
            return;



        }
        
        void Loggable.addTurn(Player player, Point point)
        {
            TurnData CurrentMove = new TurnData(player, point);
            stack.Push(CurrentMove);
            return;
        }
        void Loggable.removeTurn()
        {
            stack.Pop();
        }

        public override void round()
        {
            turn(player1);

            //if no one has won yet it's player2's turn

            if (newGame.AndTheWinnerIs() == null && newGame.FreeSpaces() == true)
            {
                turn(player2);
            }

            //player 1 won
            else
            {
                announceWinner();
            }
            return;
        }

        public override void turn(Player player)
        {
            int SelectedColumn = -1;
            if (currentPlayer == 1)
            {
                Console.WriteLine("It's " + player1.name + "'s turn!");
            }
            else
            {
                Console.WriteLine("It's " + player2.name + "'s turn!");
            }

            //Console.WriteLine("{0}",WhichPlayer);

            if (currentPlayer == 1 || (currentPlayer == 2 && player2.type == Player.playerType.Human))
            {
                while (SelectedColumn == -1)
                {
                    Console.WriteLine("Drop a new checker piece by entering the column number (as stated above the board)");
                    try
                    {
                        SelectedColumn = Convert.ToInt32(Console.ReadLine());
                    }

                    catch (Exception e) {

                        SelectedColumn = -1;
                    }

                    // incorrect input or no space in column
                    if (SelectedColumn > Length ||  newGame.PlaceInColumn(SelectedColumn - 1) == -1)
                    {
                        Console.WriteLine("Incorrect Input.");
                        SelectedColumn = -1;
                    }

                }

                newGame.HighlightSpace(SelectedColumn - 1, newGame.PlaceInColumn(SelectedColumn-1), currentPlayer);
              
                newGame.display();
            }

            //player 2 is bot
            else
            {
                int RandomCol = RandomColumn();
                //check board for 3 connected pieces 

                int RecommendedColumn = newGame.AIAdvisor(2);
                if (RecommendedColumn < 1000) //can win?
                {
                    newGame.HighlightSpace(RecommendedColumn, newGame.PlaceInColumn(RecommendedColumn), 2);
                }

                //otherwise: keep player 1 from winning
                RecommendedColumn = newGame.AIAdvisor(1);
                        if (RecommendedColumn >=0 && RecommendedColumn < 1000)
                        {
                            newGame.HighlightSpace(RecommendedColumn, newGame.PlaceInColumn(RecommendedColumn), 2);
                        }

                        else 
                        {                
                            newGame.HighlightSpace(RandomColumn(), newGame.PlaceInColumn(RandomCol), 2);
                        }


                newGame.display();

            }


            //switch player
            currentPlayer = (currentPlayer % 2) + 1; // 1 -> 2 ; 2 -> 1
            return;
        }

        public bool ExistsInStack (Point point, Player player)
        {

            int x = point.X;
            int y = point.Y;

            foreach (TurnData item in stack)
            {
                // horizontally 
                if (item.coords.X == x && item.coords.Y == y && item.player == player)
                {
                    return true;
                }
            }
            return false;
        }
        public int RandomColumn()
        {
            Random rnd = new Random();
            int RandomColumn = rnd.Next(0, Length-1);

            // if placing a piece here would help the opponent select new 
            if (newGame.AIAdvisor(1) < 0)
            {
                while (RandomColumn == ((newGame.AIAdvisor(1) * -1)))
                {
                    RandomColumn = rnd.Next(0, Length-1);

                }
            }
            //no space
            while (newGame.PlaceInColumn(RandomColumn) == -1)
            {
                RandomColumn = rnd.Next(0, Length-1);

            }
            return RandomColumn;

        }

        public void announceWinner()
        {
            //player1 won
            if (newGame.AndTheWinnerIs() == "player1")
            {
                Console.WriteLine("{0} won!", player1.name);
            }

            //player2 won
            else if (newGame.AndTheWinnerIs() == "player2")
            {
                Console.WriteLine("{0} won!", player2.name);
            }

            //all spaces filled
            else if (newGame.FreeSpaces() == false)
            {
                Console.WriteLine("it's a draw!");
            }
            return;
        }
    }
}

