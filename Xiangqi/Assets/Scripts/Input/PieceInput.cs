
using UnityEngine;


public class PieceInput : MonoBehaviour
{
    private Piece piece;
    
       //check if player click on mouse           check if the player color equal to the turn color         if this piece color equal to the now turn color
        //if(Input.GetMouseButtonDown(0) && PlayerScript.getPlayerColor() == GameManager.getTurnColor() && pc.GetColor() == GameManager.getTurnColor())
        
    void Start()
    {
        piece = gameObject.GetComponent<Piece>();
    }

    protected void Click()
    {
        //check if the current piece color equal to current player color and the player is human
        if((int)piece.GetPieceColor() == (int)GameObject.FindWithTag("GameManager").GetComponent<GameManager>().GetTurnColor() && GameObject.FindWithTag("GameManager").GetComponent<GameManager>().GetTurnPlayer().GetType() == typeof(HumanPlayer))
        {
            piece.GetDots();
        }
        //clear the dots from the last piece
        //clearDots();
    }

    void OnMouseOver()
    {   
        if(Input.GetMouseButtonDown(0))
        {
            Click();
        }
    }
}
