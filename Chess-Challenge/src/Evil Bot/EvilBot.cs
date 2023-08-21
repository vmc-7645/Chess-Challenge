using ChessChallenge.API;
using System;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();

            // Do first available move if time is running out
            if (timer.MillisecondsRemaining < 200) {
                return allMoves[0];
            }

            // Move[] captureMoves = board.GetLegalMoves(true);
            // // Agressive
            // if (captureMoves.Length > 0){
            //     allMoves=captureMoves;
            // }

            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
            int highestVal = 0;

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

                // If Move is not a King, do it
                if (move.MovePieceType == PieceType.King){
                    continue;
                }


                // Find highest value capture
                Piece capturedPiece = board.GetPiece(move.TargetSquare);
                
                int currentMoveVal = pieceValues[(int)capturedPiece.PieceType];// weight relative to value of piece capturing

                // If move is check, add some value
                if(MoveIsCheck(board, move)){
                    currentMoveVal+=80;
                }

                // If square being moved to is attacked, subtract piece value
                if(board.SquareIsAttackedByOpponent(move.TargetSquare)){
                    currentMoveVal -= pieceValues[(int)move.MovePieceType];
                }

                // If current square is attacked, add piece value
                if(board.SquareIsAttackedByOpponent(move.StartSquare)){
                    currentMoveVal += pieceValues[(int)move.MovePieceType];
                }

                if(MoveIsDraw(board, move)){
                    currentMoveVal -= 1000;
                }
                
                // Set best move
                if (currentMoveVal > highestVal)
                {
                    moveToPlay = move;
                    highestVal = currentMoveVal;
                }

            }
            
            // Console.WriteLine("Move value being played "+highestVal);

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
    }
}