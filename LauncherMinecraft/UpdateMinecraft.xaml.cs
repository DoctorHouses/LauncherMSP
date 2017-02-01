using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace LauncherMinecraft
{
    /// <summary>
    /// Logique d'interaction pour UpdateMinecraft.xaml
    /// </summary>
    public partial class UpdateMinecraft : Window
    {
        public UpdateMinecraft()
        {
            InitializeComponent();
        }

        public void GetVersion()
        {
            DirectoryInfo dossier;
            string chemin;
            chemin = @"Minecraft\";
            dossier = Directory.CreateDirectory(chemin);
            string xml1 = Directory.GetCurrentDirectory() + @"\Minecraft\MSP\MSP.xml";
            string xml2 = Directory.GetCurrentDirectory() + @"\Minecraft\MSP\MSP_Update.xml";
            Dispatcher.Invoke(new Action(() => {
                this.UpdateProgression.Value = 1;
                this.Progress.Content = "1/4 : Vérification des mises à jours";
            }));
            if (!File.Exists(xml1))
            {
                TelechargementFichiers(xml1, "http://mcpatch.more-salt-plz.fr/Client/MSP.xml");
                var X1 = new XmlDocument();
                X1.Load(xml1);
                this.MajMinecraft();
            }
            else
            {
                TelechargementFichiers(xml2, "http://mcpatch.more-salt-plz.fr/Client/MSP.xml");
                var X1 = new XmlDocument();
                var X2 = new XmlDocument();
                X1.Load(xml1);
                X2.Load(xml2);
                if (X1.DocumentElement.SelectSingleNode("Modpack").InnerText != X2.DocumentElement.SelectSingleNode("Modpack").InnerText)
                {
                    this.MajMinecraft();
                }
                File.Delete(xml1);
                File.Move(xml2, xml1);
            }
        }
        public static void TelechargementFichiers(string path, string url)
        {
            try
            {
                string chemin = System.IO.Path.GetDirectoryName(path);
                DirectoryInfo dossier = Directory.CreateDirectory(chemin);
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, path);
                }
            }
            catch
            {
                System.Diagnostics.Debug.Write("Problème rencontré lors du téléchargement");
            }
        }

        private void MajMinecraft()
        {
            DirectoryInfo dossier;
            string chemin;
            Dispatcher.Invoke(new Action(() => {
                this.UpdateProgression.Value = 2;
                this.Progress.Content = "2/4 : Vérification de l'existence des dossiers";
            }));

            #region Création des dossiers Minecraft
            chemin = @"Minecraft\assets\indexes\";
            dossier = Directory.CreateDirectory(chemin);
            chemin = @"Minecraft\assets\objects\";
            dossier = Directory.CreateDirectory(chemin);
            chemin = @"Minecraft\libraries\";
            dossier = Directory.CreateDirectory(chemin);
            chemin = @"Minecraft\logs\";
            dossier = Directory.CreateDirectory(chemin);
            chemin = @"Minecraft\resourcepacks\";
            dossier = Directory.CreateDirectory(chemin);
            chemin = @"Minecraft\saves\";
            dossier = Directory.CreateDirectory(chemin);
            chemin = @"Minecraft\server-resource-packs\";
            dossier = Directory.CreateDirectory(chemin);
            chemin = @"Minecraft\mods\";
            dossier = Directory.CreateDirectory(chemin);
            chemin = @"Minecraft\config\";
            dossier = Directory.CreateDirectory(chemin);
            chemin = @"Minecraft\natives\";
            dossier = Directory.CreateDirectory(chemin);
            TelechargementFichiers(Directory.GetCurrentDirectory() + @"\Minecraft\MSP\ListeMods.xml", "http://mcpatch.more-salt-plz.fr/Client/ModList.xml");
            Patcher.GenerationXML();
            XDocument Patch = XDocument.Load("http://mcpatch.more-salt-plz.fr/Client/Patcher.xml");
            XDocument Local = XDocument.Load(Directory.GetCurrentDirectory() + @"\Minecraft\MSP\ModPack.xml");
            var ElementsPatch = Patch.Descendants().Where(x => x.Name == "Fichier");
            var ElementsLocal = Patch.Descendants().Where(x => x.Name == "Fichier");
            Dispatcher.Invoke(new Action(() => {
                this.UpdateProgression.Value = 3;
                this.Progress.Content = "3/4 : Téléchargement des fichiers";
            }));
            foreach (XContainer X in ElementsPatch)
            {
                var ids = X.Element("MD5").Value;
                var result = Local.Descendants("Fichier").FirstOrDefault(x => (string)x.Element("MD5") == ids);
                if (result == null)
                {
                    Dispatcher.Invoke(new Action(() => {
                        this.Telechargement.Content = X.Element("Nom").Value;
                    }));

                    TelechargementFichiers(Directory.GetCurrentDirectory() + @"\Minecraft" + X.Element("Chemin").Value, "http://mcpatch.more-salt-plz.fr/Client/" + X.Element("Chemin").Value);
                }
            }
            foreach (XContainer X in ElementsLocal)
            {
                var ids = X.Element("MD5").Value;
                var result = Patch.Descendants("Fichier").FirstOrDefault(x => (string)x.Element("MD5") == ids);
                if (result == null)
                {
                    File.Delete(Directory.GetCurrentDirectory() + @"\Minecraft" + X.Element("Chemin").Value);
                }
            }
            #endregion
            Dispatcher.Invoke(new Action(() => {
                this.UpdateProgression.Value = 4;
                this.Progress.Content = "4/4 : Fin de la mise à jour";
            }));
            Patcher.GenerationXML();
        }

        private void Ouverture(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate (object s, DoWorkEventArgs args)
            {
                this.GetVersion();
            };

            // RunWorkerCompleted will fire on the UI thread when the background process is complete
            worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                if (args.Error != null)
                {
                    // an exception occurred on the background process, do some error handling
                }
                this.Close();
            };
            worker.RunWorkerAsync();
            
        }
    }
}
