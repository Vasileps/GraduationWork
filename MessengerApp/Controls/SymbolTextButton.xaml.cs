namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для IconTextButton.xaml
    /// </summary>
    public partial class SymbolTextButton : Button
    {
        public static readonly DependencyProperty TextProperty;
        public static readonly DependencyProperty SymbolProperty;

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string Symbol
        {
            get => (string)GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }

        static SymbolTextButton()
        {
            TextProperty = DependencyProperty.Register(nameof(Text), typeof(string),
                typeof(SymbolTextButton), new PropertyMetadata(TextChanged));

            SymbolProperty = DependencyProperty.Register(nameof(Symbol), typeof(string),
                typeof(SymbolTextButton), new PropertyMetadata(SymbolChanged));
        }

        public SymbolTextButton()
        {
            InitializeComponent();
        }

        private static void TextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (SymbolTextButton)obj;
            control.TextRun.Text = (string)args.NewValue;
        }

        private static void SymbolChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (SymbolTextButton)obj;
            control.SymbolRun.Text = (string)args.NewValue;
        }
    }
}
