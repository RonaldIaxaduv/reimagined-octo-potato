using System;
using System.Drawing;
using System.Collections.Generic;

public class Chomp : Game, Loggable
{
    private Stack<TurnData> turnStack;
    public Stack<TurnData> turnStackProp
    {
        get { return turnStack; }
        set { turnStack = value; }
    }
    private int firstPlayer;
    private bool running = true;
    private const int DIFF = 3; //difficulty (number of turns that the AI calculates in advance)

    public Chomp(Size boardSize, Player.playerType opponentType, string[] playerNames, int firstPlayer)
    {
        //prepare board
        try
        { this.board = new ChompBoard(boardSize); }
        catch (Exception e)
        { throw e; }

        //prepare players
        try
        {
            players[0] = new Player(playerNames[0], Player.playerType.Human);
            if (opponentType == Player.playerType.Computer)
            { players[1] = new Player("Computer", Player.playerType.Computer); }
            else
            { players[1] = new Player(playerNames[1], Player.playerType.Human); }
        }        
        catch (Exception e)
        { throw e; }
        
        if (players[0].name == players[1].name)
        { throw new Exception("Players may not share the same name."); }

        this.firstPlayer = firstPlayer;
        if (firstPlayer != 1 && firstPlayer != 0)
        { throw new Exception("Invalid value for first player."); }

        //prepare stack
        turnStackProp = new Stack<TurnData>();

        //start game
        round();

        //run game until end (which will cancel the following loop automatically). it has been put into a loop instead of being recursive so that no stack overflows can occur
        while (running) 
        { round(); }
    }

    public override void round()
    {
        for (int i = 0, selection = firstPlayer; i < players.Length; i++, selection = (selection + 1) % 2) //selection switches between 0 and 1 (players.Length-1 times, i.e. once)
        {
            turn(players[selection]);
            if (!((ChompBoard)board).squares[0, 0])
            {
                Console.WriteLine(players[selection].name + " has won.");
                gameWon(players[selection]); //will cancel the game afterwards
                return;
            }
        }
    }
    public override void turn(Player player)
    {
        board.display();

        if (player.type == Player.playerType.Computer)
        {
            //Console.WriteLine("#### Computer's turn...");
            Point choice = computerTurn();
            ((ChompBoard)board).snap(choice);
            addTurn(player, choice);
        }
        else //player.type == Player.playerType.Human
        {
            bool validInput = false;
            do
            {
                Console.WriteLine(player.name + ": Please enter the coordinates of the top-left corner of the rectangle that you want to remove. (e.g. '2,0')");
                string input = Console.ReadLine();
                try
                {
                    //evaluate input
                    Point destination = new Point();
                    destination.X = int.Parse(input.Substring(0, input.IndexOf(",")));
                    destination.Y = int.Parse(input.Substring(input.IndexOf(",") + 1));

                    //perform turn
                    ((ChompBoard)board).snap(destination);
                    validInput = true;

                    //log turn
                    addTurn(player, destination);

                }
                catch (Exception e)
                { Console.WriteLine("Error: " + e.Message); }

            } while (!validInput);
        }
    }

