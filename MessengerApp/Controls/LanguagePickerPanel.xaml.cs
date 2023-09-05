using System.Collections.Generic;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для LanguagePickerPanel.xaml
    /// </summary>
    public partial class LanguagePickerPanel : UserControl
    {
        private readonly Dictionary<string, string> languages = new Dictionary<string, string>();

        public LanguagePickerPanel()
        {
            InitializeComponent();

            languages = App.LangueagesList;
            CreateButtons();
        }

        private void CreateButtons()
        {
            foreach (var language in languages)
            {
                Button button = new Button();
                button.Style = (Style)Application.Current.Resources["HyperLinkButton"];
                button.Content = language.Key;
                button.FontSize = 20;
                button.HorizontalAlignment = HorizontalAlignment.Center;
                button.Click += LanguageButton_Click;
                button.Margin = new Thickness(5);

                LanguagesList.Items.Add(button);
            }
        }

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            var key = ((Button)sender).Content.ToString()!;
            var language = languages[key];
            App.Language = new System.Globalization.CultureInfo(language);
        }
    }
}
