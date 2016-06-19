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
            int jt = int.Parse(config.GetConfig(ConfigKey.JumpTime).ToString());
            int sec = jt / 1000;
            if (jt % 1000 == 0)
            {
                txt_jumptime.Text = sec.ToString();
                combo_unit.SelectedIndex = 0;
            }
            else
            {
                txt_jumptime.Text = jt.ToString();
                combo_unit.SelectedIndex = 1;
            }
            dgrid_hotkeys.ItemsSource = config.HotKeys;
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

        private void check_topmost_Checked(object sender, RoutedEventArgs e)
        {
            config.SetConfig(ConfigKey.Topmost, true);
        }

        private void check_topmost_Unchecked(object sender, RoutedEventArgs e)
        {
            config.SetConfig(ConfigKey.Topmost, false);
        }

        private void btn_jtSave_Click(object sender, RoutedEventArgs e)
        {
            int jt = int.Parse(txt_jumptime.Text);
            switch (combo_unit.SelectedIndex)
            {
                case 0: jt *= 1000; break;
            }
            config.SetConfig(ConfigKey.JumpTime, jt);
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

        }

        private void check_intelligent_playing_Unchecked(object sender, RoutedEventArgs e)
        {

        }

    }
}
