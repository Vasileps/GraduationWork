using System;

namespace MessengerApp.Controls.MessageBubbles
{
    /// <summary>
    /// Логика взаимодействия для FileContent.xaml
    /// </summary>
    public partial class FileContent : UserControl
    {
        public static readonly DependencyProperty FileNameProperty;
        public static readonly DependencyProperty FileSizeProperty;

        public Action DownloadClick;

        public string FileName
        {
            get => (string)GetValue(FileNameProperty);
            set => SetValue(FileNameProperty, value);
        }

        public string FileSize
        {
            get => (string)GetValue(FileSizeProperty);
            set => SetValue(FileSizeProperty, value);
        }

        static FileContent()
        {
            FileNameProperty = DependencyProperty.Register(nameof(FileName), typeof(string),
                typeof(FileContent), new PropertyMetadata(FileNameChanged));
            FileSizeProperty = DependencyProperty.Register(nameof(FileSize), typeof(string),
                typeof(FileContent), new PropertyMetadata(FileSizeChanged));
        }


        public FileContent()
        {
            InitializeComponent();
        }

        private static void FileNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FileContent)obj;
            var value = (string)args.NewValue;
            control.NameTextBlock.Text = value;
        }

        private static void FileSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FileContent)obj;
            var value = (string)args.NewValue;
            control.SizeTextBlock.Text = value;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadClick?.Invoke();
        }
    }
}
