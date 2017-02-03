using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LauncherMinecraftV3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void max_Click(object sender, RoutedEventArgs e)
        {
            if(this.WindowState == WindowState.Maximized){
                this.WindowState = WindowState.Normal;
                ImageBrush image = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/max.png")),
                    Stretch = Stretch.UniformToFill
                };
                this.max.Background = image;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                ImageBrush image = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/restore.png")),
                    Stretch = Stretch.UniformToFill
                };
                this.max.Background = image;
            }
        }

        private void min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DragClick(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void btnLeftMenuHide_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbHideLeftMenu", btnLeftMenuHide, btnLeftMenuShow, pnlLeftMenu);
        }

        private void btnLeftMenuShow_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbShowLeftMenu", btnLeftMenuHide, btnLeftMenuShow, pnlLeftMenu);
        }

        private void ShowHideMenu(string Storyboard, Button btnHide, Button btnShow, StackPanel pnl)
        {
            Storyboard sb = Resources[Storyboard] as Storyboard;
            sb.Begin(pnl);

            if (Storyboard.Contains("Show"))
            {
                btnHide.Visibility = System.Windows.Visibility.Visible;
                btnShow.Visibility = System.Windows.Visibility.Hidden;
            }
            else if (Storyboard.Contains("Hide"))
            {
                btnHide.Visibility = System.Windows.Visibility.Hidden;
                btnShow.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
