using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;

public class MaximaxBotD1 : IChessBot
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

        if (piece.Square.Rank is 0 or 1 or 6 or 7)
        {
            return isWhite ? -40 : 40;
        }

        return 0;
    }

    private int GetLongDiagonalPositionalBonus(Piece piece, bool isWhite)
    {
        if (piece.Square.File == piece.Square.Rank)
        {
            return isWhite ? 40 : -40;
        }

        return 0;
    }

    private int AttackedSquare(Board board, Piece piece)
    {
        if (board.SquareIsAttackedByOpponent(piece.Square))
        {
            return piece.IsWhite ? -200 : 200;
        }

        return piece.IsWhite ? 200 : -200;
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
                        evaluation += AttackedSquare(board, piece);
                        break;
                    case PieceType.Bishop:
                        evaluation += EvaluatePiece(220, piece.IsWhite);
                        evaluation += GetLongDiagonalPositionalBonus(piece, piece.IsWhite);
                        evaluation += AttackedSquare(board, piece);
                        break;
                    case PieceType.Rook:
                        evaluation += EvaluatePiece(400, piece.IsWhite);
                        evaluation += AttackedSquare(board, piece);
                        break;
                    case PieceType.Queen:
                        evaluation += EvaluatePiece(800, piece.IsWhite);
                        evaluation += GetMiddlePositionalBonus(piece, piece.IsWhite);
                        evaluation += AttackedSquare(board, piece);
                        break;
                    case PieceType.King:
                        evaluation += EvaluatePiece(2000, piece.IsWhite);
                        evaluation += AttackedSquare(board, piece);
                        evaluation += AttackedSquare(board, piece);
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
            
            var newScore = Evaluate(board, isWhite);
            board.UndoMove(move);
            
            if (newScore > bestScore)
            {
                bestScore = newScore;
                moveToMake = move;
            }
        }

        return moveToMake;
    }
}