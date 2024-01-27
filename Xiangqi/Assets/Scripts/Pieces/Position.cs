using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position 
{
    public int x ;
    public int y {get; set;}
    
    public Position(Position position)
    {
        this.x = position.x;
        this.y = position.y;
    }
    public Position(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void ChangeSidePosition()
    {
        //x = 8 - x;
        y = 9 - y;
    }

}
