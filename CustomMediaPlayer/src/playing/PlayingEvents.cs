using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CustomMediaPlayer
{
    interface PlayingEvents
    {
        void PlayingStarted();
        void PlayingPaused();
        void JumpedBackward(int MilliSecond);
        void JumpedForward(int MilliSecond);
        void KeyPressed(Keys Key);
    }
}
