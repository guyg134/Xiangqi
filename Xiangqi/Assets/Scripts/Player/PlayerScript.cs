using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerScript : MonoBehaviour
{
    private GameColor playerColor;
    private bool downSide;
   

    public PlayerScript SetPlayer(GameColor c, bool downSide)
    {
        this.playerColor = c;
        this.downSide = downSide;
        return this;
    }
    
    public GameColor GetPlayerColor()
    {
        return playerColor;
    }

    public bool playOnDownSide()
    {
        return downSide;
    }

}
