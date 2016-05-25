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

        private void Init()
        {
            changeAllowed = true;
            jmp = JMediaPlayer.GetJMediaPlayer();
            Config.SignUp(ConfigChanged);
            jmp.SetMediaEvents(MediaEnded, MediaOpened, PositionChanged, PlayChanged);
            if (JMediaPlayer.NowPlaying != String.Empty)
            {
                AddToRecentlyOpen(JMediaPlayer.NowPlaying);
                btn_start.IsEnabled = true;
                btn_start_Click(null, null);
            }
            ofd = new OpenFileDialog();
            slide_volume.Value = 50;
            hook = new GlobalKeyboardHook(KeyDownHandler);
            SetConfig();
            hook.Hook();
        }

        private void SetConfig()
        {
            Config.SetConfig(ConfigKey.JumpTime, 2000);
            Config.SetConfig(ConfigKey.Topmost, false);
            Function f1 = new Function(PlayPause, "Lejátszás gomb");
            Function f2 = new Function(JumpBackward, "Ugrás hátrafelé");
            Settings.SignFunction(f1);
            Settings.SignFunction(new Function(StopPlaying, "Stop gomb"));
            Settings.SignFunction(f2);
            Settings.SignFunction(new Function(JumpForward, "Ugrás előre"));
            Settings.SignFunction(new Function(IncreasePlaySpeed, "Lejátszási sebesség növelése"));
            Settings.SignFunction(new Function(DecreasePlaySpeed, "Lejátszási sebesség csökkentése"));
            Settings.SignFunction(new Function(SetPlaySpeedNormal, "Normál lejátszási sebesség"));

            Config.AddHotKeyHandler(new HotKey(Forms.Keys.LControlKey, f1));
            Config.AddHotKeyHandler(new HotKey(Forms.Keys.RControlKey, f2));
        }

        public void btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (JMediaPlayer.NowPlaying == String.Empty) return;
            if (!jmp.Playing) jmp.Play();
            else jmp.Pause();
        }

        private void SetTime(Label TimeLabel, TimeSpan Value)
        {
            string hour = Value.Hours < 10 ? "0" + Value.Hours.ToString() : Value.Hours.ToString();
            string min = Value.Minutes < 10 ? "0" + Value.Minutes.ToString() : Value.Minutes.ToString();
            string sec = Value.Seconds < 10 ? "0" + Value.Seconds.ToString() : Value.Seconds.ToString();
            TimeLabel.Content = hour + ':' + min + ':' + sec;
        }

        private void ConfigChanged(ConfigKey Key)
        {
            switch (Key)
            {
                case ConfigKey.JumpTime:
                jmp.JumpTimeMS = Convert.ToInt32(Config.GetConfig(ConfigKey.JumpTime)); break;
                case ConfigKey.Topmost:
                mainWindow.Topmost = Convert.ToBoolean(Config.GetConfig(ConfigKey.Topmost)); break;
                case ConfigKey.LastOpened:
                AddToRecentlyOpen(Config.GetConfig(ConfigKey.LastOpened).ToString()); break;
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
        }

        private void MediaEnded(object sender, EventArgs e)
        {
            jmp.StopTimer();
            jmp.Stop();
            jmp.Playing = false;
            slider_time.Value = 0;
            label_time.Content = "00:00:00";
            PlayChanged(null, null);
        }

        private void PositionChanged(object sender, EventArgs e)
        {
            if(changeAllowed) slider_time.Value = jmp.Position.TotalMilliseconds;
        }

        private void PlayChanged(object sender, EventArgs e)
        {
            if (!jmp.Playing) // Pause playing
            {
                img_play.Source = new BitmapImage(new Uri(@"/img/play2.png", UriKind.Relative));
                this.Title = JMediaPlayer.FileName;
            }
            else // Start playing
            {
                img_play.Source = new BitmapImage(new Uri(@"/img/pause.png", UriKind.Relative));
                this.Title = '\u25b6' + " " + JMediaPlayer.FileName;
            }
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool? ok = ofd.ShowDialog();
            if (ok == true)
            {
                JMediaPlayer.NowPlaying = ofd.FileName;
                btn_start.IsEnabled = true;
                mainWindow.Title = JMediaPlayer.FileName;
                AddToRecentlyOpen(JMediaPlayer.NowPlaying);
            }
        }

        private void AddToRecentlyOpen(string FileName)
        {
            if (!menu_recently_opened.Items.Contains(FileName))
            {
                MenuItem item = new MenuItem();
                item.Header = FileName;
                item.Icon = new Image()
                {
                    Source = new BitmapImage(new Uri(@"/img/play2.png", UriKind.Relative)),
                    Width = 20
                };
                item.Click += RecentlyOpenedClick;
                menu_recently_opened.Items.Add(item);
            }
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
        }

        private void btn_next_Click(object sender, RoutedEventArgs e)
        {
            jmp.JumpFwd();
            slider_time.Value += jmp.JumpTimeMS; ;
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

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Settings settingsWindow = new Settings();
            settingsWindow.Show();
        }

        private void KeyDownHandler(object sender, Forms.KeyEventArgs e)
        {
            var hotkeys = Config.HotKeys;
            Forms.Keys key = e.KeyCode;
            HotKeyHandler handler = Config.GetHandler(key);
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

        
    }    
}
