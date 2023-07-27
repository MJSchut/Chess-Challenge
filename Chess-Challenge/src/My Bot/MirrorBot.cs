using System;
using System.Linq;
using ChessChallenge.API;

public class MirrorBot : IChessBot
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

        // prefer checkmate and checks
        foreach (var move in moves)
        {
            var checks = MoveIsCheckmate(board, move);
            if (checks.Item1 || checks.Item2)
                return move;
        }

        // prefer capturing
        var captureMoves = moves.Where(m => m.CapturePieceType != PieceType.None).ToArray();
        if (captureMoves.Any())
            return captureMoves[rand.Next(captureMoves.Length)];
        
        // otherwise just do a completely random move
        return moves[rand.Next(moves.Length)]; 
    }
    
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        if (board.GameMoveHistory.Length <= 0) return DoRandomMove(board, moves);
        
        var priorMove = board.GameMoveHistory.Last();
        var priorRank = priorMove.TargetSquare.Rank;
        var flippedRank = Math.Abs(priorRank - 7);
        var mirrorRanks = moves.Where(m => m.TargetSquare.Rank == flippedRank).ToArray();
        var mirrorSquares = mirrorRanks.Where(m => m.TargetSquare.File == priorMove.TargetSquare.File).ToArray();
        var mirrorPieces = mirrorSquares.Where(m => m.MovePieceType == priorMove.MovePieceType).ToArray();

        if (mirrorPieces.Any())
            return DoRandomMove(board, mirrorPieces);
        if (mirrorSquares.Any())
            return DoRandomMove(board, mirrorSquares);
        if (mirrorRanks.Any())
            return DoRandomMove(board, mirrorRanks);

        return DoRandomMove(board, moves);
    }
}