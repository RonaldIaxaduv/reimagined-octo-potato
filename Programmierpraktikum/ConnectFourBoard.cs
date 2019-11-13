using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ConnectFour
{

    public class ConnectFourBoard : Board
    {
        int[,] array;

        public ConnectFourBoard(int width, int height)
        {
            array = new int[width, height];
        }

        public override void display()
        {
            Console.Clear();
            int Columns = 1;

            //mark Columns
            Console.WriteLine("Columns:");
            for (int a = 0; a < array.GetLength(0); a++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("  {0}\t", Columns);
                Columns++;
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();

            for (int b = 0; b < array.GetLength(1); b++)
            {
                for (int i = 0; i < array.GetLength(0); i++)

                {
                    //if space is empty
                    if (array[i, b] == 0)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                    }

                    //if space belongs to player 1
                    else if (array[i, b] == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.Blue;

                    }
                    //if space belongs to player 2
                    else if (array[i, b] == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkYellow;

                    }
                    Console.Write("\t");
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write("|");


                }
                Console.Write("\n");
                Console.WriteLine();



            }
            Console.BackgroundColor = ConsoleColor.Black;
            return;


        }

        //checks if there are free spaces
        public bool FreeSpaces()
        {
           
            foreach (var item in array)
            {
                if (item == 0)
                {
                    return true;
                }
            }
            return false;
        }

        //check if anyone has won yet
        public string AndTheWinnerIs()
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {

                for (int b = 0; b < array.GetLength(1); b++)
                {
                    //4 connected horizontally
                    //check if out of bounds
                    if ((i + 3) < array.GetLength(0))
                    {
                        if ((array[i, b] == 1) && (array[i+1, b ] == 1) && (array[i+2, b ] == 1) && (array[i+3, b] == 1))
                        {
                            return "player1";
                        }
                        else if ((array[i, b] == 2) && (array[i+1, b ] == 2) && (array[i+2, b] == 2) && (array[i+3, b] == 2)) 
                        {
                            return "player2";
                        }
                    }
                    //4 connected vertically
                    //out of bounds?
                    if ((b+3)< array.GetLength(1))
                    {
                        if ((array[i, b] == 1) && (array[i, b + 1] == 1) && (array[i, b + 2] == 1) && (array[i, b + 3] == 1))
                        {
                            return "player1";
                        }
                        else if ((array[i, b] == 2) && (array[i, b + 1] == 2) && (array[i, b + 2] == 2) && (array[i, b + 3] == 2))
                        {
                            return "player2";
                        }
                    }

                    //4 connected diagonally(checking the upper right pieces)
                    if ((b+3)< array.GetLength(1) && (i + 3) < array.GetLength(0))
                    {
                        if ((array[i, b] == 1) && (array[i+1, b + 1] == 1) && (array[i+2, b + 2] == 1) && (array[i+3, b + 3] == 1))
                        {
                            return "player1";
                        }
                        else if ((array[i, b] == 2) && (array[i+1, b + 1] == 2) && (array[i+2, b + 2] == 2) && (array[i+3, b + 3] == 2))
                        {
                            return "player2";
                        }
                    }

                    //4 connected diagonally(checking the pieces to the left of the selected one)
                    if ((b + 3)< array.GetLength(1) && (i - 3) >=0)
                    {
                        if ((array[i, b] == 1) && (array[i - 1, b + 1] == 1) && (array[i - 2, b + 2] == 1) && (array[i - 3, b +3] == 1))
                        {
                            return "player1";
                        }
                        else if ((array[i, b] == 2) && (array[i - 1, b + 1] == 2) && (array[i - 2, b + 2] == 2) && (array[i - 3, b +3] == 2))
                        {
                            return "player2";
                        }
                    }
                    
                }
              
            }
            return null;
        }

       
        public int AIAdvisor(int Whichplayer)
        {
            

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int b = 0; b < array.GetLength(1); b++)
                {
                    // horizontally
                    if (i + 3 < array.GetLength(0))
                    {
                        // 3 connected horizontally
                        if (array[i, b] == Whichplayer && array[i + 1, b] == Whichplayer && array[i + 2, b] == Whichplayer)
                        {

                            // 4th is empty, can be placed
                            if (((b == array.GetLength(1) - 1 && array[i + 3, b] == 0)) || (b < array.GetLength(1) - 1 && array[i + 3, b + 1] != 0))
                            {
                                //tells AI that where it needs to place a checked piece to not let the opponent win
                                return (i + 3);
                            }
                            // if placed here opponent can connect 4 in their next turn
                            //else if (b < (array.GetLength(1) - 1) && array[i + 3, b + 1] != 0 && array[i + 3, b + 2] == 0)
                            //{
                            //    //warns AI, that placing a checked piece here would help the opponent
                            //    return -(i + 3);
                            //}
                        }
                    }

                    // vertically
                    if (b + 3 < array.GetLength(1))
                    {
                        //3 connected vertically
                        if (array[i, b + 1] == Whichplayer && array[i, b + 2] == Whichplayer && array[i, b + 3] == Whichplayer)
                        {
                            // 4th is empty, can be placed
                            if (array[i, b] == 0)
                            {
                                //tells AI that where it needs to place a checked piece to not let the opponent win
                                return (i);
                            }
                        }
                    }

                    //3 diagonally (upper-right)
                    if (b + 3 < array.GetLength(1) && i + 3 < array.GetLength(0))
                    {
                        if (array[i, b + 1] == Whichplayer && array[i + 1, b + 2] == Whichplayer && array[i + 2, b + 3] == Whichplayer)
                        {
                            //tells AI that where it needs to place a checked piece to not let the opponent win
                            if (array[i + 3, b] != 0)
                            {
                                return (i + 3);
                            }
                            // if placed here opponent can connect 4 in their next turn
                            //else if (array[i + 3, b] == 0 && array[i + 3, b + 1] != 0)
                            //{
                            //    return -(i + 3);
                            //}

                        }
                    }

                    //3 diagonally (upper-left)
                    else if (b + 3 < array.GetLength(1) && (i - 3) >= 0)
                    {
                        if (array[i, b + 1] == Whichplayer && array[i - 1, b + 2] == Whichplayer && array[i - 2, b + 3] == Whichplayer)
                        {
                            //tells AI that where it needs to place a checked piece to not let the opponent win

                            if (array[i - 3, b] != 0)
                            {
                                return (i - 3);
                            }

                            // if placed here opponent can connect 4 in their next turn

                            //else if (array[i - 3, b] == 0 && array[i - 3, b + 1] != 0)
                            //{
                            //    return -(i - 3);
                            //}
                        }

                    }
                }
            }
            // return 1000 if there are no 3 connected pieces
            return 1000;
        }

        public void HighlightSpace(int column, int row, int player)
        {
            array[column, row] = player;
        }

        public int PlaceInColumn(int Column)
        {

            // incorrect user input/ no such column
            if (Column >= array.GetLength(0) || Column< 0)
            {
                return -1;
            }
            //going through column bottom to top
            for (int b= array.GetLength(1)-1; b>=0 ; b--)
            {
                if(array[Column, b]==0)
                { 
                    return b;
                }
            }
            //if there's no space in the column
            return -1;

        }
        //public override Size size
        //{

        //    set
        //    {
        //        board.size = new Size();
        //    }

        //    get
        //    {
        //        return board.size;
        //    }

        //}
    }
}
