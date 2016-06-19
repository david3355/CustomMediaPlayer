using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;

namespace CustomMediaPlayer
{
    class JMediaPlayer
    {
        private JMediaPlayer()
        {
            playing = false;
            changeRate = 300;
            jumpTime = 2000;
        }
        static JMediaPlayer() { mp = new MediaPlayer(); }

        private static JMediaPlayer jmp;
        private static MediaPlayer mp;
        private static string nowPlaying = String.Empty;
        private static string fileName;
        private EventHandler posChange, playChange;
        private DispatcherTimer timer;
        private int changeRate;
        private bool playing;
        private double prevVolume;
        private int jumpTime;
        private float speedChange = 0.1f;

        public static string NowPlaying
        {
            get { return nowPlaying; }
            set
            {
                nowPlaying = value;
                string[] tags = nowPlaying.Split('\\');
                if (tags.Length > 0) fileName = tags[tags.Length - 1];
                mp.Open(new Uri(nowPlaying, UriKind.Absolute));
            }
        }

        public static string FileName
        {
            get { return fileName; }
        }

        public TimeSpan Position
        {
            get { return mp.Position; }
            set { mp.Position = value; }
        }

        public double MediaLengthInMS
        {
            get { return mp.NaturalDuration.TimeSpan.TotalMilliseconds; }
        }

        public bool Playing
        {
            get { return playing; }
            set { playing = value; }
        }

        public double Volume
        {
            get { return mp.Volume; }
            set { mp.Volume = value; }
        }

        public double Speed
        {
            get { return mp.SpeedRatio; }
            set { mp.SpeedRatio = value; }
        }

        public bool HasVideo
        {
            get { return mp.HasVideo; }
        }

        public int JumpTimeMS
        {
            get { return jumpTime; }
            set { jumpTime = value; }
        }

        public float PlaySpeedChange
        {
            get { return speedChange; }
            set { speedChange = value; }
        }


        public static JMediaPlayer GetJMediaPlayer()
        {
            if (jmp == null) jmp = new JMediaPlayer();
            return jmp;
        }

        public void SetMediaEvents(EventHandler MediaEnded, EventHandler MediaOpened, EventHandler PositionChanged, EventHandler PlayChanged)
        {
            mp.MediaOpened += MediaOpened;
            mp.MediaEnded += MediaEnded;
            playChange = PlayChanged;
            posChange = PositionChanged;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(changeRate);
            timer.Tick += posChange;
        }

        public void Play()
        {
            if (nowPlaying == String.Empty) return;
            mp.Play();
            StartTimer();
            playing = true;
            playChange(this, new EventArgs());
        }

        public void Stop()
        {
            if (nowPlaying == String.Empty) return;
            mp.Stop();
            StopTimer();
            playing = false;
            playChange(this, new EventArgs());
        }

        public void Pause()
        {
            if (nowPlaying == String.Empty) return;
            mp.Pause();
            StopTimer();
            playing = false;
            playChange(this, new EventArgs());
        }

        public void Previous()
        {

        }

        public void Next()
        {

        }

        public void JumpFwd(double MSecond)
        {
            mp.Position = mp.Position.Add(TimeSpan.FromMilliseconds(MSecond));
        }

        public void JumpBwd(double MSecond)
        {
            mp.Position = mp.Position.Subtract(TimeSpan.FromMilliseconds(MSecond));
        }

        public void JumpFwd()
        {
            JumpFwd(jumpTime);
        }

        public void JumpBwd()
        {
            JumpBwd(jumpTime);
        }

        public void SetPosition(int Value)
        {
            mp.Position = TimeSpan.FromSeconds(Value);
        }

        public void IncreasePlaySpeed()
        {
            Speed += speedChange;
        }

        public void DecreasePlaySpeed()
        {
            Speed -= speedChange;
        }

        public void SetPlaySpeedNormal()
        {
            Speed = 1;
        }

        public void StopTimer()
        {
            timer.Stop();
        }

        public void StartTimer()
        {
            timer.Start();
        }

        public bool Mute()
        {
            if (Volume != 0)
            {
                prevVolume = Volume;
                Volume = 0;
                return true;
            }
            Volume = prevVolume;
            return false;
        }
    }
}
