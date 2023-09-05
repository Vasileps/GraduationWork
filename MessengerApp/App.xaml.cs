using MessengerApp.Views;
using Notifications.Wpf.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MessengerApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string dataFolderName = "Mirrornet";

        public static readonly Dictionary<string, string> LangueagesList = new();
        public static readonly NotificationManager NotificationManager = new();
        public static event EventHandler LanguageChanged;

        static App()
        {
            AddLaguages();
        }

        private static void AddLaguages()
        {
            LangueagesList.Add("Русский", "ru");
            LangueagesList.Add("English", "en-US");
        }

        public static DirectoryInfo DataDirectory
        {
            get
            {
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var messengerDataFolder = Path.Combine(appDataFolder, dataFolderName);
                var dir = Directory.CreateDirectory(messengerDataFolder);
                return dir;
            }
        }

        public static CultureInfo Language
        {
            get => System.Threading.Thread.CurrentThread.CurrentUICulture;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;

                string path = "Assets/Localization/Lang{0}.xaml";
                path = value.IsNeutralCulture ?
                    string.Format(path, string.Empty) :
                    string.Format(path, $".{value.Name}");

                if (!Uri.TryCreate(path, UriKind.Relative, out Uri langPath))
                    throw new Exception("Language is not supported.");

                System.Threading.Thread.CurrentThread.CurrentUICulture = value;

                ResourceDictionary newLang = new();
                newLang.Source = langPath;

                ResourceDictionary oldLang =
                    (from dict in Current.Resources.MergedDictionaries
                     where dict.Source != null && dict.Source.OriginalString.StartsWith("Assets/Localization")
                     select dict).First();

                if (oldLang is not null)
                {
                    int index = Current.Resources.MergedDictionaries.IndexOf(oldLang);
                    Current.Resources.MergedDictionaries.Remove(oldLang);
                    Current.Resources.MergedDictionaries.Insert(index, newLang);
                }
                else Current.Resources.MergedDictionaries.Add(newLang);

                if (LanguageChanged is not null)
                    LanguageChanged(Current, new EventArgs());
            }
        }

        public static async void LogOut()
        {
            await ConnectionHub.Http.SignOut();
            Current.MainWindow.Content = new WelcomeView();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ConfigManager.Save();
        }
    }
}
