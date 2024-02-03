using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AiPlayer : Player
{
    private SearchMove searchMove;
   
    public Player SetPlayer(GameColor playerColor, bool downSide)
    {
        this.playerColor = playerColor;
        playOnDownSide = downSide;

        searchMove = GetComponent<SearchMove>();
        searchMove.SetSearchMove(this);
        return this;
    }


    public void YourTurn()
    {
        searchMove.DoTurn();
        //print(base.GetPlayerColor());
    }
}
