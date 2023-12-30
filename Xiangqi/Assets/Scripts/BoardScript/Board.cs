using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class Board : MonoBehaviour
{
    public static float BOARD_SQUARE_LENGTH = 0.8625f;
    public static float BOARD_SQUARE_HEIGHT = 3.88f;
    
    //array that keeps all the positions with int that represent the piece and the color
    private Piece[,] pieces = new Piece[10, 9];
     
    //bitboard
    private BitBoard bitBoard;
    
    private GameManager gameManager;
    private UIManager uIManager;



    public void CreateBoard(PlayerColor playerColor, GameObject gameManager)
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

        gameFen = playerColor==PlayerColor.Red ? startFenRed : startFenBlack;
        LoadPositionFromFen(gameFen);

        bitBoard.SetBitBoards(pieces);
        bitBoard.PrintCurrentBitBoard();
    }



    //get a fen string and set the position of every piece in the positions array
    private void LoadPositionFromFen(string fen) 
    {
        //change char to the int of specific piece
        var pieceTypeFromSymbol = new Dictionary<char, Piece.PieceType> (){
            ['k'] = Piece.PieceType.King, ['p'] = Piece.PieceType.Soldier, ['n'] = Piece.PieceType.Knight,
            ['e'] = Piece.PieceType.Elephant, ['c'] = Piece.PieceType.Cannon, ['r'] = Piece.PieceType.Rook,
            ['a'] = Piece.PieceType.Advisor
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
                    Piece.PieceColor pieceColor = (char.IsUpper(symbol)) ? Piece.PieceColor.Red : Piece.PieceColor.Black;
                    //get the piece int by sent the letter to the piecetypefromsymbol 
                    Piece.PieceType pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
                    
                    pieces[rank, file] = uIManager.DrawPiece(file, rank, pieceType, pieceColor);
                    
                    file++;
                }
            }
        }
    }

    public List<Vector2> CreatePieceDots(BigInteger dotsBitboard, GameObject piece, int startX, int startY)
    {
        uIManager.DeleteDots();
        //delete all the positions that have piece with the same color of this piece
        dotsBitboard = bitBoard.BitboardMovesWithoutDefence(dotsBitboard, gameManager.GetTurnColor());

        //change the bitboard moves to vector2 positions
        List<Vector2> dotsPos = bitBoard.BitboardToVector2s(dotsBitboard);

        //save the valids moves
        List<Vector2> validMoves = new List<Vector2>();
        //create dots for every position
        foreach(Vector2 dotPos in dotsPos)
        {
            bool isCheckAfterThisMove = IsKingUnderAttackAfterMove(new Move(startX, startY, (int)dotPos.x, (int)dotPos.y), (PlayerColor)((int)gameManager.GetTurnColor() ^ 1));
            //if there is no check after the move draw the dot
            if(!isCheckAfterThisMove)
            {
                uIManager.DrawDot(piece, dotPos);
                validMoves.Add(dotPos);
            }
            
        }
        return validMoves;
    }

    public BitBoard GetBitBoard()
    {
        return bitBoard;
    }


    public Piece GetPieceAtPosition(int x, int y)
    {
        return pieces[y, x];
    }

    //check there is piece in the input position
    public bool CheckIfThereIsPiece(int x, int y)
    {
        if(checkIfInBorders(x, y)){
            return pieces[y, x]!=null;
        }
        return false;
    }

    public bool CheckIfPieceIsKing(int x, int y)
    {
        return pieces[y, x].GetPieceType() == Piece.PieceType.King;
    }

    //return if position is in the board borders
    public static bool checkIfInBorders(int x, int y)
    {
        return x>=0 && y>=0 && x<9 && y < 10;
    } 

    public void  UpdatePieceInBoard(Move move)
    {
        uIManager.DeleteDots();

        Piece piece = pieces[move.getStartY(), move.getStartX()];
        //check if there is piece and take the piece if true
        if(pieces[move.getEndY(), move.getEndX()])
            uIManager.RemovePiece(pieces[move.getEndY(), move.getEndX()]);

        //update piece in board array
        pieces[move.getStartY(), move.getStartX()] = null;
        pieces[move.getEndY(), move.getEndX()] = piece;

        //update piece axis
        uIManager.MovePieceInScreen(piece, move);

        //update the piece in the bitboard
        bitBoard.UpdateBitBoard(move, piece.GetPieceColor());

        //check if there is check on the king now
        IsKingUnderAttack();

        gameManager.ChangeTurn();
    }

    private void IsKingUnderAttack()
    {
        if(bitBoard.IsCheck(pieces, gameManager.GetTurnColor()))
        {
            //find the enemy king and draw the check circle
            Vector2 enemyKingPos = FindEnemyKingPos();
            uIManager.DrawCheckCircle((int)enemyKingPos.x, (int)enemyKingPos.y, true);

            //if enemy in check and dont have moves is checkmate
            if(EnemyDoesntHaveMoves())
            {
                gameManager.CheckMate();
            }
        }
        //check if it stalemate
        else if(EnemyDoesntHaveMoves())
        {
            gameManager.CheckMate();
        }
        else
        {
            //if there is no check remove the circle
            uIManager.DrawCheckCircle(0, 0, false);
        }
    }

    private Vector2 FindEnemyKingPos()
    {
        PlayerColor enemyColor = (PlayerColor)((int)gameManager.GetTurnColor()^1);
        //if the enemy is on the down side check just down palace to find the king
        if(!gameManager.GetTurnPlayer().playOnDownSide())
            for(int x = 3; x<6; x++)
            {
                for(int y = 0; y < 3; y++)
                    if(pieces[y, x] && pieces[y, x].GetPieceType() == Piece.PieceType.King)
                        return new Vector2(x, y);
            }
        else
            for(int x = 3; x<6; x++)
            {
                for(int y = 7; y < 10; y++)
                    if(pieces[y, x] && pieces[y, x].GetPieceType() == Piece.PieceType.King)
                        return new Vector2(x, y);
            }

        return Vector2.zero;
    }

    public bool IsKingUnderAttackAfterMove(Move move, PlayerColor playerColor)
    {
        //create clone to save the board before
        Piece[,] saveBoard = CloneBoard(pieces);
        //do the move
        Piece pieceToMove = pieces[move.getStartY(), move.getStartX()];
        pieces[move.getStartY(), move.getStartX()] = null;
        pieces[move.getEndY(), move.getEndX()] = pieceToMove;

        bool isKingUnderAttackAfterMove = bitBoard.IsCheck(pieces, playerColor);

        pieces = saveBoard;

        return isKingUnderAttackAfterMove;
    }

    private Piece[,] CloneBoard(Piece[,] board)
    {
        Piece[,] clone = new Piece[10, 9];
        
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                clone[y, x] = board[y, x];
            }
        }

        return clone;
    }

    private bool EnemyDoesntHaveMoves()
    {
        //save the enemy color to check if he has valid moves to do
        PlayerColor enemyColor = (PlayerColor)((int)gameManager.GetTurnColor()^1);
        
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                Piece piece = pieces[y, x];
                //if the piece is the enemy piece check what valid moves it can do
                if(piece && (int)piece.GetPieceColor() == (int)enemyColor)
                {
                    BigInteger pieceMoveOptions = piece.GetPieceBitboardMove();
                    pieceMoveOptions = bitBoard.BitboardMovesWithoutDefence(pieceMoveOptions, enemyColor);

                    List<Vector2> movesList = bitBoard.BitboardToVector2s(pieceMoveOptions);

                    foreach(Vector2 move in movesList)
                    {
                        bool isCheckAfterThisMove = IsKingUnderAttackAfterMove(new Move(x, y, (int)move.x, (int)move.y), gameManager.GetTurnColor());
                        
                        //if there is no check after the move draw the dot
                        if(!isCheckAfterThisMove){
                            return false;
                        }
                    }
                }
            }
        }
        //if it doesnt find a piece that has move return true because its check mate
        return true;
    }

}
