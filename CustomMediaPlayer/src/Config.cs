using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CustomMediaPlayer
{
    enum ConfigKey { JumpTime, Topmost, LastOpened }
    delegate void ConfigChange(ConfigKey Key);
    public delegate void HotKeyHandler();

    class Config
    {
        static Config()
        {
            conf = new Dictionary<ConfigKey, object>();
        }

        private static Dictionary<ConfigKey, object> conf;
        private static event ConfigChange handlers;
        private static List<HotKey> hotkeys = new List<HotKey>();

        public static List<HotKey> HotKeys { get { return hotkeys; } }

        public static void SetConfig(ConfigKey Key, object Value)
        {
            if (!conf.ContainsKey(Key))
            {
                conf.Add(Key, Value);
                if (handlers != null) handlers(Key);
            }
        }

        public static object GetConfig(ConfigKey Key)
        {
            if (conf.ContainsKey(Key)) return conf[Key];
            return null;
        }

        public static void SignUp(ConfigChange Handler)
        {
            handlers += Handler;
        }

        public static void SignDown(ConfigChange Handler)
        {
            handlers -= Handler;
        }

        public static bool AddHotKeyHandler(HotKey HotKeyHandler)
        {
            if (!hotkeys.Contains(HotKeyHandler))
            {
                hotkeys.Add(HotKeyHandler);
                return true;
            }
            return false;
        }

        public static bool RemoveHotKeyHandler(HotKey HotKeyHandler)
        {
            if (hotkeys.Contains(HotKeyHandler))
            {
                hotkeys.Remove(HotKeyHandler);
                return true;
            }
            return false;
        }

        public static HotKeyHandler GetHandler(Keys Key)
        {
            foreach (HotKey hk in hotkeys)
            {
                if (hk.Key == Key) return hk.KeyHandler.KeyHandler;
            }
            return null;
        }
    }
}
