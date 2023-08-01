using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class PushBot : IChessBot
{
    private Random rand = new Random();
    private int[] pieceValues = { 0, 1, 3, 4, 5, 10, 200 };

    private int EvaluateCapture(Move move, Board board)
    {
        board.MakeMove(move);
        bool legalRecapture = board.GetLegalMoves().Any(m => m.TargetSquare == move.TargetSquare);
        board.UndoMove(move);

        var capturedPiece = board.GetPiece(move.TargetSquare);
        var capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];
        if (!legalRecapture)
            capturedPieceValue *= 10;

        return capturedPieceValue;
    }
    
    private Move DoRandomMove(Board board, Move[] moves)
    {
        var captureMoves = board.GetLegalMoves(true);

        if (captureMoves.Length > 0)
        {
            // Prioritize capturing moves based on piece value
            var captureMove = captureMoves.MaxBy(m => EvaluateCapture(m, board));
            if (EvaluateCapture(captureMove, board) > 3)
            {
                return captureMove;
            }
        }

        // Check if any move gives check without recapture
        var checkMoves = moves.Where(m => {
            board.MakeMove(m);
            var isCheck = board.IsInCheck();
            var legalRecapture = board.GetLegalMoves().Any(mm => mm.TargetSquare == m.TargetSquare);
            board.UndoMove(m);
            return isCheck && !legalRecapture;
        }).ToArray();

        if (checkMoves.Length > 0)
        {
            return checkMoves[rand.Next(checkMoves.Length)];
        }

        // Pawn push in late game
        var numberOfPieces = CountSetBits(board.AllPiecesBitboard);
        if (numberOfPieces < 12)
        {
            var pawnPushes = moves.Where(m => m.MovePieceType == PieceType.Pawn).ToArray();
            if (pawnPushes.Length > 0)
            {
                var queenPush = pawnPushes.Where(m => m.PromotionPieceType == PieceType.Queen).ToArray();
                if (queenPush.Length > 0)
                {
                    return queenPush[rand.Next(queenPush.Length)];
                }
                return pawnPushes[rand.Next(pawnPushes.Length)];
            }
        }

        // Push towards opposite king
        var isWhite = board.IsWhiteToMove;
        var oppositeKing = board.GetKingSquare(!isWhite);
        var smallestMoves = new List<Move>();
        var smallestDistance = 16.0;

        foreach (var move in moves)
        {
            var distanceBefore = MathF.Sqrt(
                MathF.Pow(move.StartSquare.File - oppositeKing.File, 2) +
                MathF.Pow(move.StartSquare.Rank - oppositeKing.Rank, 2));

            var distanceAfter = MathF.Sqrt(
                MathF.Pow(move.TargetSquare.File - oppositeKing.File, 2) +
                MathF.Pow(move.TargetSquare.Rank - oppositeKing.Rank, 2));

            // Prefer moving a piece that is far away from the king towards the king
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

        // If possible don't move the king in early/mid game
        var movesWithoutKing = smallestMoves.Where(m => m.MovePieceType != PieceType.King).ToList();

        if (movesWithoutKing.Count > 0 && numberOfPieces > 12)
            return movesWithoutKing[rand.Next(movesWithoutKing.Count)];

        return smallestMoves[rand.Next(smallestMoves.Count)];
    }

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        // Prioritize checkmate
        foreach (var move in moves)
        {
            board.MakeMove(move);
            if (board.IsInCheckmate())
            {
                board.UndoMove(move);
                return move;
            }
            board.UndoMove(move);
        }

        return DoRandomMove(board, moves);
    }

    // Count set bits in an ulong number (hamming weight)
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
}
