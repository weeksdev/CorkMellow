using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
namespace CorkMellow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        ViewModel.MainWindow vm = new ViewModel.MainWindow();
        public MainWindow()
        {
            this.DataContext = vm;
            //vm.WaveForm = this.WaveForm;
            InitializeComponent();
        }

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.RecordBtn.Foreground == Brushes.Gray)
            {
                this.RecordBtn.Foreground = Brushes.Red;
            }
            else
            {
                this.RecordBtn.Foreground = Brushes.Gray;
                AddWaveFormLines();
            }
        }
        public void AddWaveFormLines()
        {
            this.WaveForm.Children.Clear();
            var half = WaveForm.Height / 2;
            var thickness = 2;  //WaveForm.Width / CurrentRecordingPlots.Count;
            var currentX = 0;
            if (WaveForm != null)
            {
                foreach (var plot in vm.CurrentRecordingPlots)
                {
                    System.Windows.Shapes.Line line = new System.Windows.Shapes.Line();
                    line.X1 = currentX;
                    line.X2 = currentX;
                    line.Y1 = ((plot.Maximum / 100) * half) + half;
                    line.Y2 = ((plot.Minimum / 100) * half) + half;
                    line.StrokeThickness = thickness;
                    currentX += thickness;
                    line.Stroke = Brushes.Gray;
                    WaveForm.Children.Add(line);
                }
            }
        }
    }
}
