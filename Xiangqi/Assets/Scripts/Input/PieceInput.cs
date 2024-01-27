
using UnityEngine;


public class PieceInput : MonoBehaviour
{
    private Piece piece;
    
        
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
    }

    void OnMouseOver()
    {   
        if(Input.GetMouseButtonDown(0))
        {
            Click();
        }
    }
}
