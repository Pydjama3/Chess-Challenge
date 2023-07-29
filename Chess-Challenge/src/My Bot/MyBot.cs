using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using ChessChallenge.API;

public class MyBot : IChessBot
{
    private Dictionary<PieceType, int> piecesValue = new Dictionary<PieceType, int>()
    {
        { PieceType.Pawn, 10 },
        { PieceType.Bishop, 30 }, { PieceType.Knight, 30 }, { PieceType.Rook, 50 }, { PieceType.Queen, 90 },
        { PieceType.King, 900 }, { PieceType.None, 0 }
    };

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();
        
        Move bestMove = Move.NullMove;
        foreach (Move move in moves)
        {
            if (move.IsCapture)
            {
                if (piecesValue[move.CapturePieceType] > piecesValue[bestMove.CapturePieceType])
                {
                    bestMove = move;
                }
            }
        }

        return !bestMove.IsNull ? bestMove : moves[RandomNumberGenerator.GetInt32(0, moves.Length)];
    }

    private int MinMax(int depth, Move studiedMove, bool maximizingPlayer, Board studiedBoard)
    {
        if (depth == 0 || studiedBoard.GetLegalMoves().Length == 0)
        {
            return BoardEval(studiedBoard);
        } if (maximizingPlayer)
        {
            int value = -9999;
            foreach (var move in studiedBoard.GetLegalMoves())
            {
                studiedBoard.MakeMove(move);
                value = Math.Max(value, MinMax(depth - 1, move, !maximizingPlayer, studiedBoard));
                studiedBoard.UndoMove(move);
            }

            return value;
        }
        else
        {
            int value = 9999;
            foreach (var move in studiedBoard.GetLegalMoves())
            {
                studiedBoard.MakeMove(move);
                value = Math.Min(value, MinMax(depth - 1, move, !maximizingPlayer, studiedBoard));
                studiedBoard.UndoMove(move);
            }

            return value;
        }
    }

    private int BoardEval(Board board)
    {
        int total = 0;
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            total += piecesValue[pieceList.TypeOfPieceInList]*(pieceList.IsWhitePieceList ? 1 : -1);
        }

        return total;
    }
}