using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DIOwpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window // The boss form
    {
        Client currentClient;
        bool isFileAttach = false;
        Dictionary<string, System.Windows.Controls.ListBox> friends = new Dictionary<string, System.Windows.Controls.ListBox>();

        public MainWindow(Client client)
        {
            InitializeComponent();
            currentClient = client;
            userNameTextBlock.Text = currentClient.UserName;
            currentClient.UserAvailable += CurrentClient_UserAvailable;
            currentClient.MessageReceived += CurrentClient_MessageReceived;
            currentClient.FileReceived += CurrentClient_FileReceived;
            this.Closing += MainWindow_Closing;
        }

        private void CurrentClient_FileReceived(object sender, FileReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke(new MethodInvoker(delegate
            {
                if (friends.ContainsKey(e.From))
                {
                    AddFileBlock(e.FileName, e.From, false);
                }    
            }));
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            currentClient.isNormClosing = true;
            currentClient.Disconnect();
            Environment.Exit(0);
        }


        // Incoming message
        private void CurrentClient_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Dispatcher.BeginInvoke(new MethodInvoker(delegate
            {
                if (friends.ContainsKey(e.From))
                    AddMessageBlock(e.Message, e.From, false);
            }));
        }

        // Adding new dialog
        // Appearing of the new window for entering nickname 
         
        private void makeDialogButton_Click(object sender, RoutedEventArgs e)
        {
            AddDialogWindow addWin = new AddDialogWindow();
            if (addWin.ShowDialog() == true)
            {
                if (addWin.friendNickname != userNameTextBlock.Text)
                    currentClient.IsAvailable(addWin.friendNickname);
                else
                {
                    System.Windows.MessageBox.Show("What's wrong ? It's your name...");
                }
            }
        }


        // Adding friend to the list and creating new field for messages
        private void AddFriendDialog(string name)
        {
            System.Windows.Controls.ListBox list = new System.Windows.Controls.ListBox();
            list.Name = name + "MessageFieldListBox";
            list.Width = dialogDockPanel.ActualWidth;
            list.Height = dialogDockPanel.ActualHeight;
            list.Background = new SolidColorBrush(Color.FromRgb(0xDD, 0xBE, 0xC3));
            if (dialogDockPanel.Children.Count > 0)
            {
                dialogDockPanel.Children.RemoveAt(dialogDockPanel.Children.Count - 1);
            }
            
            list.Visibility = Visibility.Visible;
            dialogDockPanel.Children.Insert(0, list);
            sendMessageButton.IsEnabled = true;
            addFileButton.IsEnabled = true;
            friends.Add(name, list);
        }


        // Does user exist and is user online
        private void CurrentClient_UserAvailable(object sender, MessageAvailEventArgs e)
        {
            Dispatcher.BeginInvoke(new MethodInvoker(delegate
            {
                if (e.IsAvailable)
                {
                    // /////////////////////////////////
                    // Add downloading friend's image
                    AddFriendDialog(e.UserName);
                    contactNameTextBlock.Text = e.UserName;
                    contactStatusTextBlock.Text = "Online";
                    AddNewDialogOnPanel(e.UserName);
                }
                else
                {
                    System.Windows.MessageBox.Show("User doesn't exist.");
                }
            }));
        }


        // Adding new dialog on the side panel
        private void AddNewDialogOnPanel(string friend)
        {
            DockPanel newDialog = new DockPanel();
            newDialog.Name = friend + "DialogDockPanel";
            newDialog.Height = 70;
            newDialog.LastChildFill = true;
            newDialog.Background = new SolidColorBrush(Color.FromRgb(0x5F, 0x1E, 0x76));
            newDialog.Width = contactsGrid.Width - 35;
            newDialog.Tag = friend;
            /*System.Windows.Controls.Button imageButton = new System.Windows.Controls.Button();
            imageButton.Foreground = new SolidColorBrush(Color.FromRgb(0xD3, 0xB0, 0xEE));
            imageButton.Background = new SolidColorBrush(Color.FromRgb(0x5F, 0x1E, 0x76));
            imageButton.Content = friend;
            imageButton.Width = 70;
            DockPanel.SetDock(imageButton, Dock.Left);
            newDialog.Children.Add(imageButton);*/
            Image img = new Image();
            BitmapImage bitImg = new BitmapImage();
            bitImg.BeginInit();
            bitImg.UriSource = new Uri("Stuff/dio_main.jpg", UriKind.Relative);
            bitImg.EndInit();
            img.Width = 70;
            img.Stretch = Stretch.UniformToFill;
            img.Source = bitImg;
            DockPanel.SetDock(img, Dock.Left);
            newDialog.Children.Add(img);
            TextBlock friendName = new TextBlock();
            friendName.Name = friend + "NameDialogTextBlock";
            friendName.Text = friend;
            friendName.Height = 20;
            friendName.Foreground = new SolidColorBrush(Color.FromRgb(0xD3, 0xB0, 0xEE));
            DockPanel.SetDock(friendName, Dock.Top);
            newDialog.Children.Add(friendName);
            System.Windows.Controls.TextBox lastMessage = new System.Windows.Controls.TextBox();
            lastMessage.Name = friend + "LastMessageTextBox";
            lastMessage.Text = "Nothing";
            DockPanel.SetDock(lastMessage, Dock.Bottom);
            newDialog.Children.Add(lastMessage);
            newDialog.MouseDown += NewDialog_MouseDown;

            DialogsListBox.Items.Add(newDialog);
            DialogsListBox.Visibility = Visibility.Visible;
        }

        // /////////////////////////////////
        // Add downloading friend's image
        private void NewDialog_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DockPanel main = sender as DockPanel;
            TextBlock name = main.Children[1] as TextBlock;
            System.Windows.Controls.ListBox list;
            friends.TryGetValue(name.Text, out list);
            if (dialogDockPanel.Children.Count > 0)
            {
                dialogDockPanel.Children.RemoveAt(dialogDockPanel.Children.Count - 1);
            }
            list.Visibility = Visibility.Visible;
            dialogDockPanel.Children.Insert(0, list);
            contactNameTextBlock.Text = name.Text;
        }


        // Load dialog's history
        private void LoadDialogHistory(string friendNick)
        { 
            
        }


        // Adding file to the message
        // Window for choosing necessary file
        private void addFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                isFileAttach = true;
                addFileButton.Tag = dialog.FileName;
                addFileButton.ToolTip = dialog.SafeFileName;
                TextBlock text = new TextBlock();
                text.Text = "Add file(1)";
                text.TextWrapping = TextWrapping.Wrap;
                text.Foreground = new SolidColorBrush(Color.FromRgb(0xD3, 0xB0, 0xEE));
                text.FontSize = 16;
                text.FontFamily = new FontFamily("Comic Sans MS");
                text.FontWeight = FontWeights.Bold;
                addFileButton.Content = text;
            }
        }

        // Sending a message
        // A message appers on the screen
        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            string message = messageTextBox.Text;
            string to = contactNameTextBlock.Text;
            if (message.Length > 0)
            { 
                AddMessageBlock(message, userNameTextBlock.Text, true);
                currentClient.SendMessage(to, message);
            }
            if (isFileAttach)
            {
                AddFileBlock(addFileButton.Tag.ToString().Remove(0, addFileButton.Tag.ToString().LastIndexOf("\\") + 1), userNameTextBlock.Text, true);
                currentClient.SendFile(to, addFileButton.Tag.ToString());
                isFileAttach = false;
                TextBlock text = new TextBlock();
                text.Text = "Add file";
                text.TextWrapping = TextWrapping.Wrap;
                text.Foreground = new SolidColorBrush(Color.FromRgb(0xD3, 0xB0, 0xEE));
                text.FontSize = 16;
                text.FontFamily = new FontFamily("Comic Sans MS");
                text.FontWeight = FontWeights.Bold;
                addFileButton.Content = text;
            }
            messageTextBox.Clear();
        }

        private void AddFileBlock(string fileName, string from, bool isSender)
        {
            StackPanel newFile = new StackPanel();
            newFile.MaxWidth = dialogGrid.Width - 60;
            System.Windows.Controls.ListBox list;
            if (isSender)
            {
                newFile.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                list = dialogDockPanel.Children[0] as System.Windows.Controls.ListBox;
                list.Visibility = Visibility.Visible;
            }
            else
            {
                newFile.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                if (from == contactNameTextBlock.Text)
                {
                    list = dialogDockPanel.Children[0] as System.Windows.Controls.ListBox;
                    list.Visibility = Visibility.Visible;
                }
                else
                {
                    friends.TryGetValue(from, out list);
                }
            }

            newFile.Name = "file" + list.Items.Count.ToString() + "StackPanel";
            System.Windows.Controls.Button fileButton = new System.Windows.Controls.Button();
            fileButton.Content = fileName;
            fileButton.Click += FileButton_Click;
            fileButton.IsEnabled = true;
            fileButton.Visibility = Visibility.Visible;
            newFile.Children.Add(fileButton);
            TextBlock dateTextBox = new TextBlock();
            dateTextBox.Text = DateTime.Now.ToString("d MMM H:mm");
            dateTextBox.MaxWidth = newFile.MaxWidth;
            newFile.Children.Add(dateTextBox);
            System.Windows.Controls.ListBoxItem item = new System.Windows.Controls.ListBoxItem();
            if (isSender)
                item.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            else
                item.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            item.Content = newFile;
            list.Items.Add(item);
            AddLastMessage(from, fileName);
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = (sender as System.Windows.Controls.Button).Content as string;
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream file = File.OpenRead("Temp" + fileName);
                FileStream newFile = File.Create(dialog.FileName);
                int size = (int)file.Length;
                byte[] buffer = new byte[2048];

                while (size > 0)
                {
                    int count = file.Read(buffer, 0, buffer.Length);
                    newFile.Write(buffer, 0, count);
                    size -= count;
                }

                file.Close();
                newFile.Close();
            }
        }


        // Drawing message block on the panel
        private void AddMessageBlock(string message, string from, bool isSender)
        {
            StackPanel newMessage = new StackPanel();
            newMessage.MaxWidth = dialogGrid.Width - 60;
            System.Windows.Controls.ListBox list;
            if (isSender)
            {
                newMessage.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                list = dialogDockPanel.Children[0] as System.Windows.Controls.ListBox;
               
                list.Visibility = Visibility.Visible;
            }
            else
            {
                newMessage.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                if (from == contactNameTextBlock.Text)
                {
                    list = dialogDockPanel.Children[0] as System.Windows.Controls.ListBox;
                    list.Visibility = Visibility.Visible;
                }
                else
                {
                    friends.TryGetValue(from, out list);
                }
            }

            newMessage.Name = "message" + list.Items.Count.ToString() + "StackPanel";
            System.Windows.Controls.TextBox mesTextBox = new System.Windows.Controls.TextBox();
            mesTextBox.Name = "message" + list.Items.Count.ToString() + "TextBox";
            mesTextBox.TextWrapping = TextWrapping.Wrap;
            mesTextBox.MaxWidth = newMessage.MaxWidth;
            mesTextBox.Text = message;
            mesTextBox.IsReadOnly = true;
            newMessage.Children.Add(mesTextBox);
            TextBlock dateTextBox = new TextBlock();
            dateTextBox.Text = DateTime.Now.ToString("d MMM H:mm");
            dateTextBox.MaxWidth = newMessage.MaxWidth;
            newMessage.Children.Add(dateTextBox);
            System.Windows.Controls.ListBoxItem item = new System.Windows.Controls.ListBoxItem();
            if (isSender)
                item.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            else
                item.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            item.Content = newMessage;
            list.Items.Add(item);
            AddLastMessage(from, message);
        }


        private void AddLastMessage(string friend, string message)
        {
            DockPanel panel, temp;
            if (DialogsListBox.SelectedIndex >= 0)
                panel = DialogsListBox.SelectedItem as DockPanel;
            else
                panel = DialogsListBox.Items[DialogsListBox.Items.Count - 1] as DockPanel;

            int count = DialogsListBox.Items.Count;
            for (int i = 0; i < count; i++)
            {
                temp = DialogsListBox.Items[i] as DockPanel;
                if (temp.Tag.ToString() == friend)
                {
                    panel = temp;
                    break;
                }
            }
            System.Windows.Controls.TextBox text = panel.Children[2] as System.Windows.Controls.TextBox;
            string last;
            if (message.Length <= 20)
                last = message + "..";
            else
                last = message.Substring(0, 20) + "..";
            text.Text = last;
        }


        // Change avatar
        // /////////////////////////////////
        // Add loading image on server
        private void changeAvatarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "jpeg (*.jpg)|*.jpg|bitmap (*.bmp)|*.bmp|gif (*.gif)|*.gif|PNG (*.png)|*.png|PNG (*.PNG)|*.png";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BitmapImage bitImg = new BitmapImage();
                bitImg.BeginInit();
                bitImg.UriSource = new Uri(dialog.FileName);
                bitImg.EndInit();
                if (bitImg.Height > bitImg.Width)
                    userAvatarImage.Width = 242;
                else
                    userAvatarImage.Height = 200;
                userAvatarImage.Stretch = Stretch.UniformToFill;
                userAvatarImage.Source = bitImg;
            }
            
        }


        // Customization
        // I'm a designer
        private void customizeFormButton_Click(object sender, RoutedEventArgs e)
        {
            CustomizeWindow cusWin = new CustomizeWindow();
            if (cusWin.ShowDialog() == true)
            {
                if (cusWin.mainBrush != null)
                {
                    mainDockPanel.Background = cusWin.mainBrush;
                }

                if (cusWin.additionalBrush != null)
                {
                    contactInfoGrid.Background = cusWin.additionalBrush;
                    typingAreaGrid.Background = cusWin.additionalBrush;
                }

                if (cusWin.buttonBrush != null)
                {
                    makeDialogButton.Background = cusWin.buttonBrush;
                    sendMessageButton.Background = cusWin.buttonBrush;
                    addFileButton.Background = cusWin.buttonBrush;
                    changeAvatarButton.Background = cusWin.buttonBrush;
                    customizeFormButton.Background = cusWin.buttonBrush;
                    logOutButton.Background = cusWin.buttonBrush;
                }
            }
        }


        // Load starting window
        private void logOutButton_Click(object sender, RoutedEventArgs e)
        {
            currentClient.isNormClosing = true;
            currentClient.Disconnect();
            
            LoginWindow win = new LoginWindow();
            win.Visibility = Visibility.Visible;
            win.Show();
            this.Visibility = Visibility.Hidden;
        }


    }
}
