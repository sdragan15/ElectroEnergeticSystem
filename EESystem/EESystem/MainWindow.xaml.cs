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
        private int nodesWidth = 1;
        private int resolution = 2;

        public MainWindow()
        {
            _calcService = new CalculationService(resolution);
            _fileService = new FileService(_calcService, "Geographic.xml");

            InitializeComponent();
            LoadSubstations();
            LoadNodes();
        }

        private void LoadSubstations()
        {
            var substations = _fileService.LoadSubstationNetwork();

            var newSubstations = _calcService.CalculateSubstaionCoordByResolution(substations);

            foreach(SubstationEntity item in newSubstations)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 5;
                ellipse.Height = 5;
                ellipse.Fill = new SolidColorBrush(Colors.Red);

                CanvasArea.Children.Add(ellipse);

                Canvas.SetLeft(ellipse, item.X);
                Canvas.SetTop(ellipse, item.Y);
            }
        }

        private void LoadNodes()
        {
            var nodes = _fileService.LoadNodesNetwork();

            var newNodes = _calcService.CalculateNodesCoordByResolution(nodes);

            foreach(var node in newNodes)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = nodesWidth;
                ellipse.Height = nodesWidth;
                ellipse.Fill = new SolidColorBrush(Colors.Black);

                CanvasArea.Children.Add(ellipse);

                Canvas.SetLeft(ellipse, node.X);
                Canvas.SetTop(ellipse, node.Y);
            }
        }
    }
}
