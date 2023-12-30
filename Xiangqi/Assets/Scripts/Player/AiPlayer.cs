using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AiPlayer : PlayerScript
{
    SearchMove searchMove;
    public AiPlayer(PlayerColor c, bool downSide, SearchMove searchMove) : base(c, downSide)
    {
        this.searchMove = searchMove;
        searchMove.SetSearchMove(c);
        print(c);
    }

    public AiPlayer SetPlayerScript(PlayerColor c, bool downSide)
    {
        searchMove = GetComponent<SearchMove>();
        return new AiPlayer(c, downSide, GetComponent<SearchMove>());
    }


    public void YourTurn()
    {
        searchMove.FindMove();
        print(base.GetPlayerColor());
    }
}
