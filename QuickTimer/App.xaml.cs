using Microsoft.Shell;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QuickTimer
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string UNIQUE_APP_ID = "QuickTimers App 5b5a9bf8890a8a6ef4a3911b4c9a0d47";

        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(UNIQUE_APP_ID))
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();

                SingleInstance<App>.Cleanup();
            }
        }

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            // if the app is already running, bring the window to foreground
            if (this.MainWindow is QuickTimer.MainWindow)
            {
                var mainWindow = (QuickTimer.MainWindow)this.MainWindow;
                mainWindow.ShowAndFocusWindow();
            }
            else if (this.MainWindow.WindowState == WindowState.Minimized)
            {
                this.MainWindow.WindowState = WindowState.Normal;
            }

            this.MainWindow.Activate();

            return true;
        }
    }
}
