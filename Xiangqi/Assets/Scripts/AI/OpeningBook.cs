using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpeningBook
{

    private Tree Root;
    private List<List<string>> openings = new List<List<string>>
        {
        //Central Cannon 8th file Horses VS Screen Horses defence
        new List<string> { "C2C5", "J2H3", "A2C3", "J1J2", "A1A2", "G3F3",
         "A2G2", "G7F7", "A8C7", "J8H7", "A9B9", "J7H5", "B9B6", "J6I5",
         "D5E5", "J9J6", "B6J6", "J5J6", "C7D5", "H8G8", "G2B2", "J6J5",
         "D7E7", "H2D2", "D3E3", "F3E3", "E7F7", "E3D3", "B2B7", "H7J6",
         "F7G7", "G8F8", "D5E7", "F8F7"}, 
        
        //opening 2
        new List<string> { "C8C5", "J8H7", "A8C7", "J9J8", "A9A8", "G3F3",
         "A2C1", "J2H3", "D7E7", "G1F1", "C2C3", "H3F2", "A1B1", "J3H5",
          "B1B4", "J1G1", "H7G5", "B4G4", "C7B8", "G4G2", "E7D7", "F1G1",
          "D7D6", "C1A2", "D6C6", "H2E2", "C5C6", "G2G3", "C6B6", "C3C4"
        }, 

        //opening 3
        new List<string> { "C8C5", "J2H3", "A8C7", "J8H7", "A9A8", "J9J8",
         "D3E3", "G7F7", "A8G8", "J4I5", "A2C3", "H8H9", "G8G7", "H9I9",
         "C2C1", "I9I7", "G7G6", "H7F8", "A1A2", "J1J2", "G6I6", "H2I2",
         "I6F6", "J3H5", "A2H2", "F8D7", "F6D6", "I7H7", "C3E4", "J2J4",
         "D6E6", "I2I4", "C5C3", "F7E7", "E6D6"     
         }, 

        //opening 4 2 knights for black with no central cannon
        new List<string> {  "C8C5", "J8H7", "A8C7", "J2H3" , "A9A8", "J9J8",
         "G7F7", "A8G8"
         },

        //opening 5 same direction cannon
        new List<string> { "C8C5", "H8H5", "A8C7", "J8H7", "A9A8", "J9I9",
         "A2C3", "I9I4", "D3E3", "J2H3", "D7E7", "I4D4"
         },

        };
    //   C8C5 J8H7 A8C7
    // Creates a tree of all the openings
    public OpeningBook()
    {
        Root = new Tree("Start");
        foreach (List<string> opening in openings)
        {
            
            TreeNode<string> currentNode = Root.Root;
            // Add the opening to the tree
            foreach (string move in opening)
            {
                currentNode = Root.AddChild(currentNode, move);
            }
            //Root.PrintTree(Root.Root);
        }
    }

    public string GetRandomOpeningMove(List<string> playedMoves)
    {
        List<string> possibleMoves = new List<string>();
        string allmoves = "";
        foreach(string move in playedMoves)
        {
            allmoves += " " + move;
        }
        Debug.Log(allmoves);

        foreach (var opening in openings)
        {
            if (IsMoveListMatch(playedMoves, opening))
            {
                if (playedMoves.Count == opening.Count)
                {
                    // The opening has been played out
                    continue;
                }
                // Get the remaining moves in the opening
                var remainingMoves = opening.Skip(playedMoves.Count).ToList();

                possibleMoves.Add(remainingMoves[0]);
            }
        }

        if (possibleMoves.Count != 0)
        {
            // Return a random move from the possible moves
            return possibleMoves[Random.Range(0, possibleMoves.Count)];
        }
        // If no matching opening is found, return null or handle it accordingly
        return null;
    }

    private bool IsMoveListMatch(List<string> playedMoves, List<string> opening)
    {
        if (playedMoves.Count > opening.Count)
        {
            return false; // More moves played than in the opening
        }

        // Check if the played moves match the opening regardless of order
        return playedMoves.All(move => opening.Take(playedMoves.Count).Contains(move));
    }

    
}
