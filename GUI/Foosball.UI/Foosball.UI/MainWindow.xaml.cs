
namespace Foosball.UI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Reflection;
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
        private readonly HttpCommandsManager httpManager;

        // Define event
        public event HttpEvent GoalEvent;
        public event HttpEvent MissEvent;
        public event HttpEvent StartEvent;
        public event HttpEvent StatusEvent;

        public MainWindow()
        {
            InitializeComponent();

            // Assign a function
            GoalEvent += OnGoalEvent;
            MissEvent += OnMissEvent;
            StatusEvent += OnStatusEvent;
            StartEvent += OnStartEvent;
            
            // Send the event to the HTTP manager so it can be invoked by need
            this.httpManager = new HttpCommandsManager(new Dictionary<string, HttpEvent>
            {
                {"Status", StatusEvent},
                {"Goal", GoalEvent},
                {"Miss", MissEvent},
                {"Start", StartEvent},
            });
        }

        private void OnStartEvent(Team team)
        {
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Open.mp3")));
            mediaPlayer.Play();
        }

        private void OnStatusEvent(Team team)
        {
            // UI commands must be invoked from this command since this is the BG thread and not the UI thread
            Dispatcher.Invoke(ChangeBgColor);
        }

        private void OnMissEvent(Team team)
        {
            Dispatcher.Invoke(() =>
            {
                this.DockPanel.Children.Clear();
                this.DockPanel.Children.Add(new MediaElement { Source = new Uri("C:/Users/moshec/Desktop/SoccerBallMiss.gif") });
            });
        }

        private void OnGoalEvent(Team team)
        {
            MediaPlayer mediaPlayer = new MediaPlayer();
            mediaPlayer.Open(new Uri(Path.Combine(Directory.GetCurrentDirectory(), team == Team.Blue ? @"Goal-Brazil.mp3" : @"Goal-Meir.mp3")));
            mediaPlayer.Play();
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
    }
}
