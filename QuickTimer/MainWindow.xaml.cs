using Hardcodet.Wpf.TaskbarNotification;
using QuickTimer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickTimer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskbarIcon taskBarIcon;
        public ObservableCollection<TimerRow> TimerRows = new ObservableCollection<TimerRow>();

        private TimerRow _lastNotifiedTimerRow;
        private bool _persistNotification = false;

        private static string quickTimerFolder = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "QuickTimers"
            );
        private static string timersFilePath = System.IO.Path.Combine(quickTimerFolder, "Timers.csv");

        public MainWindow()
        {
            InitializeComponent();

            taskBarIcon = (TaskbarIcon)FindResource("TrayIconResource");
            taskBarIcon.TrayMouseDoubleClick += TaskBarIcon_TrayMouseDoubleClick;
            taskBarIcon.TrayBalloonTipClicked += TaskBarIcon_TrayBalloonTipClicked;

            taskBarIcon.TrayBalloonTipClosed += TaskBarIcon_TrayBalloonTipClosed;

            itemsControl.ItemsSource = TimerRows;

            RestoreTimers();
        }

        private void TaskBarIcon_TrayBalloonTipClosed(object sender, RoutedEventArgs e)
        {
            // spam user with notifications until they acknowledge it
            if (_persistNotification)
            {
                ShowTimerElapsedNotification(_lastNotifiedTimerRow);
            }
        }

        private void TaskBarIcon_TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            if (_lastNotifiedTimerRow != null && TimerRows.Contains(_lastNotifiedTimerRow)) { 
                var itemContainer = itemsControl.ItemContainerGenerator.ContainerFromItem(_lastNotifiedTimerRow) as ContentPresenter;
                ShowAndFocusWindow();
                itemContainer.BringIntoView();
            }
        }

        private void TaskBarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowAndFocusWindow();
        }

        public void ShowAndFocusWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Focus();
            // window is open, we can stop spamming notifications
            _persistNotification = false;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            TimerRow timerRow = (sender as FrameworkElement).DataContext as TimerRow;
            timerRow.ToggleTimer();
        }

        private void NewTimerRowButton_Click(object sender, RoutedEventArgs e)
        {
            InputDialog inputDialog = new InputDialog("Enter new timer name:", "New Timer");
            if (inputDialog.ShowDialog() == true)
            {
                CreateTimerRow(inputDialog.Answer, TimeSpan.FromMinutes(10));
            }
        }

        private void CreateTimerRow(string name, TimeSpan timeSpan)
        {
            TimerRow newRow = new TimerRow(name, timeSpan);
            newRow.TimerRowElapsed += TimerRowElapsed;
            TimerRows.Add(newRow);
        }

        private void TimerRowElapsed(object sender)
        {
            TimerRow timerRow = sender as TimerRow;
            ShowTimerElapsedNotification(timerRow);
            _lastNotifiedTimerRow = timerRow;
            _persistNotification = true;
        }

        private void ShowTimerElapsedNotification(TimerRow timerRow)
        {
            taskBarIcon.ShowBalloonTip("Time is up!", string.Format("Timer \"{0}\" is finished. Click to dismiss", timerRow.Name), BalloonIcon.Info);
        }

        private void DeleteTimerRowButton_Click(object sender, RoutedEventArgs e)
        {
            TimerRow timerRow = (sender as FrameworkElement).DataContext as TimerRow;
            timerRow.TimerRowElapsed -= TimerRowElapsed;
            timerRow.StopTimer();
            TimerRows.Remove(timerRow);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Closing the app stops and resets the timers, you can minimize it instead.\nAre you sure you want to close QuickTimers?", "Exit QuickTimers?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                StoreTimers();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void StoreTimers()
        {
            if (!Directory.Exists(quickTimerFolder))
            {
                Directory.CreateDirectory(quickTimerFolder);
            }

            var csvBuilder = new StringBuilder();
            foreach (TimerRow timerRow in TimerRows)
            {
                timerRow.StopTimer();
                csvBuilder.AppendLine(string.Format("{0};{1}", timerRow.Name, timerRow.InitialTime.TotalMilliseconds));
            }
            File.WriteAllText(timersFilePath, csvBuilder.ToString());
        }

        private void RestoreTimers()
        {
            if (!Directory.Exists(quickTimerFolder))
            {
                Directory.CreateDirectory(quickTimerFolder);
            }

            if (!File.Exists(timersFilePath))
            {
                return;
            }

            using (var reader = new StreamReader(timersFilePath))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] values = line.Split(';');
                    CreateTimerRow(values[0], TimeSpan.FromMilliseconds(double.Parse(values[1])));
                }
            }
        }
    }
}
