﻿using System;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    int[] piecesValue = { 0, 10, 30, 30, 50, 90, 900 };
    bool amIWhite;

    private Dictionary<Move, List<int>> history = new Dictionary<Move, List<int>>();
    private Move lastMove = Move.NullMove;
    private int lastEval = 0;

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        amIWhite = board.IsWhiteToMove;

        var boardEval = BoardEval(board);

        if (lastMove != Move.NullMove)
            if (history.TryGetValue(lastMove, out var element))
            {
                element.Add(lastEval - boardEval);
            }
            else
            {
                history.Add(lastMove, new List<int>() { lastEval - boardEval });
            }

        Move bestMove = moves[new Random().Next(moves.Length)];
        int bestScore = amIWhite ? Int32.MinValue : Int32.MaxValue;
        
        foreach (Move move in moves)
        {
            board.MakeMove(move);
            var eval = AlphaBeta(3, !amIWhite, -1000, 1000, board);
            board.UndoMove(move);


            if (amIWhite)
            {
                if (eval < bestScore) continue;
                bestScore = eval;
                bestMove = move;
            }
            else
            {
                if (eval > bestScore) continue;
                bestScore = eval;
                bestMove = move;
            }
        }

        lastMove = bestMove;
        lastEval = boardEval;

        history.TryGetValue(bestMove, out var stats);
        
        Console.WriteLine((amIWhite ? "White" : "Black") + " —— Current board evaluation: " + boardEval);
        Console.WriteLine((amIWhite ? "White" : "Black") + " —— Stats of best " + (stats?.Average() ?? bestScore));
        Console.WriteLine((amIWhite ? "White" : "Black") + " —— Best " + bestMove + " with score of " + bestScore);
        Console.WriteLine("--------------------------------------------");

        return bestMove;
    }

    /// <summary>
    /// A move evaluation function using the AlphaBeta algorithm
    /// </summary>
    /// <param name="depth">Depth of the tree to explore</param>
    /// <param name="maximizingPlayer">If the evaluation is maximized in this recursion (changes in the next recursion)</param>
    /// <param name="alpha">Set initially to a very low value (ex: -inf ^_^)</param>
    /// <param name="beta">Set initially to a very high value (ex: inf ^_^)</param>
    /// <param name="studiedBoard">The board on which the move is played</param>
    /// <returns></returns>
    private int AlphaBeta(int depth, bool maximizingPlayer, int alpha, int beta,
        Board studiedBoard)
    {
        Span<Move> moves = stackalloc Move[128];
        studiedBoard.GetLegalMovesNonAlloc(ref moves);
        
        // Return final evaluation if this node is at the end of a branch or the max depth has been reached
        if (depth == 0 || moves.Length == 0)
        {
            return BoardEval(studiedBoard);
        }

        // Maximal evaluation
        if (maximizingPlayer)
        {
            var value = Int32.MinValue;
            foreach (var move in moves/*OrderMoves(history, studiedBoard.GetLegalMoves(), false)*/)
            {
                studiedBoard.MakeMove(move);
                value = Math.Max(value,
                    AlphaBeta(depth - 1, !maximizingPlayer, alpha, beta, studiedBoard));
                studiedBoard.UndoMove(move);

                if (value > beta)
                {
                    break;
                }

                alpha = Math.Max(alpha, value);
            }

            return value;
        }
        // Minimize evaluation
        else
        {
            var value = Int32.MaxValue;
            foreach (var move in moves/*OrderMoves(history, studiedBoard.GetLegalMoves(), true)*/)
            {
                studiedBoard.MakeMove(move);
                value = Math.Min(value,
                    AlphaBeta(depth - 1, !maximizingPlayer, alpha, beta, studiedBoard));
                studiedBoard.UndoMove(move);

                if (value < alpha)
                {
                    break;
                }

                beta = Math.Min(beta, value);
            }

            return value;
        }
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
        {
            Span<Move> moves = stackalloc Move[128];
            board.GetLegalMovesNonAlloc(ref moves, true);

            foreach (var move in moves)
            {
                total += piecesValue[(int)move.CapturePieceType] * (board.GetPiece(move.StartSquare).IsWhite ? 1 : -1) /
                         2;
            }
        }
        if (evalCheckMate && board.IsInCheckmate())
        {
            total += board.IsWhiteToMove != amIWhite
                ? (piecesValue[(int)PieceType.King] * (amIWhite ? 1 : -1))
                : -(piecesValue[(int)PieceType.King] * (amIWhite ? 1 : -1));
        }

        return total;
    }

    Move[] OrderMoves(Dictionary<Move, List<int>> history, Move[] moves, bool ascending = true)
    {
        if (history.Count > 0)
        {
            List<Move> orderedMoves = new List<Move>();

            Dictionary<Move, List<int>> orderedHistory;
            if (ascending)
            {
                orderedHistory = history.OrderBy(move => move.Value.Average()).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                orderedHistory = history.OrderByDescending(move => move.Value.Average())
                    .ToDictionary(x => x.Key, x => x.Value);
            }

            foreach (var playedMove in orderedHistory.Keys)
            {
                if (moves.Contains(playedMove))
                {
                    orderedMoves.Add(playedMove);
                }
            }

            foreach (Move move in moves)
            {
                if (!orderedMoves.Contains(move))
                    orderedMoves.Add(move);
            }

            return orderedMoves.ToArray();
        }

        return moves;
    }
}