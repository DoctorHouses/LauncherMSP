using ICSharpCode.SharpZipLib.Zip;
using Microsoft.XmlDiffPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace LauncherMinecraft
{
    class MinecraftInstall
    {
        public static void GetVersion()
        {
            string xml1 = Directory.GetCurrentDirectory() + @"\Minecraft\MSP\MSP.xml";
            string xml2 = Directory.GetCurrentDirectory() + @"\Minecraft\MSP\MSP_Update.xml";
            if (!File.Exists(xml1))
            {
                TelechargementFichiers(xml1, "http://mcpatch.more-salt-plz.fr/Client/MSP.xml", false);
                var X1 = new XmlDocument();
                X1.Load(xml1);
                MajMinecraft(X1.DocumentElement.SelectSingleNode("Minecraft").InnerText);
            }
            else
            {
                TelechargementFichiers(xml2, "http://mcpatch.more-salt-plz.fr/Client/MSP.xml", false);
                var X1 = new XmlDocument();
                var X2 = new XmlDocument();
                X1.Load(xml1);
                X2.Load(xml2);
                if (X1.DocumentElement.SelectSingleNode("Minecraft").InnerText != X2.DocumentElement.SelectSingleNode("Minecraft").InnerText)
                {
                    MajMinecraft(X2.DocumentElement.SelectSingleNode("Minecraft").InnerText);
                }
                if (X1.DocumentElement.SelectSingleNode("Modpack").InnerText != X2.DocumentElement.SelectSingleNode("Modpack").InnerText)
                {
                    Debug.Write(X1.DocumentElement.SelectSingleNode("Modpack").InnerText);
                    Debug.Write(X2.DocumentElement.SelectSingleNode("Modpack").InnerText);
                }
                File.Delete(xml1);
                File.Move(xml2, xml1);
            }
        }
        public static void MajMinecraft(string version)
        {
            StreamReader fichier;
            JsonTextReader lecture;
            JObject Contenu;
            JObject ContenuP;
            JToken Resultat;
            DirectoryInfo dossier;
            string chemin;
            SuppressionArboresence(Directory.GetCurrentDirectory() + @"\Minecraft\assets\");
            SuppressionArboresence(Directory.GetCurrentDirectory() + @"\Minecraft\libraries\");
            SuppressionArboresence(Directory.GetCurrentDirectory() + @"\Minecraft\natives\");
            SuppressionArboresence(Directory.GetCurrentDirectory() + @"\Minecraft\versions\");
            //Téléchargement du manifeste des versions Minecraft
            #region Manifeste
            chemin = @"Minecraft\versions\";
            dossier = Directory.CreateDirectory(chemin);
            using (var telechargement = new WebClient())
            {
                telechargement.DownloadFile("https://launchermeta.mojang.com/mc/game/version_manifest.json", @"Minecraft\versions\version_manifest.json");
            }
            // read JSON directly from a file
            using (fichier = File.OpenText(@"Minecraft\versions\version_manifest.json"))
            using (lecture = new JsonTextReader(fichier))
            {
                Contenu = (JObject)JToken.ReadFrom(lecture);
                string request = "$.versions[?(@.id == '" + version + "')]";
                Resultat = Contenu.SelectToken(request);
                using (var telechargement = new WebClient())
                {
                    telechargement.DownloadFile((string)Resultat.SelectToken("$.url"), @"Minecraft\versions\"+version+".json");
                }
            }
            #endregion
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
            string log = Directory.GetCurrentDirectory() + @"\Minecraft\logs\latest.log";
            if (!File.Exists(log))
            {
                File.Create(log);
            }
            #endregion
            #region Librairies & Assets
            using (fichier = File.OpenText(@"Minecraft\versions\"+version+".json"))
            using (lecture = new JsonTextReader(fichier))
            {
                ContenuP = (JObject)JToken.ReadFrom(lecture);
            }
            #region Téléchargement des assets
            using (var telechargement = new WebClient())
            {
                telechargement.DownloadFile((string)ContenuP.SelectToken("assetIndex.url"), @"Minecraft\assets\indexes\" + (string)ContenuP.SelectToken("assetIndex.id") + ".json");
                telechargement.DownloadFile((string)ContenuP.SelectToken("downloads.client.url"), @"Minecraft\versions\"+version+".jar");
            }

            using (fichier = File.OpenText(@"Minecraft\assets\indexes\" + (string)ContenuP.SelectToken("assetIndex.id") + ".json"))
            using (lecture = new JsonTextReader(fichier))
            {
                Contenu = (JObject)JToken.ReadFrom(lecture);
            }
            char[] hash2;
            string hash;
            foreach (JProperty prop in Contenu.SelectToken("objects"))
            {
                hash = (string)prop.Value.SelectToken("hash");
                hash2 = hash.Take(2).ToArray();
                chemin = @"Minecraft\assets\objects\" + new string(hash2) + @"\";
                dossier = Directory.CreateDirectory(chemin);
                using (var client = new WebClient())
                {
                    string url = "http://resources.download.minecraft.net/" + new string(hash2) + @"/" + hash;
                    client.DownloadFile(url, chemin + hash);
                }
            }
            #endregion
            #region Téléchargement et extraction des librairies
            foreach (JObject prop in ContenuP.SelectToken("libraries"))
            {
                if (prop.SelectToken("downloads.classifiers.natives-windows") != null)
                {
                    if (prop.SelectToken("rules") != null)
                    {
                        foreach (JObject règle in prop.SelectToken("rules"))
                        {
                            if (((string)règle.SelectToken("action") == "Allow" && (string)règle.SelectToken("rules.os") == "windows") || ((string)règle.SelectToken("action") == "disallow" && (string)règle.SelectToken("rules.os") != "windows"))
                            {
                                if (prop.SelectToken("downloads.classifiers.natives-windows.url") != null)
                                {
                                    if (prop.SelectToken("extract.exclude") != null)
                                    {
                                        TelechargementFichiers(@"Minecraft\libraries\" + (string)prop.SelectToken("downloads.classifiers.natives-windows.path"), (string)prop.SelectToken("downloads.classifiers.natives-windows.url"), true);
                                    }
                                    else
                                    {
                                        TelechargementFichiers(@"Minecraft\libraries\" + (string)prop.SelectToken("downloads.classifiers.natives-windows.path"), (string)prop.SelectToken("downloads.classifiers.natives-windows.url"), false);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (prop.SelectToken("extract.exclude") != null)
                        {
                            TelechargementFichiers(@"Minecraft\libraries\" + (string)prop.SelectToken("downloads.classifiers.natives-windows.path"), (string)prop.SelectToken("downloads.classifiers.natives-windows.url"), true);
                        }
                        else
                        {
                            TelechargementFichiers(@"Minecraft\libraries\" + (string)prop.SelectToken("downloads.classifiers.natives-windows.path"), (string)prop.SelectToken("downloads.classifiers.natives-windows.url"), false);
                        }
                    }
                }
                else if (prop.SelectToken("downloads.artifact.path") != null && prop.SelectToken("downloads.artifact.url") != null)
                {
                    if (prop.SelectToken("rules") != null)
                    {
                        foreach (JObject règle in prop.SelectToken("rules"))
                        {
                            if ((string)règle.SelectToken("action") == "Allow" && (string)règle.SelectToken("rules.os") == "windows" || (string)règle.SelectToken("action") == "disallow" && (string)règle.SelectToken("rules.os") != "windows")
                            {
                                if (prop.SelectToken("downloads.artifact") != null)
                                {
                                    if (prop.SelectToken("extract.exclude") != null)
                                    {
                                        TelechargementFichiers(@"Minecraft\libraries\" + (string)prop.SelectToken("downloads.artifact.path"), (string)prop.SelectToken("downloads.artifact.url"), true);
                                    }
                                    else
                                    {
                                        TelechargementFichiers(@"Minecraft\libraries\" + (string)prop.SelectToken("downloads.artifact.path"), (string)prop.SelectToken("downloads.artifact.url"), false);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (prop.SelectToken("extract.exclude") != null)
                        {
                            TelechargementFichiers(@"Minecraft\libraries\" + (string)prop.SelectToken("downloads.artifact.path"), (string)prop.SelectToken("downloads.artifact.url"), true);
                        }
                        else
                        {
                            TelechargementFichiers(@"Minecraft\libraries\" + (string)prop.SelectToken("downloads.artifact.path"), (string)prop.SelectToken("downloads.artifact.url"), false);
                        }
                    }
                }
            }
            #endregion
            #endregion
        }
        public static void ExtractionJar(string chemin)
        {
            FastZip fz = new FastZip();
            chemin = chemin.Replace(@"/", @"\");
            fz.ExtractZip(chemin, Directory.GetCurrentDirectory() + @"\Minecraft\natives", null);
            SuppressionArboresence(Directory.GetCurrentDirectory() + @"\Minecraft\natives\META-INF\");
        }
        public static void TelechargementFichiers(string path, string url, bool extraction)
        {
            string chemin = Path.GetDirectoryName(path);
            DirectoryInfo dossier = Directory.CreateDirectory(chemin);
            using (var client = new WebClient())
            {
                client.DownloadFile(url, path);
            }
            if (extraction)
            {
                ExtractionJar(Directory.GetCurrentDirectory()+@"\"+path);
            }
        }
        public static void SuppressionArboresence(string chemin)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(chemin);
            if (di.Exists)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                di.Delete();
            }
        }
    }
}
