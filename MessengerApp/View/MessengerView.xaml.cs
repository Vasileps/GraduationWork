using MessengerApp.Connection.Schemas;
using MessengerApp.Controls;
using MessengerApp.Controls.UserProfileControls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MessengerApp.Views
{
    /// <summary>
    /// Логика взаимодействия для MessengerView.xaml
    /// </summary>
    public partial class MessengerView : UserControl
    {
        private bool settingVisible = false;
        private bool overlayVisible = false;

        public MessengerView()
        {
            InitializeComponent();
            OverlayGrid.Visibility = Visibility.Collapsed;

            if (ConfigManager.TryGet("MessengerViewColumn1Width", out string strChatListColumnWidth))
                if (double.TryParse(strChatListColumnWidth, out double chatListColumnWidth))
                    ChatListColumn.Width = new GridLength(chatListColumnWidth);
        }

        private void ChatList_ChatSelected(Chat chat)
        {
            var chatControl = new ChatControl(chat);
            ChatViewPlaceholder.Content = chatControl;
            chatControl.ImageMessageClick += ShowImage;
            chatControl.AttachButtonClick += ShowAttachPanel;
        }

        private void ShowImage(ImageSource image)
        {
            var imageControl = new Image();
            imageControl.Source = image;
            imageControl.VerticalAlignment = VerticalAlignment.Center;
            imageControl.HorizontalAlignment = HorizontalAlignment.Center;
            RenderOptions.SetBitmapScalingMode(imageControl, BitmapScalingMode.HighQuality);
            SetOverlayView(imageControl);
        }

        private void ShowAttachPanel(AttachPanel panel)
        {
            SetOverlayView(panel);
            panel.Complete += HideOverlay;
        }

        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            ConfigManager.Set("MessengerViewColumn1Width", ChatListColumn.ActualWidth.ToString());
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            App.LogOut();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlay();
            ShowSettings();
        }

        private void OverlayGrid_Click(object sender, RoutedEventArgs e)
        {
            HideSettings();
            HideOverlay();
        }

        private void OverlayPlaceholder_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ChangeLanguageButton_Click(object sender, RoutedEventArgs e)
        {
            SetOverlayView(new LanguagePickerPanel());
        }

        private void UserSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SetOverlayView(new UserSearchPanel());
        }

        private void ChangeProfileImageButton_Click(object sender, RoutedEventArgs e)
        {
            SetOverlayView(new UserProfile());
        }

        private void SetOverlayView(UIElement userControl)
        {
            OverlayPlaceholder.Content = userControl;
            ShowOverlay();
            HideSettings();
        }

        private void ShowSettings()
        {
            if (settingVisible) return;

            settingVisible = true;
            var storyboard = (Storyboard)Resources["ShowSettings"];
            storyboard.Begin();
        }

        private void HideSettings()
        {
            if (!settingVisible) return;

            settingVisible = false;
            var storyboard = (Storyboard)Resources["HideSettings"];
            storyboard.Begin();
        }

        private void ShowOverlay()
        {
            if (overlayVisible) return;

            overlayVisible = true;
            var storyboard = (Storyboard)Resources["ShowOverlay"];
            storyboard.Begin();
        }

        private void HideOverlay()
        {
            if (!overlayVisible) return;

            overlayVisible = false;
            var storyboard = (Storyboard)Resources["HideOverlay"];
            storyboard.Begin();
            OverlayPlaceholder.Content = null;
        }
    }
}
