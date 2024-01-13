using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class GameBoard : MonoBehaviour
{
    public static float BOARD_SQUARE_LENGTH = 0.8625f;
    public static float BOARD_SQUARE_HEIGHT = 3.88f;
    
    //array that keeps all the positions with int that represent the piece and the color
    //private Piece[,] pieces = new Piece[10, 9];
    //array that saves all pieces with the index of the type + color * 10
    //private Piece[] piecesCounter = new Piece[18];

    private Board board;
     
    //bitboard
    private BitBoard bitBoard;
    
    private GameManager gameManager;
    private UIManager uIManager;


    public void CreateBoard(GameColor playerColor, GameObject gameManager)
    {
        //setup 
        this.gameManager = gameManager.GetComponent<GameManager>();
        this.uIManager = gameManager.GetComponent<UIManager>();
        bitBoard = GetComponent<BitBoard>();

        //when player playing red pieces
        string startFenRed = "rneakaenr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNEAKAENR";
        //when player playing black pieces
        string startFenBlack = "RNEAKAENR/9/1C5C1/P1P1P1P1P/9/9/p1p1p1p1p/1c5c1/9/rneakaenr";

        string gameFen = "";

        gameFen = playerColor==GameColor.Red ? startFenRed : startFenBlack;
        board = new Board();
        LoadPositionFromFen(gameFen);

        bitBoard.SetBitBoards(board);
        bitBoard.PrintCurrentBitBoard();
    }



    //get a fen string and set the position of every piece in the positions array
    private void LoadPositionFromFen(string fen) 
    {
        //change char to the int of specific piece
        var pieceTypeFromSymbol = new Dictionary<char, PieceType> (){
            ['k'] = PieceType.King, ['p'] = PieceType.Soldier, ['n'] = PieceType.Knight,
            ['e'] = PieceType.Elephant, ['c'] = PieceType.Cannon, ['r'] = PieceType.Rook,
            ['a'] = PieceType.Advisor
        };

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
                    GameColor pieceColor = (char.IsUpper(symbol)) ? GameColor.Red : GameColor.Black;
                    //get the piece int by sent the letter to the piecetypefromsymbol 
                    PieceType pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                    
                    Piece currentPiece = uIManager.DrawPiece(file, rank, pieceType, pieceColor);
                    
                    //add piece to the board
                    board.AddPiece(currentPiece);
                    
                    file++;
                }
            }
        }
    }

    public void CreatePieceDots(GameObject piece, List<Vector2> validMovews)
    {
        uIManager.DeleteDots();
        
        foreach(Vector2 pos in validMovews)
        {
            uIManager.DrawDot(piece, pos);
        }
    }

    public Board GetBoard()
    {
        return board;
    }

    public List<Vector2> GetValidMoves(BigInteger dotsBitboard, Piece piece, int startX, int startY)
    {
        //delete all the positions that have piece with the same color of this piece
        dotsBitboard = bitBoard.BitboardMovesWithoutDefence(dotsBitboard, piece.GetPieceColor());

        //change the bitboard moves to vector2 positions
        List<Vector2> dotsPos = bitBoard.BitboardToVector2s(dotsBitboard);

        //save the valids moves
        List<Vector2> validMoves = new List<Vector2>();
        //create dots for every position
        foreach(Vector2 dotPos in dotsPos)
        {
            Move move = new Move(startX, startY, (int)dotPos.x, (int)dotPos.y, piece, board.FindPiece((int)dotPos.x, (int)dotPos.y));
            bool isCheckAfterThisMove = IsKingUnderAttackAfterMove(move, piece.GetPieceColor().OppositeColor());
            //if there is no check after the move add the move to the valids moves list
            if(!isCheckAfterThisMove)
            {
                validMoves.Add(dotPos);
            }
            
        }
        return validMoves;  
    }

    public Piece GetPiece(int x, int y)
    {
        return board.FindPiece(x, y);
    }

    public bool CheckIfPieceIsKing(int x, int y)
    {
        return board.FindPiece(x, y, PieceType.King).GetPieceType() == PieceType.King;
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
        move.SetEatenPiece(board.FindPiece(move.getEndX(), move.getEndY()));

        //check if there is piece and take the piece if true
        if(move.EatenPiece() != null)
            uIManager.RemovePiece(move.EatenPiece());

        //update the piece on the board
        board.MovePieceOnBoard(move);

        //update piece on screen
        uIManager.MovePieceInScreen(move.GetPiece(), move);

        //update the piece in the bitboard
        bitBoard.UpdateBitBoard(move, move.GetPiece().GetPieceColor());

        //check if there is check on the king now
        CheckGameState();

        gameManager.ChangeTurn();
    }

    //check if there is check, checkmate, stalemate or draw by dont have attacking pieces
    private void CheckGameState()
    {
        GameColor turnColor = gameManager.GetTurnColor();
        if(bitBoard.IsCheck(board, turnColor))
        {
            //find the enemy king and draw the check circle
            Vector2 enemyKingPos = board.FindKing(gameManager.GetTurnColor().OppositeColor()).GetPos();
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
        if(board.GetPieceCount(PieceType.Cannon) != 0 || board.GetPieceCount(PieceType.Rook) != 0 || board.GetPieceCount(PieceType.Soldier) != 0 || board.GetPieceCount(PieceType.Knight) != 0)
        {
            return false;
        }
        
        return true;
    }

    public bool IsKingUnderAttackAfterMove(Move move, GameColor playerColor)
    {
        //create clone to save the board before
        Board boardAfterMove = new Board(board);
        //do the move
        boardAfterMove.MovePieceOnBoard(move);

        bool isKingUnderAttackAfterMove = bitBoard.IsCheck(boardAfterMove, playerColor);

        boardAfterMove.UndoLastMove();

    

        return isKingUnderAttackAfterMove;
    }

    public void BackLastMove()
    {
        board.UndoLastMove();
    }

}
