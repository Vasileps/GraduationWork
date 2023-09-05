using MessengerApp.Connection.Schemas;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MessengerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для Registrarion.xaml
    /// </summary>
    public partial class SignUpView : UserControl
    {
        private Task signingUp;
        private bool signedUp = false;

        public SignUpView()
        {
            InitializeComponent();
            UsernameBox.Focus();
        }

        private void UsernameBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: MailBox.Focus(); break;
                case Key.Escape: Focus(); break;
            }
        }

        private void MailBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter: PasswordBox.Focus(); break;
                case Key.Escape: UsernameBox.Focus(); break;
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape: MailBox.Focus(); break;
                case Key.Enter: TrySignUp(); break;
            }
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            TrySignUp();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new SignInView();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new WelcomeView();
        }

        private async void TrySignUp()
        {
            if (signedUp) return;
            if (signingUp is not null) await signingUp;
            signingUp = SignUp();
        }

        private async Task SignUp()
        {
            if (signedUp) return;

            string
               username = UsernameBox.Text,
               password = PasswordBox.Password,
               mail = MailBox.Text;

            var schema = new SignUpViaMailSchema(username, mail, password);
            var response = await ConnectionHub.Http.SignUpViaMailAsync(schema);

            if (response.Success)
            {
                signedUp = true;
                Application.Current.MainWindow.Content = new ConfirmMailView(mail, password);
            }

            else MessageBox.Show(response.ErrorMessage);
        }
    }
}
