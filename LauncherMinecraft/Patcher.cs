using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
            fileSystemTree.Elements().Where(el => (string)el.Attribute("Nom") != "config" && (string)el.Attribute("Nom") != "mods").Remove();
            fileSystemTree.Save("test.xml");
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
                    new XElement("Chemin", CheminRelatif(fi.FullName))
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
            string DossierActuel = Environment.CurrentDirectory+@"\Minecraft";
            if(absolu.StartsWith(DossierActuel))
            {
                return absolu.Substring(DossierActuel.Length + 1);
            }
            return absolu;
        }
        public static void Comparaison(string Original,string Update)
        {
            var node1 = XElement.Load(Original).CreateReader();
            var node2 = XElement.Load(Update).CreateReader();
            var result = new XDocument();
            var writer = result.CreateWriter();
            var diff = new Microsoft.XmlDiffPatch.XmlDiff();
            diff.Compare(node1, node2, writer);
            writer.Flush();
            writer.Close();
        }
    }
}
