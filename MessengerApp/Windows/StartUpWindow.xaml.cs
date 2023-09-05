using MessengerApp.Views;
using System.Threading.Tasks;

namespace MessengerApp.Windows
{
    /// <summary>
    /// Логика взаимодействия для StartUpWindow.xaml
    /// </summary>
    public partial class StartUpWindow : Window
    {
        public StartUpWindow()
        {
            InitializeComponent();
            Loaded += StartUp;
        }

        public async void StartUp(object sender, RoutedEventArgs e)
        {
            var minDelay = Task.Delay(1000);

        Reconnect:
            var response = await ConnectionHub.Http.CheckIfSignedInAsync();

            UserControl nextView;

            if (response.Success) nextView = new MessengerView();
            else if (response.ErrorMessage is not "ConnectionError") nextView = new WelcomeView();
            else
            {
                await Task.Delay(5000);
                goto Reconnect;
            }

            await minDelay;

            Application.Current.MainWindow = new MainWindow();
            Application.Current.MainWindow.Content = nextView;
            Application.Current.MainWindow.Show();

            Close();
        }
    }
}
