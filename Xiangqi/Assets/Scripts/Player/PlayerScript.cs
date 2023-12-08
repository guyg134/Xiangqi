using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerScript : MonoBehaviour
{
    private PlayerColor color;
    private bool downSide;
    private PlayerColor c;

    public PlayerScript(PlayerColor c, bool downSide)
    {
       this.color = c;
       this.downSide = downSide;
    }

    public PlayerColor getPlayerColor()
    {
        return color;
    }

    public bool playOnDownSide()
    {
        return downSide;
    }
}
