
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

//this class is the game board that keep all the pieces and the positions, create the board and connect with object in the scene
public class GameBoard : MonoBehaviour
{

    private Board board;
    
    private GameManager gameManager;
    private UIManager uIManager;


    public void CreateBoard(GameColor playerColor, GameObject gameManager)
    {
        //setup 
        this.gameManager = gameManager.GetComponent<GameManager>();
        this.uIManager = gameManager.GetComponent<UIManager>();

        //when player playing red pieces
        string startFenRed = "rneakaenr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNEAKAENR";
        //when player playing black pieces
        string startFenBlack = "RNEAKAENR/9/1C5C1/P1P1P1P1P/9/9/p1p1p1p1p/1c5c1/9/rneakaenr";

        string gameFen = playerColor==GameColor.Red ? startFenRed : startFenBlack;

        board = new Board(gameFen);
        LoadPositionFromFen(gameFen);
        board.SetBitBoard();
    }



    //get a fen string and set the position of every piece in the positions array
    private void LoadPositionFromFen(string fen) 
    {
        string fenBoard = fen;
        int file = 0, rank = 9;
    

        foreach(char symbol in fenBoard)
        {
            //if the symbol is / so it goes to lower rank
            if(symbol == '/'){
                file = 0;
                rank--;
            }
            else{
                //if its digit so it need to get forward in the file
                if(char.IsDigit(symbol)){
                    file += (int) char.GetNumericValue(symbol);
                }
                //else its digit so its piece
                else{
                    //find the piece color int by checking if its upper or lower letter
                    GameColor pieceColor = char.IsUpper(symbol) ? GameColor.Red : GameColor.Black;
                    //get the piece int by sent the letter to the piecetypefromsymbol 
                    PieceType pieceType = PieceTypeMethods.CharToPieceType(char.ToLower(symbol));
                    
                    Piece currentPiece = uIManager.DrawPiece(file, rank, pieceType, pieceColor);
                    
                    //add piece to the board
                    board.AddPiece(currentPiece);
                    
                    file++;
                }
            }
        }
    }

    public void CreatePieceDots(GameObject piece, List<Position> validMovews)
    {
        uIManager.DeleteDots();
        
        foreach(Position pos in validMovews)
        {
            uIManager.DrawDot(piece, pos);
        }
    }

    public Board GetBoard()
    {
        return board;
    }

    public Board GetBoardCopy()
    {
        return new Board(board);
    }

    public Piece GetPiece(int x, int y)
    {
        return board.FindPiece(x, y);
    }

    public bool CheckIfPieceIsKing(int x, int y)
    {
        return board.FindPiece(x, y).GetPieceType() == PieceType.King;
    }

    //return if position is in the board borders
    public static bool CheckIfInBorders(int x, int y)
    {
        return x>=0 && y>=0 && x<9 && y < 10;
    } 

    public void  UpdatePieceInBoard(Move move)
    {
        uIManager.DeleteDots();
        //add the eaten piece to the move
        move.EatenPiece = board.FindPiece(move.EndX, move.EndY);

        //check if there is piece and take the piece if true
        if(move.EatenPiece != null)
            uIManager.RemovePiece(move.EatenPiece);

        //update the piece on the board
        board.MovePieceOnBoard(move);
        print("fen: " + board.GetFen() + " fen length: " + board.GetFenLength());
        board.GetFen();

        //update piece on screen 
        uIManager.MovePieceInScreen(move.MovingPiece, move);

        //check if there is check on the king now
        CheckGameState();

        gameManager.ChangeTurn();
    }

    //check if there is check, checkmate, stalemate or draw by dont have attacking pieces
    private void CheckGameState()
    {
        GameColor turnColor = gameManager.GetTurnColor();
        if(board.IsCheck(turnColor))
        {
            //find the enemy king and draw the check circle
            Position enemyKingPos = board.FindKing(gameManager.GetTurnColor().OppositeColor()).GetPos();
            uIManager.CheckCircleUI((int)enemyKingPos.x, (int)enemyKingPos.y, true);

            //if enemy in check and dont have moves is checkmate
            if(!board.PlayerHaveMoves(turnColor.OppositeColor()))
            {
                gameManager.CheckMate();
            }
        }
        //check if it stalemate
        else if(!board.PlayerHaveMoves(turnColor.OppositeColor()))
        {
            //if there is no check remove the circle
            uIManager.CheckCircleUI(0, 0, false);
            gameManager.CheckMate();
        }
        else if(IsDraw())
        {
            uIManager.CheckCircleUI(0, 0, false);
            gameManager.Draw();
        }
        else
        {
            //if there is no check remove the circle
            uIManager.CheckCircleUI(0, 0, false);
        }
    }

    private bool IsDraw()
    {
        return board.IsDraw();
    }

}
