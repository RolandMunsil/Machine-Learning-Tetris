using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris
{
    /// <summary>
    /// Just made this for fun. Not used at all for final project.
    /// </summary>
    class RulesBasedPlayer
    {
        public enum Move
        {
            Left,
            Right,
            Down,
            Rotate
        }

        private struct PossibleEndState
        {
            public static readonly PossibleEndState WONT_WORK = new PossibleEndState
            {
                numEmptySquaresUnder = -1,
                lowestRow  = -1,
                numLowestRowSquares = -1,
                xpos = Int32.MinValue,
                rotation = Int32.MaxValue
            };

            public int numEmptySquaresUnder;
            public int lowestRow;
            public int numLowestRowSquares;
            public int rowsThatWouldBeCleared;

            public int xpos;
            public int rotation;

            public Move RecommendedMove(Board board)
            {
                if (rotation > 0)
                    return Move.Rotate;
                else if (xpos < board.currentBlock.topLeft.col)
                    return Move.Left;
                else if (xpos > board.currentBlock.topLeft.col)
                    return Move.Right;
                else
                    return Move.Down;
            }
        }

        public Move DecideMove(Board board)
        {
            Coordinate topLeftBefore = board.currentBlock.topLeft.Clone();
            Coordinate[] squareCoordsBefore = new Coordinate[board.currentBlock.squareCoords.Length];
            for(int i = 0; i < squareCoordsBefore.Length; i++)
            {
                squareCoordsBefore[i] = board.currentBlock.squareCoords[i].Clone();
            }

            List<PossibleEndState> ratedStates = new List<PossibleEndState>();

            for(int xpos = -2; xpos < board.numColumns; xpos++)
            {
                for(int rotation = 0; rotation < 4; rotation++)
                {
                    PossibleEndState ratedState = RateState(board, xpos, rotation);
                    if (!ratedState.Equals(PossibleEndState.WONT_WORK))
                        ratedStates.Add(ratedState);
                }
            }
            

            if (board.currentBlock.topLeft.row != topLeftBefore.row)
                throw new Exception();
            if (board.currentBlock.topLeft.col != topLeftBefore.col)
                throw new Exception();
            for (int i = 0; i < squareCoordsBefore.Length; i++)
            {
                if (board.currentBlock.squareCoords[i].row != squareCoordsBefore[i].row)
                    throw new Exception();
                if (board.currentBlock.squareCoords[i].col != squareCoordsBefore[i].col)
                    throw new Exception();
            }

            //if (!board.CanRotateBlock())
            //    ratedStates = ratedStates.Where(s => s.RecommendedMove(board) != Move.Rotate).ToList();

            //int maxRowsCleared = ratedStates.Max(s => s.rowsThatWouldBeCleared);
            //ratedStates = ratedStates.Where(s => s.rowsThatWouldBeCleared == maxRowsCleared).ToList();

            int minEmptySquares = ratedStates.Min(s => s.numEmptySquaresUnder);
            ratedStates = ratedStates.Where(s => s.numEmptySquaresUnder == minEmptySquares).ToList();
            
            int minLowestRow = ratedStates.Max(s => s.lowestRow);
            ratedStates = ratedStates.Where(s => s.lowestRow == minLowestRow).ToList();
            int maxSquares = ratedStates.Max(s => s.numLowestRowSquares);
            ratedStates = ratedStates.Where(s => s.numLowestRowSquares == maxSquares).ToList();
            int minRotation = ratedStates.Min(s => s.rotation);
            ratedStates = ratedStates.Where(s => s.rotation == minRotation).ToList();
            //Order by xdiff
            ratedStates = ratedStates.OrderBy(s => Math.Abs(s.xpos - topLeftBefore.col)).ToList();

            int bestXpos = ratedStates[0].xpos;
            int bestRotation = ratedStates[0].rotation;

            if (bestXpos == -3)
                throw new Exception();

            if (bestRotation > 0 && board.CanRotateBlock())
                return Move.Rotate;
            else if (bestXpos < board.currentBlock.topLeft.col)
                return Move.Left;
            else if (bestXpos > board.currentBlock.topLeft.col)
                return Move.Right;
            else
                return Move.Down;
        }

        private PossibleEndState RateState(Board board, int xpos, int rotation)
        {
            Coordinate origTopLeft = board.currentBlock.topLeft.Clone();
            Coordinate[] origSquareCoords = board.currentBlock.squareCoords;

            for (int i = 0; i < rotation; i++)
            {
                board.currentBlock = board.currentBlock.RotatedClockwise();
            }
            board.currentBlock.topLeft.col = xpos;

            if (!board.CanBeHere(board.currentBlock))
            {
                board.currentBlock.topLeft = origTopLeft;
                board.currentBlock.squareCoords = origSquareCoords;
                return PossibleEndState.WONT_WORK;
            }

            while (board.CanLowerBlock())
                board.TryLowerBlock();

            PossibleEndState endState = new PossibleEndState
            {
                xpos = xpos,
                rotation = rotation
            };

            int lowestSquareRow = 0;
            int numSquaresInLowestRow = 0;
            foreach (Coordinate coord in board.currentBlock.squareCoords)
            {
                Coordinate boardCoord = board.currentBlock.ToBoardCoordinates(coord);
                if (boardCoord.row > lowestSquareRow)
                {
                    lowestSquareRow = boardCoord.row;
                    numSquaresInLowestRow = 1;
                }
                else if (boardCoord.row == lowestSquareRow)
                {
                    numSquaresInLowestRow++;
                }
            }
            endState.lowestRow = lowestSquareRow;
            endState.numLowestRowSquares = numSquaresInLowestRow;

            //Calc below spaces created
            foreach(Coordinate lowCoord in board.currentBlock.squareCoords.GroupBy(c => c.col).Select(g => g.OrderByDescending(c => c.row).First()))
            {
                Coordinate below = board.currentBlock.ToBoardCoordinates(new Coordinate(lowCoord.row + 1, lowCoord.col));
                while(below.row < 0 || below.row < board.numRows && board[below.row, below.col] == Board.EMPTY_SPACE)
                {
                    endState.numEmptySquaresUnder++;
                    below.row++;
                }
            }

            //Calc rows that would be cleared
            foreach(int row in board.currentBlock.squareCoords.Select(c => c.row + board.currentBlock.topLeft.row).Distinct())
            {
                if (row < 0)
                    continue;
                bool rowFull = true;
                for (int col = 0; col < board.numColumns; col++)
                {
                    Coordinate blockCoord = new Coordinate(row - board.currentBlock.topLeft.row, col - board.currentBlock.topLeft.col);
                    if (board[row, col] == Board.EMPTY_SPACE &&
                        !board.currentBlock.squareCoords.Any(c => c.col == blockCoord.col && c.row == blockCoord.row))
                    {
                        rowFull = false;
                        break;
                    }
                }

                if(rowFull)
                    endState.rowsThatWouldBeCleared++;
            }

            board.currentBlock.topLeft = origTopLeft;
            board.currentBlock.squareCoords = origSquareCoords;

            return endState;
        }
    }
}
