using MessengerApp.Connection.Schemas;
using MessengerApp.Controls.MessageBubbles;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для ChatControl.xaml
    /// </summary>
#nullable enable
    public partial class ChatControl : UserControl
    {
        private const int MessagesAtATime = 5;

        private ObservableCollection<MessageBubble> messagesCollection = new();
        private MessageManager manager = new();
        private Chat Data;

        private string? lastID;
        private MessageBubble? seletedBubble;
        private Action messageAction;

        private bool firstMessagesLoaded;
        private bool gettingMessages;

        private bool toolPanelShown = false;
        private bool editPanelShown = false;

        public Action<ImageSource>? ImageMessageClick
        {
            get => manager.ImageMessageClick;
            set => manager.ImageMessageClick = value;
        }

        public Action<AttachPanel>? AttachButtonClick;

        public ChatControl(Chat data)
        {
            InitializeComponent();

            Data = data;
            messageAction = SendTextMessage;

            ChatNameBlock.Text = Data.Name;
            MessageList.ItemsSource = messagesCollection;

            manager.MessageSelected += MessageSelected;

            var token = ConnectionHub.Notifications.HoldNotifications();
            GetFirstMessages();
            ConnectionHub.Notifications.MessageRecieved += MessageRecievedHandler;
            ConnectionHub.Notifications.MessageDeleted += MessageDeletedHandler;
            token.Release();
        }

        #region ServerEventsHandlers
        private void MessageDeletedHandler(object obj)
        {
            var message = obj as Message;
            if (message is null) return;

            if (message.ChatID != Data.ID) return;

            var bubble = manager[message.ID];
            manager.Delete(message.ID);
            messagesCollection.Remove(bubble);
        }

        private void MessageRecievedHandler(object obj)
        {
            var message = obj as Message;
            if (message is null) return;

            if (message.ChatID != Data.ID) return;

            AddMessageToTheBottom(message);
        }
        #endregion

        #region GettingMesseges
        private async Task<IEnumerable<Message>> GetMessages(int count = MessagesAtATime)
        {
            var schema = new GetMessagesSchema(Data.ID, count, lastID);
            var response = await ConnectionHub.Http.GetMessagesDescendingAsync(schema);
            if (response.Success)
            {
                if (response.Data!.Length > 0) lastID = response.Data![^1].ID;
                return response.Data!.OrderByDescending(x => x.SendTime);
            }
            return new Message[0];
        }

        private async void GetFirstMessages()
        {
            IEnumerable<Message> messages;
            do
            {
                messages = await GetMessages();
                foreach (var message in messages) AddMessageToTheTop(message);
                MessageScroll.UpdateLayout();
                MessageScroll.ScrollToBottom();
            }
            while (MessageScroll.VerticalOffset <= 25 && messages.Any());
            firstMessagesLoaded = true;
        }

        private async void GetMoreMessages()
        {
            if (!firstMessagesLoaded) return;
            if (gettingMessages) return;

            gettingMessages = true;
            var messages = await GetMessages();
            if (messages.Count() is 0) return;

            var oldHeight = MessageScroll.ExtentHeight;
            var offset = MessageScroll.VerticalOffset;

            foreach (var message in messages) AddMessageToTheTop(message);

            MessageScroll.UpdateLayout();
            var diff = MessageScroll.ExtentHeight - oldHeight;
            MessageScroll.ScrollToVerticalOffset(offset + diff);

            gettingMessages = false;
        }
        #endregion

        #region AddingMessages
        private void AddMessageToTheBottom(Message schema)
        {
            var scroller = MessageScroll;
            bool autoScroll = scroller.VerticalOffset + scroller.ViewportHeight == scroller.ExtentHeight;

            messagesCollection.Add(manager.CreateBubble(schema));
            if (autoScroll) scroller.ScrollToBottom();
        }

        private void AddMessageToTheTop(Message schema)
        {
            messagesCollection.Insert(0, manager.CreateBubble(schema));
        }
        #endregion

        #region Visual

        public void ShowToolPanel()
        {
            if (toolPanelShown) return;

            toolPanelShown = true;
            var storyboard = (Storyboard)Resources["ShowToolPanel"];
            storyboard.Begin();
        }

        public void HideToolPanel()
        {
            if (!toolPanelShown) return;

            toolPanelShown = false;
            var storyboard = (Storyboard)Resources["HideToolPanel"];
            storyboard.Begin();
        }

        public void ShowEditPanel()
        {
            if (editPanelShown) return;

            editPanelShown = true;
            var storyboard = (Storyboard)Resources["ShowEditPanel"];
            storyboard.Begin();
        }

        public void HideEditPanel()
        {
            if (!editPanelShown) return;

            editPanelShown = false;
            var storyboard = (Storyboard)Resources["HideEditPanel"];
            storyboard.Begin();
        }

        #endregion

        #region States

        public void SelectMode(MessageBubble bubble)
        {
            seletedBubble?.TurnOffHighlight();
            seletedBubble = bubble;
            seletedBubble.TurnOnHighlight();

            DownloadMessageButton.Visibility =
                bubble.Data.Type is MessageType.Image ?
                Visibility.Visible :
                Visibility.Collapsed;

            HideEditPanel();
            ShowToolPanel();
        }

        public void EditMode()
        {
            if (seletedBubble is null)
            {
                ReadAndWriteMode();
                return;
            }

            HideToolPanel();
            switch (seletedBubble.Data.Type)
            {
                case MessageType.Text:
                    ShowEditPanel();
                    messageAction = EditMessage;

                    if (seletedBubble.MessageArea.Content is TextContent content)
                    {
                        MessageTextBox.Text = content.TextMessageBlock.Text;
                        messageAction = EditMessage;
                    }
                    else ReadAndWriteMode();
                    break;

                case MessageType.Image:
                    EditImageMessage(seletedBubble.Data);
                    ReadAndWriteMode();
                    break;

                case MessageType.File:
                    EditFileMessage(seletedBubble.Data);
                    ReadAndWriteMode();
                    break;
            }
        }

        public void ReadAndWriteMode()
        {
            seletedBubble?.TurnOffHighlight();
            seletedBubble = null;

            HideToolPanel();
            HideEditPanel();

            messageAction = SendTextMessage;
            MessageTextBox.Text = string.Empty;
        }

        #endregion

        #region EventHandlers

        private void SendButton_Click(object sender, RoutedEventArgs e) => messageAction.Invoke();

        private void EditMessageButton_Click(object sender, RoutedEventArgs e) => EditMode();

        private async void DeleteMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (seletedBubble is null) return;

            var response = await ConnectionHub.Http.DeleteMessage(new(seletedBubble.Data.ID));
            if (!response.Success) MessageBox.Show(response.ErrorMessage);

            ReadAndWriteMode();
        }

        private void MessageList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ReadAndWriteMode();
        }

        private void MessageSelected(MessageBubble bubble) => SelectMode(bubble);

        private void MessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (!Keyboard.IsKeyDown(Key.LeftShift)) messageAction.Invoke();
                    else
                    {
                        MessageTextBox.Text += Environment.NewLine;
                        MessageTextBox.CaretIndex = MessageTextBox.Text.Length - 1;
                    }
                    break;
            }
        }

        private void MessageScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (MessageScroll.VerticalOffset <= 0) return;

            if (MessageScroll.ExtentHeight > 50 && MessageScroll.VerticalOffset < 50) GetMoreMessages();
            if (MessageScroll.ExtentHeight < 50)
            {
                var gap = MessageScroll.ExtentHeight * 0.9;
                if (MessageScroll.VerticalOffset < gap) GetMoreMessages();
            }
        }

        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            AttachButtonClick?.Invoke(new(Data.ID));
        }

        private void DownloadMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (seletedBubble is null) return;

            if (seletedBubble.MessageArea.Content is ImageContent content)
            {
                var fileDialog = new SaveFileDialog();
                fileDialog.Filter = "Image| *.PNG";
                if (fileDialog.ShowDialog() == false) return;
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)content.ImageBox.Source));

                using var fileStream = new FileStream(fileDialog.FileName, FileMode.Create);
                encoder.Save(fileStream);
            }

            ReadAndWriteMode();
        }

        #endregion

        private string? GetMessageBoxText()
        {
            var text = MessageTextBox.Text;
            text = text?.TrimEnd(new char[] { ' ', '\r', '\n' });
            return text;
        }

        private async void SendTextMessage()
        {
            var text = GetMessageBoxText();
            if (string.IsNullOrEmpty(text)) return;

            var schema = new AddTextMessageSchema(Data.ID, text, DateTime.UtcNow);
            var response = await ConnectionHub.Http.AddTextMessageAsync(schema);

            if (response.Success) MessageTextBox.Text = string.Empty; //AddErrorParse
        }

        private async void EditImageMessage(Message message)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image| *.BMP;*.JPG; *.JPEG; *.PNG";
            if (fileDialog.ShowDialog() == false) return;

            var stream = File.Open(fileDialog.FileName, FileMode.Open);

            await ConnectionHub.Http.UpdateImageMessage(new(stream, fileDialog.SafeFileName, message.ID));

            stream.Dispose();
        }

        private async void EditFileMessage(Message message)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == false) return;

            var stream = File.Open(fileDialog.FileName, FileMode.Open);

            await ConnectionHub.Http.UpdateFileMessage(new(stream, fileDialog.SafeFileName, message.ID));

            stream.Dispose();
        }

        private async void EditMessage()
        {
            if (seletedBubble is null) goto CancelEditig;


            var text = GetMessageBoxText();
            if (string.IsNullOrEmpty(text)) return;

            var schema = new EditTextMessageSchema(seletedBubble.Data.ID, text);
            var response = await ConnectionHub.Http.EditTextMessage(schema);

            if (!response.Success) MessageBox.Show(response.ErrorMessage);

            CancelEditig: ReadAndWriteMode();
        }
    }
}
