using MessengerApp.Connection.Schemas;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MessengerApp.Controls.MessageBubbles
{
#nullable enable
    public class MessageManager
    {
        public MessageBubble this[string ID] => messageCollection[ID];

        private readonly Dictionary<string, MessageBubble> messageCollection = new();
        public MessageDisplayMode DisplayMode = MessageDisplayMode.TwoSide;

        public Action<MessageBubble>? MessageSelected;
        public Action<ImageSource>? ImageMessageClick;

        public MessageManager()
        {
            ConnectionHub.Notifications.MessageUpdated += MessageUpdatedHandler;
        }

        private void MessageUpdatedHandler(object obj)
        {
            var message = obj as Message;
            if (message is null) return;

            if (!messageCollection.ContainsKey(message.ID)) return;
            var bubble = messageCollection[message.ID];

            SetContent(bubble, message);
        }

        private void Bubble_Selected(string messageID)
        {
            MessageSelected?.Invoke(messageCollection[messageID]);
        }

        private void ImageMessage_Click(ImageSource image)
        {
            ImageMessageClick?.Invoke(image);
        }

        private async void DownloadFile(Message message)
        {
            Metadata metadata = JsonSerializer.Deserialize<Metadata>(message.Data)!;
            var fileDialog = new SaveFileDialog();
            fileDialog.FileName = metadata.Filename;
            if (fileDialog.ShowDialog() == false) return;

            var response = await ConnectionHub.Http.GetMessageFile(new(message.ID));
            if (!response.Success) MessageBox.Show(response.ErrorMessage);

            try
            {
                using var fileStream = new FileStream(fileDialog.FileName, FileMode.CreateNew);
                response.Data!.Position = 0;
                response.Data!.CopyTo(fileStream);
                fileStream.Flush();
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void Delete(string ID)
        {
            var bubble = messageCollection[ID];
            if (bubble is null) return;
            bubble.Selected -= Bubble_Selected;
            messageCollection.Remove(ID);
        }

        public MessageBubble CreateBubble(Message message)
        {
            var bubble = new MessageBubble(message);
            messageCollection.Add(message.ID, bubble);

            bubble.BubbleBorder.Background = bubble.SendByUser ?
                Application.Current.Resources["SecondaryBitDarkerBrush"] as SolidColorBrush :
                Application.Current.Resources["SecondaryBitLighterBrush"] as SolidColorBrush;

            bubble.TimeBlock.Text = message.SendTime.ToDisplayableString();

            SetContent(bubble, message);
            bubble.SetAlignMode(DisplayMode);

            if (bubble.SendByUser) bubble.Selected += Bubble_Selected;

            return bubble;
        }

        private async void SetContent(MessageBubble bubble, Message message)
        {
            bubble.Data = message;
            switch (message.Type)
            {
                case MessageType.Text: bubble.SetTextContent(message.Data); break;
                case MessageType.Image:
                    var response = await ConnectionHub.Http.GetMessageFile(new(message.ID));
                    if (response.Success) bubble.SetImageContent(response.Data!.ToBitmapImage(), ImageMessage_Click);
                    break;
                case MessageType.File:
                    Metadata? metadata = null;
                    try
                    {
                        metadata = JsonSerializer.Deserialize<Metadata>(message.Data);
                    }
                    catch { }
                    bubble.SetFileContent(metadata?.Filename, metadata?.Size.ToSizeString(), DownloadFile);
                    break;
                default: break;
            }
        }

        public enum MessageDisplayMode : byte
        {
            TwoSide,
        }

        private record Metadata(string Filename, long Size);
    }
}
