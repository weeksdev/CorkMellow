using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CorkMellow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var manager = MahApps.Metro.ThemeManager.DetectAppStyle(Application.Current);
            MahApps.Metro.Accent accent = new MahApps.Metro.Accent();
            accent.Resources = new ResourceDictionary();
            accent.Resources.Add("FlatButtonPressedBackgroundBrush", System.Windows.Media.Brushes.White);
            foreach(var key in manager.Item1.Resources.Keys){
                Console.WriteLine(key.ToString());
            }
            MahApps.Metro.ThemeManager.ChangeAppStyle(Application.Current, accent, manager.Item1);
        }
    }
}
