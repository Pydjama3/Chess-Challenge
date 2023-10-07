using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;


// MiniMax + Simple Eval
public class MyBotMiniMaxSimpleEval : IChessBot
{
    int[] piecesValue = { 0, 10, 30, 30, 50, 90, 900 };
    bool amIWhite;

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        amIWhite = board.IsWhiteToMove;

        var boardEval = BoardEval(board);

        Move bestMove = moves[new Random().Next(moves.Length)];
        int bestScore = amIWhite ? Int32.MinValue : Int32.MaxValue;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            var eval = MiniMax(3, move, !amIWhite, board /*, new List<Move>() { move }*/);
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

        Console.WriteLine(amIWhite ? "---White---" : "---Black---");
        Console.WriteLine("Best " + bestMove + " with score of " + bestScore);
        Console.WriteLine("--------------------------------------------");

        return bestMove;
    }

    /// <summary>
    /// A move evaluation function using the MiniMax algorithm
    /// </summary>
    /// <param name="depth">Depth of the tree to explore</param>
    /// <param name="studiedMove">The move to evaluate</param>
    /// <param name="maximizingPlayer">If the evaluation is maximized in this recursion (changes in the next recursion)</param>
    /// <param name="studiedBoard">The board on which the move is played</param>
    /// <returns></returns>
    private int MiniMax(int depth, Move studiedMove, bool maximizingPlayer,
        Board studiedBoard /*, List<Move> sequence*/)
    {
        // Return final evaluation if this node is at the end of a branch or the max depth has been reached
        if (depth == 0 || studiedBoard.GetLegalMoves().Length == 0)
        {
            return BoardEval(studiedBoard);
        }

        // Maximal evaluation
        if (maximizingPlayer)
        {
            var value = Int32.MinValue;
            foreach (var move in studiedBoard.GetLegalMoves())
            {
                // sequence.Add(move);
                studiedBoard.MakeMove(move);
                value = Math.Max(value,
                    MiniMax(depth - 1, move, !maximizingPlayer, studiedBoard /*, sequence*/));
                studiedBoard.UndoMove(move);
                // sequence.RemoveAt(sequence.Count - 1);
            }

            return value;
        }
        // Minimize evaluation
        else
        {
            var value = Int32.MaxValue;
            foreach (var move in studiedBoard.GetLegalMoves())
            {
                // sequence.Add(move);
                studiedBoard.MakeMove(move);
                value = Math.Min(value,
                    MiniMax(depth - 1, move, !maximizingPlayer, studiedBoard /*, sequence*/));
                studiedBoard.UndoMove(move);
                // sequence.RemoveAt(sequence.Count - 1);
            }

            return value;
        }
    }

    /// <summary>
    /// Method to evaluate the status of the board (positive => white are winning || negative => black are winning).
    /// Checkmates count as a king (ex: white checkmated = board_evaluation - white_king).
    /// </summary>
    /// <param name="board">The board to evaluate</param>
    /// <returns></returns>
    private int BoardEval(Board board)
    {
        int total = 0;
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            total += piecesValue[(int)pieceList.TypeOfPieceInList] * pieceList.Count *
                     (pieceList.IsWhitePieceList ? 1 : -1);
        }

        return total;
    }
}