    private Point computerTurn()
    {
        ChompBoard thisBoard = (ChompBoard)board;
        Point destination = getWinningPoint(thisBoard.squares); //look for several simple win conditions

        if (destination == new Point(-1, -1))
        {
            //simple AI
            /*
            destination = getBiasedRandom(thisBoard.squares); //look for a point that doesn't result in a simple win condition for the other player
            if (destination == new Point(-1, -1))
            { Console.WriteLine("getFullRandom"); return getFullRandom(thisBoard.squares); } //select random
            else Console.WriteLine("getBiasedRandom");
            */

            //advanced with list (can display total number of remaining choices)
            /*
            List<Point> nextLs;
            for (int degree = DIFF; degree > 0; degree--)
            {
                try
                { nextLs = getBiasedRandomRecursive(thisBoard.squares, degree); Console.WriteLine("Degree " + degree + ": " + nextLs.Count + " choices."); }
                catch (Exception e)
                { Console.WriteLine(degree + " failed: " + e.Message); continue; }

                if (nextLs.Count > 0)
                { destination = nextLs[(new Random()).Next(nextLs.Count)]; }
                else destination = new Point(-1, -1);

                if (destination != new Point(-1, -1)) return destination;
            }
            Console.WriteLine("getFullRandom"); return getFullRandom(thisBoard.squares);
            */

            //advanced with point
            for (int degree = DIFF; degree > 0; degree--)
            {
                try
                { destination = getBiasedRandomRecursivePoint(thisBoard.squares, degree); }
                catch (Exception e)
                { Console.WriteLine(degree + " failed: " + e.Message); continue; }

                if (destination != new Point(-1, -1)) return destination;
            }
            Console.WriteLine("getFullRandom"); return getFullRandom(thisBoard.squares);

        }
        else Console.WriteLine("getWinningPoint");

        return destination;
    }
    private Point getWinningPoint(bool[,] fields)
    {
        //this function handles several simple win conditions. if a constellation isn't handled, (-1,-1) is returned.

        if (!(fields[0, 1] || fields[1,0]))
        { return new Point(0, 0); } //-> win

        Rectangle rect = containedRectangle(fields);

        if (rect.Size != new Size(0,0)) //remaining area contains a rectangle (and nothing else)
        {
            //1 unit-wide row:
            if (rect.Size.Width == 1) //only vertical strip on the left remaining
            {
                if (rect.Size.Height > 2) return new Point(0, 2); //-> win
                //else return new Point(0, 1); //-> loss (only one choice)
                else return new Point(-1, -1);
            }
            else if (rect.Size.Height == 1) //only horizontal strip at the top remaining
            {
                if (rect.Size.Width > 2) return new Point(2, 0); //-> win
                //else return new Point(1, 0); //-> loss (only one choice)
                else return new Point(-1, -1);
            }

            //squares:
            if (rect.Size.Width == rect.Size.Height)
            {
                if (rect.Size.Width == 2) return new Point(0, 1); //-> win
                else return new Point(1, 1); //-> win
            }

            //other rectangles:
            if (rect.Size.Width == 2) return new Point(0, 1); //-> win
            else if (rect.Size.Height == 2) return new Point(1, 0); //-> win

            //rest: too complex
        }
        else //no (single) rectangle contained
        {
            //get sizes of the outer rows
            int topSize = getTopRowSize(fields);
            int leftSize = getLeftRowSize(fields);

            //two 1 unit-wide rows:
            if (fields[0,1] && fields[1,0] && !fields[1,1])
            {             
                if ((topSize + leftSize - 1) % 2 == 1) //uneven number of remaining fields
                {
                    if (topSize == leftSize) //rows are of the same size
                    {
                        if (topSize < 3) return new Point(1, 0); //-> win
                        //else return new Point(-1, -1); //-> loss (several choices)
                    }
                }
                else //uneven number of remaining fields and rows are of different sizes or even number remaining fields
                {
                    if (topSize == 2) return new Point(0, 1); //-> win
                    else if (leftSize == 2) return new Point(1, 0); //-> win

                    if (topSize > leftSize) return new Point(leftSize, 0); //-> win
                    else if (leftSize > topSize) return new Point(0, topSize); //-> win
                }

                //rest: too complex
            }
            else //not two 1 unit-wide rows (nor single rectangle) -> more complex area
            {
                if (topSize == 2) return new Point(0, 1); //-> win
                else if (leftSize == 2) return new Point(1, 0); //-> win
                else if (topSize == leftSize) return new Point(1, 1); //-> win

                //rest: too complex
            }
        }

        return new Point(-1, -1);
    }
    private Point getBiasedRandom(bool[,] fields)
    {
        //this function select a random viable point in fields that doesn't result in a non-negative output of aaronsChoice (i.e. the other player is less likely to make a winning snap with their next turn). if there are none, (-1,-1) is returned.
        //note: by adding another index variable as an argument, this function could be used to calculate x turns in advance -> difficulty selection possible

        List<Point> viable = new List<Point>();

        //get viable
        if (!(fields[0, 1] || fields[1, 0]))
        { return new Point(0, 0); }

        for (int x = 0; x < fields.GetLength(0); x++)
        {
            for (int y = 0; y < fields.GetLength(1); y++)
            {
                if (fields[x, y])
                {
                    if (x == 0 && y == 0)
                    { continue; }

                    bool[,] snapped = clone2DArray<Boolean>(fields);

                    for (int snapX = x; snapX < fields.GetLength(0); snapX++) //get fields after snapping at [x,y]
                    {
                        for (int snapY = y; snapY < fields.GetLength(1); snapY++)
                        {
                            if (snapped[x, y])
                            { snapped[x, y] = false; } //set all squares to be active
                            else
                            { continue; } //all squares behind one that has already been broken off are false -> skip to next line
                        }
                    }

                    if (getWinningPoint(snapped) == new Point(-1, -1)) //no easy optimal choice -> consider
                    { viable.Add(new Point(x, y)); }
                }
            }
        }

        //select and return random viable point (if possible)
        Console.WriteLine(viable.Count + " choices.");
        if (viable.Count != 0)
        { return viable[(new Random()).Next(viable.Count)]; }

        return new Point(-1, -1);
    }
    private Point getFullRandom(bool[,] fields)
    {
        //this function returns a random viable point in fields

        ChompBoard thisBoard = (ChompBoard)board;
        List<Point> viable = new List<Point>();

        //get viable 
        if (!(fields[0,1] || fields[1,0]))
        { return new Point(0, 0); }

        for (int x = 0; x < fields.GetLength(0); x++)
        {
            for (int y = 0; y < fields.GetLength(1); y++)
            {
                if (fields[x,y] && !(x == 0 && y == 0))
                { viable.Add(new Point(x, y)); }
            }
        }

        //select and return random viable point
        return viable[(new Random()).Next(viable.Count)];
    }

