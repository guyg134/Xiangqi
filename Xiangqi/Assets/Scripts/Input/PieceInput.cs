
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
        GameManager gameManager = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        //check if the current piece color equal to current player color and the player is human
        if((int)piece.GetPieceColor() == (int)gameManager.GetTurnColor() && gameManager.GetTurnPlayer().GetType() == typeof(HumanPlayer))
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
