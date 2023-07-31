using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    int[] piecesValue = { 0, 10, 30, 30, 50, 90, 1000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        Move bestMove = Move.NullMove;
        int bestScore = piecesValue.Sum() * (board.IsWhiteToMove ? -1 : 1);
        foreach (Move move in moves)
        {
            Console.WriteLine("MinMax");

            board.MakeMove(move);
            var eval = MinMax(3, move, true, board/*, new List<Move>() { move }*/);
            board.UndoMove(move);

            Console.WriteLine(eval);
            Console.WriteLine("--------------------");

            if (board.IsWhiteToMove)
            {
                if (eval > bestScore)
                {
                    bestScore = eval;
                    bestMove = move;
                }
            }
            else
            {
                if (eval < bestScore)
                {
                    bestScore = eval;
                    bestMove = move;
                }
            }
        }

        Console.WriteLine("Best " + bestMove + " with score of " + bestScore);

        return !bestMove.IsNull ? bestMove : moves[new Random().Next(moves.Length)];
    }

    /// <summary>
    /// A method to find the best move in a certain situation by using the min-max algorithm.
    /// </summary>
    /// <param name="depth">To which node depth should the possible moves tree can be explored</param>
    /// <param name="studiedMove">The move to search.</param>
    /// <param name="maximizingPlayer">Evaluate by maximizing or minimizing (changes throughout recursions)</param>
    /// <param name="studiedBoard">The board on which the move is playd</param>
    /// <param name="sequence">The sequence of moves.</param>
    /// <returns></returns>
    private int MinMax(int depth, Move studiedMove, bool maximizingPlayer, Board studiedBoard/*, List<Move> sequence*/)
    {

        // Return final evaluation if this node is at the end of a branch or the max depth has been reached
        if (depth == 0 || studiedBoard.GetLegalMoves().Length == 0)
        {
            return BoardEval(studiedBoard);
        }
        
        // Maximal evaluation
        if (maximizingPlayer)
        {
            var value = -9999;
            foreach (var move in studiedBoard.GetLegalMoves())
            {
                // sequence.Add(move);
                studiedBoard.MakeMove(move);

                value = Math.Max(value, MinMax(depth - 1, move, !maximizingPlayer, studiedBoard/*, sequence*/));

                // sequence.RemoveAt(sequence.Count - 1);
                studiedBoard.UndoMove(move);
            }

            return value;
        }
        // Minimize evaluation
        else
        {
            var value = 9999;
            foreach (var move in studiedBoard.GetLegalMoves())
            {
                // sequence.Add(move);
                studiedBoard.MakeMove(move);

                value = Math.Min(value, MinMax(depth - 1, move, !maximizingPlayer, studiedBoard/*, sequence*/));

                // sequence.RemoveAt(sequence.Count - 1);
                studiedBoard.UndoMove(move);
            }

            return value;
        }
    }

    /// <summary>
    /// Method to evaluate the status of the board (positive => white are winning || negative => black are winning)
    /// </summary>
    /// <param name="board">The board to be evaluated</param>
    /// <returns></returns>
    private int BoardEval(Board board)
    {
        int total = 0;
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            total += piecesValue[(int)pieceList.TypeOfPieceInList] * pieceList.Count *
                     (pieceList.IsWhitePieceList ? 1 : -1);
        }

        if (board.IsInCheckmate())
        {
            total += piecesValue.Sum() * (board.IsInCheckmate() ? 1 : -1);
        }

        return total;
    }
}