using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningBook
{

    private Dictionary<string, List<Move>> redBook;
    private Dictionary<string, List<Move>> blackBook;
    
    public OpeningBook()
    {
        // Initialize your opening book with positions and candidate moves
        // Example:
        redBook = new Dictionary<string, List<Move>>
        {
            { "RNEAKAENR/9/1C5C1/P1P1P1P1P/9/9/p1p1p1p1p/1c5c1/9/rneakaenr", new List<Move> { new Move(1, 2, 4, 2), new Move(7, 2, 4, 2), new Move(1, 0, 2, 2) } },
            // Add more positions and moves as needed
        };
    }


    public Move GetRandomOpeningMove(string fen, GameColor turnColor)
    {   
        //just first move
            int x = Random.Range(0, redBook[fen].Count);
            return redBook[fen][x];
        
    } 
}
