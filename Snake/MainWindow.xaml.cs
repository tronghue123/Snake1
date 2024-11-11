using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields
        private readonly Dictionary<GridValue, ImageSource> _gridValueToImage = new()
        {
            { GridValue.Empty, Images.Empty },
            { GridValue.Snake, Images.Body },
            { GridValue.Food, Images.Food }
        };

        private readonly Dictionary<Direction, int> _directionToRotation = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, 90 },
            { Direction.Down, 180 },
            { Direction.Left, 270 }
        };

        private const int ROWS = 15;
        private const int COLUMNS = 15;
        private readonly Image[,] _gridImages;
        private GameState _gameState;
        private bool _isGameRunning;
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            _gridImages = SetupGrid();
            _gameState = new GameState(ROWS, COLUMNS);
        }
        #endregion

        #region Game Logic Methods
        private async Task RunGame()
        {
            Draw();
            await ShowCountDown();
            Overlay.Visibility = Visibility.Hidden;
            await GameLoop();
            await ShowGameOver();
            _gameState = new GameState(ROWS, COLUMNS);
        }

        private async Task GameLoop()
        {
            while (!_gameState.GameOver)
            {
                await Task.Delay(100);
                _gameState.Move();
                Draw();
            }
        }
        #endregion

        #region Event Handlers
        private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Overlay.Visibility == Visibility.Visible)
            {
                e.Handled = true;
            }

            if (!_isGameRunning)
            {
                _isGameRunning = true;
                await RunGame();
                _isGameRunning = false;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (_gameState.GameOver)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    _gameState.ChangeDirection(Direction.Left);
                    break;
                case Key.Right:
                    _gameState.ChangeDirection(Direction.Right);
                    break;
                case Key.Up:
                    _gameState.ChangeDirection(Direction.Up);
                    break;
                case Key.Down:
                    _gameState.ChangeDirection(Direction.Down);
                    break;
            }
        }
        #endregion

        #region Grid Setup and Drawing
        private Image[,] SetupGrid()
        {
            Image[,] images = new Image[ROWS, COLUMNS];
            GameGrid.Rows = ROWS;
            GameGrid.Columns = COLUMNS;
            GameGrid.Width = GameGrid.Height * (COLUMNS / (double)ROWS);

            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLUMNS; c++)
                {
                    Image image = new Image
                    {
                        Source = Images.Empty,
                        RenderTransformOrigin = new Point(0.5, 0.5)
                    };

                    images[r, c] = image;
                    GameGrid.Children.Add(image);
                }
            }

            return images;
        }

        private void Draw()
        {
            DrawGrid();
            DrawSnakeHead();
            ScoreText.Text = $"Score {_gameState.Score}";
        }

        private void DrawGrid()
        {
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLUMNS; c++)
                {
                    GridValue gridVal = _gameState.Grid[r, c];
                    _gridImages[r, c].Source = _gridValueToImage[gridVal];
                    _gridImages[r, c].RenderTransform = Transform.Identity;
                }
            }
        }

        private void DrawSnakeHead()
        {
            Position headPos = _gameState.HeadPosition();
            Image image = _gridImages[headPos.Row, headPos.Col];
            image.Source = Images.Head;

            int rotation = _directionToRotation[_gameState.Dir];
            image.RenderTransform = new RotateTransform(rotation);
        }
        #endregion

        #region Animation Methods
        private async Task DrawDeadSnake()
        {
            List<Position> positions = new List<Position>(_gameState.SnakePositions());

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];
                ImageSource source = (i == 0) ? Images.DeadHead : Images.DeadBody;
                _gridImages[pos.Row, pos.Col].Source = source;
                await Task.Delay(50);
            }
        }

        private async Task ShowCountDown()
        {
            for (int i = 3; i >= 1; i--)
            {
                OverlayText.Text = i.ToString();
                await Task.Delay(500);
            }
        }

        private async Task ShowGameOver()
        {
            await DrawDeadSnake();
            await Task.Delay(1000);
            Overlay.Visibility = Visibility.Visible;
            OverlayText.Text = "PRESS ANY KEY TO START";
        }
        #endregion
    }
}
