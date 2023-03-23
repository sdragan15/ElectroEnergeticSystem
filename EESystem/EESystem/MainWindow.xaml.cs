using EESystem.Model;
using EESystem.Services.Implementation;
using EESystem.Services.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Xml;

namespace EESystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public double CanvasWidth = 700;
        public double CanvasHeight = 400;
        private IFileService _fileService;
        private ICalculationService _calcService;

        public MainWindow()
        {
            _calcService = new CalculationService();
            _fileService = new FileService(_calcService, "Geographic.xml");

            InitializeComponent();
            LoadSubstations();
        }

        private void LoadSubstations()
        {
            var substations = _fileService.LoadSubstationNetwork();

            foreach(SubstationEntity item in substations)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 5;
                ellipse.Height = 5;
                ellipse.Fill = new SolidColorBrush(Colors.Red);

                CanvasArea.Children.Add(ellipse);

                Canvas.SetLeft(ellipse, item.X * CanvasWidth);
                Canvas.SetTop(ellipse, item.Y * CanvasHeight);
            }
        }
    }
}
