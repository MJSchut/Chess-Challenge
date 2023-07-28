﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private int EvaluatePiece(int value, bool isWhite)
    {
        return isWhite ? value : -value;
    }
   
    private int GetMiddlePositionalBonus(Piece piece, bool isWhite)
    {
        if (piece.Square.File is 4 or 5 &&
            piece.Square.Rank is 4 or 5)
        {
            return isWhite ? 80 : -80;
        }

        if (piece.Square.Rank is 1 or 2 or 7 or 8)
        {
            return isWhite ? -40 : 40;
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
                        break;
                    case PieceType.Knight:
                        evaluation += EvaluatePiece(200, piece.IsWhite);
                        evaluation += GetMiddlePositionalBonus(piece, piece.IsWhite);
                        break;
                    case PieceType.Bishop:
                        evaluation += EvaluatePiece(220, piece.IsWhite); 
                        break;
                    case PieceType.Rook:
                        evaluation += EvaluatePiece(300, piece.IsWhite);
                        break;
                    case PieceType.Queen:
                        evaluation += EvaluatePiece(800, piece.IsWhite);
                        evaluation += GetMiddlePositionalBonus(piece, piece.IsWhite);
                        break;
                    case PieceType.King:
                        evaluation += EvaluatePiece(2000, piece.IsWhite); 
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
            
            var skip = board.TrySkipTurn();
            var newScore = Maximax(board, 3, isWhite, 0);
            if (skip)
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
        if (depth == 0)  
        {
            return Evaluate(board, isWhite);
        }
        
        var moves = board.GetLegalMoves();
        var bestScore = 0;
        foreach (var move in moves)
        {
            board.MakeMove(move);
            var skip = board.TrySkipTurn();
            bestScore = Math.Max(bestScore, Evaluate(board, isWhite));
            var score = Maximax(board, depth - 1, isWhite, bestScore);
            bestScore = Math.Max(bestScore, score);
            if (skip)
                board.UndoSkipTurn();
            board.UndoMove(move);
        }
        return bestScore / (10 - depth);
    }
}