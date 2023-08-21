using ChessChallenge.API;
using System;
// using System.Numerics;
// using System.Collections.Generic;
// using System.Linq;

// https://seblague.github.io/chess-coding-challenge/documentation/#square-struct

public class MyBot : IChessBot
{

    // Piece values: null, pawn, knight, bishop, rook, queen, king
    int[] pieceValues = { 0, 100, 330, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();

        // Do first available move if time is running out
        if (timer.MillisecondsRemaining < 200) {
            return allMoves[0];
        }

        // Pick a random move to play if nothing better is found
        Random rng = new();
        Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
        
        // Move[] topMoves = []

        int highestVal = 0;
        
        (moveToPlay, highestVal) = SelectBestMove(board, board.GetLegalMoves());

        return moveToPlay;
    }

    // TODO Combine "MoveIs" below to single function, should save some space

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    bool MoveIsCheck(Board board, Move move)
    {
        board.MakeMove(move);
        bool isCheck = board.IsInCheck();
        board.UndoMove(move);
        return isCheck;
    }

    bool MoveIsDraw(Board board, Move move)
    {
        board.MakeMove(move);
        bool isDraw = board.IsDraw();
        board.UndoMove(move);
        return isDraw;
    }

    (Move, int) SelectBestMove(Board board, Move[] allMoves)
    {
        int highestVal = 0;
        Move moveToPlay = allMoves[0];

        foreach (Move move in allMoves)
        {
            // Always play checkmate in one
            if (MoveIsCheckmate(board, move))
            {
                moveToPlay = move;
                break;
            }

            // If promotion is a Queen, do it
            if (move.PromotionPieceType == PieceType.Queen){
                moveToPlay = move;
            }

            // Find highest value capture
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            
            int currentMoveVal = pieceValues[(int)capturedPiece.PieceType];// weight relative to value of piece capturing

            // If move is check, add some value
            if(MoveIsCheck(board, move)){
                currentMoveVal+=50;
            }

            // If square being moved to is attacked, subtract piece value
            if(board.SquareIsAttackedByOpponent(move.TargetSquare)){
                currentMoveVal -= pieceValues[(int)move.MovePieceType];
            }

            // If current square is attacked, add piece value
            if(board.SquareIsAttackedByOpponent(move.StartSquare)){
                currentMoveVal += pieceValues[(int)move.MovePieceType];
            }

            // TODO: make sure that this is correctly weighted
            if(MoveIsDraw(board, move)){
                currentMoveVal -= 1000;
            }
            
            // Set best move
            if (currentMoveVal > highestVal)
            {   
                // topMoves.Add(move);
                moveToPlay = move;
                highestVal = currentMoveVal;
            }
        }

        return (moveToPlay, highestVal);
    }


}