using System.Windows;

namespace Group4337
{
    public partial class MainWindow : Window
    {
        public MainWindow()
            => InitializeComponent();
    }
    private void Author_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1();
            window.Show();
        }
    }