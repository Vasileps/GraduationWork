using MessengerApp.Connection.Schemas;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MessengerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginView.xaml
    /// </summary>
    public partial class SignInView : UserControl
    {
        private Task signingIn;
        private bool signedIn = false;

        public SignInView()
        {
            InitializeComponent();
            MailBox.Focus();
        }

        private void MailBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: PasswordBox.Focus(); break;
                case Key.Escape: Focus(); break;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape: MailBox.Focus(); break;
                case Key.Enter: TrySignIn(); break;
            }
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            TrySignIn();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new SignUpView();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new WelcomeView();
        }

        private async void TrySignIn()
        {
            if (signedIn) return;
            if (signingIn is not null) await signingIn;
            signingIn = SignIn();
        }

        private async Task SignIn()
        {
            if (signedIn) return;

            string
                login = MailBox.Text,
                password = PasswordBox.Password;

            var schema = new SignInViaMailSchema(login, password);
            var response = await ConnectionHub.Http.SignInViaMailAsync(schema);

            if (response.Success)
            {
                Application.Current.MainWindow.Content = new MessengerView();
                signedIn = true;
            }
            else MessageBox.Show(response.ErrorMessage);
        }
    }
}
