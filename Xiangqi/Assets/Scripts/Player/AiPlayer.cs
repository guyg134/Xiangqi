using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AiPlayer : MonoBehaviour,PlayerScript
{
    private SearchMove searchMove;
    private GameColor playerColor;
    private bool playOnDownSide;

    public GameColor GetPlayerColor()
    {
        return playerColor;
    }

    public bool PlayOnDownSide()
    {
        return playOnDownSide;
    }

    public PlayerScript SetPlayer(GameColor playerColor, bool downSide)
    {
        this.playerColor = playerColor;
        playOnDownSide = downSide;

        searchMove = GetComponent<SearchMove>();
        searchMove.SetSearchMove(playerColor);
        return this;
    }


    public void YourTurn()
    {
        searchMove.DoTurn();
        //print(base.GetPlayerColor());
    }
}
