using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;

namespace LauncherMinecraft
{
    /// <summary>
    /// Logique d'interaction pour OptionsJava.xaml
    /// </summary>
    public partial class OptionsJava : Window
    {
        public OptionsJava()
        {
            InitializeComponent();
        }

        private void OpenDialog(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    CheminJava.Text = dialog.SelectedPath;
                }
            }
        }

        private void Quitter(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            new XDocument(
            new XElement("Java",
                new XElement("Min", JavaMin.Text),
                new XElement("Max", JavaMax.Text),
                new XElement("Perm", PermGen.Text),
                new XElement("Chemin", CheminJava.Text)
            )
            ).Save("config.xml");
            this.Close();
        }

        private void Chargement(object sender, RoutedEventArgs e)
        {
            if (File.Exists("config.xml"))
            {
                XDocument Mods = XDocument.Load("config.xml");
                var ElementsPatch = Mods.Descendants().Where(x => x.Name == "Java");
                foreach (XElement X in ElementsPatch)
                {
                    JavaMin.Text = X.Attribute("Min").Value.ToString();
                    JavaMax.Text = X.Attribute("Max").Value.ToString();
                    PermGen.Text = X.Attribute("Perm").Value.ToString();
                    CheminJava.Text = X.Attribute("Chemin").Value.ToString();
                }
                if (String.IsNullOrWhiteSpace(JavaMin.Text))
                {
                    JavaMin.Text = "512";
                }
                if (String.IsNullOrWhiteSpace(JavaMax.Text))
                {
                    JavaMax.Text = "1024";
                }
                if (String.IsNullOrWhiteSpace(PermGen.Text))
                {
                    PermGen.Text = "128";
                }
            }
            else
            {
                JavaMin.Text = "512";
                JavaMax.Text = "1024";
                PermGen.Text = "128";
            }
        }
    }
}