    private Point getBiasedRandomRecursivePt(bool[,] fields, int degree)
    {
        //this function select a random viable point in fields that doesn't result in a non-negative output of aaronsChoice (i.e. the other player is less likely to make a winning snap with their next turn). if there are none, (-1,-1) is returned.
        //degree: number of possible turns that the function calculates in advance; >0
        //simple description: biasedRandomRecursive(bool[,] fields, [UNEVEN number x]) -> "With this Point, the opponent probably cannot make a winning snap in x turns"
        //                    biasedRandomRecursive(bool[,] fields, [EVEN number y]) -> "With this Point, I probably cannot make a winning snap in y turns"

        if (degree < 1)
        { throw new Exception("Invalid value of degree."); }

        List<Point> viable = new List<Point>();

        //get viable
        if (!(fields[0, 1] || fields[1, 0]))
        {
            if (degree % 2 == 1) return new Point(0, 0);
            else return new Point(-1, -1);
        }

        for (int x = 0; x < fields.GetLength(0); x++)
        {
            for (int y = 0; y < fields.GetLength(1); y++)
            {
                if (fields[x, y])
                {
                    if (x == 0 && y == 0)
                    { continue; }

                    bool[,] snapped = clone2DArray<Boolean>(fields);
                    for (int snapX = x; snapX < fields.GetLength(0); snapX++) //get fields after snapping at [x,y]
                    {
                        for (int snapY = y; snapY < fields.GetLength(1); snapY++)
                        {
                            if (snapped[x, y])
                            { snapped[x, y] = false; } //set all squares to be active
                            else
                            { continue; } //all squares behind one that has already been broken off are false -> skip to next line
                        }
                    }

                    if (degree > 1)
                    {
                        if (degree % 2 == 0 ^ getBiasedRandomRecursivePt(snapped, degree - 1) == new Point(-1, -1)) //EITHER next turn == opponent's turn and no winning turns exist OR next turn == my turn and winning turn exists
                        { viable.Add(new Point(x, y)); }
                    }
                    else //degree == 1
                    {
                        if (getWinningPoint(snapped) == new Point(-1, -1))
                        { viable.Add(new Point(x, y)); }
                    }                    
                }
            }
        }

        //select and return random viable point (if possible)
        if (viable.Count != 0)
        { return viable[(new Random()).Next(viable.Count)]; }

        return new Point(-1, -1);
    }
    private List<Point> getBiasedRandomRecursive(bool[,] fields, int degree)
    {
        //this function select a random viable point in fields that doesn't result in a non-negative output of aaronsChoice (i.e. the other player is less likely to make a winning snap with their next turn). if there are none, (-1,-1) is returned.
        //degree: number of possible turns that the function calculates in advance; >0

        if (degree < 1)
        { throw new Exception("Invalid value of degree."); }

        List<Point> viable = new List<Point>();

        //get viable
        if (!(fields[0, 1] || fields[1, 0]))
        {
            viable.Add(new Point(0, 0));
            return viable;
        }

        for (int x = 0; x < fields.GetLength(0); x++)
        {
            for (int y = 0; y < fields.GetLength(1); y++)
            {
                if (fields[x, y])
                {
                    if (x == 0 && y == 0)
                    { continue; }

                    bool[,] snapped = clone2DArray<Boolean>(fields);
                    for (int snapX = x; snapX < fields.GetLength(0); snapX++) //get fields after snapping at [x,y]
                    {
                        for (int snapY = y; snapY < fields.GetLength(1); snapY++)
                        {
                            if (snapped[snapX, snapY])
                            { snapped[snapX, snapY] = false; } //set all squares to be active
                            else
                            { continue; } //all squares behind one that has already been broken off are false -> skip to next line
                        }
                    }

                    if (degree > 1)
                    {                       
                        if (getWinningPoint(snapped) == new Point(-1, -1)) //take a further look at the usual viable points
                        {
                            List<Point> nextDeg = getBiasedRandomRecursive(snapped, degree - 1);
                            if (nextDeg.Count == 0) //next player would not have a good answer (i.e. the game is basically won) -> remember
                            { viable.Add(new Point(x, y)); }  //Console.WriteLine("Taken (degree " + degree + ")."); }
                        }
                    }
                    else //degree == 1 -> like the original biasedRandom
                    {
                        if (getWinningPoint(snapped) == new Point(-1, -1))
                        { viable.Add(new Point(x, y)); }
                    }                   
                }
            }
        }

        return viable;
    }
    private Point getBiasedRandomRecursivePoint(bool[,] fields, int degree)
    {
        //this function select a random viable point in fields that doesn't result in a non-negative output of aaronsChoice (i.e. the other player is less likely to make a winning snap with their next turn). if there are none, (-1,-1) is returned.
        //degree: number of possible turns that the function calculates in advance; >0

        if (degree < 1)
        { throw new Exception("Invalid value of degree."); }

        List<Point> viable = new List<Point>();

        //get viable
        if (!(fields[0, 1] || fields[1, 0]))
        {
            return new Point(0, 0);
        }

        for (int x = 0; x < fields.GetLength(0); x++)
        {
            for (int y = 0; y < fields.GetLength(1); y++)
            {
                if (fields[x, y])
                {
                    if (x == 0 && y == 0)
                    { continue; }

                    bool[,] snapped = clone2DArray<Boolean>(fields);
                    for (int snapX = x; snapX < fields.GetLength(0); snapX++) //get fields after snapping at [x,y]
                    {
                        for (int snapY = y; snapY < fields.GetLength(1); snapY++)
                        {
                            if (snapped[snapX, snapY])
                            { snapped[snapX, snapY] = false; } //set all squares to be active
                            else
                            { continue; } //all squares behind one that has already been broken off are false -> skip to next line
                        }
                    }

                    if (degree > 1)
                    {
                        if (getWinningPoint(snapped) == new Point(-1, -1)) //take a further look at the usual viable points
                        {
                            if (getBiasedRandomRecursivePoint(snapped, degree - 1) == new Point(-1, -1)) //next player would not have a good answer (i.e. the game is basically won) -> remember
                            { viable.Add(new Point(x, y)); }  //Console.WriteLine("Taken (degree " + degree + ")."); }
                        }
                    }
                    else //degree == 1 -> like the original biasedRandom
                    {
                        if (getWinningPoint(snapped) == new Point(-1, -1))
                        { viable.Add(new Point(x, y)); }
                    }
                }
            }
        }

        if (viable.Count < 1) return new Point(-1, -1);
        else return viable[(new Random()).Next(viable.Count)];
    }

