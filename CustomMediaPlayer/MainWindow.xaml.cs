using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using System.IO;
using System.Reflection;
using PopUp;

namespace CustomMediaPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private JMediaPlayer jmp;
        private double prevVolume;
        private OpenFileDialog ofd;
        private GlobalKeyboardHook hook;
        private const int interval = 10;
        private bool changeAllowed;
        private Config config;
        private IntelligentPlayingManager intelligentPlayingManager;
        private TitleAssembler title;

        private Dictionary<string, Function> functions = new Dictionary<string, Function>();

        private void Init()
        {
            config = Config.GetInstanceInit(functions);
            intelligentPlayingManager = new IntelligentPlayingManager();
            title = new TitleAssembler(SetTitle);
            changeAllowed = true;
            SetFunctions();
            jmp = JMediaPlayer.GetJMediaPlayer();
            config.SignUp(ConfigChanged);
            SetConfig();
            jmp.SetMediaEvents(MediaEnded, MediaOpened, PositionChanged, PlayChanged);
            if (JMediaPlayer.NowPlaying != String.Empty)
            {
                btn_start.IsEnabled = true;
                btn_start_Click(null, null);
            }
            ofd = new OpenFileDialog();
            slide_volume.Value = 50;
            hook = new GlobalKeyboardHook(KeyDownHandler);
            hook.Hook();
        }

        #region Config

        private void SetFunctions()
        {
            Function f1 = new Function(PlayPause, App.Current.TryFindResource("btn_play").ToString());
            Function f2 = new Function(JumpBackward, App.Current.TryFindResource("jmp_back").ToString());
            Function f3 = new Function(StopPlaying, App.Current.TryFindResource("btn_stop").ToString());
            Function f4 = new Function(JumpForward, App.Current.TryFindResource("jmp_fwd").ToString());
            Function f5 = new Function(IncreasePlaySpeed, App.Current.TryFindResource("inc_playspeed").ToString());
            Function f6 = new Function(DecreasePlaySpeed, App.Current.TryFindResource("decr_playspeed").ToString());
            Function f7 = new Function(SetPlaySpeedNormal, App.Current.TryFindResource("normal_playspeed").ToString());
            AddFunctions(f1, f2, f3, f4, f5, f6, f7);
        }

        private void AddFunctions(params Function[] functionsToAdd)
        {
            foreach (Function f in functionsToAdd)
            {
                functions.Add(f.FunctionName, f);
                Settings.SignFunction(f);
            }
        }

        private void SetConfig()
        {
            bool configLoaded = config.LoadConfig();
            if (!configLoaded)
            {
                SetDefaultConfig();
            }
        }

        private void SetDefaultConfig()
        {
            config.SetConfig(ConfigKey.JumpTime, 2000);
            config.SetConfig(ConfigKey.PauseTime, 1000);
            config.SetConfig(ConfigKey.Topmost, false);
            config.SetConfig(ConfigKey.IntelligentPlaying, true);
            HotKeyHandler playpause = PlayPause;
            HotKeyHandler jumpbck = JumpBackward;
            config.AddHotKeyHandler(new HotKey(Forms.Keys.RControlKey, functions[playpause.Method.Name]));
            config.AddHotKeyHandler(new HotKey(Forms.Keys.LControlKey, functions[jumpbck.Method.Name]));
        }

        #endregion

        public void btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (JMediaPlayer.NowPlaying == String.Empty) return;
            if (!jmp.Playing)
            {
                jmp.Play();                
            }
            else
            {
                jmp.Pause();
                intelligentPlayingManager.UserPausesPlaying();
            }
        }

        private void SetTime(Label TimeLabel, TimeSpan Value)
        {
            string hour = Value.Hours < 10 ? "0" + Value.Hours.ToString() : Value.Hours.ToString();
            string min = Value.Minutes < 10 ? "0" + Value.Minutes.ToString() : Value.Minutes.ToString();
            string sec = Value.Seconds < 10 ? "0" + Value.Seconds.ToString() : Value.Seconds.ToString();
            TimeLabel.Content = hour + ':' + min + ':' + sec;
        }

        private void SetTitle(String Title)
        {
            this.Title = Title;
        }

        private void ConfigChanged(ConfigKey Key)
        {
            switch (Key)
            {
                case ConfigKey.JumpTime:
                    jmp.JumpTimeMS = Convert.ToInt32(config.GetConfig(ConfigKey.JumpTime)); break;
                case ConfigKey.Topmost:
                    mainWindow.Topmost = Convert.ToBoolean(config.GetConfig(ConfigKey.Topmost)); break;
                case ConfigKey.LastOpened:
                    AddToRecentlyOpened(config.GetConfig(ConfigKey.LastOpened).ToString()); break;
                case ConfigKey.PauseTime:
                    intelligentPlayingManager.SetPauseTime(Convert.ToInt32(config.GetConfig(ConfigKey.PauseTime))); break;
                case ConfigKey.IntelligentPlaying:
                    intelligentPlayingManager.Enabled = Convert.ToBoolean(config.GetConfig(ConfigKey.IntelligentPlaying)); break;
            }
        }

        private void MediaOpened(object sender, EventArgs e)
        {
            //if (jmp.HasVideo)
            //{
            //    Video video = new Video(null);
            //    video.Show();
            //}
            slider_time.Maximum = jmp.MediaLengthInMS;
            SetTime(label_fulltime, new TimeSpan(0, 0, 0, 0, (int)jmp.MediaLengthInMS));
            config.SetConfig(ConfigKey.LastOpened, JMediaPlayer.NowPlaying);
        }

        private void MediaEnded(object sender, EventArgs e)
        {
            jmp.StopTimer();
            jmp.Stop();
            jmp.Playing = false;
            slider_time.Value = 0;
            label_time.Content = "00:00:00";
            PlayChanged(null, null);
            intelligentPlayingManager.MediaEnded();
        }

        private void PositionChanged(object sender, EventArgs e)
        {
            if (changeAllowed)
            {
                slider_time.Value = jmp.Position.TotalMilliseconds;
                if (jmp.MediaOpened) title.PlayerTimeChanged(jmp.Position, jmp.Duration);
            }
        }

        private void PlayChanged(object sender, EventArgs e)
        {
            if (!jmp.Playing) PlayPaused();
            else PlayStarted();
        }

        private void PlayStarted()
        {
            if (!btn_start.IsEnabled) btn_start.IsEnabled = true;
            img_play.Source = new BitmapImage(new Uri(@"/img/pause.png", UriKind.Relative));
            intelligentPlayingManager.PlayingStarted();
            title.PlayStarted();
            intelligentPlayingManager.UserStartsPlaying();
        }

        private void PlayPaused()
        {
            img_play.Source = new BitmapImage(new Uri(@"/img/play2.png", UriKind.Relative));
            intelligentPlayingManager.PlayingPaused();
            title.PlayPaused();
        }

        private void AllowSlideChange()
        {
            changeAllowed = true;
        }

        private void DisableSlideChange()
        {
            changeAllowed = false;
        }

        private void slider_time_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DisableSlideChange();
            if (JMediaPlayer.NowPlaying == String.Empty) return;
            //jmp.StopTimer();
        }

        private void slider_time_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (JMediaPlayer.NowPlaying == String.Empty) return;
            jmp.Position = new TimeSpan(0, 0, 0, 0, (int)slider_time.Value);
            //jmp.StartTimer();
            AllowSlideChange();
        }

        private void slider_time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetTime(label_time, new TimeSpan(0, 0, 0, 0, (int)slider_time.Value));
        }


        private void btn_voloff_Click(object sender, RoutedEventArgs e)
        {
            if (jmp.Volume != 0)
            {
                prevVolume = slide_volume.Value;
                slide_volume.Value = 0;
            }
            else
            {
                slide_volume.Value = prevVolume;
            }
        }

        private void slide_volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            jmp.Volume = slide_volume.Value / 100;
            if (slide_volume.Value == 0) img_mute.Source = new BitmapImage(new Uri("img/voloff.png", UriKind.Relative));
            else img_mute.Source = new BitmapImage(new Uri("img/volon.png", UriKind.Relative));
        }

        private void menuitem_openfile_Click(object sender, RoutedEventArgs e)
        {
            bool? ok = ofd.ShowDialog();
            if (ok == true)
            {
                StopPlaying();
                JMediaPlayer.NowPlaying = ofd.FileName;
                btn_start.IsEnabled = true;
                mainWindow.Title = JMediaPlayer.FileName;
                PlayPause();
            }
        }

        private void AddToRecentlyOpened(string FileName)
        {
            MenuItem item = new MenuItem();
            item.Header = FileName;
            item.Icon = new Image()
            {
                Source = new BitmapImage(new Uri(@"/img/play2.png", UriKind.Relative)),
                Width = 20
            };
            item.Click += RecentlyOpenedClick;

            if (!RecentlyPlayedListContainsItem(FileName))
            {
                menu_recently_opened.Items.Add(item);
            }
        }

        private bool RecentlyPlayedListContainsItem(String Item)
        {
            foreach (MenuItem historyItem in menu_recently_opened.Items)
            {
                if (historyItem.Header.Equals(Item)) return true;
            }
            return false;
        }

        private void RecentlyOpenedClick(object sender, RoutedEventArgs args)
        {
            MenuItem item = sender as MenuItem;
            string file = item.Header.ToString();
            if (!File.Exists(file))
            {
                MessageBox.Show("A fájl már nem létezik!");
                menu_recently_opened.Items.Remove(item);
            }
            else
            {
                JMediaPlayer.NowPlaying = file;
                if (jmp.Playing) btn_stop_Click(null, null);
                btn_start_Click(null, null);
            }
        }


        private void btn_stop_Click(object sender, RoutedEventArgs e)
        {
            if (JMediaPlayer.NowPlaying == String.Empty) return;
            jmp.Stop();
            img_play.Source = new BitmapImage(new Uri(@"/img/play2.png", UriKind.Relative));
            slider_time.Value = 0;
            label_time.Content = String.Empty;
        }

        private void slide_speed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double v = slide_speed.Value;
            if (jmp != null) jmp.Speed = v;
            if (combo_speed != null)
            {
                combo_speed.SelectionChanged -= combo_speed_SelectionChanged;
                if (v < 0.5) combo_speed.SelectedIndex = 0;
                else if (v >= 0.5 && v < 1) combo_speed.SelectedIndex = 1;
                else if (v == 1) combo_speed.SelectedIndex = 2;
                else if (v <= 2) combo_speed.SelectedIndex = 3;
                else if (v <= 3) combo_speed.SelectedIndex = 4;
                combo_speed.SelectionChanged += combo_speed_SelectionChanged;
            }
        }

        private void combo_speed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            slide_speed.ValueChanged -= slide_speed_ValueChanged;
            switch (combo_speed.SelectedIndex)
            {
                case 0: slide_speed.Value = 0.25; break;
                case 1: slide_speed.Value = 0.5; break;
                case 2: slide_speed.Value = 1; break;
                case 3: slide_speed.Value = 2; break;
                case 4: slide_speed.Value = 3; break;
            }
            if (jmp != null) jmp.Speed = slide_speed.Value;
            slide_speed.ValueChanged += slide_speed_ValueChanged;
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            hook.Unhook();
        }

        private void btn_prev_Click(object sender, RoutedEventArgs e)
        {
            jmp.JumpBwd();
            slider_time.Value -= jmp.JumpTimeMS;
            intelligentPlayingManager.JumpedBackward(jmp.JumpTimeMS);
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            jmp.JumpFwd();
            slider_time.Value += jmp.JumpTimeMS; ;
            intelligentPlayingManager.JumpedForward(jmp.JumpTimeMS);
        }

        private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (mainWindow.Height < mainWindow.MaxHeight) menu.Visibility = Visibility.Collapsed;
            else menu.Visibility = Visibility.Visible;

            if (mainWindow.Width < 570)
            {
                slide_speed.Visibility = Visibility.Collapsed;
                slide_volume.Visibility = Visibility.Collapsed;
            }
            else
            {
                slide_speed.Visibility = Visibility.Visible;
                slide_volume.Visibility = Visibility.Visible;
            }

            if (mainWindow.Width < 390)
            {
                combo_speed.Visibility = Visibility.Collapsed;
                btn_voloff.Visibility = Visibility.Collapsed;
            }
            else
            {
                combo_speed.Visibility = Visibility.Visible;
                btn_voloff.Visibility = Visibility.Visible;
            }
            if (mainWindow.Width < 300) btn_stop.Visibility = Visibility.Collapsed;
            else btn_stop.Visibility = Visibility.Visible;
        }

        private void menuitem_settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Show();
        }

        private void KeyDownHandler(object sender, Forms.KeyEventArgs e)
        {
            var hotkeys = config.HotKeys;
            Forms.Keys key = e.KeyCode;
            intelligentPlayingManager.KeyPressed(key);
            HotKeyHandler handler = config.GetHandler(key);
            if (handler != null) handler();
        }

        #region HotKey Functions

        private void PlayPause()
        {
            btn_start_Click(null, null);
        }

        private void StopPlaying()
        {
            btn_stop_Click(null, null);
        }

        private void JumpForward()
        {
            btn_next_Click(null, null);
        }

        private void JumpBackward()
        {
            btn_prev_Click(null, null);
        }

        private void SetPlaySpeedNormal()
        {
            slide_speed.Value = 1;
        }

        private void IncreasePlaySpeed()
        {
            slide_speed.Value += jmp.PlaySpeedChange;
        }

        private void DecreasePlaySpeed()
        {
            slide_speed.Value -= jmp.PlaySpeedChange;
        }

        #endregion

        private void mainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (JMediaPlayer.FileName != null && JMediaPlayer.FileName != String.Empty)
            {
                String file = JMediaPlayer.FileName.Split('.')[0];
                Clipboard.SetText(file);
                new Popup(String.Format("Title ({0}) copied to clipboard!", file), PopupDuration.SHORT, 300, Color.FromArgb(255, 0, 0, 100), Colors.White).Show();
            }
        }


    }
}
