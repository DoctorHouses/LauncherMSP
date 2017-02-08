using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace LauncherMinecraftV3
{
    /// <summary>
    /// Logique d'interaction pour UpdateMinecraft.xaml
    /// </summary>
    public class UpdateMinecraft
    {
        private readonly string _modpack;
        private readonly string _serveur;
        public UpdateMinecraft(string modpack, string serveur)
        {
            _modpack = modpack;
            _serveur = serveur;
        }
        public bool GetVersion()
        {
            try
            {
                string chemin = _modpack + @"\";
                if (Directory.Exists(chemin))
                {
                    string xml1 = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\modpack\" + _modpack + @".xml";
                    string xml2 = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\modpack\" + _modpack + @"_update.xml";
                    if (!File.Exists(xml1))
                    {
                        TelechargementFichiers(xml1, string.Concat(_serveur, @"modpack/", _modpack, @"/", _modpack, ".xml"));
                        XmlDocument x1 = new XmlDocument();
                        x1.Load(xml1);
                        return MajMinecraft();
                    }
                    else
                    {
                        TelechargementFichiers(xml2, string.Concat(_serveur, @"modpack/", _modpack, @"/", _modpack, ".xml"));
                        XmlDocument x1 = new XmlDocument();
                        XmlDocument x2 = new XmlDocument();
                        x1.Load(xml1);
                        x2.Load(xml2);
                        DateTime time1 = DateTime.ParseExact(x1.DocumentElement?.SelectSingleNode("LastUpdate")?.InnerText, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime time2 = DateTime.ParseExact(x2.DocumentElement?.SelectSingleNode("LastUpdate")?.InnerText, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
                        bool state = true;
                        if (time1 < time2)
                        {
                            state = MajMinecraft();
                        }
                        File.Delete(xml1);
                        File.Move(xml2, xml1);
                        return state;
                    }
                }
                Directory.CreateDirectory(chemin);
                string fichier = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\" + _modpack + @".zip";
                TelechargementFichiers(fichier, string.Concat(_serveur, @"modpack/", _modpack, @"/", _modpack, ".zip"));
                ZipFile.ExtractToDirectory(fichier,Directory.GetCurrentDirectory() + @"\" + _modpack + @"\");
                File.Delete(fichier);
                fichier = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\modpack\" + @"filelist.xml";
                TelechargementFichiers(fichier, string.Concat(_serveur, @"modpack/", _modpack, @"/filelist.xml"));
                fichier = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\modpack\" + _modpack + @".xml";
                TelechargementFichiers(fichier, string.Concat(_serveur, @"modpack/", _modpack,_modpack,@"/.xml"));
                return true;
            }
            catch
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(
    "Il semblerait qu'un problème est survenu ! Peut-être un fichier manquant",
    "Une erreur s'est produite",
    MessageBoxButton.OK, (Style)Application.Current.FindResource("MessageBoxStyle1"));
                return false;
            }
            
        }
        public static void TelechargementFichiers(string path, string url)
        {
            try
            {
                string chemin = Path.GetDirectoryName(path);
                if (chemin != null) Directory.CreateDirectory(chemin);
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, path);
                }
            }
            catch
            {
                Xceed.Wpf.Toolkit.MessageBox.Show(
    "Problème rencontré lors du téléchargement",
    "Une erreur s'est produite",
    MessageBoxButton.OK, (Style)Application.Current.FindResource("MessageBoxStyle1"));
            }
        }
        private bool MajMinecraft()
        {
            try
            {
                #region Création des dossiers Minecraft
                string chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\assets\indexes\";
                Directory.CreateDirectory(chemin);
                chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\assets\objects\";
                Directory.CreateDirectory(chemin);
                chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\libraries\";
                Directory.CreateDirectory(chemin);
                chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\logs\";
                Directory.CreateDirectory(chemin);
                chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\resourcepacks\";
                Directory.CreateDirectory(chemin);
                chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\saves\";
                Directory.CreateDirectory(chemin);
                chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\server-resource-packs\";
                Directory.CreateDirectory(chemin);
                chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\mods\";
                Directory.CreateDirectory(chemin);
                chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\config\";
                Directory.CreateDirectory(chemin);
                chemin = Directory.GetCurrentDirectory() + @"\" + _modpack + @"\natives\";
                Directory.CreateDirectory(chemin);
                Patcher.GenerationXml(_modpack);
                string url = string.Concat(_serveur, @"/modpack/", _modpack, @"/filelist.xml");
                XDocument patch = XDocument.Load(url);
                XDocument local = XDocument.Load(string.Concat(Directory.GetCurrentDirectory(), @"\", _modpack, @"\modpack\filelist.xml"));
                IEnumerable<XElement> elementsPatch = patch.Descendants().Where(x => x.Name == "Fichier");
                IEnumerable<XElement> elementsLocal = patch.Descendants().Where(x => x.Name == "Fichier");
                foreach (XElement content in elementsPatch)
                {
                    string ids = content.Element("MD5")?.Value;
                    XElement result = local.Descendants("Fichier").FirstOrDefault(x => (string)x.Element("MD5") == ids);
                    if (result != null) continue;
                    TelechargementFichiers(Directory.GetCurrentDirectory() + @"\" + _modpack + content.Element("Chemin")?.Value, _serveur + @"/" + _modpack + content.Element("Chemin")?.Value);
                }
                foreach (XElement content in elementsLocal)
                {
                    string ids = content.Element("MD5")?.Value;
                    XElement result = patch.Descendants("Fichier").FirstOrDefault(x => (string)x.Element("MD5") == ids);
                    if (result == null)
                    {
                        File.Delete(Directory.GetCurrentDirectory() + @"\" + _modpack + content.Element("Chemin")?.Value);
                    }
                }
                #endregion
                Patcher.GenerationXml(_modpack);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
