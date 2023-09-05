using MessengerApp.Connection.Schemas;
using MessengerApp.Controls.MessageBubbles;
using System;
using System.Windows.Input;
using System.Windows.Media;
using static MessengerApp.Controls.MessageBubbles.MessageManager;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для MessageBubble.xaml
    /// </summary>
    #nullable enable
    public partial class MessageBubble : UserControl
    {
        public Message Data;
        public readonly bool SendByUser;

        public Action<string>? Selected;

        public MessageBubble(Message message)
        {
            InitializeComponent();
            Data = message;
            SendByUser = Data.SenderID == ConnectionHub.UserInfo.ID;
        }

        public void SetAlignMode(MessageDisplayMode messageDisplayMode)
        {
            switch (messageDisplayMode)
            {
                case MessageDisplayMode.TwoSide:
                    if (SendByUser)
                    {
                        BubbleBorder.CornerRadius = new CornerRadius(15, 15, 0, 15);
                        HorizontalAlignment = HorizontalAlignment.Right;
                    }
                    else
                    {
                        BubbleBorder.CornerRadius = new CornerRadius(15, 15, 15, 0);
                        HorizontalAlignment = HorizontalAlignment.Left;
                    }
                    break;
            }
        }

        public void SetTextContent(string text)
        {
            var textContent = new TextContent();
            textContent.TextMessageBlock.Text = text;
            MessageArea.Content = textContent;
        }

        public void SetImageContent(ImageSource? imageSource, Action<ImageSource> imageClickHandler)
        {
            var imageContent = new ImageContent();
            imageContent.Image = imageSource;
            imageContent.ImageClick += imageClickHandler;
            MessageArea.Content = imageContent;
        }

        public void SetFileContent(string? fileName, string? size, Action<Message> fileDownloadHanlder)
        {
            var fileContent = new FileContent();
            fileContent.FileName = fileName ?? Application.Current.Resources["lcLoading"] as string;
            fileContent.FileSize = size ?? string.Empty;
            fileContent.DownloadClick += () => fileDownloadHanlder?.Invoke(Data);
            fileContent.DownloadButton.IsEnabled = fileName is not null;
            MessageArea.Content = fileContent;
        }

        public void TurnOnHighlight()
        {
            BubbleBorder.BorderBrush = Application.Current.Resources["TertiaryBitDarkerBrush"] as SolidColorBrush;
        }

        public void TurnOffHighlight()
        {
            BubbleBorder.BorderBrush = null;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed) Selected?.Invoke(Data.ID);
            e.Handled = true;
        }
    }
}
