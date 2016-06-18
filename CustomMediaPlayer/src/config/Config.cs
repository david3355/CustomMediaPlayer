using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CustomMediaPlayer
{
    class Config
    {
        private Config(Dictionary<string, Function> Functions)
        {
            conf = new Dictionary<ConfigKey, object>();
            hotkeys = new List<HotKey>();
            configSaveManager = new ConfigSaveManager(CONFIGFILE, Functions);
        }

        private static Config self;

        public static Config GetInstance
        {
            get
            {
                if (self == null) throw new Exception("You must call GetInstanceInit(Dictionary<string,Function>) method first!");
                else return self;
            }
        }

        public static Config GetInstanceInit(Dictionary<string, Function> Functions)
        {
            self = new Config(Functions);
            return self;
        }

        private Dictionary<ConfigKey, object> conf;
        private List<HotKey> hotkeys;

        private event ConfigChange handlers;
        private ConfigSaveManager configSaveManager;
        private const String CONFIGFILE = "config.xml";

        public List<HotKey> HotKeys { get { return hotkeys; } }

        public bool LoadConfig()
        {
            ConfigKey[] keys = (ConfigKey[])Enum.GetValues(typeof(ConfigKey));
            foreach (ConfigKey key in keys)
            {
                String value = configSaveManager.GetConfig(key);
                if (value != null && value != String.Empty) SetConfig(key, value, false);
            }
            List<HotKey> hotkeys = configSaveManager.GetHotKeys();
            foreach (HotKey hk in hotkeys)
            {
                AddHotKeyHandler(hk, false);
            }

            return conf.Count != 0;
        }

        /// <summary>
        /// Saves the given configuration automatically
        /// </summary>
        public void SetConfig(ConfigKey Key, object Value)
        {
            SetConfig(Key, Value, true);
        }

        public void SetConfig(ConfigKey Key, object Value, bool Save)
        {
            if (!conf.ContainsKey(Key))
            {
                conf.Add(Key, Value);
            }
            else conf[Key] = Value;
            if (Save) configSaveManager.SaveConfig(Key, Value.ToString());
            if (handlers != null) handlers(Key);
        }

        public object GetConfig(ConfigKey Key)
        {
            if (conf.ContainsKey(Key)) return conf[Key];
            return null;
        }

        public void SignUp(ConfigChange Handler)
        {
            handlers += Handler;
        }

        public void SignDown(ConfigChange Handler)
        {
            handlers -= Handler;
        }

        /// <summary>
        /// Saves the given HotKey automatically
        /// </summary>
        public bool AddHotKeyHandler(HotKey HotKeyHandler)
        {
            return AddHotKeyHandler(HotKeyHandler, true);
        }

        public bool AddHotKeyHandler(HotKey HotKeyHandler, bool Save)
        {
            if (!hotkeys.Contains(HotKeyHandler))
            {
                hotkeys.Add(HotKeyHandler);
                if (Save) configSaveManager.SaveHotKey(HotKeyHandler);
                return true;
            }
            return false;
        }

        public bool RemoveHotKeyHandler(HotKey HotKeyHandler)
        {
            if (hotkeys.Contains(HotKeyHandler))
            {
                hotkeys.Remove(HotKeyHandler);
                configSaveManager.RemoveHotKey(HotKeyHandler);
                return true;
            }
            return false;
        }

        public HotKeyHandler GetHandler(Keys Key)
        {
            foreach (HotKey hk in hotkeys)
            {
                if (hk.Key == Key) return hk.KeyHandler.KeyHandler;
            }
            return null;
        }
    }
}
