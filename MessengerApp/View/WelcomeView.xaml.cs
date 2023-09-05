namespace MessengerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для WelcomeView.xaml
    /// </summary>
    public partial class WelcomeView : UserControl
    {
        public WelcomeView()
        {
            InitializeComponent();
        }

        private void ChangeLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayControl.ShowOverlay();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new SignUpView();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new SignInView();
        }
    }
}
