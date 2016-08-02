
namespace Foosball.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Pipes;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;
    using Path = System.IO.Path;

    public enum Team
    {
        Red,
        Blue
    }

    public delegate void HttpEvent(Team team);
    public delegate void HttpData(long[] data);
    public delegate void HttpMatrix(List<List<int>> hotspots);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int redScore = 0;
        private int blueScore = 0;

        private int gameGoals = 3;

        private static Random rand = new Random();

        private readonly List<string> goalList = new List<string>
        {
            "BrazilGolaso1.mp3",
            "Goal-Brazil.mp3",
            "Goal-Meir.mp3",
            "goal4.mp3",
            // "SpanishGoal1.mp3",
            // "SpanishGoal2.mp3",
            "SpanishGoal3.mp3",
            "harush.mp3"
        };

        long[] possession = new long[3];
        private Dictionary<Team, int> attempts = new Dictionary<Team, int> {{Team.Blue, 0}, {Team.Red, 0}};
        private int[][] hotspots = new int[20][];

        private bool isGameOn = false;
        private bool isFirstStart = true;

        private MediaPlayer mediaPlayer;

        private readonly HttpCommandsManager httpManager;
        private Process p;

        // Define event
        public event HttpEvent NewGame;
        public event HttpEvent StartEvent;
        public event HttpEvent GoalEvent;
        public event HttpEvent MissEvent;
        public event HttpEvent StatusEvent;

        public event HttpData PossessionEvent;
        public event HttpMatrix HotspotEvent;

        public MainWindow()
        {
            string cmd = @"/c start python C:\Users\moshec\Documents\InteractiveFoosball-Hackaton\Foos-Python\ball-tracking\ball-tracking\ball_tracking.py";
            p = Process.Start("cmd.exe", cmd);
            
            InitializeComponent();

            // Assign a function
            NewGame += OnNewGame;
            StartEvent += OnStartEvent;
            GoalEvent += OnGoalEvent;
            MissEvent += OnMissEvent;
            StatusEvent += OnStatusEvent;

            PossessionEvent += OnPossession;
            HotspotEvent += OnHotspotEvent;

            this.mediaPlayer = new MediaPlayer();
            
            // Send the event to the HTTP manager so it can be invoked by need
            this.httpManager = new HttpCommandsManager(PossessionEvent, HotspotEvent,
            new Dictionary<string, HttpEvent>
            {
                {"NewGame", NewGame},
                {"Start", StartEvent},
                {"Goal", GoalEvent},
                {"Miss", MissEvent},
                {"Status", StatusEvent}
            });

            OnNewGame(Team.Blue);
        }

        private void OnHotspotEvent(List<List<int>> data)
        {
            if (this.hotspots[0] == null || !this.hotspots[0].Any())
            {
                for (int i = 0; i < 20; i++)
                {
                    this.hotspots[i] = new int[10];
                    for (int j = 0; j < data[0].Count; j++)
                    {
                        this.hotspots[i][j] = 0;
                    }
                }
            }

            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data[0].Count; j++)
                {
                    this.hotspots[i][j] += data[i][j];
                }
            }
        }

        void OnPossession(long[] data)
        {
            for(int i=0; i<3; i++)
            {
                this.possession[i] += data[i];
            }
        }

        private void OnNewGame(Team team)
        {
            this.possession = new long[3];
            this.attempts = new Dictionary<Team, int> { { Team.Blue, 0 }, { Team.Red, 0 } };
            this.hotspots = new int[20][]; 

            this.redScore = 0;
            this.blueScore = 0;
            UpdateScoreBoard();

            this.isGameOn = true;
            this.isFirstStart = true;

            var randonNum = new Random().Next(0, 2);
            PlayFile(randonNum == 1 ? "NewGame.mp3" : "FIFA98.mp3");

            Dispatcher.Invoke(() =>
            {
                this.DockPanel.Children.Clear();
                this.DockPanel.Children.Add(new MediaElement
                {
                    //Height = 700,
                    //Width = 700,
                    Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Goals.mp4")),
                    IsMuted = true
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
            if (!this.isGameOn)
            {
                return;
            }
            
            int randomNum = new Random().Next(0,2);
            Dispatcher.Invoke(() => this.DockPanel.Children.Clear());
            PlayFile(randomNum == 0 ? "fans2.mp3" : "Buz.mp3");
            PlayFile(this.isFirstStart ? "AirHorn.mp3" : "StartGame.mp3");
            this.isFirstStart = false;
        }

        private void OnGoalEvent(Team team)
        {
            if (!this.isGameOn)
            {
                return;
            }

            this.attempts[team]++;

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

            if (this.redScore == gameGoals || this.blueScore == gameGoals)
            {
                Win(this.redScore > this.blueScore ? Colors.Red : Colors.Blue);
                return;
            }

            if (team == Team.Blue)
            {
                Dispatcher.Invoke(() =>
                {
                    this.DockPanel.Children.Clear();
                    //this.DockPanel.Background = new SolidColorBrush(Colors.Blue);
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

                PlayReply();
            }
            else // RED
            {
                Dispatcher.Invoke(() =>
                {
                    this.DockPanel.Children.Clear();
                    //this.DockPanel.Background = new SolidColorBrush(Colors.Red);
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

                PlayReply();
            }


            int randomNum = rand.Next(0, this.goalList.Count);
            PlayFile(this.goalList[randomNum]);
        }

        private void Win(Color winnerColor)
        {
            if (!this.isGameOn)
            {
                return;
            }

            this.isGameOn = false;

            Dispatcher.Invoke(() =>
            {
                this.DockPanel.Children.Clear();
                //this.DockPanel.Background = new SolidColorBrush(winnerColor);
                this.DockPanel.Orientation = Orientation.Horizontal;
                this.DockPanel.Children.Add(new MediaElement
                {
                    Height = 600,
                    Width = 600,
                    Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "fire1.gif")),
                    Margin = new Thickness(30, -600, 0, 0)
                });
                this.DockPanel.Children.Add(new MediaElement
                {
                    Height = 600,
                    Width = 600,
                    Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "fire2.gif")),
                    Margin = new Thickness(30, 300, 0, 0)
                });
                this.DockPanel.Children.Add(new MediaElement
                {
                    Height = 600,
                    Width = 600,
                    Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "fire4.gif")),
                    Margin = new Thickness(30, -600, 0, 0)
                });
                this.DockPanel.Children.Add(new MediaElement
                {
                    Height = 600,
                    Width = 600,
                    Source = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "fire3.gif")),
                    Margin = new Thickness(30, 100, 0, 0)
                });
            });

            Task.Factory.StartNew(() =>
            {
                Task.Delay(5000).Wait();
                Dispatcher.Invoke(() =>
                {
                    this.DockPanel.Children.Clear();

                    StackPanel stackPanel = new StackPanel() {Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Stretch};

                    Grid statsGrid = GetStatsGrid();
                    stackPanel.Children.Add(statsGrid);

                    Grid grid = GetGotspotGrid();
                    stackPanel.Children.Add(grid);

                    this.DockPanel.Children.Add(stackPanel);
                });
            });

            PlayFile("Win.mp3");
        }

        private void OnMissEvent(Team team)
        {
            if (!this.isGameOn)
            {
                return;
            }

            this.attempts[team]++;

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

            int randomNum = new Random().Next(0, 2);
            PlayFile(randomNum == 0 ? "Miss.mp3" : "miss2.mp3");

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


        private Grid GetStatsGrid()
        {
            var statsGrid = new Grid() { HorizontalAlignment = HorizontalAlignment.Stretch };

            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition());

            statsGrid.RowDefinitions.Add(new RowDefinition());

            var label = new Label() { FontSize = 50, Content = "Attempts", Width = 250 };
            statsGrid.Children.Add(label);
            Grid.SetColumn(label, 1);

            label = new Label() { FontSize = 50, Content = this.attempts[Team.Red], Width = 150};
            statsGrid.Children.Add(label);
            Grid.SetColumn(label, 2);

            label = new Label() { FontSize = 50, Content = this.attempts[Team.Blue], Width = 150 };
            statsGrid.Children.Add(label);
            Grid.SetColumn(label, 0);

            return statsGrid;
        }

        private Grid GetGotspotGrid()
        {
            var grid = new Grid()
            {
                Margin = new Thickness(0,150,0,0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                //MinWidth = this.DockPanel.ActualWidth,
                //MinHeight = this.DockPanel.ActualHeight,
                Background =
                    new ImageBrush()
                    {
                        ImageSource =
                            new BitmapImage(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "field.jpg")))
                    }
            };
            for (int i = 0; i < 20; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int i = 0; i < 10; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            var max = this.hotspots.Max(s => s.Max());

            for (int i = 0; i < this.hotspots.Length; i++)
            {
                for (int j = 0; j < this.hotspots[i].Length; j++)
                {
                    double ratio = this.hotspots[i][j] / (double)max;
                    int intRatio = 255 - (int)(ratio * 255);
                    intRatio -= 100;
                    if (intRatio < 0)
                        intRatio = 0;

                    byte red = (byte)intRatio;
                    var panel = new DockPanel { Height = 50, Width = 50, Background = new RadialGradientBrush(Color.FromRgb(255, red, red), Colors.Transparent) };
                    grid.Children.Add(panel);
                    Grid.SetRow(panel, j);
                    Grid.SetColumn(panel, i);
                }
            }

            return grid;
        }

        private void PlayReply()
        {
            Task.Factory.StartNew(() =>
            {
                Task.Delay(4000).Wait();
                Dispatcher.Invoke(() =>
                {
                    this.DockPanel.Background = new SolidColorBrush(Colors.Transparent);
                    this.DockPanel.Children.Clear();
                    var player = new MediaElement
                    {
                        LoadedBehavior = MediaState.Manual,
                        Source =
                            new Uri(

                                "C:/Users/moshec/Documents/InteractiveFoosball-Hackaton/Foos-Python/ball-tracking/ball-tracking/Output/output_Goal.avi")
                    };
                    this.MainGrid.Children.Add(player);
                    Grid.SetColumnSpan(player, 2);
                    Grid.SetRowSpan(player, 2);
                    player.Play();
                });
            });

            Task.Factory.StartNew(() =>
            {
                Task.Delay(8000).Wait();
                Dispatcher.Invoke(() =>
                {
                    this.DockPanel.Background = new SolidColorBrush(Colors.Transparent);
                    this.DockPanel.Children.Clear();
                    //this.MainGrid.Children.OfType<MediaElement>().Single().Visibility = Visibility.Hidden;
                    var reply = this.MainGrid.Children.OfType<MediaElement>().FirstOrDefault();
                    if (reply != null)
                    {
                        this.MainGrid.Children.Remove(reply);
                    }
                });
            });

            Task.Factory.StartNew(() =>
            {
                Task.Delay(5000).Wait();
                File.Delete(
                    "C:/Users/moshec/Documents/InteractiveFoosball-Hackaton/Foos-Python/ball-tracking/ball-tracking/Output/output_Goal.avi");
            });
        }
       
        private void UpdateScoreBoard()
        {
            Dispatcher.Invoke(() =>
            {
                this.BlueScoreLabel.Content = "Blue: " + this.blueScore;
                this.RedScoreLabel.Content = "Red: " + this.redScore;

            });
        }

        private void PlayFile(string file)
        {
            Dispatcher.Invoke(() =>
            {
                this.mediaPlayer.Stop();
                this.mediaPlayer = new MediaPlayer();
                this.mediaPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", file)));
                this.mediaPlayer.Play();
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
