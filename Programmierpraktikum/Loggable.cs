using System;
using System.Drawing;
using System.Collections.Generic;

public interface Loggable
{
    Stack<TurnData> turnStackProp
    {
        get;
        set;
    }

    void addTurn(Player player, Point coords);
    void removeTurn();
}

public struct TurnData
{
    public Player player;
    public Point coords;

    public TurnData(Player player, Point coords)
    {
        this.player = player;
        this.coords = coords;
    }
}