using System.Windows.Input;

namespace InperStudioControlLib.Command
{
    public static class CustomCommands
    {
        public static RoutedCommand ConfirmCommand { get; } = new RoutedCommand(nameof(ConfirmCommand), typeof(CustomCommands));
        public static RoutedCommand OtherCommand { get; } = new RoutedCommand(nameof(OtherCommand), typeof(CustomCommands));
        public static RoutedCommand CancleCommand { get; } = new RoutedCommand(nameof(CancleCommand), typeof(CustomCommands));
    }
}
