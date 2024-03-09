using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{

    public HumanPlayer(GameColor c, bool downSide)
    {
        playerColor = c;
        playOnDownSide = downSide;
    }

}