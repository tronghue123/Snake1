using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake
{
    public class GameState
    {
        #region Properties
        public int Rows { get; }
        public int Cols { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }
        #endregion

        #region Private Fields
        private readonly LinkedList<Direction> _dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> _snakePositions = new LinkedList<Position>();
        private readonly Random _random = new Random();
        #endregion

        #region Constructor
        public GameState(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Grid = new GridValue[rows, cols];
            Dir = Direction.Right;

            AddSnake();
            AddFood();
        }
        #endregion

        #region Snake Management
        private void AddSnake()
        {
            int r = Rows / 2;

            for (int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                _snakePositions.AddFirst(new Position(r, c));
            }
        }

        private void AddHead(Position pos)
        {
            _snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Col] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = _snakePositions.Last.Value;
            Grid[tail.Row, tail.Col] = GridValue.Empty;
            _snakePositions.RemoveLast();
        }
        #endregion

        #region Position Management
        private IEnumerable<Position> EmptyPositions()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());

            if (empty.Count == 0)
            {
                return;
            }

            Position pos = empty[_random.Next(empty.Count)];
            Grid[pos.Row, pos.Col] = GridValue.Food;
        }

        public Position HeadPosition() => _snakePositions.First.Value;

        public Position TailPosition() => _snakePositions.Last.Value;

        public IEnumerable<Position> SnakePositions() => _snakePositions;

        private bool OutsideGrid(Position pos)
        {
            return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
        }
        #endregion

        #region Direction Management
        private Direction GetLastDirection()
        {
            return _dirChanges.Count == 0 ? Dir : _dirChanges.Last.Value;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            if (_dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();
            return newDir != lastDir && newDir != lastDir.Opposite();
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
                _dirChanges.AddLast(dir);
            }
        }
        #endregion

        #region Game Logic
        private GridValue WillHit(Position newHeadPos)
        {
            if (OutsideGrid(newHeadPos))
            {
                return GridValue.Outside;
            }

            if (newHeadPos == TailPosition())
            {
                return GridValue.Empty;
            }

            return Grid[newHeadPos.Row, newHeadPos.Col];
        }

        public void Move()
        {
            if (_dirChanges.Count > 0)
            {
                Dir = _dirChanges.First.Value;
                _dirChanges.RemoveFirst();
            }

            Position newHeadPos = HeadPosition().Translate(Dir);
            GridValue hit = WillHit(newHeadPos);

            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPos);
            }
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPos);
                Score++;
                AddFood();
            }
        }
        #endregion
    }
}
