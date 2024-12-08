using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiskRemover
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewwModel)
        {
            InitializeComponent();
            this.DataContext = viewwModel;
            this.Loaded += (s, e) =>
            {
                Hide();
            };
            Closing +=(s, e) => { e.Cancel = true; Hide(); };


            App.LogEvent += (s, e) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    logs.Items.Add(e);
                    if (logs.Items.Count > 9)
                    {
                        logs.Items.RemoveAt(0);
                    }
                    logs.ScrollIntoView(logs.Items[^1]);
                });
            };
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            TrayIcon.Dispose();
            Application.Current.Shutdown();
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void TrayIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
    }
}