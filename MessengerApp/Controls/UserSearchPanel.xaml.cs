using System;
using System.Threading.Tasks;

namespace MessengerApp.Controls
{
    /// <summary>
    /// Логика взаимодействия для UserSearchPanel.xaml
    /// </summary>
    public partial class UserSearchPanel : UserControl
    {
        private const int usersAtATime = 25;
        private static TimeSpan minDelay = TimeSpan.FromMilliseconds(250);
        private Task searchingTask;
        private Task waitingTask = Task.CompletedTask;

        public UserSearchPanel()
        {
            InitializeComponent();
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs args)
        {
            if (waitingTask.IsCompleted) waitingTask = WaitToSearch();
        }

        private async Task WaitToSearch()
        {
            if (searchingTask is not null) await searchingTask;
            searchingTask = Search();
        }

        private async Task Search()
        {
            var startTime = DateTime.Now;

            if (string.IsNullOrEmpty(SearchBar.Text))
            {
                UserList.Items.Clear();
                return;
            }

            var response = await ConnectionHub.Http.SearchUsersAsync(new(SearchBar.Text, null, usersAtATime));
            if (!response.Success) return;

            UserList.Items.Clear();
            foreach (var user in response.Data!)
            {
                var panel = new UserPanel(user);
                UserList.Items.Add(panel);
            }
            var endTime = DateTime.Now;
            var diff = endTime - startTime;

            if (diff < minDelay) await Task.Delay(minDelay - diff);
        }
    }
}