    private Rectangle containedRectangle(bool[,] fields)
    {
        //if fields contains a (single) rectangle, it is returned by this function. otherwise an empty rectangle is returned.

        int width = getTopRowSize(fields); int height = getLeftRowSize(fields);

        //if there is more than one rectangle, a corner has been snapped out of this rectangle -> its bottom right corner isn't active anymore
        if (fields[width - 1, height - 1]) return new Rectangle(0, 0, width, height);
        else return new Rectangle(0, 0, 0, 0);
    }
    private int getTopRowSize(bool[,] fields)
    {
        //returns the number of active fields in the top row of fields

        int output = 0;
        for (int x = 0; x < fields.GetLength(0); x++)
        {
            if (fields[x, 0]) output++;
            else break;
        }
        return output;
    }
    private int getLeftRowSize(bool[,] fields)
    {
        //returns the number of active fields in the left row of fields

        int output = 0;
        for (int y = 0; y < fields.GetLength(1); y++)
        {
            if (fields[0, y]) output++;
            else break;
        }
        return output;
    }
    private T[,] clone2DArray<T>(T[,] input)
    {
        T[,] output = new T[input.GetLength(0), input.GetLength(1)];

        for (int x = 0; x < input.GetLength(0); x++)
        {
            for (int y = 0; y < input.GetLength(1); y++)
            {
                output[x, y] = input[x, y];
            }
        }

        return output;
    }

