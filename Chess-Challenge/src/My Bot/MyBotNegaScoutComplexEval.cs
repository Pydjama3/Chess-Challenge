using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessChallenge.API;

public class MyBotNegaScoutComplexEval : IChessBot
{
    int[] piecesValue = { 0, 10, 30, 30, 50, 90, 900 };
    bool amIWhite;

    public Move Think(Board board, Timer timer)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        
        //////////////////////////////////////////////////
        
        Move[] moves = board.GetLegalMoves();
        amIWhite = board.IsWhiteToMove;

        Move bestMove = moves[new Random().Next(moves.Length)];
        int bestScore = amIWhite ? Int32.MinValue : Int32.MaxValue;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            var eval = NegaScout(3, -1000, 1000, board /*, new List<Move>() { move }*/);
            board.UndoMove(move);


            if (amIWhite)
            {
                if (eval >= bestScore)
                {
                    bestScore = eval;
                    bestMove = move;
                }
            }
            else
            {
                if (eval <= bestScore)
                {
                    bestScore = eval;
                    bestMove = move;
                }
            }
        }

        stopwatch.Stop();
        
        // Console.WriteLine(amIWhite ? "---White---" : "---Black---");
        // Console.WriteLine("Best " + bestMove + " with score of " + bestScore);
        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        Console.WriteLine("--------------------------------------------");

        return bestMove;
    }

    /// <summary>
    /// A move evaluation function using the NegaScout algorithm
    /// </summary>
    /// <param name="depth">Depth of the tree to explore</param>
    /// <param name="alpha">Set initially to a very low value (ex: -inf ^_^)</param>
    /// <param name="beta">Set initially to a very high value (ex: inf ^_^)</param>
    /// <param name="studiedBoard">The board on which the move is played</param>
    /// <returns></returns>
    private int NegaScout(int depth, int alpha, int beta,
        Board studiedBoard /*, List<Move> sequence*/)
    {
        // Return final evaluation if this node is at the end of a branch or the max depth has been reached
        if (depth == 0 || studiedBoard.GetLegalMoves().Length == 0)
        {
            return BoardEval(studiedBoard);
        }

        var moves = studiedBoard.GetLegalMoves();
        int score;
        bool first = true;
        
        foreach (var move in moves)
        {
            studiedBoard.MakeMove(move);
            if (first)
            {
                score = -NegaScout(depth - 1, -alpha, -beta, studiedBoard);
            }
            else
            {
                // TODO: To be completed and finished
            }
        }

        return alpha;
    }

    /// <summary>
    /// Method to evaluate the status of the board (positive => white are winning || negative => black are winning).
    /// Checkmates count as a king (ex: white checkmated = board_evaluation - white_king).
    /// </summary>
    /// <param name="board">The board to evaluate</param>
    /// <param name="evalCheckMate">If the checkmates should be checked</param>
    /// <param name="evalNextCaptures">If the next captures should be checked</param>
    /// <returns></returns>
    private int BoardEval(Board board, bool evalCheckMate = true, bool evalNextCaptures = true)
    {
        int total = 0;
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            total += piecesValue[(int)pieceList.TypeOfPieceInList] * pieceList.Count *
                     (pieceList.IsWhitePieceList ? 1 : -1);
        }

        if (evalNextCaptures)
            foreach (var move in board.GetLegalMoves(true))
            {
                total += piecesValue[(int)move.CapturePieceType] * (board.GetPiece(move.StartSquare).IsWhite ? 1 : -1) /
                         2;
            }

        if (evalCheckMate && board.IsInCheckmate())
        {
            total += board.IsWhiteToMove != amIWhite
                ? (piecesValue[(int)PieceType.King] * (amIWhite ? 1 : -1))
                : -(piecesValue[(int)PieceType.King] * (amIWhite ? 1 : -1));
        }

        return total;
    }
}