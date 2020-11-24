using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace DIOwpf
{
    /// <summary>
    /// Логика взаимодействия для CustomizeWindow.xaml
    /// </summary>
    public partial class CustomizeWindow : Window   // Form for customixation stuff
    {
        public CustomizeWindow()
        {
            InitializeComponent();

        }

        public System.Windows.Media.Brush mainBrush;
        public System.Windows.Media.Brush additionalBrush;
        public System.Windows.Media.Brush buttonBrush;



        private void customizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void mainColorStackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorDialog mainDialog = new ColorDialog();
            if (mainDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int color = mainDialog.Color.ToArgb();
                byte aColor = (byte)((color & 0xFF000000) >> 24);
                byte rColor = (byte)((color & 0x00FF0000) >> 16);
                byte gColor = (byte)((color & 0x0000FF00) >> 8);
                byte bColor = (byte)(color & 0x000000FF);
                mainBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(aColor, rColor, gColor, bColor));
                mainColorStackPanel.Background = mainBrush;
            }
        }

        private void additionalColorStackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorDialog additionalDialog = new ColorDialog();
            if (additionalDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int color = additionalDialog.Color.ToArgb();
                byte aColor = (byte)((color & 0xFF000000) >> 24);
                byte rColor = (byte)((color & 0x00FF0000) >> 16);
                byte gColor = (byte)((color & 0x0000FF00) >> 8);
                byte bColor = (byte)(color & 0x000000FF);
                additionalBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(aColor, rColor, gColor, bColor));
                additionalColorStackPanel.Background = additionalBrush;
            }
        }

        private void buttonColorStackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorDialog buttonDialog = new ColorDialog();
            if (buttonDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int color = buttonDialog.Color.ToArgb();
                byte aColor = (byte)((color & 0xFF000000) >> 24);
                byte rColor = (byte)((color & 0x00FF0000) >> 16);
                byte gColor = (byte)((color & 0x0000FF00) >> 8);
                byte bColor = (byte)(color & 0x000000FF);
                buttonBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(aColor, rColor, gColor, bColor));
                buttonColorStackPanel.Background = buttonBrush;
            }
        }
    }
}