    /*
    private Point computerTurnMinEven()
    {
        ChompBoard thisBoard = (ChompBoard)board;

        if (!(thisBoard.squares[0, 1] || thisBoard.squares[1, 0])) //only top-left square remains
        { return new Point(0, 0); }
        else
        {
            int[,] remSquares = new int[board.size.Width, board.size.Height]; //values: number of squares that will remain if this square is removed
            int counter; //used to count the remaining squares
            Point minimumEven = new Point(board.size.Width, board.size.Height); //points to the field that will yield the least remaining fields (even)
            Point maximumUneven = new Point(board.size.Width, board.size.Height); //points to the field that will yield the most remaining fields (uneven)

            for (int x = 0; x < thisBoard.size.Width; x++)
            {
                for (int y = 0; y < thisBoard.size.Height; y++)
                {
                    if (x == 0 && y == 0)
                    { continue; }

                    //Console.WriteLine("[" + x + "," + y + "]");
                    if (!thisBoard.squares[x, y])
                    { remSquares[x, y] = 0; }
                    else
                    {
                        counter = 0;

                        bool[,] tempBoard = clone2DArray<Boolean>(thisBoard.squares);
                        for (int snapX = x; snapX < thisBoard.size.Width; snapX++) //snap the temp board at [x,y]
                        {
                            for (int snapY = y; snapY < thisBoard.size.Height; snapY++)
                            {
                                if (tempBoard[snapX, snapY])
                                { tempBoard[snapX, snapY] = false; } //set all squares to be active
                                else
                                { continue; } //all squares behind one that has already been broken off are false -> skip to next line
                            }
                        }

                        for (int countX = 0; countX < thisBoard.size.Width; countX++) //count remaining squares
                        {
                            for (int countY = 0; countY < thisBoard.size.Height; countY++)
                            {
                                if (tempBoard[countX, countY])
                                { counter++; } //set all squares to be active
                                else
                                { continue; } //all squares behind one that has already been broken off are false -> skip to next line
                            }
                        }

                        remSquares[x, y] = counter; //update pointers
                        if (counter % 2 == 0)
                        {
                            if (minimumEven == new Point(thisBoard.size.Width, thisBoard.size.Height))
                            { minimumEven = new Point(x, y); }
                            else
                            {
                                if (remSquares[minimumEven.X, minimumEven.Y] > counter)
                                { minimumEven = new Point(x, y); }
                            }                            
                        }
                        else
                        {
                            if (maximumUneven == new Point(thisBoard.size.Width, thisBoard.size.Height))
                            { maximumUneven = new Point(x, y); }
                            else
                            {
                                if (remSquares[minimumEven.X, minimumEven.Y] < counter)
                                { maximumUneven = new Point(x, y); }
                            }
                        }
                    }                   

                }
            }

            //for (int y = 0; y < thisBoard.size.Height; y++)
            //{
            //    for (int x = 0; x < thisBoard.size.Width; x++)
            //    {
            //        Console.Write(remSquares[x, y]);
            //        Console.Write("\t"); //tabulators inbetween the blocks
            //    }
            //    Console.WriteLine();
            //}

            if (!(minimumEven == new Point(thisBoard.size.Width, thisBoard.size.Height))) //contains a field that will leave the opponent with an even number of fields -> optimal -> remove as many squares as possible
            {
                //Console.WriteLine(minimumEven);
                return minimumEven;
            }  
            else //each removed square would leave the opponent with an uneven number of remaining fields -> not optimal -> remove as few as possible
            {
                //Console.WriteLine(maximumUneven);
                return maximumUneven;
            }

        }
        

    }
    private Point computerTurnOptimal()
    {
        ChompBoard thisBoard = (ChompBoard)board;

        //search for optimal (game-winning) turn
        if (!(thisBoard.squares[0,1] || thisBoard.squares[1,0])) //only top-left square remains
        { return new Point(0, 0); }
        else
        {
            bool[,] bias = new bool[board.size.Width, board.size.Height]; //fields that are true: optimal turns

            for (int x = 0; x < thisBoard.size.Width; x++)
            {
                for (int y = 0; y < thisBoard.size.Height; y++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        bias[x, y] = optimalTurn(clone2DArray<Boolean>(thisBoard.squares), new Point(x, y), 0);
                        if (bias[x, y]) { return new Point(x, y); }
                    }
                }
            }            
            
            //no optimal turn found -> select a point that only removes a single square
            for (int x = thisBoard.size.Width - 1; x >= 0; x--)
            {
                for (int y = thisBoard.size.Height - 1; y >= 0; y--)
                {
                    if (thisBoard.squares[x, y])
                    {
                        if (x + 1 >= thisBoard.size.Width && y + 1 >= thisBoard.size.Height) //bottom-right corner
                        {
                            return new Point(x, y);
                        }
                        else if (x + 1 >= thisBoard.size.Width)
                        {
                            if (!thisBoard.squares[x, y + 1])
                            { return new Point(x, y); }
                            else
                            { break; } //since y-loop is performed first
                        }
                        else if (y + 1 >= thisBoard.size.Height)
                        {
                            if (!thisBoard.squares[x + 1, y])
                            { return new Point(x, y); }
                            //else
                            //{ continue; } //since y-loop is performed first
                        }
                        else
                        {
                            if (!(thisBoard.squares[x + 1, y] || thisBoard.squares[x, y + 1]))
                            { return new Point(x, y); }
                            else
                            { break; }
                        }
                    }
                }
            }

            throw new Exception("No valid space found -> check Chomp.computerTurn()");
        }        
    }
    private bool optimalTurn(bool[,] squares, Point destination, int iteration)
    {
        //this method will determine whether snapping at the target destination is an optimal (game-winning) turn
        //it will edit the squares variable, so a clone should be used as an argument

        //snap at target destination
        for (int x = destination.X; x < squares.GetLength(0); x++)
        {
            for (int y = destination.Y; y < squares.GetLength(1); y++)
            {
                if (squares[x, y])
                { squares[x, y] = false; } //set all squares to be active
                else
                { continue; } //all squares behind one that has already been broken off are false -> skip to next line
            }
        }

        //recursively search for optimal turn
        if (!(squares[0, 1] || squares[1, 0])) //only top-left square remains
        {
            if (iteration % 2 == 0)
            { return false; }
            else
            { return true; }
        }
        else
        {
            bool[,] bias = new bool[squares.GetLength(0), squares.GetLength(1)]; //fields that are true: optimal turns

            for (int x = 0; x < squares.GetLength(0); x++)
            {
                for (int y = 0; y < squares.GetLength(1); y++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        bias[x, y] = optimalTurn(clone2DArray<Boolean>(squares), new Point(x, y), iteration + 1);
                        if (bias[x, y]) { return true; }
                    }
                }
            }

        }

        //no optimal turn found
        return false;
    }
    */

    private void gameWon(Player player)
    {
        //WIP: handle end of game
        board.display();

        Console.WriteLine("Congratulations! " + player.name + " has won the game.");

        /*
        Console.WriteLine("List of all turns:");
        TurnData td;
        while (turnStack.Count > 0)
        {
            td = turnStack.Pop();
            Console.WriteLine(td.player.name + ": " + td.coords);
        }
        */

        running = false; //this will cause the round()-loop in the constructor to finish -> game ends -> back to main method in MainMenu
    }

    public void addTurn(Player player, Point coords)
    {
        turnStack.Push(new TurnData(player, coords));
    }
    public void removeTurn()
    {
        try
        { turnStack.Pop(); }
        catch (Exception e)
        { Console.WriteLine("Error: " + e.Message); }
    }
}
