using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;

namespace PopUp
{
    class Popup
    {
        public Popup(String Text, TimeSpan Duration, int PopupWindowWidth, Color BackgroundColor, Color ForegroundColor)
        {
            this.text = Text;
            this.windowWidth = PopupWindowWidth;
            this.bgrColor = BackgroundColor;
            this.fgrColor = ForegroundColor;
            this.durationTime = Duration;
            appearTime = new Duration(new TimeSpan(0, 0, 0, 0, 300));
        }

        public Popup(String Text, PopupDuration Duration, int PopupWindowWidth, Color BackgroundColor, Color ForegroundColor) :
            this(Text, GetDurationTime(Duration), PopupWindowWidth, BackgroundColor, ForegroundColor)
        {

        }

        public Popup(String Text, PopupDuration Duration)
            : this(Text, Duration, 400)
        {

        }

        public Popup(String Text, PopupDuration Duration, int PopupWindowWidth)
            : this(Text, Duration, PopupWindowWidth, DEF_BACKGROUND_COLOR, DEF_FOREGROUND_COLOR)
        {
        }


        private String text;
        private int windowWidth;
        private Color bgrColor;
        private Color fgrColor;
        private TimeSpan durationTime;
        private PopupWindow window;
        private readonly Duration appearTime;
        private DispatcherTimer showTimer;

        private const int LINE_HEIGHT = 16;
        private const int POPUP_PADDING = 20;
        private static Color DEF_BACKGROUND_COLOR = Color.FromArgb(255, 163, 213, 163);
        private static Color DEF_FOREGROUND_COLOR = Color.FromArgb(255, 70, 70, 70);

        public String PopupText
        {
            get { return text; }
            set { text = value; }
        }

        public void Show()
        {
            window = new PopupWindow();
            SetDisplay();
            Animate();
        }

        private static TimeSpan GetDurationTime(PopupDuration Duration)
        {
            return new TimeSpan(0, 0, 0, 0, (int)Duration);
        }

        private void SetDisplay()
        {
            window.Width = windowWidth;
            window.popup_text.Foreground = new SolidColorBrush(fgrColor);
            window.popup_background.Background = new SolidColorBrush(bgrColor);
        }

        private void SetText(String Text)
        {
            window.popup_text.Text = Text;
            int lines = window.popup_text.LineCount;
            window.popup_window.Height = POPUP_PADDING + LINE_HEIGHT * lines;
        }

        private void Animate()
        {
            window.Show();
            SetText(text);
            DoubleAnimation appear = new DoubleAnimation(0, 1, appearTime);
            appear.Completed += PopupWindow_Appeared;
            window.popup_window.BeginAnimation(Window.OpacityProperty, appear);
        }

        private void FadeWindow(object sender, EventArgs e)
        {
            showTimer.Stop();
            showTimer = null;
            DoubleAnimation disappear = new DoubleAnimation(1, 0, appearTime);
            disappear.Completed += PopupWindow_Disappeared;
            window.popup_window.BeginAnimation(Window.OpacityProperty, disappear);
        }

        private void PopupWindow_Appeared(object sender, EventArgs e)
        {
            showTimer = new DispatcherTimer();
            showTimer.Interval = durationTime;
            showTimer.Tick += FadeWindow;
            showTimer.Start();
        }

        private void PopupWindow_Disappeared(object sender, EventArgs e)
        {
            window.Close();
            window = null;
        }
    }
}
