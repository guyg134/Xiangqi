using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : PlayerScript
{
    private GameColor playerColor;
    private bool playOnDownSide;

    public HumanPlayer(GameColor c, bool downSide)
    {
        playerColor = c;
        playOnDownSide = downSide;
    }

    public GameColor GetPlayerColor()
    {
        return playerColor;
    }

    public bool PlayOnDownSide()
    {
        return playOnDownSide;
    }

}
