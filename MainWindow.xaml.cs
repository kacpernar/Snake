using Snake123.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace Snake123
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region constants
        const int SnakeSquareSize = 40;
        const int SnakeStartLength = 3;
        const int SnakeStartSpeed = 400;
        const int SnakeSpeedThreshold = 100;
        #endregion
        #region Private members
        private readonly Image Apple = new Image();
        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;
        private List<SnakePart> snakeParts = new List<SnakePart>();
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        private readonly Random rnd = new Random();
        private int snakeLength, currentScore, x;   
        private string fileName;
        private enum SnakeDirection { Left, Right, Up, Down };
        #endregion
        #region Methods
        public MainWindow()
        {
            InitializeComponent();
            gameTickTimer.Tick += GameTickTimer_Tick;
            

        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
            SetApple();
            StartNewGame();
        }
        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
            
        }
        private void StartNewGame()
        {
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            currentScore = 0;
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);
            if (Apple != null) { GameArea.Children.Remove(Apple); } 
            DrawSnake();
            DrawSnakeFood();
            UpdateGameStatus();
                      
            gameTickTimer.IsEnabled = true;
        }
        private void EndGame()
        {
            gameTickTimer.IsEnabled = false;
            MessageBox.Show("Oooops, you died!\n\nTo start a new game, just press the Space bar...", "SnakeWPF");
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
                case Key.Escape:
                    System.Environment.Exit(0);
                    break;
                    
            }
            if (snakeDirection != originalSnakeDirection)
                MoveSnake();
        }
        private void SetApple()
        {
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri("pack://application:,,,/Images/apple.png");
            bi3.EndInit();
            Apple.Source = bi3;
        }
        private void DrawSnakeFood()
        {
            Apple.Width = SnakeSquareSize;
            Apple.Height = SnakeSquareSize;
            Point foodPosition = GetNextFoodPosition();
            GameArea.Children.Add(Apple);
            Canvas.SetTop(Apple, foodPosition.Y);
            Canvas.SetLeft(Apple, foodPosition.X);
        }
        private Point GetNextFoodPosition()
        {
            int maxX = (int)(GameArea.ActualWidth / SnakeSquareSize);
            int maxY = (int)(GameArea.ActualHeight / SnakeSquareSize);
            int foodX = rnd.Next(0, maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePart snakePart in snakeParts)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return GetNextFoodPosition();
            }

            return new Point(foodX, foodY);
        }
        private void EatSnakeFood()
        {
            snakeLength++;
            currentScore++;
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            GameArea.Children.Remove(Apple);
            DrawSnakeFood();
            UpdateGameStatus();
        }
        private void DoCollisionCheck()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];

            if ((snakeHead.Position.X == Canvas.GetLeft(Apple)) && (snakeHead.Position.Y == Canvas.GetTop(Apple)))
            {
                EatSnakeFood();
                DrawGameArea();
                return;
            }

            if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= GameArea.ActualHeight) ||
            (snakeHead.Position.X < 0) || (snakeHead.Position.X >= GameArea.ActualWidth))
            {
                EndGame();
            }

            foreach (SnakePart snakeBodyPart in snakeParts.Take(snakeParts.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                    EndGame();
            }
        }
        private void DrawSnake()
        {
            foreach (SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    GameArea.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }
        private void MoveSnake()
        {
            
            while (snakeParts.Count >= snakeLength)
            {
                GameArea.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }
            foreach (SnakePart snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }
            SnakePart snakeHead = snakeParts[^1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }
            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            
            DrawSnake();
            
            DoCollisionCheck();
        }
        private void UpdateGameStatus()
        {
            this.Title = "SnakeWPF - Score: " + currentScore + " - Game speed: " + gameTickTimer.Interval.TotalMilliseconds;
        }
        private void DrawGameArea()
        {
            
            string[] remoteUri = new string[] { "https://upload.wikimedia.org/wikipedia/commons/0/00/Black_mamba.jpg",
                                                "https://upload.wikimedia.org/wikipedia/commons/6/66/Indiancobra.jpg",
                                                "https://upload.wikimedia.org/wikipedia/commons/d/db/Californiakingsnake.jpg",
                                                "https://upload.wikimedia.org/wikipedia/commons/c/c4/Baumpython.jpg",
                                                "https://upload.wikimedia.org/wikipedia/commons/b/b3/Anaconda_jaune_34.JPG"};
            
            x = rnd.Next(0, remoteUri.Length);
            fileName = Environment.CurrentDirectory + "\\" + Convert.ToString(x) + ".bmp";
            if (!File.Exists(fileName))
            {
                WebClient myWebClient = new WebClient();
                //Occurs when an asynchronous file download operation completes.
                myWebClient.DownloadFileCompleted += DownloadCompleted;
                // Download the Web resource and save it into the current filesystem folder, without blocking the calling thread.
                myWebClient.DownloadFileTaskAsync(remoteUri[x], fileName);
            }
            else
            {
                SetBackground();
            }

        }
        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SetBackground();
        }
        private void SetBackground()
        {
            ImageBrush myBrush = new ImageBrush { ImageSource = new BitmapImage(new Uri(fileName, UriKind.Absolute)) };
            Background = myBrush;
        }
        #endregion
    }
}
