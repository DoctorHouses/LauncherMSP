using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;

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

        private void ParcourirJava_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    JavaChemin.Text = dialog.SelectedPath;
                }
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("config.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("config.xml");
                XmlNode root = doc.DocumentElement;
                if (root.SelectSingleNode("//Java") != null && root.SelectSingleNode("//Server") != null)
                {
                    if (JavaMin.Text != string.Empty)
                        root.SelectSingleNode("//Java/Min").InnerText = JavaMin.Text;
                    if (JavaMax.Text != string.Empty)
                        root.SelectSingleNode("//Java/Max").InnerText = JavaMax.Text;
                    if (JavaPerm.Text != string.Empty)
                        root.SelectSingleNode("//Java/Perm").InnerText = JavaPerm.Text;
                    if (JavaChemin.Text != string.Empty)
                        root.SelectSingleNode("//Java/Path").InnerText = JavaChemin.Text;
                    if (ServeurModPack.Text != string.Empty)
                        root.SelectSingleNode("//Server/url").InnerText = ServeurModPack.Text;
                    doc.Save("config.xml");
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show(
                        "Il semblerait qu'il y ait un soucis avec votre fichier config.xml ! Supprimez le fichier dans le dossier de l'application et relancez l'application pour regénérer le fichier",
                        "Une erreur s'est produire",
                        MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(
                    "Il semblerait que le fichier config.xml est manquant ! Relancez l'application pour le régénérer",
                    "Une erreur s'est produire",
                    MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
            }
        }
    }
}
