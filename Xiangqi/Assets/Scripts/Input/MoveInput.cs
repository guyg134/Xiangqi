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

    protected override void click()
    {
        piece.MovePiece(pos);
    }

    protected override void setPiece()
    {
        throw new System.NotImplementedException();
    }
}
