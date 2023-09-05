using MessengerApp.Connection.Schemas;
using Microsoft.Win32;
using System.IO;
using static MessengerApp.Controls.UserProfileControls.ProfileInfoChangePanel;

namespace MessengerApp.Controls.UserProfileControls
{
    /// <summary>
    /// Логика взаимодействия для UserProfile.xaml
    /// </summary>
    public partial class UserProfile : UserControl
    {
        public UserProfile()
        {
            InitializeComponent();

            UsernameBlock.Text = ConnectionHub.UserInfo.Username;
            UsernameButtonBlock.Text = ConnectionHub.UserInfo.Username;
            MailButtonBlock.Text = ConnectionHub.UserInfo.Mail;

            ConnectionHub.Notifications.ProfileUpdated += InfoChangedHandler;
            ConnectionHub.Notifications.UserImageUpdated += ImageUpdateHandler;

            LoadImage();
        }

        private void ImageUpdateHandler(object schema)
        {
            var info = schema as ImageUpdateSchema;
            if (info is null) return;

            if (ConnectionHub.UserInfo.ID == info.ID) LoadImage();
        }

        private void InfoChangedHandler(object schema)
        {
            var info = schema as UserInfo;
            if (info is null) return;

            UsernameBlock.Text = info.Username;
            UsernameButtonBlock.Text = info.Username;
            MailButtonBlock.Text = info.Mail;
        }

        private void UsernameButton_Click(object sender, RoutedEventArgs e)
        {
            var panel = new ProfileInfoChangePanel(ProfileInfo.Username);
            panel.ChangeComplete += InfoPanel_ChangeComplete;
            OverlayArea.OverlayContent = panel;
        }

        private void MailButton_Click(object sender, RoutedEventArgs e)
        {
            var panel = new ProfileInfoChangePanel(ProfileInfo.Mail);
            panel.ChangeComplete += InfoPanel_ChangeComplete;
            OverlayArea.OverlayContent = panel;
        }

        private void PasswordButton_Click(object sender, RoutedEventArgs e)
        {
            var panel = new ProfileInfoChangePanel(ProfileInfo.Password);
            panel.ChangeComplete += InfoPanel_ChangeComplete;
            OverlayArea.OverlayContent = panel;
        }

        private void InfoPanel_ChangeComplete()
        {
            OverlayArea.HideOverlay();
        }

        private async void LoadImage()
        {
            var imageResponse = await ConnectionHub.Http.GetUserProfileImageAsync(new(ConnectionHub.UserInfo.ID));
            if (imageResponse.Success)
            {
                var image = imageResponse.Data!;
                Image.Source = image;
            }
        }

        private async void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image| *.BMP;*.JPG; *.JPEG; *.PNG";
            if (fileDialog.ShowDialog() == true)
            {
                using var stream = File.Open(fileDialog.FileName, FileMode.Open);
                var resp = await ConnectionHub.Http.UpdateProfileImage(stream);
                if (!resp.Success) MessageBox.Show(resp.ErrorMessage);
            }
        }
    }
}
