using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CustomMediaPlayer
{
    public class HotKey
    {
        public HotKey(Keys Key, Function KeyHandler)
        {
            this.Key = Key;
            this.KeyHandler = KeyHandler;
        }

        public Keys Key { get; set; }
        public Function KeyHandler { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is HotKey)
            {
                HotKey hk = obj as HotKey;
                return this.Key == hk.Key && this.KeyHandler.Equals(hk.KeyHandler);
            }
            return false;
        }

        public override int GetHashCode() { return base.GetHashCode(); }
    }
}
