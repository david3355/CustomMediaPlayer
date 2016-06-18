using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace CustomMediaPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        static CustomMediaPlayer.App app;

        private const string Unique = "CustomMediaPlayerUniqueInstance";
        [STAThread]
        public static void Main(string[] args)
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(Unique))
            {
                SetMediaSource(args);
                app = new CustomMediaPlayer.App();
                app.InitializeComponent();
                app.Run();
                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            string[] Args = null;
            if (args.Count > 1)
            {
                Args = new string[args.Count - 1];
                for (int i = 0; i < Args.Length; i++)
                {
                    Args[i] = args[i + 1];
                }
            }
            if (SetMediaSource(Args))
            {
                JMediaPlayer.GetJMediaPlayer().Play();
                MainWindow.Topmost = true;
                MainWindow.Topmost = false;
            }

            return true;
        }

        static bool SetMediaSource(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                JMediaPlayer jmp = JMediaPlayer.GetJMediaPlayer();
                if (jmp.Playing) jmp.Stop();
                JMediaPlayer.NowPlaying = args[0];
                Config.GetInstance.SetConfig(ConfigKey.LastOpened, JMediaPlayer.NowPlaying);
                return true;
            }
            return false;
        }
    }
}
