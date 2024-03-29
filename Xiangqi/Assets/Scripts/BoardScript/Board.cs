
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;



public class Board : MonoBehaviour
{
    //private Dictionary<PieceType, List<Piece>> board;
    private Dictionary<string, Piece> board;
    private Dictionary<PieceType, int> pieceCount;
    //bitboard
    private BitBoard bitBoard;
    private Stack<Move> movesSave;
    private Stack<Fen> fenPositionsSave;

    private GameState gameState;

    public Board(Fen fen)
    {
        //board = new Dictionary<PieceType, List<Piece>>();
        board = new Dictionary<string, Piece>();
        pieceCount = new Dictionary<PieceType, int>{{PieceType.King, 0},{PieceType.Soldier, 0},
         {PieceType.Knight, 0}, {PieceType.Elephant, 0}, {PieceType.Cannon, 0},
          {PieceType.Advisor, 0}, {PieceType.Rook, 0}};

        movesSave = new Stack<Move>();
        fenPositionsSave = new Stack<Fen>();
        fenPositionsSave.Push(fen);

        bitBoard = null;
        gameState = GameState.Opening;
    }

    public Board(Board otherBoard)
    {
        //create clone of the board with different dictionary, list but the same pieces to save time
        board = CloneBoard(otherBoard.board);
        pieceCount = new Dictionary<PieceType, int>(otherBoard.pieceCount);

        movesSave = new Stack<Move>(otherBoard.movesSave);
        fenPositionsSave = new Stack<Fen>(otherBoard.fenPositionsSave);

        bitBoard = new BitBoard(otherBoard.bitBoard);
        gameState = otherBoard.gameState;
    }

    public Fen GetFen()
    {
        return fenPositionsSave.Peek();
    }

    public int GetFenLength()
    {
        return fenPositionsSave.Count;
    }

    public void SetBitBoard()
    {
        bitBoard = new BitBoard(this);
    }

    public List<Piece> GetPiecesList()
    {
        return board.Values.ToList();
    }

    public GameState GetGameState()
    {
        int moveCount = movesSave.Count;
        gameState = GameStateFactory.GetGameState(moveCount);
        return gameState;
    }

    public BigInteger GetBitboardByColor(GameColor gameColor)
    {
        return bitBoard.GetBitboardByColor(gameColor);
    }

    //add new piece to the board
    public void AddPiece(Piece piece)
    {
        try{
            board.Add(piece.GetPos().Name, piece);
            pieceCount[piece.GetPieceType()]++;
        }
        catch (ArgumentException)
        {
            throw new System.Exception("There is already a piece in position: " + piece.GetPos().x + " " + piece.GetPos().y);
        }
    }

    private void RemovePiece(Piece piece)
    {
        try{
            board.Remove(piece.GetPos().Name);
            pieceCount[piece.GetPieceType()]--;
        }
        catch (KeyNotFoundException)
        {
            throw new System.Exception("There is no piece in position: " + piece.GetPos().x + " " + piece.GetPos().y);
        }
    }

    private void EatPiece(Piece removePiece)
    {
        //remove
        RemovePiece(removePiece);

        //clear the all positions save because the board cant be the same
        Fen lastFen = fenPositionsSave.Pop();
        fenPositionsSave.Clear();
        fenPositionsSave.Push(lastFen);
    }

    public void MovePieceOnBoard(Move move)
    {
        //find the pieces
        Piece movingPiece = move.MovingPiece;
        Piece eatenPiece = move.EatenPiece;
        
        //remove the piece from the board
        RemovePiece(movingPiece);
        //moving the piece
        movingPiece.SetPos(move.endPosition);

        //remove the eaten piece from the board, if there is
        if(eatenPiece)
        {
            EatPiece(eatenPiece);
        }

        //add the piece to the new position
        AddPiece(movingPiece);

        //update the bitboard
        bitBoard.UpdateBitBoard(move, movingPiece.GetPieceColor());

        movesSave.Push(move);
        //save the fen after the move
        fenPositionsSave.Push(fenPositionsSave.Peek().FenAfterMove(move));
    }

