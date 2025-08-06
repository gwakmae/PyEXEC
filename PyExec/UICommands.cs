// PyExec/UICommands.cs

using System.Windows.Input;

namespace PyExec
{
    public static class UICommands
    {
        // 정적 커맨드 선언
        public static readonly RoutedUICommand SaveTemplateCommand = new RoutedUICommand("Save Template", "SaveTemplateCommand", typeof(MainWindow));
        public static readonly RoutedUICommand LoadTemplateCommand = new RoutedUICommand("Load Template", "LoadTemplateCommand", typeof(MainWindow));
    }
}