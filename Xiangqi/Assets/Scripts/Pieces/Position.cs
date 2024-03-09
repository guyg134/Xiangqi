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
    public Position(string name)
    {
        y = LetterToNumber(name[0]);
        x = int.Parse(name[1].ToString()) - 1;
    }

    public string Name => "" + NumberToLetter(y) + (x + 1);


    private static char NumberToLetter(int number)
    {
        return (char)('A' + number);
    }   

    private static int LetterToNumber(char letter)
    {
        return letter - 'A';
    }
    

    public void ChangeSidePosition()
    {
        //x = 8 - x;
        y = 9 - y;
    }

}
