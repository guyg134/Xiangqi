using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInput : InputScript
{

    private int position;
    private Vector2 pos;

    void Start()
    {
    
    }

    public void createDot(int position)
    {
        this.position = position;
        
    }

    public void SetPos(Vector2 pos)
    {
        this.pos = pos;
        piece = transform.parent.gameObject.GetComponent<Piece>();
    }

    /*void OnMouseOver()
    {   //check if player click on mouse           check if the player color equal to the turn color         if this piece color equal to the now turn color
        //if(Input.GetMouseButtonDown(0) && pc.GetColor() == GameManager.getTurnColor())
        {
            //PieceInput.clearDots(transform.parent.gameObject);
            //pc.movePiece(position);
        }
    }*/

    protected override void setPiece()
    {
        print(transform.parent.gameObject.GetComponent<Piece>() == null);
        piece = transform.parent.gameObject.GetComponent<Piece>();
    }

    protected override void click()
    {
        piece.MovePiece(pos);
    }
}
