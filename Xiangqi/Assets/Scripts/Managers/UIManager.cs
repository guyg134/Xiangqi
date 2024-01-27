using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class UIManager : MonoBehaviour
{
    private const float BOARD_SQUARE_LENGTH = 0.8625f;
    private const float BOARD_SQUARE_HEIGHT = 3.88f;

    //pieces sprite asset of black and redd pieces and the prefab of the piece
    [SerializeField] private Sprite[] redPiecesSprites;
    [SerializeField] private Sprite[] blackPiecesSprites;
    [SerializeField] private GameObject chessPiecePrefab;
    [SerializeField] private GameObject moveDotPrefab;

    //Game UI
    [SerializeField] private TextMeshProUGUI movesNumberText;
    [SerializeField] private TextMeshProUGUI turnColorText;
    [SerializeField] private Color redColor;
    [SerializeField] private Color blackColor;
    [SerializeField] private ScrollRect movesScroll;
    [SerializeField] private GameObject moveNameTextPrefab;

    //Eval UI
    [SerializeField] private Image evalBar;
    [SerializeField] private TextMeshProUGUI evalText;

    //EndGame texts and UI
    [SerializeField] private GameObject winnerText;
    [SerializeField] private GameObject drawText;
    [SerializeField] private GameObject winnerColorText;
    [SerializeField] private GameObject checkCircleUI;

    //Debug UI
    [SerializeField] public TextMeshProUGUI bitboardText;
     

    public Piece DrawPiece(int x, int y, PieceType pieceType, GameColor pieceColor)
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
        if(pieceColor == GameColor.Red){
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
        piece.gameObject.transform.position = PositionToVector2(move.EndX, move.EndY);
        AddMoveToMovesGrid(move.Name());
    }

    //add new move to the moves grid in the game and set the 
    private void AddMoveToMovesGrid(string moveName)
    {
        Transform content = movesScroll.gameObject.transform.GetChild(0).transform.GetChild(0).transform;
        GameObject newMoveText = Instantiate(moveNameTextPrefab);
        newMoveText.transform.SetParent(content, true);
        newMoveText.GetComponent<TextMeshProUGUI>().text = moveName;
    }

    public void RemovePiece(Piece piece)
    {
        Destroy(piece.gameObject);
    }

    private  Vector2 PositionToVector2(int x, int y)
    {
        return new Vector2((x * BOARD_SQUARE_LENGTH) - (BOARD_SQUARE_LENGTH*4), (y * BOARD_SQUARE_LENGTH)- BOARD_SQUARE_HEIGHT);
    }

    public void ChangeMovesNumberText(int moves)
    {
        movesNumberText.text = moves.ToString();
    }

    public void DrawDot(GameObject piece, Position pos)
    {
        //draw dot in the given pos
        GameObject dotObject = Instantiate(moveDotPrefab, PositionToVector2(pos.x, pos.y), Quaternion.identity);
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

    public void CheckCircleUI(int x, int y, bool isCheckNow)
    {
        if(isCheckNow)
        {
            checkCircleUI.SetActive(true);
            checkCircleUI.transform.position = PositionToVector2(x, y);
        }
        else
            checkCircleUI.SetActive(false);
    }

    public void ChangeTurnText(GameColor playerColor)
    {
        turnColorText.GetComponent<TextMeshProUGUI>().text = playerColor.ToString();
        if(playerColor == GameColor.Red)
            turnColorText.GetComponent<TextMeshProUGUI>().color = redColor;
        else
            turnColorText.GetComponent<TextMeshProUGUI>().color = blackColor;
    }

    public void ChangeEvalBar(double value)
    {
        evalBar.fillAmount = (float)value + 0.5f;
        //set the eval bar number to the first 4 numbers so 0.123
        evalText.text = value.ToString("#0.000").Substring(0, 5);
    }

    public void CheckMateText(GameColor winnerColor)
    {
        turnColorText.gameObject.SetActive(false);
        //show the winner text, color winner text and confeti 
        winnerText.SetActive(true);
        //change the color of the color winner text
        winnerColorText.GetComponent<TextMeshProUGUI>().text = winnerColor.ToString();
        if(winnerColor == GameColor.Red)
            winnerColorText.GetComponent<TextMeshProUGUI>().color = redColor;
        else
            winnerColorText.GetComponent<TextMeshProUGUI>().color = blackColor;
    }

    public void DrawText()
    {
        turnColorText.gameObject.SetActive(false);
        //show the winner text, color winner text and confeti 
        drawText.SetActive(true);
        
    }
}
