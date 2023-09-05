using System;
using System.Windows.Input;
using System.Windows.Media;

namespace MessengerApp.Controls.MessageBubbles
{
    /// <summary>
    /// Логика взаимодействия для ImageContent.xaml
    /// </summary>
#nullable enable
    public partial class ImageContent : UserControl
    {
        public static readonly DependencyProperty ImageProperty;

        public Action<ImageSource>? ImageClick;

        public ImageSource? Image
        {
            get => (ImageSource)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        static ImageContent()
        {
            ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ImageSource),
                typeof(ImageContent), new PropertyMetadata(ImageChanged));
        }

        public ImageContent()
        {
            InitializeComponent();
        }

        private static void ImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (ImageContent)obj;
            var image = (ImageSource)args.NewValue;
            control.ImageBox.Source = image;
            control.LoadingText.Visibility = image is null ? Visibility.Visible : Visibility.Collapsed;
            control.ImageBox.Visibility = image is null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ImageBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ImageClick?.Invoke(ImageBox.Source);
                e.Handled = true;
            }
        }
    }
}
