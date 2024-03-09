
using UnityEngine;

public class MoveInput : MonoBehaviour
{

    //private int position;
    private Position pos;
    private Piece piece;

    public void SetPos(Position pos)
    {
        this.pos = pos;
        piece = transform.parent.gameObject.GetComponent<Piece>();
    }

    protected void Click()
    {
        piece.MovePiece(pos);
    }

    void OnMouseOver()
    {   
        if(Input.GetMouseButtonDown(0))
        {
            Click();
        }
    }

}