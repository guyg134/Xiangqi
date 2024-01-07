using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random=UnityEngine.Random;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Board board;
    private UIManager uIManager;
    private  PlayerScript[] players = new PlayerScript[2];
    

    //save the color of the player
    private int turnInt;
    private int movesCounter;
    [SerializeField] private GameObject KingCirclePrefab;
    //save the object of the king circle
    private GameObject KingCircle;

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

            computersPlayers[0].GetComponent<AiPlayer>().SetPlayerScript(playerColor, true);
            computersPlayers[1].GetComponent<AiPlayer>().SetPlayerScript((GameColor)((int)playerColor ^ 1), false);
        }
        else if(computersPlayers.Length == 1)
        {
            print("Human vs Computer");
            players[0] = new HumanPlayer(playerColor, true);
            players[1] = computersPlayers[0].GetComponent<AiPlayer>();

            computersPlayers[0].GetComponent<AiPlayer>().SetPlayerScript((GameColor)((int)playerColor ^ 1), false);
        }
        //no computer players
        else
        {
            print("Human vs Human");
            players[0] = new HumanPlayer(playerColor, true);
            players[1] = new HumanPlayer((GameColor)((int)playerColor ^ 1), false);
        }
        
        //initial the board and spawn the pieces
        board.CreateBoard(playerColor, this.gameObject);

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

    public GameColor GetTurnColor()
    {
        return players[turnInt].GetPlayerColor();
    }

    public PlayerScript GetTurnPlayer()
    {
        return players[turnInt];
    }
    public void ChangeTurn()
    {
        turnInt ^= 1;

        //change the turn text to the current turn
        uIManager.ChangeTurnText(GetTurnColor());
        if(Time.timeScale != 0)
            IsAiTurn();

        uIManager.ChangeEvalBar((float)Evaluate.EvaluateFunc(board.GetBoard(), GetTurnColor()));
        movesCounter++;
    }

    private void IsAiTurn()
    {
        //if the current player is ai tell him its his turn
        AiPlayer currentPlayer = GetTurnPlayer() as AiPlayer;
        if(currentPlayer)
        {
            currentPlayer.YourTurn();
        }
    }
    public void CheckMate()
    {
        GameColor winnerColor = GetTurnColor();
        print("GG good game the winner is " + winnerColor.ToString());
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
