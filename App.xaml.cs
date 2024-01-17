using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Deployer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the arguments when this app starts. the first argument is usually file location.
        /// </summary>
        public string[] Args { get; private set; } = Array.Empty<string>();

        protected override void OnStartup(StartupEventArgs e)
        {
            Args = e.Args;
            base.OnStartup(e);
        }
    }
}
