using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Forms;

namespace CustomMediaPlayer
{
    class IntelligentPlayingManager : PlayingEvents
    {
        public IntelligentPlayingManager()
        {
            mediaPlayer = JMediaPlayer.GetJMediaPlayer();
            config = Config.GetInstance;
            pauseTimer = new DispatcherTimer();
            pauseTimer.Tick += PauseTimeElapsed;
            pauseTimer.Interval = new TimeSpan(0, 0, 0, 0, pauseTime);
        }

        private JMediaPlayer mediaPlayer;
        private Config config;
        private DispatcherTimer pauseTimer;
        private int pauseTime = 1000;

        private static Keys[] stopKeys = { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.Insert, Keys.End, Keys.Down, Keys.PageDown, Keys.Left, Keys.Clear, Keys.Right, Keys.Home, Keys.Up, Keys.Prior, Keys.Q, Keys.W, Keys.E, Keys.R, Keys.T, Keys.Z, Keys.U, Keys.I, Keys.O, Keys.P, Keys.Oem4, Keys.Oem6, Keys.A, Keys.S, Keys.D, Keys.F, Keys.G, Keys.H, Keys.J, Keys.K, Keys.L, Keys.OemSemicolon, Keys.OemQuotes, Keys.OemPipe, Keys.Oem102, Keys.Y, Keys.X, Keys.C, Keys.V, Keys.B, Keys.N, Keys.M, Keys.Oemtilde, Keys.Oem2, Keys.Oemplus, Keys.Oemcomma, Keys.OemPeriod, Keys.OemMinus, Keys.Delete, Keys.Back, Keys.Enter, Keys.Home, Keys.End, Keys.Enter, Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.Space, Keys.Divide, Keys.Multiply};

        private void PauseTimeElapsed(object sender, EventArgs e)
        {
            mediaPlayer.Play();
            pauseTimer.Stop();
        }

        private void RestartPauseTimer()
        {
            pauseTimer.Stop();
            pauseTimer.Start();
        }

        public void PlayingStarted()
        {
        }

        public void PlayingPaused()
        {
        }

        public void JumpedBackward(int MilliSecond)
        {
        }

        public void JumpedForward(int MilliSecond)
        {
        }

        public void KeyPressed(System.Windows.Forms.Keys Key)
        {
            if (stopKeys.Contains<Keys>(Key))
            {
                mediaPlayer.Pause();
                RestartPauseTimer();
            }
        }
    }
}
