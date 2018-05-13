using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace QuickTimer.Model
{
    public class TimerRow : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public delegate void TimerRowElapsedEventHandler(object sender);
        public event TimerRowElapsedEventHandler TimerRowElapsed;
        public void OnTimerRowElapsed() => TimerRowElapsed?.Invoke(this);

        private string _name = "";
        public string Name
        {
            get { return _name; }
            set
            {
                string oldName = _name;
                _name = value;
                if (oldName != _name)
                {
                    OnPropertyChanged("Name");
                }
            }
        }

        private TimeSpan _initialTime = TimeSpan.FromSeconds(0);
        public TimeSpan InitialTime { get { return _initialTime; } }

        private TimeSpan _time = TimeSpan.FromSeconds(0);
        public TimeSpan Time
        {
            get { return _time; }
            set
            {
                TimeSpan oldTime = _time;
                _time = value;
                if (oldTime != _time)
                {
                    OnPropertyChanged("Time");
                }
            }
        }

        public bool IsRunning { get { return _timer.Enabled; } }

        private Timer _timer = new Timer
        {
            AutoReset = true,
            Interval = 1000,
            Enabled = false
        };

        public TimerRow(string name, TimeSpan time)
        {
            Name = name;
            Time = time;
            _initialTime = time;
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Time -= TimeSpan.FromMilliseconds(1000);
            if (Time <= TimeSpan.FromMilliseconds(0))
            {
                StopTimer();
                Time = _initialTime;
                OnTimerRowElapsed();
            }
        }

        public void StartTimer()
        {
            _initialTime = Time;
            _timer.Start();
            OnPropertyChanged("IsRunning");
        }

        public void StopTimer()
        {
            _timer.Stop();
            OnPropertyChanged("IsRunning");
        }

        public void ToggleTimer()
        {
            if (IsRunning)
            {
                StopTimer();
            }
            else
            {
                StartTimer();
            }
        }
    }
}
