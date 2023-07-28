using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;

public class PushBot : IChessBot
{
    public static int CountSetBits(ulong number)
    {
        int count = 0;
        while (number > 0)
        {
            number &= (number - 1);
            count++;
        }
        return count;
    }
    
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
        int[] pieceValues = { 0, 1, 3, 4, 5, 10, 200 };

        // prefer capturing
        var captureMoves = board.GetLegalMoves(true);
        
        if (captureMoves.Any())
        {
            var moveToPlay = captureMoves.First();
            var highestValueCapture = 0;
            foreach (var move in captureMoves)
            {
                // capture an undefended piece
                board.MakeMove(move);
                var legalRecapture = board.GetLegalMoves().Any(m => m.TargetSquare == move.TargetSquare);
                board.UndoMove(move);
                
                var capturedPiece = board.GetPiece(move.TargetSquare);
                var capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];
                if (!legalRecapture)
                    capturedPieceValue *= 10;
                
                if (capturedPieceValue <= highestValueCapture) continue;

                moveToPlay = move;
                highestValueCapture = capturedPieceValue;
            }

            return moveToPlay;
        }
        
        // prefer check without recapture
        var checkMoves = moves.Where(m => MoveIsCheckmate(board, m).Item2).ToArray();
        if (checkMoves.Any())
        {
            foreach (var move in checkMoves)
            {
                board.MakeMove(move);
                var legalRecapture = board.GetLegalMoves().Any(m => m.TargetSquare == move.TargetSquare);
                board.UndoMove(move);
                if (legalRecapture) continue;
                return move;
            }
        }

        // pawn push in late game
        var numberOfPieces = CountSetBits(board.AllPiecesBitboard);
        if (numberOfPieces < 12)
        {
            var pawnPushes = moves.Where(m => m.MovePieceType == PieceType.Pawn).ToArray();
            if (pawnPushes.Length > 0)
            {
                var queenPush = pawnPushes.Where(m => m.PromotionPieceType == PieceType.Queen).ToArray();
                if (queenPush.Any())
                {
                    return queenPush.First();
                }
                return pawnPushes[rand.Next(pawnPushes.Length)]; 
            }
        }
        
        var isWhite = board.IsWhiteToMove;
        var oppositeKing = board.GetKingSquare(!isWhite);
        var smallestMoves = new List<Move>();
        var smallestDistance = 16.0;
        
        // push towards opposite king
        foreach (var move in moves)
        {
            var distanceBefore = MathF.Sqrt(
                MathF.Pow(move.StartSquare.File - oppositeKing.File, 2) +
                MathF.Pow(move.StartSquare.Rank - oppositeKing.Rank, 2));
            
            var distanceAfter = MathF.Sqrt(
                MathF.Pow(move.TargetSquare.File - oppositeKing.File, 2) +
                MathF.Pow(move.TargetSquare.Rank - oppositeKing.Rank, 2));

            // prefer moving a piece that is far away from the king towards the king
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
        
        // if possible don't move the king in early/mid game
        var movesWithoutKing = smallestMoves.Where(m => m.MovePieceType != PieceType.King).ToList();
        
        if (movesWithoutKing.Count > 0 && numberOfPieces > 12)
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