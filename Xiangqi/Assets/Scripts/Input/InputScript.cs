using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputScript : MonoBehaviour
{

    protected Piece piece;
   
    // Start is called before the first frame update
    void  Start()
    {
        setPiece();
    }

    protected abstract void setPiece();

    protected abstract void click();

    void OnMouseOver()
    {   
        if(Input.GetMouseButtonDown(0))
        {
            click();
        }
    }
}
