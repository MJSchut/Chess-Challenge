using ChessChallenge.API;
using System;

public class RandomBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        var rand = new Random();
        return moves[rand.Next(moves.Length)];
    }
}