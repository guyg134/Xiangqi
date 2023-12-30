using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AiPlayer : PlayerScript
{
    SearchMove searchMove;
    public AiPlayer(PlayerColor c, bool downSide) : base(c, downSide)
    {
    }

    public AiPlayer SetPlayerScript(PlayerColor c, bool downSide)
    {
        searchMove = GetComponent<SearchMove>();
        searchMove.SetSearchMove(c);
        print("got set");
        return this;
    }


    public void YourTurn()
    {
        searchMove.FindMove();
    }
}
