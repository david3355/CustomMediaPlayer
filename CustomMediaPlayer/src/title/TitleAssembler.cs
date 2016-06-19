using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomMediaPlayer
{
    class TitleAssembler
    {
        public TitleAssembler(TitleSetter TitleSetter)
        {
            this.setTitle = TitleSetter;
            titleTemplate = "{0}";
        }

        private TitleSetter setTitle;
        private String titleTemplate;
        private int playPercent;

        public void PlayStarted()
        {
            titleTemplate = "\u25b6 {0}% {1}";
            setTitle(String.Format(titleTemplate, playPercent, JMediaPlayer.FileName));
        }

        public void PlayPaused()
        {
            titleTemplate = "{0}% {1}";
            setTitle(String.Format(titleTemplate, playPercent, JMediaPlayer.FileName));
        }

        public void PlayerTimeChanged(TimeSpan CurrentTime, TimeSpan Duration)
        {
            double duration = Duration.TotalMilliseconds;
            double currentTime = CurrentTime.TotalMilliseconds;
            double playRate = currentTime / duration;
            playPercent = (int)Math.Round(playRate * 100, 0);
            setTitle(String.Format(titleTemplate, playPercent, JMediaPlayer.FileName));
        }
    }
}
