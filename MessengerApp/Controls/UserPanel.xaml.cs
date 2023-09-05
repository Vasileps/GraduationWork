using MessengerApp.Connection.Schemas;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserPanel.xaml
    /// </summary>
    public partial class UserPanel : UserControl
    {
        public readonly SearchedUser User;

        public UserPanel(SearchedUser user)
        {
            InitializeComponent();
            User = user;
            UsernameBlock.Text = user.Username;

            LoadImage();
        }

        private async void LoadImage()
        {
            var imageResponse = await ConnectionHub.Http.GetUserProfileImageAsync(new(User.ID));
            if (imageResponse.Success)
            {
                var image = imageResponse.Data!;
                Image.Source = image;
            }
        }

        private async void CreateChatButton_Click(object sender, RoutedEventArgs e)
        {
            CreateChatButton.IsEnabled = false;
            var response = await ConnectionHub.Http.CreatePersonalChatAsync(new(User.ID));
            if (response.Success) CreateChatButton.Content = "\uE10B";
            else
            {
                CreateChatButton.IsEnabled = true;
                MessageBox.Show(response.ErrorMessage);
            }
        }
    }
}
