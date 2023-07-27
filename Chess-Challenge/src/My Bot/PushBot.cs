using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class PushBot : IChessBot
{
    // Test if this move gives checkmate
    (bool, bool) MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        bool isCheck = board.IsInCheck();
        board.UndoMove(move);
        return (isMate, isCheck);
    }
    
    private Move DoRandomMove(Board board, Move[] moves)
    {
        var rand = new Random();
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 1, 3, 4, 5, 10, 20 };

        // prefer capturing
        var captureMoves = moves.Where(m => m.CapturePieceType != PieceType.None).ToArray();
        
        if (captureMoves.Any())
        {
            var moveToPlay = captureMoves.First();
            var highestValueCapture = 0;
            foreach (var move in captureMoves)
            {
                var capturedPiece = board.GetPiece(move.TargetSquare);
                var capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                if (capturedPieceValue <= highestValueCapture) continue;

                moveToPlay = move;
                highestValueCapture = capturedPieceValue;
            }

            return moveToPlay;
        }
       
        var isWhite = board.IsWhiteToMove;
        var oppositeKing = board.GetKingSquare(!isWhite);
        var smallestMoves = new List<Move>();
        var smallestDistance = 16.0;
        
        // push 
        foreach (var move in moves)
        {
            var distanceBefore = MathF.Sqrt(
                MathF.Pow(move.StartSquare.File - oppositeKing.File, 2) +
                MathF.Pow(move.StartSquare.Rank - oppositeKing.Rank, 2));
            
            // Pythagoras to determine distance to opponent king
            var distanceAfter = MathF.Sqrt(
                MathF.Pow(move.TargetSquare.File - oppositeKing.File, 2) +
                MathF.Pow(move.TargetSquare.Rank - oppositeKing.Rank, 2));

            var distance = distanceAfter / distanceBefore;

            if (distance < smallestDistance)
            {
                smallestMoves.Clear();
                smallestDistance = distance;
            }

            if (Math.Abs(smallestDistance - distance) < 0.0001)
            {
                smallestMoves.Add(move);
            }
        }
        
        // if possible don't move the king
        var movesWithoutKing = smallestMoves.Where(m => m.MovePieceType != PieceType.King).ToList();
        
        if (movesWithoutKing.Count > 0)
            return movesWithoutKing[rand.Next(movesWithoutKing.Count)];
        return smallestMoves[rand.Next(smallestMoves.Count)];
    }
    
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        // prefer checkmate
        foreach (var move in moves)
        {
            var checks = MoveIsCheckmate(board, move);
            if (checks.Item1)
                return move;
        }

        return DoRandomMove(board, moves);
    }
}