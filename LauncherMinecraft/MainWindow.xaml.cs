using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            session.PlayerName = "didderen.jeremy@gmail.com";
            string Password = "X3TY8Z39Nyp2gOA7Rohe";
            MinecraftInstall.GetVersion();
            ConnexionMinecraft.LoginResult result = ConnexionMinecraft.GetLogin(session.PlayerName, Password, out session);
            LancerMinecraft();
            InitializeComponent();
        }

        public void LancerMinecraft()
        {
            StreamReader fichier;
            JsonTextReader lecture;
            JObject ContenuP;
            string AssetID = String.Empty;
            string MainClass = String.Empty;
            string librairies = String.Empty;
            using (fichier = File.OpenText(@"Minecraft\versions\1.7.10.json"))
            using (lecture = new JsonTextReader(fichier))
            {
                ContenuP = (JObject)JToken.ReadFrom(lecture);
            }
            AssetID = (string)ContenuP.SelectToken("assetIndex.id");
            string[] chemins_librairies = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Minecraft\libraries\", "*", SearchOption.AllDirectories);
            foreach (string f in chemins_librairies)
            {
                librairies = librairies + quote + f + quote + ";";
                Debug.Write(f);
                Debug.Write("\n");
            }
            string installPath = ConnexionMinecraft.GetJavaInstallationPath();
            if (installPath == null)
            {
                MessageBoxResult result = MessageBox.Show("Il semblerait que Java ne soit pas installé sur votre machine,Erreur Java");
            }
            else
            {
                string filePath = System.IO.Path.Combine(installPath, "bin\\Java.exe");
                if (System.IO.File.Exists(filePath))
                {
                    string[] jars = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Minecraft\versions\", "*.jar", SearchOption.AllDirectories);
                    foreach (string f in jars)
                    {
                        librairies = librairies + quote + f + quote + ";";
                    }
                    string commande = @"-Xms512m -Xmx1024m -Djava.library.path=" + quote + Directory.GetCurrentDirectory() + @"\Minecraft\natives" + quote + " -cp " + librairies + " net.minecraft.launchwrapper.Launch --username " + session.PlayerName + " --version 1.7.10 --gameDir " + quote + Directory.GetCurrentDirectory() + @"\minecraft" + quote + " --assetsDir " + quote + Directory.GetCurrentDirectory() + @"\minecraft\assets" + quote + " --assetIndex 1.7.10 --uuid " + session.PlayerID + " --accessToken " + session.ID + " --userProperties {} --userType mojang --tweakClass cpw.mods.fml.common.launcher.FMLTweaker";
                    Process.Start(filePath, commande);
                }
            }
        }

    }
}
