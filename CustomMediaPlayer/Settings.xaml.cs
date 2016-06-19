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
using System.Windows.Shapes;
using Forms = System.Windows.Forms;

namespace CustomMediaPlayer
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            LoadSettings();
        }

        private static List<Function> functions = new List<Function>();
        private Config config;

        private void LoadSettings()
        {
            config = Config.GetInstance;
            GlobalKeyboardHook.SignKeyDownHandler(KeyDownHandler);
            combo_functions.ItemsSource = functions;
            combo_keys.ItemsSource = Enum.GetValues(typeof(Forms.Keys));

            check_topmost.IsChecked = (bool)Boolean.Parse(config.GetConfig(ConfigKey.Topmost).ToString());
            SetTimeSetting(txt_jumptime, combo_unit, config.GetConfig(ConfigKey.JumpTime));
            SetTimeSetting(txt_pausetime, combo_pauseunit, config.GetConfig(ConfigKey.PauseTime));
            check_intelligent_playing.IsChecked = (bool)Boolean.Parse(config.GetConfig(ConfigKey.IntelligentPlaying).ToString());

            dgrid_hotkeys.ItemsSource = config.HotKeys;
        }

        private void SetTimeSetting(TextBox ValueTextBox, ComboBox UnitComboBox, object ConfigValue)
        {
            if (ConfigValue == null || ConfigValue.ToString() == String.Empty) return;
            int Value = int.Parse(ConfigValue.ToString());
            int sec = Value / 1000;
            if (Value % 1000 == 0)
            {
                ValueTextBox.Text = sec.ToString();
                UnitComboBox.SelectedIndex = 0;
            }
            else
            {
                ValueTextBox.Text = Value.ToString();
                UnitComboBox.SelectedIndex = 1;
            }
        }

        public static void SignFunction(Function Function)
        {
            functions.Add(Function);
        }

        private void KeyDownHandler(object sender, Forms.KeyEventArgs e)
        {
            Forms.Keys key = e.KeyCode;
            combo_keys.SelectedItem = key;
        }

        private void SaveJumpTime()
        {
            String txt = txt_jumptime.Text;
            if (txt != null && txt != String.Empty)
            {
                int jt = int.Parse(txt);
                switch (combo_unit.SelectedIndex)
                {
                    case 0: jt *= 1000; break;
                }
                config.SetConfig(ConfigKey.JumpTime, jt);
            }
        }

        private void SavePauseTime()
        {
            String txt = txt_pausetime.Text;
            if (txt != null && txt != String.Empty)
            {
                int pt = int.Parse(txt);
                switch (combo_pauseunit.SelectedIndex)
                {
                    case 0: pt *= 1000; break;
                }
                config.SetConfig(ConfigKey.PauseTime, pt);
            }
        }

        private void check_topmost_Checked(object sender, RoutedEventArgs e)
        {
            config.SetConfig(ConfigKey.Topmost, true);
        }

        private void check_topmost_Unchecked(object sender, RoutedEventArgs e)
        {
            config.SetConfig(ConfigKey.Topmost, false);
        }

        private void txt_jumptime_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveJumpTime();
        }

        private void combo_unit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveJumpTime();
        }  

        private void btn_add_hotkey_Click(object sender, RoutedEventArgs e)
        {
            if (combo_functions.SelectedIndex != -1 && combo_keys.SelectedIndex != -1)
            {
                HotKey hk = new HotKey((Forms.Keys)combo_keys.SelectedItem, functions[combo_functions.SelectedIndex]);
                if (!config.AddHotKeyHandler(hk)) return;
                dgrid_hotkeys.Items.Refresh();
            }
        }

        private void btn_delete_hotkey_Click(object sender, RoutedEventArgs e)
        {
            HotKey hk = (HotKey)dgrid_hotkeys.Items[dgrid_hotkeys.SelectedIndex];
            config.RemoveHotKeyHandler(hk);
            dgrid_hotkeys.Items.Refresh();
        }

        private void dgrid_hotkeys_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgrid_hotkeys.SelectedIndex == -1) btn_delete_hotkey.IsEnabled = false;
            else btn_delete_hotkey.IsEnabled = true;
        }

        private void combo_keys_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;   // Így az esemény lekezelődik, és nem adja tovább automatikusan a lenyomott billentyűt, különben a comboboxban név alapján keresne
        }

        private void check_intelligent_playing_Checked(object sender, RoutedEventArgs e)
        {
            group_intellientplay.IsEnabled = true;
            config.SetConfig(ConfigKey.IntelligentPlaying, true);
        }

        private void check_intelligent_playing_Unchecked(object sender, RoutedEventArgs e)
        {
            group_intellientplay.IsEnabled = false;
            config.SetConfig(ConfigKey.IntelligentPlaying, false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GlobalKeyboardHook.UnsignKeyDownHandler(KeyDownHandler);
        }

        private void txt_pausetime_TextChanged(object sender, TextChangedEventArgs e)
        {
            SavePauseTime();
        }

        private void combo_pauseunit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SavePauseTime();
        }

                  

    }
}
