using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomMediaPlayer
{
    public class Function
    {
        public Function(HotKeyHandler Handler, string FunctionText)
        {
            this.KeyHandler = Handler;
            this.FunctionText = FunctionText;
        }

        public HotKeyHandler KeyHandler { get; set; }
        public string FunctionText { get; set; }
        public String FunctionName
        {
            get { return this.KeyHandler.Method.Name; }
        }

        public override string ToString()
        {
            return FunctionText;
        }

        public override bool Equals(object obj)
        {
            if (obj is Function)
            {
                Function f = obj as Function;
                return KeyHandler.Equals(f.KeyHandler);
            }
            return false;
        }

        public override int GetHashCode() { return base.GetHashCode(); }
    }
}
