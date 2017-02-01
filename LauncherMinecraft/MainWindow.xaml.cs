using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace LauncherMinecraft
{

    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string quote = "\"";
        SessionToken session = new SessionToken();
        public MainWindow()
        {
            UpdateMinecraft PB = new UpdateMinecraft();
            PB.Show();
            InitializeComponent();
        }
        private void LancerMinecraft(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Mail.Text))
            {
                MessageBox.Show("Le champ pour l'adresse mail ne peut pas être vide !", "Erreur");
            }
            else if (String.IsNullOrWhiteSpace(MotDePasse.Password))
            {
                MessageBox.Show("Le champ pour le mot de passe ne peut pas être vide !", "Erreur");
            }
            else
            {
                session.PlayerName = Mail.Text;
                ConnexionMinecraft.LoginResult result = ConnexionMinecraft.GetLogin(session.PlayerName, MotDePasse.Password, out session);
                if (result == ConnexionMinecraft.LoginResult.Success)
                {
                    string AssetID = String.Empty;
                    string MainClass = String.Empty;
                    string librairies = String.Empty;
                    string version_minecraft = String.Empty;
                    string version_assets = String.Empty;
                    string[] chemins_librairies = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Minecraft\libraries\", "*", SearchOption.AllDirectories);
                    if (File.Exists("config.xml"))
                    {
                        XDocument Mods = XDocument.Load("config.xml");
                        var ElementsPatch = Mods.Descendants().Where(x => x.Name == "Java");
                        string Minimum = String.Empty;
                        string Maximum = String.Empty;
                        string Perm = String.Empty;
                        string Chemin = String.Empty;
                        foreach (XElement X in ElementsPatch)
                        {
                            Minimum = X.Attribute("Min").Value.ToString();
                            Maximum = X.Attribute("Max").Value.ToString();
                            Perm = X.Attribute("Perm").Value.ToString();
                            Chemin = X.Attribute("Chemin").Value.ToString();
                        }
                        if (String.IsNullOrWhiteSpace(Minimum))
                        {
                            Minimum = "512";
                        }
                        if (String.IsNullOrWhiteSpace(Maximum))
                        {
                            Maximum = "1024";
                        }
                        if (String.IsNullOrWhiteSpace(Perm))
                        {
                            Perm = "128";
                        }
                        if (String.IsNullOrWhiteSpace(Chemin))
                        {
                            Chemin = ConnexionMinecraft.GetJavaInstallationPath();
                            Chemin = System.IO.Path.Combine(Chemin, "bin\\Java.exe");
                            if (Chemin == null)
                            {
                                MessageBox.Show("Il semblerait que Java ne soit pas installé sur votre machine,Erreur Java");
                            }
                        }
                        if (System.IO.File.Exists(Chemin))
                        {
                            XDocument VersionFile = XDocument.Load(Directory.GetCurrentDirectory() + @"\Minecraft\MSP\MSP.xml");
                            var Versions = Mods.Descendants().Where(x => x.Name == "MSP");
                            foreach (XElement X in Versions)
                            {
                                version_minecraft = X.Attribute("Minecraft").Value.ToString();
                                version_assets = X.Attribute("Assets").Value.ToString();
                            }
                            foreach (string f in chemins_librairies)
                            {
                                librairies = librairies + quote + f + quote + ";";
                            }
                            string[] jars = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Minecraft\versions\", "*.jar", SearchOption.AllDirectories);
                            foreach (string f in jars)
                            {
                                librairies = librairies + quote + f + quote + ";";
                            }
                            string commande = @"-Xms"+Minimum+@"m -Xmx"+Maximum+ @"m -XX:MaxPermSize="+ Perm +"-Djava.library.path=" + quote + Directory.GetCurrentDirectory() + @"\Minecraft\natives" + quote + " -cp " + librairies + " net.minecraft.launchwrapper.Launch --username " + session.PlayerName + " --version"+ version_minecraft +"--gameDir " + quote + Directory.GetCurrentDirectory() + @"\minecraft" + quote + " --assetsDir " + quote + Directory.GetCurrentDirectory() + @"\minecraft\assets" + quote + " --assetIndex" + version_assets + "--uuid " + session.PlayerID + " --accessToken " + session.ID + " --userProperties {} --userType mojang --tweakClass cpw.mods.fml.common.launcher.FMLTweaker";
                            Process.Start(Chemin, commande);
                        }
                    }
                }
                else if (result == ConnexionMinecraft.LoginResult.InvalidToken)
                {
                    MessageBox.Show("Le jeton de connexion est invalide ! Veuillez réessayer ou vous renseigner sur le Discord MSP", "Erreur");
                }
                else if (result == ConnexionMinecraft.LoginResult.LoginRequired)
                {
                    MessageBox.Show("Le mail est requis pour la connexion au serveur Minecraft !", "Erreur");
                }
                else if (result == ConnexionMinecraft.LoginResult.NotPremium)
                {
                    MessageBox.Show("Il faut acheter Minecraft pour pouvoir jouer !", "Erreur");
                }
                else if (result == ConnexionMinecraft.LoginResult.ServiceUnavailable)
                {
                    MessageBox.Show("Le serveur de connexion Mojant semble indisponible !", "Erreur");
                }
                else if (result == ConnexionMinecraft.LoginResult.WrongPassword)
                {
                    MessageBox.Show("Vous avez rentré un mauvais mot de passe !", "Erreur");
                }
                else
                {
                    MessageBox.Show("Il semblerait qu'une erreur soit survenue lors de la connexion ! Veuillez réessayer ou vous renseigner sur le Discord MSP", "Erreur");
                }
            }
        }

        private void Chargement(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Directory.GetCurrentDirectory() + @"\Minecraft\MSP\ListeMods.xml"))
            {
                XDocument Mods = XDocument.Load(Directory.GetCurrentDirectory() + @"\Minecraft\MSP\ListeMods.xml");
                var ElementsPatch = Mods.Descendants().Where(x => x.Name == "Mod");
                foreach (XElement X in ElementsPatch)
                {
                    ListBoxItem itm = new ListBoxItem();
                    itm.Content = X.Attribute("Nom").Value.ToString() + " " + X.Value.ToString();
                    ModList.Items.Add(itm);
                }
            }
        }

        private void OuvertureOptions(object sender, RoutedEventArgs e)
        {
            OptionsJava FenJava = new OptionsJava();
            FenJava.ShowDialog();
        }

        private void Fermer(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
