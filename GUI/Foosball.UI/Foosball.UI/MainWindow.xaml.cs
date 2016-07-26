
namespace Foosball.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public enum Team
    {
        Red,
        Blue
    }

    public delegate void HttpEvent(Team team);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int redScore = 0;
        private int blueScore = 0;

        private readonly HttpCommandsManager httpManager;

        // Define event
        public event HttpEvent NewGame;
        public event HttpEvent StartEvent;
        public event HttpEvent GoalEvent;
        public event HttpEvent MissEvent;
        public event HttpEvent StatusEvent;

        public MainWindow()
        {
            InitializeComponent();

            // Assign a function
            NewGame += OnNewGame;
            StartEvent += OnStartEvent;
            GoalEvent += OnGoalEvent;
            MissEvent += OnMissEvent;
            StatusEvent += OnStatusEvent;
                    
            
            // Send the event to the HTTP manager so it can be invoked by need
            this.httpManager = new HttpCommandsManager(new Dictionary<string, HttpEvent>
            {
                {"NewGame", NewGame},
                {"Start", StartEvent},
                {"Goal", GoalEvent},
                {"Miss", MissEvent},
                {"Status", StatusEvent},
            });
        }

        private void OnNewGame(Team team)
        {
            this.redScore = 0;
            this.blueScore = 0;
            UpdateScoreBoard();

            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "NewGame.mp3")));
            mediaPlayer.Play();

            Dispatcher.Invoke(() =>
            {
                this.DockPanel.Children.Clear();
                this.DockPanel.Children.Add(new MediaElement
                {
                    //Height = 700,
                    //Width = 700,
                    Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Best100GoalsCutNoSound.mp4"))
                });
            });

            Task.Factory.StartNew(() =>
            {
                Task.Delay(57000).Wait();
                Dispatcher.Invoke(() => { this.DockPanel.Children.Clear(); });
            });
        }

        private void OnStartEvent(Team team)
        {
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "StartGame.mp3")));
            mediaPlayer.Play();
        }

        private void OnGoalEvent(Team team)
        {
            if (team == Team.Blue)
            {
                this.blueScore++;
                UpdateScoreBoard();
            }
            else // RED
            {
                this.redScore++;
                UpdateScoreBoard();
            }

            if (this.redScore == 2 || this.blueScore == 2)
            {
                Win();
                return;
            }

            if (team == Team.Blue)
            {
                Dispatcher.Invoke(() =>
                {
                    this.DockPanel.Children.Clear();
                    this.DockPanel.Children.Add(new MediaElement
                    {
                        Height = 550,
                        Width = 550,
                        Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "goal_zoomout.gif")),
                        Margin = new Thickness(0, -200, 0, -300)
                    });
                    this.DockPanel.Children.Add(new MediaElement
                    {
                        Height = 1200,
                        Width = 1200,
                        Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "BallInGoal.jpg")),
                        Margin = new Thickness(0,-150,0,0)
                    });
                });

                Task.Factory.StartNew(() =>
                {
                    Task.Delay(13000).Wait();
                    Dispatcher.Invoke(() => { this.DockPanel.Children.Clear(); });
                });
            }
            else // RED
            {
                Dispatcher.Invoke(() =>
                {
                    this.DockPanel.Children.Clear();
                    this.DockPanel.Children.Add(new MediaElement
                    {
                        Height = 900,
                        Width = 900,
                        Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "goal.gif")),
                        Margin = new Thickness(0, -300, 0, -200)
                    });
                    this.DockPanel.Children.Add(new MediaElement
                    {
                        Height = 700,
                        Width = 700,
                        Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "FireBall.gif")),
                        Margin = new Thickness(0, -200, 0, 0)
                    });
                });

                Task.Factory.StartNew(() =>
                {
                    Task.Delay(7000).Wait();
                    Dispatcher.Invoke(() => { this.DockPanel.Children.Clear(); });
                });
            }

            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", team == Team.Blue ? @"Goal-Brazil.mp3" : @"Goal-Meir.mp3")));
            mediaPlayer.Play();
        }

        private void Win()
        {
            if (this.blueScore == 10)
            {
                
            }

            Dispatcher.Invoke(() =>
            {
                this.DockPanel.Children.Clear();
                this.DockPanel.Children.Add(new MediaElement
                {
                    Height = 700,
                    Width = 700,
                    Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Win.gif")),
                    Margin = new Thickness(0, 0, 0, 0)
                });
            });

            Task.Factory.StartNew(() =>
            {
                Task.Delay(34000).Wait();
                Dispatcher.Invoke(() => { this.DockPanel.Children.Clear(); });
            });

            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Win.mp3")));
            mediaPlayer.Play();
        }

        private void OnMissEvent(Team team)
        {
            Dispatcher.Invoke(() =>
            {
                this.DockPanel.Children.Clear();
                this.DockPanel.Children.Add(new MediaElement
                {
                    Height = 700,
                    Width = 700,
                    Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "SoccerBallMiss.gif"))
                });
            });

            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Miss.mp3")));
            mediaPlayer.Play();

            Task.Factory.StartNew(() =>
            {
                Task.Delay(2000).Wait();
                Dispatcher.Invoke(() => { this.DockPanel.Children.Clear(); });
            });
        }

        private void OnStatusEvent(Team team)
        {
            // UI commands must be invoked from this command since this is the BG thread and not the UI thread
            Dispatcher.Invoke(ChangeBgColor);
        }

        private void UpdateScoreBoard()
        {
            Dispatcher.Invoke(() =>
            {
                this.BlueScoreLabel.Content = "Blue: " + this.blueScore;
                this.RedScoreLabel.Content = "Red: " + this.redScore;

            });
        }



        private void ChangeBgColor()
        {
            Background = PickBrush();
        }

        private Brush PickBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            this.httpManager.Dispose();
        }

        private void NewGameButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.blueScore = this.redScore = 0;
            UpdateScoreBoard();

            OnNewGame(Team.Blue);
        }
    }
}
