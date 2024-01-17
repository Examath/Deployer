using System;
using System.Collections.Generic;
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
using Deployer.Properties;

namespace Deployer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DeployerVM VM { get; set; } = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VM = (DeployerVM)DataContext;

            // File selected
            if (((App)Application.Current).Args?.Length >= 1)
            {
                string fileLocation = ((App)Application.Current).Args[0];
                if (Path.GetExtension(fileLocation) == ".xlsx")
                {
                    VM.BomLocation = fileLocation;
                }
            }

            if (!VM.Load())
            {
                Settings.Default.Save();
                Close();
            }
        }

        private void Deploy_Click(object sender, RoutedEventArgs e)
        {
            if (VM.Deploy() == true)
            {
                Settings.Default.Save();
                Close();
            }
        }
    }
}
