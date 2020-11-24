using System.Windows;

namespace DIOwpf
{
    /// <summary>
    /// Логика взаимодействия для AddDialogWindow.xaml
    /// </summary>
    public partial class AddDialogWindow : Window // Form for entering friends' nicknames and adding new dialogs
    {
        public AddDialogWindow()
        {
            InitializeComponent();
            this.Closing += AddDialogWindow_Closing;
        }


        // Closing without finishing
        private void AddDialogWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsSet)
                this.DialogResult = false;
        }

        public string friendNickname;
        private bool IsSet = false;


        // Return the friend nickname
        private void AddFriendButton_Click(object sender, RoutedEventArgs e)
        {
            friendNickname = FriendNicknameTextBox.Text;
            IsSet = true;
            this.DialogResult = true;
        }
    }
}
