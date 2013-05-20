using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Communications.Can;
using CanLighthouse.Models;
using System.Text.RegularExpressions;
using System.Timers;

namespace CanLighthouse
{
    /// <summary>
    /// Логика взаимодействия для SendWindow.xaml
    /// </summary>
    public partial class SendWindow : Window
    {
        public CanPort Port { get; private set; }
        
        private SendingModel model { get; set; }

        private string FramePattern = @"(?<descriptor>[0-9a-fA-F]{4})\s+(?<data>([\s-]??(?<databyte>[0-9a-fA-F]{2})[\s-]??){1,8})";
        private string DelayPattern = @"delay\s+(?<count>\d+(.\d+)?)(\s*(?<unit>(s|ms)))?";

        public SendWindow(CanPort WithPort)
        {
            this.Port = WithPort;

            model = new SendingModel();
            model.SelectedFrameModifed += EditingFrame_PropertyChanged;
            this.DataContext = model;

            InitializeComponent();

            this.Title = string.Format("Отправка в {0}", Port.Name);
            
            t = new Timer();
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);

            Dispatcher.BeginInvoke((Action<String>)(txt => SendEdit.Text = txt), Properties.Settings.Default.LastSend);
        }

        private FrameModel ParseFrameString(String str)
        {
            Regex regex = new Regex(FramePattern);
            Match match = regex.Match(str);
            if (!match.Success) return null;

            return new FrameModel()
            {
                Descriptor = Convert.ToUInt16(match.Groups["descriptor"].Value, 16),
                Data = match.Groups["databyte"].Captures.OfType<Capture>().Select(c => Convert.ToByte(c.Value, 16)).ToArray()
            };
        }

        private TimeSpan? ParseSendDelayFromString(String str)
        {
            Regex regex = new Regex(DelayPattern);
            Match match = regex.Match(str);
            if (!match.Success) return null;

            TimeSpan delay;
            switch(match.Groups["unit"].Value)
            {
                case "s": delay = TimeSpan.FromSeconds(Double.Parse(match.Groups["count"].Value, System.Globalization.CultureInfo.InvariantCulture));
                    break;
                default: delay = TimeSpan.FromMilliseconds(Double.Parse(match.Groups["count"].Value, System.Globalization.CultureInfo.InvariantCulture));
                    break;
            }
            return delay;

        }

        private void SendEdit_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var s = sender as TextBox;
            var line = s.GetLineText(s.GetLineIndexFromCharacterIndex(s.SelectionStart));
            model.SelectedFrame = ParseFrameString(line);
        }

        void EditingFrame_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var frame = sender as FrameModel;
            var LineIndex = SendEdit.GetLineIndexFromCharacterIndex(SendEdit.SelectionStart);
            var OriginalLine = SendEdit.GetLineText(LineIndex);

            Regex regex = new Regex(FramePattern);
            Match match = regex.Match(OriginalLine);
            if (!match.Success) return;

            List<int> Positions = new List<int>()
                {
                    match.Groups["descriptor"].Index,
                    match.Groups["descriptor"].Index + match.Groups["descriptor"].Length,
                    match.Groups["data"].Index,
                    match.Groups["data"].Index + match.Groups["data"].Length
                };

            string NewLine =
                OriginalLine.Substring(0, Positions[0]) + 
                frame.Descriptor.ToString("X4") +
                OriginalLine.Substring(Positions[1], Positions[2] - Positions[1]) +
                frame.HexString +
                OriginalLine.Substring(Positions[3]);
            
            var FirstOldCharIndex = SendEdit.GetCharacterIndexFromLineIndex(LineIndex);
            SendEdit.Text =
                SendEdit.Text.Substring(0, FirstOldCharIndex) +
                NewLine +
                SendEdit.Text.Substring(FirstOldCharIndex + OriginalLine.Length);

            Dispatcher.BeginInvoke((Action<int, int>)SendEdit.Select, FirstOldCharIndex, 0);
        }

        private void SendFrames(IList<FrameModel> Frames)
        {
            Port.Send(Frames.Select(fm => fm.GetFrame()).ToList());
        }
        private void DoSendOperation(SendOperation op)
        {
            SendFrames(op.PushingFrames);
            System.Threading.Thread.Sleep(op.DelayAfter);
        }

        private IList<SendOperation> ParseAll(string ParseText)
        {
            Regex regex = new Regex(FramePattern);
            var matches = regex.Matches(ParseText);

            var lines = ParseText.Split(new Char[] { '\n' }).ToList();
            var Operations = lines.Aggregate(new List<SendOperation>() { new SendOperation() },
                                (seed, l) =>
                                {
                                    FrameModel frame; TimeSpan? delay;
                                    if ((frame = ParseFrameString(l)) != null)
                                    {
                                        seed.Last().PushingFrames.Add(frame);
                                    }
                                    else if ((delay = ParseSendDelayFromString(l)) != null)
                                    {
                                        seed.Last().DelayAfter = delay.Value;
                                        seed.Add(new SendOperation());
                                    }
                                    return seed;
                                });
            return Operations;
        }


        private void SendOneCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = model.SelectedFrame != null;
        }
        private void SendOneCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SendFrames(new List<FrameModel>() { model.SelectedFrame });
        }

        private void SendAllCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void SendAllCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ops = ParseAll(SendEdit.Text);
            var t0 = new System.Threading.Tasks.Task(() => { });
            ops.Aggregate(t0,
                (seed, op) => seed.ContinueWith(t => DoSendOperation(op)));
            t0.Start();
        }

        Timer t;
        private void ToggleButton_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            t.Interval = Int32.Parse(TimerIntervalTextBox.Text);
            t.Enabled = (sender as System.Windows.Controls.Primitives.ToggleButton).IsChecked == true;
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action<object, IInputElement>)Commands.SendAll.Execute, null, this);
        }

        private void SendEdit_TextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.LastSend = (sender as TextBox).Text;
            Properties.Settings.Default.Save();
        }
    }



    public static class Commands
    {
        public static readonly RoutedUICommand SendOne = new RoutedUICommand("Отправить один", "SendOne", typeof(MainWindow), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.F3) }));
        public static readonly RoutedUICommand SendAll = new RoutedUICommand("Отправить все", "SendAll", typeof(MainWindow), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.F4) }));
    }
}
