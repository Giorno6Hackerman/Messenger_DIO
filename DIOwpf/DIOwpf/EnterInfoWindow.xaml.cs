using System.Windows;

namespace DIOwpf
{
    /// <summary>
    /// Логика взаимодействия для EnterInfoWindow.xaml
    /// </summary>
    public partial class EnterInfoWindow : Window // Form for entering login and password
    {

        public EnterInfoWindow()
        {
            InitializeComponent();
            this.Closing += EnterInfoWindow_Closing;
        }


        // Closing without finishing
        private void EnterInfoWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!IsSet)
                this.DialogResult = false;
        }

        public string nickname;
        public string password;
        private bool IsSet = false;


        private void OkEnterButton_Click(object sender, RoutedEventArgs e)
        {
            nickname = NicknameTextBox.Text;
            password = PasswordTextBox.Text;
            this.IsSet = true;
            this.DialogResult = true;
        }


    }
}
