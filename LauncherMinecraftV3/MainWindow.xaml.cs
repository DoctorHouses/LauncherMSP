using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Application = System.Windows.Application;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace LauncherMinecraftV3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string Quote = "\"";
        private SessionToken _session = new SessionToken();
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
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        private void min_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void DragClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                DragMove();
            }
            else
            {
                DragMove();
            }
        }
        private void btnLeftMenuHide_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbHideLeftMenu", BtnLeftMenuHide, BtnLeftMenuShow, PnlLeftMenu);
        }
        private void btnLeftMenuShow_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbShowLeftMenu", BtnLeftMenuHide, BtnLeftMenuShow, PnlLeftMenu);
        }
        private void ShowHideMenu(string storyboard, UIElement btnHide, UIElement btnShow, FrameworkElement pnl)
        {
            Storyboard sb = Resources[storyboard] as Storyboard;
            sb?.Begin(pnl);

            if (storyboard.Contains("Show"))
            {
                btnHide.Visibility = Visibility.Visible;
                btnShow.Visibility = Visibility.Hidden;
            }
            else if (storyboard.Contains("Hide"))
            {
                btnHide.Visibility = Visibility.Hidden;
                btnShow.Visibility = Visibility.Visible;
            }
        }
        private void ParcourirJava_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
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
                if (root?.SelectSingleNode("//Java") != null && root.SelectSingleNode("//Server") != null)
                {
                    if (JavaMin.Text != string.Empty)
                        // ReSharper disable once PossibleNullReferenceException
                        root.SelectSingleNode("//Java/Min").InnerText = JavaMin.Text;
                    if (JavaMax.Text != string.Empty)
                        // ReSharper disable once PossibleNullReferenceException
                        root.SelectSingleNode("//Java/Max").InnerText = JavaMax.Text;
                    if (JavaPerm.Text != string.Empty)
                        // ReSharper disable once PossibleNullReferenceException
                        root.SelectSingleNode("//Java/Perm").InnerText = JavaPerm.Text;
                    if (JavaChemin.Text != string.Empty)
                        // ReSharper disable once PossibleNullReferenceException
                        root.SelectSingleNode("//Java/Path").InnerText = JavaChemin.Text;
                    if (ServeurModPack.Text != string.Empty)
                        // ReSharper disable once PossibleNullReferenceException
                        root.SelectSingleNode("//Server/Url").InnerText = ServeurModPack.Text;
                    doc.Save("config.xml");
                }
                else
                {
                    MessageBox.Show(
                        "Il semblerait qu'il y ait un soucis avec votre fichier config.xml ! Supprimez le fichier dans le dossier de l'application et relancez l'application pour regénérer le fichier",
                        "Une erreur s'est produite",
                        MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
                }
            }
            else
            {
                MessageBox.Show(
                    "Il semblerait que le fichier config.xml est manquant ! Relancez l'application pour le régénérer",
                    "Une erreur s'est produite",
                    MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
            }
        }
        private void RecuperationStorage(object sender, EventArgs e)
        {
            List<string> logins = new List<string>();
            IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
            if (!isolatedStorage.FileExists("Logins")) return;
            StreamReader srReader = new StreamReader(new IsolatedStorageFileStream("Logins", FileMode.Open, isolatedStorage));
            Souvenir.IsChecked = true;
            while (!srReader.EndOfStream)
            {
                logins.Add(srReader.ReadLine());
            }
            LoginTextBox.Text = logins[0];
            PasswordBox.Password = logins[1];
        }
        private void SaveStorage(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool check = Souvenir.IsChecked ?? false;
            if (check)
            {
                if (!string.IsNullOrEmpty(LoginTextBox.Text) && !string.IsNullOrEmpty(PasswordBox.Password))
                {
                    Application.Current.Properties[0] = LoginTextBox.Text;
                    Application.Current.Properties[1] = PasswordBox.Password;
                    IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                    StreamWriter srWriter = new StreamWriter(new IsolatedStorageFileStream("Logins", FileMode.Create, isolatedStorage));
                    if (Application.Current.Properties[0] == null && Application.Current.Properties[1] == null) return;
                    //wriet to the isolated storage created in the above code section.
                    srWriter.WriteLine(Application.Current.Properties[0].ToString());
                    srWriter.WriteLine(Application.Current.Properties[1].ToString());
                    srWriter.Flush();
                    srWriter.Close();
                }
                else
                {
                    MessageBox.Show(
        "Il faut que les champs Login et Mot de passe soient remplis pour se souvenir de vous !",
        "Une erreur s'est produire",
        MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
                }
            }
            else
            {
                IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForAssembly();
                isolatedStorage.Remove();
            }
        }
        private void Resize(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
                ImageBrush im = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/restore.png")),
                    Stretch = Stretch.UniformToFill
                };
                Max.Background = im;
            }
            else
            {
                ImageBrush im = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/max.png")),
                    Stretch = Stretch.UniformToFill
                };
                Max.Background = im;
            }
        }
        private void LancerMinecraft(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
                {
                    MessageBox.Show("Le champ pour l'adresse mail ne peut pas être vide !", "Erreur");
                }
                else if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    MessageBox.Show("Le champ pour le mot de passe ne peut pas être vide !", "Erreur");
                }
                else
                {
                    UpdateMinecraft pb = new UpdateMinecraft(ModPack.Text, ServeurModPack.Text);
                    if (!pb.GetVersion()) return;
                    _session.PlayerName = LoginTextBox.Text;
                    ConnexionMinecraft.LoginResult result = ConnexionMinecraft.GetLogin(_session.PlayerName, PasswordBox.Password, out _session);
                    switch (result)
                    {
                        case ConnexionMinecraft.LoginResult.Success:
                            string librairies = string.Empty;
                            string versionMinecraft = string.Empty;
                            string versionAssets = string.Empty;
                            string forgeClass = string.Empty;
                            string[] cheminsLibrairies = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\" + ModPack.Text + @"\libraries\", "*", SearchOption.AllDirectories);
                            if (File.Exists("config.xml"))
                            {
                                XDocument mods = XDocument.Load("config.xml");
                                IEnumerable<XElement> elementsPatch = mods.Descendants().Where(x => x.Name == "Java");
                                string minimum = string.Empty;
                                string maximum = string.Empty;
                                string perm = string.Empty;
                                string chemin = string.Empty;
                                foreach (XElement content in elementsPatch)
                                {
                                    minimum = content.XPathSelectElement("Min").Value;
                                    maximum = content.XPathSelectElement("Max").Value;
                                    perm = content.XPathSelectElement("Perm").Value;
                                    chemin = content.XPathSelectElement("Path").Value;
                                }
                                if (string.IsNullOrWhiteSpace(minimum))
                                {
                                    minimum = "512";
                                }
                                if (string.IsNullOrWhiteSpace(maximum))
                                {
                                    maximum = "1024";
                                }
                                if (string.IsNullOrWhiteSpace(perm))
                                {
                                    perm = "128";
                                }
                                if (string.IsNullOrWhiteSpace(chemin))
                                {
                                    chemin = ConnexionMinecraft.GetJavaInstallationPath();
                                    chemin = Path.Combine(chemin, "bin\\Java.exe");
                                }
                                if (File.Exists(chemin))
                                {
                                    XDocument versionFile = XDocument.Load(Directory.GetCurrentDirectory() + @"\" + ModPack.Text + @"\modpack\" + ModPack.Text + @".xml");
                                    IEnumerable<XElement> versions = versionFile.Descendants().Where(x => x.Name == "Modpack");
                                    foreach (XElement content in versions)
                                    {
                                        versionMinecraft = content.XPathSelectElement("Minecraft").Value;
                                        versionAssets = content.XPathSelectElement("Assets").Value;
                                        forgeClass = content.XPathSelectElement("Forge").Value;
                                    }
                                    librairies = cheminsLibrairies.Aggregate(librairies, (current, f) => current + Quote + f + Quote + ";");
                                    string[] jars = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\" + ModPack.Text + @"\versions\", "*.jar", SearchOption.AllDirectories);
                                    librairies = jars.Aggregate(librairies, (current, f) => current + Quote + f + Quote + ";");
                                    librairies = librairies.Remove(librairies.Length - 1);
                                    string commande = @"-Xms" + minimum + @"m -Xmx" + maximum + @"m -XX:MaxPermSize=" + perm + " -Djava.library.path=" + Quote + Directory.GetCurrentDirectory() + @"\" + ModPack.Text + @"\natives" + Quote + " -cp " + librairies + " net.minecraft.launchwrapper.Launch --username " + _session.PlayerName + " --version " + versionMinecraft + " --gameDir " + Quote + Directory.GetCurrentDirectory() + @"\" + ModPack.Text + Quote + " --assetsDir " + Quote + Directory.GetCurrentDirectory() + @"\" + ModPack.Text + @"\assets" + Quote + " --assetIndex " + versionAssets + " --uuid " + _session.PlayerId + " --accessToken " + _session.Id + " --userProperties {} --userType mojang --tweakClass " + forgeClass;
                                    Process.Start(chemin, commande);
                                    /*ProcessStartInfo psi = new ProcessStartInfo();
                                psi.FileName = Chemin;
                                psi.UseShellExecute = false;
                                psi.Arguments = commande;
                                psi.RedirectStandardOutput = true;
                                using (Process pro = Process.Start(psi))
                                {
                                    using (StreamReader reader = pro.StandardOutput)
                                    {
                                        string res = reader.ReadToEnd();
                                        Debug.WriteLine(res);
                                    }
                                }*/
                                }
                            }
                            break;
                        case ConnexionMinecraft.LoginResult.InvalidToken:
                            MessageBox.Show("Le jeton de connexion est invalide !", "Une erreur s'est produite", MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
                            break;
                        case ConnexionMinecraft.LoginResult.LoginRequired:
                            MessageBox.Show("Le mail est requis pour la connexion au serveur Minecraft !", "Une erreur s'est produite", MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
                            break;
                        case ConnexionMinecraft.LoginResult.NotPremium:
                            MessageBox.Show("Il faut acheter Minecraft pour pouvoir jouer !", "Une erreur s'est produite", MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
                            break;
                        case ConnexionMinecraft.LoginResult.ServiceUnavailable:
                            MessageBox.Show("Le serveur de connexion Mojant semble indisponible !", "Une erreur s'est produite", MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
                            break;
                        case ConnexionMinecraft.LoginResult.WrongPassword:
                            MessageBox.Show("Vous avez rentré un mauvais mot de passe !", "Une erreur s'est produite", MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
                            break;
                        case ConnexionMinecraft.LoginResult.OtherError:
                            break;
                        case ConnexionMinecraft.LoginResult.SslError:
                            break;
                        case ConnexionMinecraft.LoginResult.AccountMigrated:
                            break;
                        case ConnexionMinecraft.LoginResult.NullError:
                            break;
                        default:
                            MessageBox.Show("Il semblerait qu'une erreur soit survenue lors de la connexion ! Veuillez réessayer", "Une erreur s'est produite", MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
                            break;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
        private void ListeMods(object sender, RoutedEventArgs e)
        {
            if (ServeurModPack.Text != null)
            {
                string xml1 = Directory.GetCurrentDirectory() + @"\" + @"ModpackList.xml";
                UpdateMinecraft.TelechargementFichiers(xml1, string.Concat(ServeurModPack.Text, @"modpack/", "ModpackList.xml"));
                string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                ModpackList.Source = new Uri(appPath + @"\ModpackList.xml");
            }
            else
            {
                MessageBox.Show(
    "Il semblerait que vous n'avez pas renseigné le serveur ! Supprimer le fichier config.xml et relancez l'application pour le régénérer",
    "Une erreur s'est produite",
    MessageBoxButton.OK, (Style)Resources["MessageBoxStyle1"]);
            }
        }
    }
}
