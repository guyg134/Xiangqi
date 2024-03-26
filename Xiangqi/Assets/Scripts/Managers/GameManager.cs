
using UnityEngine;
using Random=UnityEngine.Random;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameBoard gameBoard;
    private UIManager uIManager;
    private  Player[] players = new Player[2];
    

    //save the color of the player
    private int turnInt;
    private int movesCounter;
    

    // Start is called before the first frame update
    void Start()
    {
        movesCounter = 0;

        uIManager = GetComponent<UIManager>();
        uIManager.ChangeTurnText(GameColor.Red);

        GameColor playerColor;
        
        int x = Random.Range(0,2);
        //random color of the player
        playerColor = (GameColor)x;

        turnInt = x; // save the index of the first turn player
        
        //find all computer players in scene
        GameObject[] computersPlayers = GameObject.FindGameObjectsWithTag("Player");
        //initial the players array with 2 player of ais or users
        if(computersPlayers.Length == 2)
        {
            print("Computer vs Computer");
            players[0] = computersPlayers[0].GetComponent<AiPlayer>();//plays on the down side
            players[1] = computersPlayers[1].GetComponent<AiPlayer>();//plays on the up side

            computersPlayers[0].GetComponent<AiPlayer>().SetPlayer(playerColor, true);
            computersPlayers[1].GetComponent<AiPlayer>().SetPlayer(playerColor.OppositeColor(), false);
        }
        else if(computersPlayers.Length == 1)
        {
            print("Human vs Computer");
            players[0] = new HumanPlayer(playerColor, true);
            players[1] = computersPlayers[0].GetComponent<AiPlayer>();

            computersPlayers[0].GetComponent<AiPlayer>().SetPlayer(playerColor.OppositeColor(), false);
        }
        //no computer players
        else
        {
            print("Human vs Human");
            players[0] = new HumanPlayer(playerColor, true);
            players[1] = new HumanPlayer(playerColor.OppositeColor(), false);
        }
        
        //initial the board and spawn the pieces
        gameBoard.CreateBoard(playerColor, gameObject);

        IsAiTurn();
    }
    public static void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void StopGame()
    {
        Time.timeScale = 0;
    }

    public void GoBackLastMoves()
    {

    }

    public GameColor GetTurnColor()
    {
        return players[turnInt].playerColor;
    }

    public Player GetTurnPlayer()
    {
        return players[turnInt];
    }

    public int GetMovesCounter()
    {
        return movesCounter;
    }
    public void ChangeTurn()
    {
        //evaluate the current board for the player that just played
        float eval = (float)new Evaluate().EvaluateCurrentPosition(gameBoard.GetBoard(), players[turnInt])/EvaluateConstants.CheckMateValue;

        turnInt ^= 1;

        //change the turn text to the current turn
        uIManager.ChangeTurnText(GetTurnColor());
        if(Time.timeScale != 0)
            IsAiTurn();

        uIManager.ChangeEvalBar(eval);
        uIManager.ChangeMovesNumberText(++movesCounter);
    }

    private void IsAiTurn()
    {
        //if the current player is ai tell him its his turn
        AiPlayer currentPlayer = GetTurnPlayer() as AiPlayer;
        if(currentPlayer != null)
        {
            currentPlayer.YourTurn();
        }
    }

    public bool IsColorOnDownSide(GameColor gameColor)
    {
        Player currentPlayer = GetTurnPlayer();
        //player[0] is the player thats plays on the down side
        return players[0].playerColor == gameColor;
    }
    public void CheckMate()
    {
        GameColor winnerColor = GetTurnColor();
        print("GG good game the winner is " + winnerColor.ToString());
        print("Worth Time Of Bot " + SearchMove.worthTime + " Average Time Of Bot " + SearchMove.sumTimeToMove/movesCounter);
        uIManager.CheckMateText(winnerColor);
        StopGame();
    }

    public void Draw()
    {
        print("Draw");
        uIManager.DrawText();
        StopGame();
    }

}
