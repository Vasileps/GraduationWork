using System.Windows.Media;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для ImageFrame.xaml
    /// </summary>
    public partial class ImageFrame : UserControl
    {
        public static readonly DependencyProperty SourceProperty;

        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        static ImageFrame()
        {
            SourceProperty = DependencyProperty.Register(nameof(Source), typeof(ImageSource),
                typeof(ImageFrame), new PropertyMetadata(ImageChanged));
        }

        public ImageFrame()
        {
            InitializeComponent();           
        }

        private void Resize()
        {
            var size = ActualWidth > ActualHeight ? ActualHeight : ActualWidth;
            var half = size / 2;
            Frame.Width = Frame.Height = size;
            Frame.CornerRadius = new(half);

            ImageControl.Width = ImageControl.Height = size;
            ImageControl.Clip = new EllipseGeometry(new(half, half), half, half);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Resize();
        }

        private static void ImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var sender = (ImageFrame)obj;
            sender.ImageControl.Source = (ImageSource)args.NewValue;            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Resize();
        }
    }
}
