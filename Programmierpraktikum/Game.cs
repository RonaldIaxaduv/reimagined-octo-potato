public abstract class Game
{
    public Player[] players = new Player[2];
    public Board board;

    public abstract void turn(Player player); //a player's turn
    public abstract void round(); //both players' turns
}
