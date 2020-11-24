using System;
using System.Windows;
using System.Windows.Forms;

namespace DIOwpf
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window // Form for choosing: to register ot to login
    {
        Client currentClient = new Client();

        public LoginWindow()
        {
            InitializeComponent();

            currentClient.LoginOK += CurrentClient_LoginOK;
            currentClient.LoginFailed += CurrentClient_LoginFailed;
            currentClient.RegisterOK += CurrentClient_RegisterOK;
            currentClient.RegisterFailed += CurrentClient_RegisterFailed;
            currentClient.Disconnected += CurrentClient_Disconnected;
            this.Closing += LoginWindow_Closing;
        }


        private void CurrentClient_Disconnected(object sender, EventArgs e)
        {
            //System.Windows.MessageBox.Show("Problems with connection. Sorry(((");
            //this.Close();
        }


        // Registration error
        private void CurrentClient_RegisterFailed(object sender, MessageErrorEventArgs e)
        {
            System.Windows.MessageBox.Show("Incorrect password or nickname. Please try again.");
        }


        private void CurrentClient_RegisterOK(object sender, EventArgs e)
        {/*
            Dispatcher.BeginInvoke(new MethodInvoker(delegate
            {
                MainWindow mainWin = new MainWindow(currentClient);
                mainWin.Show();
                mainWin.Visibility = Visibility.Visible;
                //this.Visibility = Visibility.Hidden;
            }));*/
        }


        // Authorization error
        private void CurrentClient_LoginFailed(object sender, MessageErrorEventArgs e)
        {
            System.Windows.MessageBox.Show("Wrong nickname or password. This user doesn't exist.");
        }


        // Authorization success
        private void CurrentClient_LoginOK(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new MethodInvoker(delegate
            {
                MainWindow mainWin = new MainWindow(currentClient);
                mainWin.Visibility = Visibility.Visible;
                this.Visibility = Visibility.Hidden;
            }));
            
        }


        private void LoginWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            currentClient.isNormClosing = true;
            if(currentClient.IsConnected)
                currentClient.Disconnect();
        }


        // Authorize
        // Authorize window
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            EnterInfoWindow enterWin = new EnterInfoWindow();
            if (enterWin.ShowDialog() == true)
            {
                currentClient.Login(enterWin.nickname, enterWin.password);
            }
        }


        // Register 
        // Register window
        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            EnterInfoWindow enterWin = new EnterInfoWindow();
            if (enterWin.ShowDialog() == true)
            {
                currentClient.Register(enterWin.nickname, enterWin.password);
                
            }
        }


        // Main window loading 
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWin = new MainWindow(currentClient);
            mainWin.Show();
            mainWin.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Hidden;
        }
    }
}
