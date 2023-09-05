using MessengerApp.Connection.Schemas;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MessengerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для ConfirmMailView.xaml
    /// </summary>
    public partial class ConfirmMailView : UserControl
    {
        private bool confirmed;
        private Task confirming;

        private readonly string mail;
        private readonly string password;

        public ConfirmMailView(string mail, string password)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(mail))
                throw new ArgumentNullException(nameof(mail));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            this.mail = mail;
            this.password = password;

            UpdateMessage();

            App.LanguageChanged += LanguageChangeHandler;
        }

        private void LanguageChangeHandler(object sender, EventArgs e) => UpdateMessage();

        private void UpdateMessage()
        {
            var builder = new StringBuilder();
            builder.AppendLine(Application.Current.Resources["lcConfirmEmailSignUp"].ToString());
            builder.Replace("{mail}", mail);

            MessageBlock.Text = builder.ToString();
        }

        private void CodeBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape: Focus(); break;
                case Key.Enter: TryConfirm(); break;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            TryConfirm();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Content = new WelcomeView();
        }

        private async void TryConfirm()
        {
            if (confirmed) return;
            if (confirming is not null) await confirming;
            confirming = Confirm();
        }

        private async Task Confirm()
        {
            if (confirmed) return;

            string code = CodeBox.Text;

            var confirmSchema = new ConfirmAccountViaMailSchema(mail, code);
            var confirnResponse = await ConnectionHub.Http.ConfirmAccountViaMailAsync(confirmSchema);

            //Add ErrorParsing
            if (!confirnResponse.Success) MessageBox.Show(confirnResponse.ErrorMessage);
            else
            {
                confirmed = true;
                var signInResponse = await ConnectionHub.Http.SignInViaMailAsync(new(mail, password));

                if (signInResponse.Success) Application.Current.MainWindow.Content = new MessengerView();                
                else Application.Current.MainWindow.Content = new WelcomeView();
            }
        }
    }
}
