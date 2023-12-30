using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerScript : MonoBehaviour
{
    private PlayerColor playerColor;
    private bool downSide;
   

    public PlayerScript(PlayerColor c, bool downSide)
    {
       this.playerColor = c;
       this.downSide = downSide;
    }

    public PlayerColor GetPlayerColor()
    {
        return playerColor;
    }

    public bool playOnDownSide()
    {
        return downSide;
    }
}
