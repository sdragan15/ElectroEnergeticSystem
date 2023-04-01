using EESystem.Model;
using EESystem.Services.Implementation;
using EESystem.Services.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace EESystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public double CanvasWidth = 1200;
        public double CanvasHeight = 800;
        private IFileService _fileService;
        private ICalculationService _calcService;
        private double nodesWidth = 3;
        private double substationWidth = 3;
        private double switchWidth = 3;

        private const int resolution = 5;
        private double connectionThickness = 0.5;
        private const int matrixWidth = 1500;
        private const int matrixHeight = 1200;

        private bool switchesShowed = false;

        private List<List<Coordinates>> allPaths = new List<List<Coordinates>>();
        private List<Coordinates> intersections = new List<Coordinates>();

        private Polyline activeLineAnim = null;
        private Dictionary<Polyline, LinesEdges> lineNodePairs = new Dictionary<Polyline, LinesEdges>();


        private int[,] Matrix = new int[matrixWidth / resolution + 1, matrixHeight / resolution + 1];
        List<SubstationEntity> substations = new List<SubstationEntity>();
        List<NodeEntity> nodes = new List<NodeEntity>();
        List<LineEntity> lines = new List<LineEntity>();
        List<SwitchEntity> switches = new List<SwitchEntity>();
        List<List<Coordinates>> path = new List<List<Coordinates>>();

        Thread bfsThread = null;
        private bool showed = false;


        public MainWindow()
        { 
            _calcService = new CalculationService(resolution, substationWidth, nodesWidth);
            _fileService = new FileService(_calcService, "Geographic.xml", CanvasWidth, CanvasHeight);

            InitializeComponent();

            Thread loadingThread = new Thread(Loading);
            loadingThread.Start();

            LoadingIcon.Visibility = Visibility.Hidden;

            LoadLines();
            LoadSubstations();
            LoadSwitches();
            LoadNodes();

            bfsThread = ConnectEntitiesBFS();
            bfsThread.Start();

        }

        private void Loading()
        {
            Thread.Sleep(12000);
            Dispatcher.Invoke(() =>
            {
                MyGrid.Children.Remove(LoadingElement);
            });
        }

        private void Draw(object sender, RoutedEventArgs e)
        {
            if (!showed)
            {
                LoadingIcon.Visibility= Visibility.Visible;

                Thread thread = new Thread(new ThreadStart(Draw));
                thread.Start();
                showed = true;
            }
        }

        private void Draw()
        {
            bfsThread.Join();
            
            Dispatcher.Invoke(() =>
            {
                DrawSubstations();
                DrawNodes();
                DrawSwitches();
                DrawPath();

                GetIntersections();
                Panel.Children.Remove(LoadingIcon);
            });
          
        }

        private void LoadLines()
        {
            lines = _fileService.LoadLinesNetwork();
        }

        private void DrawSubstations()
        {
            foreach (SubstationEntity item in substations)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = substationWidth;
                ellipse.Height = substationWidth;
                ellipse.Fill = new SolidColorBrush(Colors.Blue);

                var tt = new ToolTip();
                tt.Content = $"Substation\nName: {item.Name}\nID: {item.Id}";

                ellipse.ToolTip = tt;


                CanvasArea.Children.Add(ellipse);

                Canvas.SetLeft(ellipse, item.X);
                Canvas.SetTop(ellipse, item.Y);
            }
        }

        private void LoadSubstations()
        {
            substations = _fileService.LoadSubstationNetwork();

            substations = _calcService.CalculateSubstaionCoordByResolution(substations);
        }

        private void DrawSwitches()
        {
            foreach (SwitchEntity item in switches)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = switchWidth;
                ellipse.Height = switchWidth;
                if (item.Status.Equals("Closed"))
                    ellipse.Fill = new SolidColorBrush(Colors.Red);
                else
                    ellipse.Fill = new SolidColorBrush(Colors.Green);

                var tt = new ToolTip();
                tt.Content = $"Switch\nName: {item.Name}\nID: {item.Id}\n{item.Status}";

                ellipse.ToolTip = tt;
                ellipse.Uid = "switch_" + Guid.NewGuid().ToString();

                CanvasArea.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, item.X);
                Canvas.SetTop(ellipse, item.Y);
            }
        }

        private void LoadSwitches()
        {
            switchesShowed = true;

            switches = _fileService.LoadSwitchesNetwork();
            switches = _calcService.CalculateSwitchesCoordByResolution(switches);

            var removeLines = new List<SwitchEntity>();
            foreach (var item in switches)
            {
                if (lines.FirstOrDefault(x => x.FirstEnd == item.Id) == null && lines.FirstOrDefault(x => x.SecondEnd == item.Id) == null)
                {
                    removeLines.Add(item);
                }
            }

            switches = switches.Except(removeLines).ToList();

        }

        private void DrawNodes()
        {
            foreach (var node in nodes)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = nodesWidth;
                ellipse.Height = nodesWidth;
                ellipse.Fill = new SolidColorBrush(Colors.Black);
                ellipse.Uid = Guid.NewGuid().ToString();
                node.Uid = ellipse.Uid;

                CanvasArea.Children.Add(ellipse);

                Canvas.SetLeft(ellipse, node.X);
                Canvas.SetTop(ellipse, node.Y);

            }
        }

        private void LoadNodes()
        {
            nodes = _fileService.LoadNodesNetwork();

            var removeNodes = new List<NodeEntity>();
            foreach (var node in nodes)
            {
                if (lines.FirstOrDefault(x => x.FirstEnd == node.Id) == null && lines.FirstOrDefault(x => x.SecondEnd == node.Id) == null)
                {
                    removeNodes.Add(node);
                }
            }

            nodes = nodes.Except(removeNodes).ToList();

            nodes = _calcService.CalculateNodesCoordByResolution(nodes);
        }

        private void GetIntersections()
        {
            intersections = _calcService.GetInersections();
            List<Coordinates> deleteCords = new List<Coordinates>();
            foreach (var item in intersections)
            {
                if (nodes.FirstOrDefault(x => x.X + nodesWidth / 2 == item.X && x.Y + nodesWidth / 2 == item.Y) != null)
                {
                    deleteCords.Add(item);
                }
            }

            intersections = intersections.Except(deleteCords).ToList();

            DrawIntersections();
        }

        private void DrawIntersections()
        {
            foreach (Coordinates item in intersections)
            {
                Line line = new Line();
                line.X1 = item.X + 3.5;
                line.Y1 = item.Y + 3.5;
                line.X2 = item.X - 3.5;
                line.Y2 = item.Y - 3.5;
                line.StrokeThickness = 0.5;
                line.Stroke = new SolidColorBrush(Colors.Black);

                Line line2 = new Line();
                line2.X1 = item.X + 3.5;
                line2.Y1 = item.Y - 3.5;
                line2.X2 = item.X - 3.5;
                line2.Y2 = item.Y + 3.5;
                line2.StrokeThickness = 0.5;
                line2.Stroke = new SolidColorBrush(Colors.Black);

                CanvasArea.Children.Add(line);
                CanvasArea.Children.Add(line2);
            }
        }

        private PowerEntity GetEntity(long id)
        {
            PowerEntity entity = switches.FirstOrDefault(x => x.Id == id);
            if (entity != null)
                return entity;

            entity = nodes.FirstOrDefault(x => x.Id == id);
            if (entity != null)
                return entity;

            entity = substations.FirstOrDefault(x => x.Id == id);
            if (entity != null)
                return entity;

            return null;
        }

        private Thread ConnectEntitiesBFS()
        {
            Thread thread = new Thread(new ThreadStart(async () =>
            {
                int skipCount = 0;
                int zeroPath = 0;
                Stopwatch stopwatch = new Stopwatch();
                Stopwatch drawintTime = new Stopwatch();
                var times = new List<long>();

                int count = 0;
                foreach (var line in lines)
                {
                    count++;
                    //if (count > 59)
                    //    return;

                    PowerEntity startNode = GetEntity(line.FirstEnd);

                    PowerEntity endNode = GetEntity(line.SecondEnd);

                    if (startNode == null || endNode == null)
                    {
                        skipCount++;
                        continue;
                    }

                    stopwatch.Start();
                    var tempLines = _calcService.CalculateEdgeCoordsBFS(Matrix, new Coordinates() { X = startNode.X, Y = startNode.Y },
                        new Coordinates() { X = endNode.X, Y = endNode.Y }, switchWidth);
                    stopwatch.Stop();

                    if (tempLines.Count() > 0)
                        allPaths.Add(tempLines);
                    else
                    {
                        zeroPath++;
                    }

                    path.Add(tempLines);

                    times = new List<long>();
                }

                var bfsTime = _calcService.GetElapsedTime(0);
                var otherTime = _calcService.GetElapsedTime(1);
                var time = stopwatch.ElapsedMilliseconds;
            }));

            return thread;
        }

        private void DrawPath()
        {
            foreach(var item in path)
            {
                DrawConnection(item, switchWidth);
            }
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

        private void DrawConnection(List<Coordinates> coords, double nodesWidth)
        {
            if(coords.Count < 3)
            {
                return;
            }

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
            connection.MouseDown += ConnectionClick;
            CanvasArea.Children.Add(connection);

            var firstNode = nodes.FirstOrDefault(x => x.X + nodesWidth / 2 == coords[0].X && x.Y + nodesWidth / 2 == coords[0].Y);
            var lastNode = nodes.FirstOrDefault(x => x.X + nodesWidth / 2 == coords[coords.Count() - 1].X && x.Y + nodesWidth / 2 == coords[coords.Count()-1].Y);

            if(firstNode != null && lastNode != null)
            {
                lineNodePairs.Add(connection, new LinesEdges(firstNode, lastNode));
            }
        }

        private void ConnectionClick(object sender, RoutedEventArgs e)
        {
            if(activeLineAnim != null)
            {
                activeLineAnim.BeginAnimation(Line.StrokeThicknessProperty, null);
                activeLineAnim.Stroke = new SolidColorBrush(Colors.Black);
                activeLineAnim.StrokeThickness = connectionThickness;
                activeLineAnim = null;
            }

            Polyline line = (Polyline)sender;
            line.Stroke = new SolidColorBrush(Colors.Blue);

            Coordinates firstPoint = new Coordinates(line.Points[0].X, line.Points[0].Y);
            Coordinates lastPoint = new Coordinates(line.Points[line.Points.Count() - 1].X, line.Points[line.Points.Count()-1].Y);

            DoubleAnimation da = new DoubleAnimation();
            da.From = connectionThickness;
            da.To = 3 * connectionThickness;
            da.AutoReverse = true;
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            da.RepeatBehavior = RepeatBehavior.Forever;

            line.BeginAnimation(Line.StrokeThicknessProperty, da);
            activeLineAnim = line;

            var firstNode = nodes.FirstOrDefault(x => x.X + nodesWidth/2 == firstPoint.X && x.Y + nodesWidth / 2 == firstPoint.Y);
            var lastNode = nodes.FirstOrDefault(x => x.X + nodesWidth / 2 == lastPoint.X && x.Y + nodesWidth / 2 == lastPoint.Y);

            if(firstNode != null && lastNode != null)
            {
                DoubleAnimation nodeDa = new DoubleAnimation();
                nodeDa.From = 1;
                nodeDa.To = 3;
                nodeDa.AutoReverse = true;
                nodeDa.Duration = new Duration(TimeSpan.FromSeconds(1));
                nodeDa.RepeatBehavior = RepeatBehavior.Forever;
                var firstScale = new ScaleTransform();
                var secondScale = new ScaleTransform();

                

                foreach(UIElement child in CanvasArea.Children)
                {
                    if(child.Uid == firstNode.Uid)
                    {
                        var position = child.TransformToAncestor(CanvasArea).Transform(new Point(0, 0));
                        var scaleTransform = CanvasArea.canvas.RenderTransform as ScaleTransform;
                        var scaledX = position.X / scaleTransform.ScaleX;
                        var scaledY = position.Y / scaleTransform.ScaleY;
                        firstScale.CenterX = scaledX - nodesWidth / 2;
                        firstScale.CenterY = scaledY - nodesWidth / 2;

                        child.RenderTransform = firstScale;
                        firstScale.BeginAnimation(ScaleTransform.ScaleXProperty, nodeDa);
                        firstScale.BeginAnimation(ScaleTransform.ScaleYProperty, nodeDa);
                    }

                    if (child.Uid == lastNode.Uid)
                    {
                        var position = child.TransformToAncestor(CanvasArea).Transform(new Point(0, 0));
                        var scaleTransform = CanvasArea.canvas.RenderTransform as ScaleTransform;
                        var scaledX = scaleTransform.ScaleX * position.X;
                        var scaledY = scaleTransform.ScaleY * position.Y;
                        secondScale.CenterX = scaledX - nodesWidth/2;
                        secondScale.CenterY = scaledY - nodesWidth / 2;

                        child.RenderTransform = secondScale;
                        secondScale.BeginAnimation(ScaleTransform.ScaleXProperty, nodeDa);
                        secondScale.BeginAnimation(ScaleTransform.ScaleYProperty, nodeDa);
                    }
                }

                //firstScale.BeginAnimation(ScaleTransform.ScaleXProperty, nodeDa);
                //firstScale.BeginAnimation(ScaleTransform.ScaleYProperty, nodeDa);

                //secondScale.BeginAnimation(ScaleTransform.ScaleXProperty, nodeDa);
                //secondScale.BeginAnimation(ScaleTransform.ScaleYProperty, nodeDa);
            }
        }

        private void ToggleSwitches(object sender, RoutedEventArgs e)
        {
            if (switchesShowed)
            {
                List<UIElement> itemstoremove = new List<UIElement>();
                foreach (UIElement ui in CanvasArea.Children)
                {
                    if (ui.Uid.StartsWith("switch"))
                    {
                        itemstoremove.Add(ui);
                    }
                }
                foreach (UIElement ui in itemstoremove)
                {
                    CanvasArea.Children.Remove(ui);
                }
                switchesShowed = false;
            }
            else
            {
                LoadSwitches();
            }

        }

    }
}
