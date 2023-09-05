using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для OverlayControl.xaml
    /// </summary>
    [ContentProperty(nameof(OverlayContent))]
    public partial class OverlayContainer : ContentControl
    {
        public static readonly DependencyProperty OverlayContentProperty;
        public static readonly DependencyProperty AutoShowOnUpdateProperty;

        private bool overlayShown = false;
        private Storyboard showOverlayStory;
        private Storyboard hideOverlayStory;

        public object OverlayContent
        {
            get { return (object)GetValue(OverlayContentProperty); }
            set { SetValue(OverlayContentProperty, value); }
        }


        public new object Content
        {
            get { return (object)GetValue(OverlayContentProperty); }
            set { SetValue(OverlayContentProperty, value); }
        }

        public bool AutoShowOnUpdate
        {
            get { return (bool)GetValue(AutoShowOnUpdateProperty); }
            set { SetValue(AutoShowOnUpdateProperty, value); }
        }

        static OverlayContainer()
        {            
            OverlayContentProperty = DependencyProperty.Register(nameof(OverlayContent),
                typeof(object), typeof(OverlayContainer), new PropertyMetadata(OverlayContentChanged));
            AutoShowOnUpdateProperty = DependencyProperty.Register(nameof(AutoShowOnUpdate),
                typeof(bool), typeof(OverlayContainer));
        }

        public OverlayContainer()
        {
            InitializeComponent();

            showOverlayStory = (Storyboard)Resources["ShowOverlay"];
            hideOverlayStory = (Storyboard)Resources["HideOverlay"];
        }

        public void ShowOverlay()
        {
            if (overlayShown) return;
            overlayShown = true;
            showOverlayStory.Begin();
        }

        public void HideOverlay()
        {
            if (!overlayShown) return;
            overlayShown = false;
            hideOverlayStory.Begin();
        }

        private static void OverlayContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var sender = obj as OverlayContainer;
            if (sender is null) return;
            if (sender.AutoShowOnUpdate) sender.ShowOverlay();
        }

        private void Overlay_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HideOverlay();
            e.Handled = true;
        }
    }
}
