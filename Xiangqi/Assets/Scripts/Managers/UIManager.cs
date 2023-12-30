using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    //pieces sprite asset of black and redd pieces and the prefab of the piece
    [SerializeField] private Sprite[] redPiecesSprites;
    [SerializeField] private Sprite[] blackPiecesSprites;
    [SerializeField] private GameObject chessPiecePrefab;
    [SerializeField] private GameObject moveDotPrefab;

    [SerializeField] public TextMeshProUGUI turnColorText;
    [SerializeField] public Color redColor;
    [SerializeField] public Color blackColor;

    [SerializeField] private GameObject WinnerText;
    [SerializeField] private GameObject WinnerColorText;
    [SerializeField] private GameObject CheckCircleUI;

    [SerializeField] public TextMeshProUGUI bitboardText;

    public Piece DrawPiece(int x, int y, Piece.PieceType pieceType, Piece.PieceColor pieceColor)
    {
        //instantiate the peice by calculate the position of the piece by multiple the *index* and the board square length and height
        GameObject obj = Instantiate(chessPiecePrefab, PositionToVector2(x, y), Quaternion.identity);
        //add the new piece to the pieces array
        obj.transform.parent = gameObject.transform;
        
        //change the name of the piece for the specific piece and color
        obj.name = pieceType + " " + pieceColor;

        obj.GetComponent<Piece>().SetPiece(pieceType, pieceColor, x, y);


        //set the piece sprite 
        //if the position / 10 is 1 so the piece its red and the piece sprite is in the red pieces sprites array
        //red
        if(pieceColor == Piece.PieceColor.Red){
            //change the piece sprite
            obj.GetComponent<SpriteRenderer>().sprite = redPiecesSprites[(int)pieceType - 1];
        }
        //if the position / 10 is 2 so the piece its black and the piece sprite is in the black pieces sprites array
        //black
        else{
            //change the piece sprite 
            obj.GetComponent<SpriteRenderer>().sprite = blackPiecesSprites[(int)pieceType - 1];
        }

        return obj.GetComponent<Piece>();
    }

    public void MovePieceInScreen(Piece piece, Move move)
    {
        piece.gameObject.transform.position = PositionToVector2(move.getEndX(), move.getEndY());
    }

    public void RemovePiece(Piece piece)
    {
        Destroy(piece.gameObject);
    }

    public  Vector2 PositionToVector2(int x, int y)
    {
        return new Vector2((x * Board.BOARD_SQUARE_LENGTH) - (Board.BOARD_SQUARE_LENGTH*4), (y * Board.BOARD_SQUARE_LENGTH)-Board.BOARD_SQUARE_HEIGHT);
    }

    public void DrawDot(GameObject piece, Vector2 pos)
    {
        //draw dot in the given pos
        GameObject dotObject = Instantiate(moveDotPrefab, PositionToVector2((int)pos.x, (int)pos.y), Quaternion.identity);
        dotObject.transform.parent = piece.transform;
        dotObject.GetComponent<MoveInput>().SetPos(pos);
    }

    public void DeleteDots()
    {
        //remove all dots in the screen
        GameObject[] dots = GameObject.FindGameObjectsWithTag("Dot");
        foreach(GameObject dot in dots)
        {
            Destroy(dot.gameObject);
        }
    }

    public void DrawCheckCircle(int x, int y, bool isCheckNow)
    {
        if(isCheckNow)
        {
            CheckCircleUI.SetActive(true);
            CheckCircleUI.transform.position = PositionToVector2(x, y);
        }
        else
            CheckCircleUI.SetActive(false);
    }

    public void ChangeTurnText(PlayerColor playerColor)
    {
        turnColorText.GetComponent<TextMeshProUGUI>().text = playerColor.ToString();
        if(playerColor == PlayerColor.Red)
            turnColorText.GetComponent<TextMeshProUGUI>().color = redColor;
        else
            turnColorText.GetComponent<TextMeshProUGUI>().color = blackColor;
    }

    public void CheckMateText(PlayerColor winnerColor)
    {
        turnColorText.gameObject.SetActive(false);
        //show the winner text, color winner text and confeti 
        WinnerText.SetActive(true);
        //change the color of the color winner text
        WinnerColorText.GetComponent<TextMeshProUGUI>().text = winnerColor.ToString();
        if(winnerColor == PlayerColor.Red)
            WinnerColorText.GetComponent<TextMeshProUGUI>().color = redColor;
        else
            WinnerColorText.GetComponent<TextMeshProUGUI>().color = blackColor;
    }
}
