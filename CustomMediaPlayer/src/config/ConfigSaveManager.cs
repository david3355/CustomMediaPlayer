using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace CustomMediaPlayer
{
    class ConfigSaveManager
    {
        public ConfigSaveManager(string FilePath, Dictionary<string, Function> Functions)
        {
            configPath = AppDomain.CurrentDomain.BaseDirectory + FilePath;
            functions = Functions;
            document = new XmlDocument();
            bool documentLoaded = LoadXMLDocument();
            if (!documentLoaded) CreateXmlDatabase();
        }

        private string configPath;
        private XmlDocument document;

        private const string TAG_ROOT = "config";
        private const string TAG_HOTKEYS = "hotkeys";
        private const string TAG_HOTKEY = "hotkey";
        private const string TAG_HISTORY = "history";
        private const string TAG_KEY = "key";
        private const string TAG_FUNCTION = "function";

        private Dictionary<string, Function> functions;       

        public bool LoadXMLDocument()
        {
            try
            {
                if (!File.Exists(configPath) || File.ReadAllText(configPath, Encoding.Default) == String.Empty) return false;
                document.Load(configPath);
                return true;
            }
            catch { return false; }
        }

        public void CreateXmlDatabase()
        {
            if (File.Exists(configPath) && File.ReadAllText(configPath, Encoding.Default) != String.Empty) return;
            XmlElement root = document.CreateElement(TAG_ROOT);
            XmlElement hotkeys = document.CreateElement(TAG_HOTKEYS);
            XmlElement history = document.CreateElement(TAG_HISTORY);
            root.AppendChild(hotkeys);
            root.AppendChild(history);
            document.AppendChild(root);
            document.Save(configPath);
        }

        private void CreateOrModifyConfig(String Tag, String Value)
        {
            XmlElement root = document.DocumentElement;
            XmlNodeList result = root.GetElementsByTagName(Tag);
            XmlElement conf = null;
            if (result.Count > 0)
            {
                conf = result[0] as XmlElement;
            }
            else
            {
                conf = document.CreateElement(Tag);
            }
            conf.InnerText = Value;
            root.AppendChild(conf);
            document.Save(configPath);
        }


        public void SaveConfig(ConfigKey Key, string Value)
        {
            String configname = Enum.GetName(typeof(ConfigKey), Key);
            CreateOrModifyConfig(configname, Value);
        }

        public string GetConfig(ConfigKey Key)
        {
            String configname = Enum.GetName(typeof(ConfigKey), Key);
            XmlNodeList result = document.GetElementsByTagName(configname);
            if (result.Count > 0) return result[0].InnerText;
            else return String.Empty;
        }

        public void SaveHotKey(HotKey HotKeyHandler)
        {
            XmlElement hotkeys = document.GetElementsByTagName(TAG_HOTKEYS)[0] as XmlElement;
            XmlElement hotkey = document.CreateElement(TAG_HOTKEY);
            XmlElement key = document.CreateElement(TAG_KEY);
            XmlElement function = document.CreateElement(TAG_FUNCTION);
            key.InnerText = HotKeyHandler.Key.ToString();
            function.InnerText = HotKeyHandler.KeyHandler.KeyHandler.Method.Name;
            hotkey.AppendChild(key);
            hotkey.AppendChild(function);
            hotkeys.AppendChild(hotkey);
            document.Save(configPath);
        }

        public void RemoveHotKey(HotKey HotKeyHandler)
        {
            XmlNodeList hotkeys = document.GetElementsByTagName(TAG_HOTKEYS)[0].ChildNodes;
            foreach (XmlElement hotkey in hotkeys)
            {
                Keys key = (Keys)Enum.Parse(typeof(Keys), hotkey.GetElementsByTagName(TAG_KEY)[0].InnerText);
                Function function = functions[hotkey.GetElementsByTagName(TAG_FUNCTION)[0].InnerText];
                if(key.Equals(HotKeyHandler.Key) && function.FunctionName.Equals(HotKeyHandler.KeyHandler.FunctionName))
                {
                    hotkey.ParentNode.RemoveChild(hotkey);
                }
            }
            document.Save(configPath);
        }

        public List<HotKey> GetHotKeys()
        {
            XmlNodeList hotkeys = document.GetElementsByTagName(TAG_HOTKEYS)[0].ChildNodes;
            List<HotKey> hotkeyList = new List<HotKey>();
            foreach (XmlElement hotkey in hotkeys)
            {
                Keys key = (Keys)Enum.Parse(typeof(Keys), hotkey.GetElementsByTagName(TAG_KEY)[0].InnerText);
                Function function = functions[hotkey.GetElementsByTagName(TAG_FUNCTION)[0].InnerText];
                HotKey hk = new HotKey(key, function);
                hotkeyList.Add(hk);
            }
            return hotkeyList;
        }

    }
}
