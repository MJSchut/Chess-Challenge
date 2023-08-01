using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;

public class MaximaxBotD2 : IChessBot
{
    private int EvaluatePiece(int value, bool isWhite)
    {
        return isWhite ? value : -value;
    }
   
    private int GetMiddlePositionalBonus(Piece piece, bool isWhite)
    {
        if (piece.Square.File is 3 or 4 &&
            piece.Square.Rank is 3 or 4)
        {
            return isWhite ? 80 : -80;
        }

        if (piece.Square.Rank is 0 or 1 or 6 or 7 && 
            piece.Square.File is 0 or 1 or 6 or 7)
        {
            return isWhite ? -40 : 40;
        }

        return 0;
    }

    private int AttackedSquare(Board board, Piece piece)
    {
        if (board.SquareIsAttackedByOpponent(piece.Square))
        {
            return piece.IsWhite ? -100 : 100;
        }
        else
        {
            return piece.IsWhite ? 100 : -100;
        }
    }
    
    private int GetLongDiagonalPositionalBonus(Piece piece, bool isWhite)
    {
        if (piece.Square.File == piece.Square.Rank)
        {
            return isWhite ? 40 : -40;
        }

        return 0;
    }
    
    
    public int Evaluate(Board board, bool isWhite)
    {
        var allPieces = board.GetAllPieceLists();

        var evaluation = 0;
        foreach (var pieceList in allPieces)
        {
            foreach (var piece in pieceList)
            {
                switch (piece.PieceType)
                {
                    case PieceType.None:
                        break;
                    case PieceType.Pawn:
                        evaluation += EvaluatePiece(100, piece.IsWhite);
                        evaluation += GetMiddlePositionalBonus(piece, piece.IsWhite);
                        evaluation += AttackedSquare(board, piece);
                        break;
                    case PieceType.Knight:
                        evaluation += EvaluatePiece(200, piece.IsWhite);
                        evaluation += GetMiddlePositionalBonus(piece, piece.IsWhite);
                        evaluation += AttackedSquare(board, piece) * 2;
                        break;
                    case PieceType.Bishop:
                        evaluation += EvaluatePiece(220, piece.IsWhite);
                        evaluation += GetLongDiagonalPositionalBonus(piece, isWhite);
                        evaluation += AttackedSquare(board, piece) * 3;
                        break;
                    case PieceType.Rook:
                        evaluation += EvaluatePiece(400, piece.IsWhite);
                        evaluation += AttackedSquare(board, piece) * 5;
                        break;
                    case PieceType.Queen:
                        evaluation += EvaluatePiece(800, piece.IsWhite);
                        evaluation += GetMiddlePositionalBonus(piece, piece.IsWhite);
                        evaluation += AttackedSquare(board, piece) * 10;
                        break;
                    case PieceType.King:
                        evaluation += EvaluatePiece(2000, piece.IsWhite); 
                        evaluation += AttackedSquare(board, piece) * 20;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        return isWhite ? evaluation : -evaluation;
    }
    
    public Move Think(Board board, Timer timer)
    {
        var moves = board.GetLegalMoves();
        var rand = new Random();
        var moveToMake = moves[rand.Next(moves.Length)];
        var bestScore = 0;
        var isWhite = board.IsWhiteToMove;

        foreach (var move in moves)
        {
            board.MakeMove(move);
            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                return move;
            }
            
            board.ForceSkipTurn();
            var newScore = Maximax(board, 1, isWhite, 0);
            board.UndoSkipTurn();
            board.UndoMove(move);
            
            if (newScore > bestScore)
            {
                bestScore = newScore;
                moveToMake = move;
            }
        }

        return moveToMake;
    }


    public int Maximax(Board board, int depth, bool isWhite, int cutOff)
    {
        if (depth == 0 || board.IsInCheckmate())  
        {
            return Evaluate(board, isWhite);
        }
        
        var moves = board.GetLegalMoves();
        var bestScore = 0;
        foreach (var move in moves)
        {
            board.MakeMove(move);
            board.ForceSkipTurn();
            var newScore = Evaluate(board, isWhite);
            bestScore = Math.Max(bestScore, newScore);

            var score = Maximax(board, depth - 1, isWhite, bestScore);
            bestScore += score; 
            board.UndoSkipTurn();
            board.UndoMove(move);
        }
        return bestScore;
    }
}