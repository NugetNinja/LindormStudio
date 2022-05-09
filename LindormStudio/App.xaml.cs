using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LindormStudio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static App Current { get => (App)Application.Current; }
        public IConfiguration Configuration { get; }
        public App()
        {
            Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                                      .AddUserSecrets<App>()
                                                      .Build();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
    }
}
