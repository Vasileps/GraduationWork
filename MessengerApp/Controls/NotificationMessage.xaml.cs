using Notifications.Wpf.Core;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для NotificationMessage.xaml
    /// </summary>
    public partial class NotificationMessage : UserControl, INotificationViewModel
    {
        public NotificationMessage(string chatName, string message)
        {
            InitializeComponent();
            ChatNameBlock.Text = chatName;
            MessageBlock.Text = message;
        }
    }
}
