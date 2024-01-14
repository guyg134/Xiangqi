using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningBook
{

    private Dictionary<List<Move>, List<Move>> Book;
    
    public OpeningBook()
    {
        // Initialize your opening book with positions and candidate moves
        // Example:
        Book = new Dictionary<List<Move>, List<Move>>
        {
            { new List<Move> { new Move(1, 2, 4, 2), new Move(7, 2, 4, 2), new Move(1, 0, 2, 2) }, new List<Move> { new Move(1, 2, 4, 2), new Move(7, 2, 4, 2), new Move(1, 0, 2, 2) } },
            
        };
    }


    public Move GetRandomOpeningMove(List<Move> boardState, GameColor turnColor)
    {   
        //just first move
        int x = Random.Range(0, Book[boardState].Count);
        return Book[boardState][x];
    } 
}
