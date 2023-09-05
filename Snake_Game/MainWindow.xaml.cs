using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static Snake_Game.DirectionManager;

namespace Snake_Game
{
    public partial class MainWindow : Window
    {
        ScoreModel model = new ScoreModel();

        Direction dir = Direction.East;
        Direction nextDirection = Direction.East;

        int size = 40;
        int interval = 128;
        int tailIndex;
        int[] dimensions = new int[2];

        List<Point> snakeCoords = new List<Point>();
        List<Rectangle> snake = new List<Rectangle>();


        public MainWindow()
        {
            InitializeComponent();
            DataContext = model;
        }


        private async Task GameLoop()
        {
            while (true)
            {
                await Task.Delay(interval);
                dir = nextDirection;

                await Dispatcher.BeginInvoke(new Action(() => CheckForApple()));
                await Move();

                if (Collision())
                    Reset();
            }
        }

        private async Task Move()
        {
            if (snakeCoords.Count > 1)
            {
                Point headPos = snakeCoords[0];
                snakeCoords[tailIndex] = headPos; //Not sure to move this 5 lines down after tail index is updated, both methods work

                if (tailIndex < snakeCoords.Count - 1)
                    tailIndex++;
                else
                    tailIndex = 1;

                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    Grid.SetColumn(snake[tailIndex], (int)headPos.X);
                    Grid.SetRow(snake[tailIndex], (int)headPos.Y);
                }));
            }

            await Dispatcher.BeginInvoke(new Action(() => MoveHead()));
        }

        private bool Collision()
        {
            return snakeCoords.GroupBy(n => n).Any(group => group.Count() > 1); //There is only a collision when there is a duplicate in the coordinates list
        }

        private void CheckForApple()
        {
            Point applePos = new Point(Grid.GetColumn(Apple), Grid.GetRow(Apple));

            if (snakeCoords[0] == applePos)
                GrowSnake();
        }

        private void GrowSnake()
        {
            model.Score++;
            snakeCoords.Insert(tailIndex, snakeCoords[tailIndex]);

            Rectangle part = new Rectangle();
            part.Fill = Brushes.Green;
            MainGrid.Children.Add(part);
            snake.Add(part);
            Grid.SetColumn(part, (int)snakeCoords[0].X);
            Grid.SetRow(part, (int)snakeCoords[0].Y);

            MoveApple();
        }

        private void MoveApple()
        {
            Point currentPos = new Point(Grid.GetColumn(Apple), Grid.GetRow(Apple));
            List<Point> unallowedCoords = new List<Point>() { currentPos };
            unallowedCoords.AddRange(snakeCoords);

            var p = GetRandCoord(unallowedCoords);
            Grid.SetColumn(Apple, (int)p.X);
            Grid.SetRow(Apple, (int)p.Y);
        }

        private Point GetRandCoord(List<Point> unallowedCoords)
        {
            Random rnd = new Random();
            while (true)
            {
                int x = rnd.Next(dimensions[0]);
                int y = rnd.Next(dimensions[1]);

                if (!unallowedCoords.Contains(new Point(x, y)))
                    return new Point(x, y);
            }
        }

        private void MoveHead()
        {
            int col = Grid.GetColumn(SnakeHead);
            int row = Grid.GetRow(SnakeHead);

            if (dir == Direction.West)
            {
                int newCol = (col - 1 + dimensions[0]) % dimensions[0];
                snakeCoords[0] = new Point(newCol, snakeCoords[0].Y);
                Grid.SetColumn(SnakeHead, newCol);
            }
            else if (dir == Direction.East)
            {
                int newCol = (col + 1) % dimensions[0];
                snakeCoords[0] = new Point(newCol, snakeCoords[0].Y);
                Grid.SetColumn(SnakeHead, newCol);
            }
            else if (dir == Direction.North)
            {
                int newRow = (row - 1 + dimensions[1]) % dimensions[1];
                snakeCoords[0] = new Point(snakeCoords[0].X, newRow);
                Grid.SetRow(SnakeHead, newRow);
            }
            else if (dir == Direction.South)
            {
                int newRow = (row + 1 + dimensions[1]) % dimensions[1];
                snakeCoords[0] = new Point(snakeCoords[0].X, newRow);
                Grid.SetRow(SnakeHead, newRow);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.A || e.Key == Key.Left) && dir != Direction.East)
                nextDirection = Direction.West;
            else if ((e.Key == Key.D || e.Key == Key.Right) && dir != Direction.West)
                nextDirection = Direction.East;
            else if ((e.Key == Key.W || e.Key == Key.Up) && dir != Direction.South)
                nextDirection = Direction.North;
            else if ((e.Key == Key.S || e.Key == Key.Down) && dir != Direction.North)
                nextDirection = Direction.South;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetRowsAndColumns();
            Reset();
            Task.Run(GameLoop);
        }

        private void SetRowsAndColumns()
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            double aspectRatio = Width / Height;
            dimensions[1] = (int)(size / aspectRatio);
            dimensions[0] = size;

            for (int i = 0; i < dimensions[0]; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int j = 0; j < dimensions[1]; j++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
            }
        }
        private void Reset()
        {
            model.Score = 1;
            tailIndex = 0;
            dir = Direction.East;
            nextDirection = Direction.East;

            int startCol = MainGrid.ColumnDefinitions.Count / 3;
            int startRow = MainGrid.RowDefinitions.Count / 2;
            int length = snake.Count - 1;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                MainGrid.Children.RemoveRange(MainGrid.Children.Count - length, MainGrid.Children.Count);
                Grid.SetColumn(SnakeHead, startCol);
                Grid.SetRow(SnakeHead, startRow);
            }));

            snakeCoords = new List<Point>();
            snake = new List<Rectangle>
            {
                SnakeHead
            };
            snakeCoords.Add(new Point(startCol, startRow));
            Dispatcher.BeginInvoke(new Action(() => MoveApple()));
        }
    }
}
