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

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var other = (Position)obj;
        return x == other.x && y == other.y;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + x.GetHashCode();
            hash = hash * 23 + y.GetHashCode();
            return hash;
        }
    }

    public void ChangeSidePosition()
    {
        //x = 8 - x;
        y = 9 - y;
    }

}
