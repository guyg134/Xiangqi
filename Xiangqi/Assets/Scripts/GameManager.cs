using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random=UnityEngine.Random;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Board board;
    private  PlayerScript[] players = new PlayerScript[2];
    

    //save the color of the player
    private int turnInt;
    private int movesCounter;
    [SerializeField] private TextMeshProUGUI turnColorText;
    [SerializeField] public TextMeshProUGUI bitboardText;
    [SerializeField] private GameObject KingCirclePrefab;
    //save the object of the king circle
    private GameObject KingCircle;

    // Start is called before the first frame update
    void Start()
    {

        PlayerColor playerColor;
        
        int x = Random.Range(0,2);
        //random color of the player
        playerColor = (PlayerColor)x;

        turnInt = x; // save the index of the first turn player
        
        //find all computer players in scene
        int computersPlayers = GameObject.FindGameObjectsWithTag("Player").Length;
        //initial the players array with 2 player of ais or users
        if(computersPlayers == 2)
        {
            print("Computer vs Computer");
            players[0] = new AiPlayer(playerColor, true);//plays on the down side
            players[1] = new AiPlayer((PlayerColor)((int)playerColor ^ 1), false);//plays on the up side
        }
        else if(computersPlayers == 1)
        {
            print("Human vs Computer");
            players[0] = new HumanPlayer(playerColor, true);
            players[1] = new AiPlayer((PlayerColor)((int)playerColor ^ 1), false);
        }
        //no computer players
        else
        {
            print("Human vs Human");
            players[0] = new HumanPlayer(playerColor, true);
            players[1] = new HumanPlayer((PlayerColor)((int)playerColor ^ 1), false);
        }
        
        //initial the board and spawn the pieces
        board.CreateBoard(playerColor, this);
    }

    void Update()
    {
        //turnColorText.text = turnColor.ToString();
    }

    public static void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public static void stopGame()
    {
        Time.timeScale = 0;
    }

    public PlayerColor getTurnColor()
    {
        return players[turnInt].getPlayerColor();
    }

    public PlayerScript getTurnPlayer()
    {
        return players[turnInt];
    }
    public void changeTurn()
    {
        turnInt ^= 1;
        turnColorText.text = getTurnColor().ToString();
    }

    public static Boolean checkIfCurrentKingIsUnderAttack()
    {
        //return Board.kingUnderAttack(); 
        return false;
    }

    public void createKingCircle(int kingPos, Boolean isCheck)
    {
        if(KingCircle)
        {
            Destroy(KingCircle);
            return;
        }
       // if(isCheck)
            //KingCircle = Instantiate(KingCirclePrefab, Board.positionToVector3(kingPos%10, kingPos/10), Quaternion.identity);
    }
}
