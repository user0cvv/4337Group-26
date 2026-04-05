using System.Windows;

namespace WpfGitAppVS20
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenAuthorWindow_Click(object sender, RoutedEventArgs e)
        {
            _4337_Sakaev window = new _4337_Sakaev();
            window.Show();
        }
    }
}