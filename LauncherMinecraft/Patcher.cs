using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace LauncherMinecraft
{
    class Patcher
    {
        public static void GenerationXML()
        {
            string chemin = Directory.GetCurrentDirectory();
            string chemin_entier = System.IO.Path.Combine(chemin, @"Minecraft");
            XElement fileSystemTree = CreateFileSystemXmlTree(chemin_entier);
            fileSystemTree.Elements().Where(el => (string)el.Attribute("Nom") != "config" && (string)el.Attribute("Nom") != "mods" && (string)el.Attribute("Nom") != "libraries" && (string)el.Attribute("Nom") != "natives" && (string)el.Attribute("Nom") != "versions" && (string)el.Attribute("Nom") != "assets").Remove();
            fileSystemTree.Save(chemin_entier + @"\MSP\Modpack.xml");
        }
        static XElement CreateFileSystemXmlTree(string source)
        {
            DirectoryInfo di = new DirectoryInfo(source);
            return new XElement("Dossier",
                new XAttribute("Nom", di.Name),
                from d in Directory.GetDirectories(source)
                select CreateFileSystemXmlTree(d),
                from fi in di.GetFiles()
                select new XElement("Fichier",
                    new XElement("Nom", fi.Name),
                    new XElement("MD5", GenerationMD5(fi.FullName)),
                    new XElement("Chemin", CheminRelatif(fi.FullName+fi.Name))
                )
            );
        }
        static string GenerationMD5(string fichier)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fichier))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }
        static string CheminRelatif(string absolu)
        {
            string chemin = Environment.CurrentDirectory;
            if (absolu.StartsWith(chemin))
            {
                return absolu.Substring(Environment.CurrentDirectory.Length);
            }
            return absolu;
        }
    }
}
