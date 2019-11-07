using System;

public class Player
{
    public string name;
    public enum playerType { Human, Computer };
    public playerType type;

    public Player(string name, playerType type)
    {
        if (name.Length == 0)
        { throw new Exception("A player's name may not be blank."); } //maybe make it "[]" instead
        else
        { this.name = name; }
        this.type = type;
    }
}
