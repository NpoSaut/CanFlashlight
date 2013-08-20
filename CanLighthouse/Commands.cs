using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CanLighthouse
{
    public static class Commands
    {
        public static readonly RoutedUICommand ToggleAutoScroll = new RoutedUICommand("Автопрокрутка", "ToggleAutoScroll", typeof(MainWindow));
        public static readonly RoutedUICommand ToggleBeep = new RoutedUICommand("Звуковой сигнал", "ToggleBeep", typeof(MainWindow));
        public static readonly RoutedUICommand Clear = new RoutedUICommand("Отчистить", "Clear", typeof(MainWindow));


        public static readonly RoutedUICommand SendOne = new RoutedUICommand("Отправить один", "SendOne", typeof(MainWindow), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.F3) }));
        public static readonly RoutedUICommand SendAll = new RoutedUICommand("Отправить все", "SendAll", typeof(MainWindow), new InputGestureCollection(new List<InputGesture>() { new KeyGesture(Key.F4) }));
    }
}
