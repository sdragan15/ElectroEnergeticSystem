using EESystem.Model;
using EESystem.Services.Implementation;
using EESystem.Services.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        public double CanvasWidth = 1200;
        public double CanvasHeight = 800;
        private IFileService _fileService;
        private ICalculationService _calcService;
        private double nodesWidth = 3;
        private double substationWidth = 10;
        private const int resolution = 5;
        private double connectionThickness = 1;
        private const int matrixWidth = 1500;
        private const int matrixHeight = 1200;

        private int[,] Matrix = new int[matrixWidth/resolution + 1, matrixHeight/resolution + 1];
        private Dictionary<long, long> nodePairs = new Dictionary<long, long>();
        List<SubstationEntity> substations = new List<SubstationEntity>();
        List<NodeEntity> nodes = new List<NodeEntity>();
        List<LineEntity> lines = new List<LineEntity>();

        public MainWindow()
        {
            _calcService = new CalculationService(resolution, substationWidth, nodesWidth);
            _fileService = new FileService(_calcService, "Geographic.xml", CanvasWidth, CanvasHeight);

            InitializeComponent();
            LoadSubstations();
            LoadNodes();
            ConnectNodesBFS();
            //ConnectNodes();
        }

        private void LoadSubstations()
        {
            substations = _fileService.LoadSubstationNetwork();

            substations = _calcService.CalculateSubstaionCoordByResolution(substations);

            foreach(SubstationEntity item in substations)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = substationWidth;
                ellipse.Height = substationWidth;
                ellipse.Fill = new SolidColorBrush(Colors.Red);
                
                var tt = new ToolTip();
                tt.Content = $"Name: {item.Name}\nID: {item.Id}";
                
                ellipse.ToolTip = tt;


                CanvasArea.Children.Add(ellipse);

                Canvas.SetLeft(ellipse, item.X);
                Canvas.SetTop(ellipse, item.Y);
            }
        }

        private void LoadNodes()
        {
            nodes = _fileService.LoadNodesNetwork();
            lines = _fileService.LoadLinesNetwork();
            nodePairs = _calcService.SetNodePairs(nodes, lines);

            var removeNodes = new List<NodeEntity>();
            foreach(var node in nodes)
            {
                if(!nodePairs.ContainsKey(node.Id) && !nodePairs.ContainsValue(node.Id))
                {
                    removeNodes.Add(node);
                }
            }

            nodes = nodes.Except(removeNodes).ToList();

            nodes = _calcService.CalculateNodesCoordByResolution(nodes);

            foreach(var node in nodes)
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

        private void ConnectNodesBFS()
        {
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch drawintTime = new Stopwatch();
            var times = new List<long>();

            int count = 0;
            foreach(var pair in nodePairs)
            {
                count++;
                
                var startNode = nodes.FirstOrDefault(x => x.Id == pair.Key);
                var endNode = nodes.FirstOrDefault(x => x.Id == pair.Value);

                stopwatch.Start();
                var tempLines = _calcService.CalculateEdgeCoordsBFS(Matrix, new Coordinates() { X = startNode.X, Y = startNode.Y },
                    new Coordinates() { X = endNode.X, Y = endNode.Y });
                stopwatch.Stop();
                
                drawintTime.Start();
                DrawConnection(tempLines);
                drawintTime.Stop();
            }

            times.Add(stopwatch.ElapsedMilliseconds);
            times.Add(drawintTime.ElapsedMilliseconds);
            //PrintTimes(times);
        }

        private void PrintTimes(List<long> times)
        {
            using (StreamWriter writer = File.AppendText("times.txt"))
            {
                writer.WriteLine("-------------------------------------");
                writer.WriteLine($"Calculation time:\t {times[0]}");
                writer.WriteLine($"Drawing time:\t\t {times[1]}");
            }
        }

        private void ConnectNodes()
        {
            int count = 0;
            foreach (var pair in nodePairs)
            {
                count++;

                var startNode = nodes.FirstOrDefault(x => x.Id == pair.Key);
                var endNode = nodes.FirstOrDefault(x => x.Id == pair.Value);
                var tempLines = _calcService.CalculateEdgeCoords(new Coordinates() { X = startNode.X, Y = startNode.Y}, 
                    new Coordinates() { X = endNode.X, Y = endNode.Y});

                DrawConnection(tempLines);
            }
        }

        private void DrawConnection(List<Coordinates> coords)
        {
            Polyline connection = new Polyline();
            PointCollection collection = new PointCollection();
            foreach (var p in coords)
            {
                collection.Add(new Point()
                {
                    X = p.X,
                    Y = p.Y
                });
            }
            connection.Points = collection;
            connection.Stroke = new SolidColorBrush(Colors.Black);
            connection.StrokeThickness = connectionThickness;
            CanvasArea.Children.Add(connection);
        }
    }
}
