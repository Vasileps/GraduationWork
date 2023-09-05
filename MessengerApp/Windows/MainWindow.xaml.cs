using System;

namespace MessengerApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                if (ConfigManager.TryGet("MainWindowHeight", out string height)) Height = Convert.ToDouble(height);
                if (ConfigManager.TryGet("MainWindowWidth", out string width)) Width = Convert.ToDouble(width);
            }
            catch { }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ConfigManager.Set("MainWindowHeight", Height.ToString());
            ConfigManager.Set("MainWindowWidth", Width.ToString());
        }
    }
}
