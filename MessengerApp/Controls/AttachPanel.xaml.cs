using MessengerApp.Connection.Schemas;
using Microsoft.Win32;
using System;
using System.IO;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для AttachPanel.xaml
    /// </summary>
    public partial class AttachPanel : UserControl
    {
        private readonly string chatID;

        public Action Complete;

        public AttachPanel(string chatID)
        {
            InitializeComponent();
            this.chatID = chatID;
        }

        private async void FileButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == false) return;
            var stream = File.Open(fileDialog.FileName, FileMode.Open);

            var mesResp = await ConnectionHub.Http.AddFileMessageAsync(new(chatID, DateTime.UtcNow, FileType.File));
            if (!mesResp.Success) return; //AddErrorHandler

            var messageData = mesResp.Data!;
            await ConnectionHub.Http.UpdateFileMessage(new(stream, fileDialog.SafeFileName, messageData.ID));

            stream.Dispose();

            Complete?.Invoke();
        }

        private async void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image| *.BMP;*.JPG; *.JPEG; *.PNG";
            if (fileDialog.ShowDialog() == false) return;
            var stream = File.Open(fileDialog.FileName, FileMode.Open);

            var mesResp = await ConnectionHub.Http.AddFileMessageAsync(new(chatID, DateTime.UtcNow, FileType.Image));
            if (!mesResp.Success) return; //AddErrorHandler

            var messageData = mesResp.Data!;
            await ConnectionHub.Http.UpdateImageMessage(new(stream, fileDialog.SafeFileName, messageData.ID));

            stream.Dispose();

            Complete?.Invoke();
        }
    }
}