    public void UndoLastMove()
    {
        //get the last move, and remove the move and the fen from the stacks
        Move lastMove = movesSave.Pop();
        fenPositionsSave.Pop();

        //find the piece
        Piece movingPiece = lastMove.MovingPiece;
        Piece eatenPiece = lastMove.EatenPiece;

        //remove the piece from the board
        RemovePiece(movingPiece);
        //moving the piece
        movingPiece.SetPos(lastMove.startPosition);

        //if there is eaten piece add it to board
        if(eatenPiece)
        {
            AddPiece(eatenPiece);
        }

        AddPiece(movingPiece);

        bitBoard.UndoMoveBitboard(lastMove, movingPiece.GetPieceColor());
    }

    public Piece FindPiece(Position pos)
    {
        try
        {
            return board[pos.Name];
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
    public Piece FindPiece(int x, int y)
    {
        try
        {
            return board[new Position(x, y).Name];
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
        catch (IndexOutOfRangeException)
        {
            return null;
        }
    
    }

    public int GetPieceCount(PieceType pieceType)
    {
        return pieceCount[pieceType];
    }

    public Piece FindKing(GameColor gameColor)
    {
        // Find the king's position and return the piece at that position
        return FindPiece(bitBoard.GetKingBitPos(gameColor));
    }

    public List<string> GetMovesList(bool isRedPlayerDown)
    {
        List<string> movesList = new List<string>();

        // Convert the Stack to a List for reverse iteration
        Move[] movesArray = movesSave.ToArray();

        // Iterate through movesReversed
        for (int i = movesArray.Length - 1; i >= 0; i--)
        {
            Move move = new Move(movesArray[i]);

            if (!isRedPlayerDown)
                move.ChangeSide();

            // Add the move name to movesList
            movesList.Add(move.Name);
        }

        return movesList;
    }

    public List<Position> GetValidMoves(BigInteger movesBitboard, Piece piece)
    {
        //delete all the positions that have piece with the same color of this piece
        movesBitboard = bitBoard.BitboardMovesWithoutDefence(movesBitboard, piece.GetPieceColor());

        //change the bitboard moves to position positions
        List<Position> movesPos = BitBoard.BitboardToPosition(movesBitboard);

        //save the valids moves
        List<Position> validMoves = new List<Position>();
        //check for each move if its valid
        foreach(Position movePos in movesPos)
        {
            Move move = new Move(piece.GetPos(), movePos, piece, FindPiece(movePos));
            bool isCheckAfterThisMove = IsKingUnderAttackAfterMove(move, piece.GetPieceColor().OppositeColor());
            
            //if there is no check after the move add the move to the valids moves list
            if(!isCheckAfterThisMove)
            {
                validMoves.Add(movePos);
            }
            
        }
        return validMoves;  
    }

    public bool IsRepetitiveMove()
    {
        int repetitions = 0;
        Stack<Fen> fenPositionsCopy = new Stack<Fen>(fenPositionsSave);
        Fen currentFen = fenPositionsCopy.Pop();
        
        foreach(Fen fen in fenPositionsCopy)
        {
            if(fen.GetFenString() == currentFen.GetFenString())
            {
                repetitions++;
            }
            if(repetitions > 2)
            {
                return true;
            }
        }
        
        return false;
    }

    private bool IsKingUnderAttackAfterMove(Move move, GameColor attackingColor)
    {
        //create clone to save the board before
        Board boardAfterMove = new Board(this);
        //do the move
        boardAfterMove.MovePieceOnBoard(move);
        //check if the king is under attack after the move
        bool isKingUnderAttackAfterMove = boardAfterMove.IsCheck(attackingColor);
        boardAfterMove.UndoLastMove();


        return isKingUnderAttackAfterMove;
    }

    public bool IsCheck(GameColor attackingColor)
    {
        // Find the king's position
        Piece king = FindKing(attackingColor.OppositeColor());
        // Save the king's position
        int x = king.GetX();
        int y = king.GetY();

        return PawnAttackingKing(x, y, attackingColor) || KnightAttackingKing(x, y, attackingColor) || FileAttackingKing(x, y, attackingColor);
    }

    private bool PawnAttackingKing(int x, int y, GameColor attackingColor)
    {
        if(y < 4)
        {
            Piece pawn = FindPiece(x, y + 1);
            if(pawn && pawn.GetPieceColor() == attackingColor && pawn.GetPieceType() == PieceType.Soldier)
            {
                return true;
            }
        }
        else
        {
            Piece pawn = FindPiece(x, y - 1);
            if(pawn && pawn.GetPieceColor() == attackingColor && pawn.GetPieceType() == PieceType.Soldier)
            {
                return true;
            }
        }

        Piece rightPawn = FindPiece(x + 1, y);
        Piece leftPawn = FindPiece(x - 1, y);
        if(rightPawn && rightPawn.GetPieceColor() == attackingColor && rightPawn.GetPieceType() == PieceType.Soldier)
        {
            return true;
        }
        if(leftPawn && leftPawn.GetPieceColor() == attackingColor && leftPawn.GetPieceType() == PieceType.Soldier)
        {
            return true;
        }
        return false;
    }

    private bool KnightAttackingKing(int x, int y, GameColor attackingColor)
    {
        Piece knight;
        if(!FindPiece(x + 1, y + 1))
        {
            knight = FindPiece(x + 1, y + 2);
            if(knight && knight.GetPieceColor() == attackingColor && knight.GetPieceType() == PieceType.Knight)
            {
                return true;
            }
            knight = FindPiece(x + 2, y + 1);
            if(knight && knight.GetPieceColor() == attackingColor && knight.GetPieceType() == PieceType.Knight)
            {
                return true;
            }
        }
        if(!FindPiece(x - 1, y + 1))
        {
            knight = FindPiece(x - 1, y + 2);
            if(knight && knight.GetPieceColor() == attackingColor && knight.GetPieceType() == PieceType.Knight)
            {
                return true;
            }
            knight = FindPiece(x - 2, y + 1);
            if(knight && knight.GetPieceColor() == attackingColor && knight.GetPieceType() == PieceType.Knight)
            {
                return true;
            }
        }
        if(!FindPiece(x + 1, y - 1))
        {
            knight = FindPiece(x + 1, y - 2);
            if(knight && knight.GetPieceColor() == attackingColor && knight.GetPieceType() == PieceType.Knight)
            {
                return true;
            }
            knight = FindPiece(x + 2, y - 1);
            if(knight && knight.GetPieceColor() == attackingColor && knight.GetPieceType() == PieceType.Knight)
            {
                return true;
            }
        }
        if(!FindPiece(x - 1, y - 1))
        {
            knight = FindPiece(x - 1, y - 2);
            if(knight && knight.GetPieceColor() == attackingColor && knight.GetPieceType() == PieceType.Knight)
            {
                return true;
            }
            knight = FindPiece(x - 2, y - 1);
            if(knight && knight.GetPieceColor() == attackingColor && knight.GetPieceType() == PieceType.Knight)
            {
                return true;
            }
        }
        return false;
    }
    
    //check if there is a piece in the file that can attack the king(rook, cannon, king)
    private bool FileAttackingKing(int x, int y, GameColor attackingColor)
    {
        List<Piece> piecesOnFile = new List<Piece>();

        int pieceCounter = 0;

        //check for the pieces on the file up, loop until got to the end or found 2 pieces on the file
        for (int i = y + 1; i < Constants.BOARD_HEIGHT && pieceCounter  < 2; i++)
        {
            Piece piece = FindPiece(x, i);
            
            if (piece)
            {
                if (pieceCounter == 0)
                {
                    if (piece.GetPieceColor() == attackingColor && (piece.GetPieceType() == PieceType.King || piece.GetPieceType() == PieceType.Rook))
                    {
                        return true;
                    }
                }
                else
                {
                    if (piece.GetPieceColor() == attackingColor && piece.GetPieceType() == PieceType.Cannon)
                    {
                        return true;
                    }
                }
                pieceCounter++;
            }
        }
        
        //reset the piece counter
        pieceCounter = 0;
        //check for the pieces on the file down, loop until got to the end or found 2 pieces on the file
        for (int i = y - 1; i >= 0 && pieceCounter < 2; i--)
        {
            Piece piece = FindPiece(x, i);
            
            if (piece)
            {
                if (pieceCounter == 0)
                {
                    if (piece.GetPieceColor() == attackingColor && (piece.GetPieceType() == PieceType.King || piece.GetPieceType() == PieceType.Rook))
                    {
                        return true;
                    }
                }
                else
                {
                    if (piece.GetPieceColor() == attackingColor && piece.GetPieceType() == PieceType.Cannon)
                    {
                        return true;
                    }
                }
                pieceCounter++;
            }
        }

        //reset the piece counter
        pieceCounter = 0;
        //check for the pieces on the file down, loop until got to the end or found 2 pieces on the file
        for (int i = x + 1; i < Constants.BOARD_WIDTH && pieceCounter < 2; i++)
        {
            Piece piece = FindPiece(i, y);
            
            if (piece)
            {
                if (pieceCounter == 0)
                {
                    if (piece.GetPieceColor() == attackingColor && (piece.GetPieceType() == PieceType.King || piece.GetPieceType() == PieceType.Rook))
                    {
                        return true;
                    }
                }
                else
                {
                    if (piece.GetPieceColor() == attackingColor && piece.GetPieceType() == PieceType.Cannon)
                    {
                        return true;
                    }
                }
                pieceCounter++;
            }
        }

        //reset the piece counter
        pieceCounter = 0;
        //check for the pieces on the file down, loop until got to the end or found 2 pieces on the file
        for (int i = x - 1; i >= 0 && pieceCounter < 2; i--)
        {
            Piece piece = FindPiece(i, y);
            
            if (piece)
            {
                if (pieceCounter == 0)
                {
                    if (piece.GetPieceColor() == attackingColor && (piece.GetPieceType() == PieceType.King || piece.GetPieceType() == PieceType.Rook))
                    {
                        return true;
                    }
                }
                else
                {
                    if (piece.GetPieceColor() == attackingColor && piece.GetPieceType() == PieceType.Cannon)
                    {
                        return true;
                    }
                }
                pieceCounter++;
            }
        }

        return false;
        
    }

    public bool IsDraw()
    {
        return !PiecesForCheckmate() || IsRepetitiveMove();
    }

    public static bool InBorders(int x, int y)
    {
        return x >= 0 && x < Constants.BOARD_WIDTH && y >= 0 && y < Constants.BOARD_HEIGHT;
    }
    private bool PiecesForCheckmate()
    {
        return pieceCount[PieceType.Soldier] > 1 || pieceCount[PieceType.Knight] != 0 || 
        pieceCount[PieceType.Cannon] > 1  || pieceCount[PieceType.Rook] != 0;
    }

    public Dictionary<string, Piece> CloneBoard(Dictionary<string, Piece> cloneBoard)
    {
        // Create a new instance of the Board
        Dictionary<string, Piece> newBoard = new Dictionary<string, Piece>();

        // Deep clone the dictionary
        foreach (var entry in cloneBoard)
        {
            string pos = entry.Key;
            Piece piece = entry.Value;

            // Add the cloned list to the cloned dictionary
            newBoard.Add(pos, piece);
        }

        return newBoard;
    }
    public Dictionary<PieceType, List<Piece>> CloneBoard(Dictionary<PieceType, List<Piece>> cloneBoard)
    {
        // Create a new instance of the Board
        Dictionary<PieceType, List<Piece>> newBoard = new Dictionary<PieceType, List<Piece>>();

        // Deep clone the dictionary
        foreach (var entry in cloneBoard)
        {
            PieceType pieceType = entry.Key;
            List<Piece> pieceList = entry.Value;

            // Create a shallow copy of the list
            List<Piece> clonedPieceList = new List<Piece>(pieceList);

            // Add the cloned list to the cloned dictionary
            newBoard.Add(pieceType, clonedPieceList);
        }

        return newBoard;
    }

    public bool PlayerHaveMoves(GameColor playerColor)
    {

        foreach(Piece piece in board.Values)
        {
            if(piece && (int)piece.GetPieceColor() == (int)playerColor && piece.GetValidMoves(this).Count != 0)
            {
                return true;
            }
        }
        
        return false;
    }
}
