using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Scripting.APIUpdating;

public class PieceInput : InputScript
{

    
       //check if player click on mouse           check if the player color equal to the turn color         if this piece color equal to the now turn color
        //if(Input.GetMouseButtonDown(0) && PlayerScript.getPlayerColor() == GameManager.getTurnColor() && pc.GetColor() == GameManager.getTurnColor())
        
    protected override void setPiece()
    {
        piece = gameObject.GetComponent<Piece>();
    }

    protected override void click()
    {
        //check if the current piece color equal to current player color and the player is human
        if((int)piece.GetPieceColor() == (int)GameObject.FindWithTag("GameManager").GetComponent<GameManager>().getTurnColor() && GameObject.FindWithTag("GameManager").GetComponent<GameManager>().getTurnPlayer().GetType() == typeof(HumanPlayer))
        {
            piece.GetDots();
        }
        //clear the dots from the last piece
        //clearDots();
    }
}
