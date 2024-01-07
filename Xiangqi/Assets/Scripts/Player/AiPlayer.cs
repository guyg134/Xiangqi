using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AiPlayer : PlayerScript
{
    SearchMove searchMove;
    public AiPlayer(GameColor c, bool downSide, SearchMove searchMove)
    {
        this.searchMove = searchMove;
        searchMove.SetSearchMove(c);
        print(c);
    }

    public AiPlayer SetPlayerScript(GameColor c, bool downSide)
    {
        searchMove = GetComponent<SearchMove>();
        SetPlayer(c, downSide);
        searchMove = GetComponent<SearchMove>();
        searchMove.SetSearchMove(c);
        return this;
    }


    public void YourTurn()
    {
        searchMove.DoTurn();
        //print(base.GetPlayerColor());
    }
}
