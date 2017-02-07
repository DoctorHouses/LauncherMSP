using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace LauncherMinecraftV3
{
    internal class Patcher
    {
        public static void GenerationXml(string modpack)
        {
            string chemin = Directory.GetCurrentDirectory();
            string cheminEntier = Path.Combine(chemin, modpack);
            /*if (!File.Exists(cheminEntier + @"\" + modpack + @"modpack\filelist.xml"))
            {
                File.Create(cheminEntier + @"\modpack\filelist.xml");
            }*/
            XElement fileSystemTree = CreateFileSystemXmlTree(cheminEntier);
            fileSystemTree.Elements().Where(el => (string)el.Attribute("Nom") != "config" && (string)el.Attribute("Nom") != "mods" && (string)el.Attribute("Nom") != "libraries" && (string)el.Attribute("Nom") != "natives" && (string)el.Attribute("Nom") != "versions" && (string)el.Attribute("Nom") != "assets").Remove();
            fileSystemTree.Save(cheminEntier +@"\modpack\filelist.xml");
        }

        private static XElement CreateFileSystemXmlTree(string source)
        {
            DirectoryInfo di = new DirectoryInfo(source);
            return new XElement("Dossier",
                new XAttribute("Nom", di.Name),
                from d in Directory.GetDirectories(source)
                select CreateFileSystemXmlTree(d),
                from fi in di.GetFiles()
                select new XElement("Fichier",
                    new XElement("Nom", fi.Name),
                    new XElement("MD5", GenerationMd5(fi.FullName)),
                    new XElement("Chemin", CheminRelatif(fi.FullName + fi.Name))
                )
            );
        }

        private static string GenerationMd5(string fichier)
        {
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(fichier))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        private static string CheminRelatif(string absolu)
        {
            string chemin = Environment.CurrentDirectory;
            return absolu.StartsWith(chemin) ? absolu.Substring(Environment.CurrentDirectory.Length) : absolu;
        }
    }
}
