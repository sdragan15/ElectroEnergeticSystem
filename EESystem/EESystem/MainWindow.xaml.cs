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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WpfPanAndZoom.CustomControls;

namespace EESystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MatrixTransform _transform = new MatrixTransform();
        private Point _initialMousePosition;
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
        private ModeEnum mode = ModeEnum.NONE;

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
        List<UIGroupElement> uiGroupElements = new List<UIGroupElement>();
        List<UIElement> animatedNodes = new List<UIElement>();

        public List<UIElement> DrawingElements = new List<UIElement>();
        public List<DrawingItem> PolygonPoints = new List<DrawingItem>();
        public List<UIElement> PolygonUIElements = new List<UIElement>();

        public EllipseWindow ellipseWindow = new EllipseWindow();
        public PolygonWindow polygonWindow = new PolygonWindow();
        public PanAndZoomCanvas panAndZoomCanvas = new PanAndZoomCanvas();
        public TextWindow textWindow = new TextWindow();
        public bool IsMouseMoved = false;

        Thread bfsThread = null;
        private bool showed = false;


        public MainWindow()
        { 
            _calcService = new CalculationService(resolution, substationWidth, nodesWidth);
            _fileService = new FileService(_calcService, "Geographic.xml", CanvasWidth, CanvasHeight);

            InitializeComponent();

            MouseDown += DrawOnCanvas;
            MouseMove += PanAndZoomCanvas_MouseMove;
            MouseUp += DrawPolygonPoints;

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
                ellipse.Uid = item.Uid;
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

                ellipse.Uid = item.Uid;
                var tt = new ToolTip();
                tt.Content = $"Switch\nName: {item.Name}\nID: {item.Id}\n{item.Status}";

                ellipse.ToolTip = tt; 

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

                var tt = new ToolTip();
                tt.Content = $"Node\nName: {node.Name}\nID: {node.Id}";

                ellipse.ToolTip = tt;

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

        private PowerEntity GetEntityByCoords(Coordinates coord)
        {
            PowerEntity entity = null;
            entity = switches.FirstOrDefault(x => x.X + nodesWidth / 2 == coord.X && x.Y + nodesWidth / 2 == coord.Y);
            if (entity != null)
                return entity;

            entity = substations.FirstOrDefault(x => x.X + nodesWidth / 2 == coord.X && x.Y + nodesWidth / 2 == coord.Y);
            if (entity != null)
                return entity;

            entity = nodes.FirstOrDefault(x => x.X + nodesWidth / 2 == coord.X && x.Y + nodesWidth / 2 == coord.Y);
            if (entity != null)
                return entity;

            return null;
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
            da.To = 5 * connectionThickness;
            da.AutoReverse = true;
            da.Duration = new Duration(TimeSpan.FromSeconds(1));
            da.RepeatBehavior = RepeatBehavior.Forever;

            line.BeginAnimation(Line.StrokeThicknessProperty, da);
            activeLineAnim = line;

            PowerEntity firstNode = GetEntityByCoords(firstPoint);
            PowerEntity lastNode = GetEntityByCoords(lastPoint);

            foreach (var item in animatedNodes)
            {
                item.BeginAnimation(Ellipse.WidthProperty, null);
                item.BeginAnimation(Ellipse.HeightProperty, null);
            }

            if(firstNode != null && lastNode != null)
            {
                DoubleAnimation nodeDa = new DoubleAnimation();
                nodeDa.From = nodesWidth;
                nodeDa.To = 3 * nodesWidth;
                nodeDa.AutoReverse = true;
                nodeDa.Duration = new Duration(TimeSpan.FromSeconds(1));
                nodeDa.RepeatBehavior = RepeatBehavior.Forever;

                foreach (UIElement child in CanvasArea.Children)
                {
                    if (child.Uid == firstNode.Uid || child.Uid == lastNode.Uid)
                    {
                        child.BeginAnimation(Ellipse.WidthProperty, nodeDa);
                        child.BeginAnimation(Ellipse.HeightProperty, nodeDa);
                        animatedNodes.Add(child);
                    }

                }
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

        private void DrawEllipse(object sender, RoutedEventArgs e)
        {
            mode = ModeEnum.ELLIPSE;
            DrawEllipseBtn.Background = new SolidColorBrush(Colors.Blue);
            DrawEllipseBtn.Foreground = new SolidColorBrush(Colors.White);

            DrawPolygonBtn.ClearValue(Button.BackgroundProperty);
            DrawPolygonBtn.ClearValue(Button.ForegroundProperty);
            DrawTextBtn.ClearValue(Button.BackgroundProperty);
            DrawTextBtn.ClearValue(Button.ForegroundProperty);

        }

        private void DrawText(object sender, RoutedEventArgs e)
        {
            mode = ModeEnum.TEXT;
            DrawTextBtn.Background = new SolidColorBrush(Colors.Blue);
            DrawTextBtn.Foreground = new SolidColorBrush(Colors.White);

            DrawPolygonBtn.ClearValue(Button.BackgroundProperty);
            DrawPolygonBtn.ClearValue(Button.ForegroundProperty);
            DrawEllipseBtn.ClearValue(Button.BackgroundProperty);
            DrawEllipseBtn.ClearValue(Button.ForegroundProperty);
        }

        private void DrawPolygon(object sender, RoutedEventArgs e)
        {
            mode = ModeEnum.POLYGON;
            DrawPolygonBtn.Background = new SolidColorBrush(Colors.Blue);
            DrawPolygonBtn.Foreground = new SolidColorBrush(Colors.White);

            DrawEllipseBtn.ClearValue(Button.BackgroundProperty);
            DrawEllipseBtn.ClearValue(Button.ForegroundProperty);
            DrawTextBtn.ClearValue(Button.BackgroundProperty);
            DrawTextBtn.ClearValue(Button.ForegroundProperty);
        }


        private void DisableDraw(object sender, RoutedEventArgs e)
        {
            mode = ModeEnum.NONE;

            DrawEllipseBtn.ClearValue(Button.BackgroundProperty);
            DrawEllipseBtn.ClearValue(Button.ForegroundProperty);

            DrawPolygonBtn.ClearValue(Button.BackgroundProperty);
            DrawPolygonBtn.ClearValue(Button.ForegroundProperty);

            DrawTextBtn.ClearValue(Button.BackgroundProperty);
            DrawTextBtn.ClearValue(Button.ForegroundProperty);
        }



        private void DrawOnCanvas(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(CanvasArea);

            if (e.ChangedButton == MouseButton.Left)
            {
                _initialMousePosition = _transform.Inverse.Transform(e.GetPosition(CanvasArea));
            }

            if (e.ChangedButton == MouseButton.Right)
            {
                if (mode == ModeEnum.ELLIPSE)
                {
                    ellipseWindow = new EllipseWindow();
                    ellipseWindow.ShowDialog();

                    DrawEllipse(mousePosition);
                }

                if (mode == ModeEnum.POLYGON)
                {
                    if(PolygonPoints.Count >= 3)
                    {
                        polygonWindow = new PolygonWindow();
                        polygonWindow.ShowDialog();

                        PointCollection pointsColl = new PointCollection();
                        var inverse = _transform.Inverse;
                        foreach (var point in PolygonPoints)
                        {
                            var newPoinst = inverse.Transform(new Point()
                            {
                                X = point.X,
                                Y = point.Y
                            });
                            pointsColl.Add(new Point()
                            {
                                X = newPoinst.X,
                                Y = newPoinst.Y
                            });
                        }
                        DrawPolygon(mousePosition, pointsColl);
                        foreach(var point in PolygonUIElements)
                        {
                            CanvasArea.Children.Remove(point);
                        }

                        PolygonUIElements.Clear();
                        PolygonPoints.Clear();
                    }
                    
                }

                if(mode == ModeEnum.TEXT)
                {
                    textWindow = new TextWindow();
                    textWindow.ShowDialog();

                    Label label = DrawTextOnCanvas(mousePosition, textWindow.TextMessage, textWindow.TextFontSize, 
                        textWindow.TextForeground);
                    CanvasArea.Children.Add(label);
                    uiGroupElements.Add(new UIGroupElement()
                    {
                        Parent = label
                    });

                }
            }
            
        }

        private Label DrawTextOnCanvas(Point mousePosition, string text, double fontSize, ColorsEnum color, double x = 0, double y = 0)
        {
            Label label = new Label();
            label.Content = text;
            label.FontSize = fontSize;

            switch (color)
            {
                case ColorsEnum.BLACK:
                    label.Foreground = new SolidColorBrush(Colors.Black);
                    break;
                case ColorsEnum.RED:
                    label.Foreground = new SolidColorBrush(Colors.Red);
                    break;
                case ColorsEnum.GREEN:
                    label.Foreground = new SolidColorBrush(Colors.Green);
                    break;
                case ColorsEnum.YELLOW:
                    label.Foreground = new SolidColorBrush(Colors.Yellow);
                    break;
                case ColorsEnum.BLUE:
                    label.Foreground = new SolidColorBrush(Colors.Blue);
                    break;
            }

            label.MouseDown += TextClicked;
            label.RenderTransform = _transform;
            var inversed = _transform.Inverse;
            var newPoinst = inversed.Transform(mousePosition);

            Canvas.SetTop(label, newPoinst.Y + y);
            Canvas.SetLeft(label, newPoinst.X + x);

            return label;
        }

        private void TextClicked(object sender, MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            textWindow = new TextWindow();

            var group = uiGroupElements.FirstOrDefault(x => x.Parent == label);

            textWindow.SetValues(label.Content.ToString(), label.FontSize);

            textWindow.ShowDialog();
            
            Point point = new Point();
            point.X = Canvas.GetLeft(label);
            point.Y = Canvas.GetTop(label);
            var inversed = _transform.Inverse;
            var newPoinst = _transform.Transform(point);

            if (group != null)
            {
                uiGroupElements.Remove(group);
                var newLabel = DrawTextOnCanvas(newPoinst, textWindow.TextMessage, textWindow.TextFontSize, textWindow.TextForeground);
                CanvasArea.Children.Add(newLabel);
                uiGroupElements.Add(new UIGroupElement()
                {
                    Parent = newLabel,
                });
                DrawingElements.Remove(label);
                CanvasArea.Children.Remove(label);
            }
        }


        private void DrawPolygonPoints(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseMoved)
            {
                IsMouseMoved = false;
                return;
            }
            IsMouseMoved = false;
                
            var mousePosition = e.GetPosition(CanvasArea);

            if (mode == ModeEnum.POLYGON)
            {
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 5;
                ellipse.Height = 5;
                ellipse.Fill = new SolidColorBrush(Colors.Black);
                ellipse.Uid = Guid.NewGuid().ToString();

                ellipse.RenderTransform = _transform;
                var inversed = _transform.Inverse;
                var newPoinst = inversed.Transform(mousePosition);

                Canvas.SetTop(ellipse, newPoinst.Y);
                Canvas.SetLeft(ellipse, newPoinst.X);

                CanvasArea.Children.Add(ellipse);

                PolygonUIElements.Add(ellipse);
                PolygonPoints.Add(new DrawingItem()
                {
                    Uid = ellipse.Uid,
                    X = mousePosition.X,
                    Y = mousePosition.Y,
                });
            }
            
        }

        private void DrawPolygon(Point mousePosition, PointCollection points)
        {
            Polygon polygon = new Polygon();
            polygon.Points = points;
            polygon.StrokeThickness = polygonWindow.PolygonStrokeThickness;
            polygon.Stroke = new SolidColorBrush(Colors.Black);
            var background = polygonWindow.PolygonBackground;

            switch (background)
            {
                case ColorsEnum.BLACK:
                    polygon.Fill = new SolidColorBrush(Colors.Black);
                    break;
                case ColorsEnum.RED:
                    polygon.Fill = new SolidColorBrush(Colors.Red);
                    break;
                case ColorsEnum.GREEN:
                    polygon.Fill = new SolidColorBrush(Colors.Green);
                    break;
                case ColorsEnum.YELLOW:
                    polygon.Fill = new SolidColorBrush(Colors.Yellow);
                    break;
                case ColorsEnum.BLUE:
                    polygon.Fill = new SolidColorBrush(Colors.Blue);
                    break;
            }

            polygon.RenderTransform = _transform;
            uiGroupElements.Add(new UIGroupElement()
            {
                Parent = polygon
            });

            CanvasArea.Children.Add(polygon);
        }

        private void DrawEllipse(Point mousePosition)
        {
            if (ellipseWindow.IsValid)
            {
                var width = ellipseWindow.EllipseWidth;
                var height = ellipseWindow.EllipseHeight;
                var strokeThickness = ellipseWindow.EllipseStrokeThickness;
                var background = ellipseWindow.EllipseBackground;


                Ellipse ellipse = new Ellipse();
                ellipse.Width = width;
                ellipse.Height = height;
                ellipse.StrokeThickness = strokeThickness;
                ellipse.Uid = Guid.NewGuid().ToString();
                ellipse.Stroke = new SolidColorBrush(Colors.Black);
                ellipse.Fill = new SolidColorBrush(Colors.Black);

                switch (background)
                {
                    case ColorsEnum.BLACK:
                        ellipse.Fill = new SolidColorBrush(Colors.Black);
                        break;
                    case ColorsEnum.RED:
                        ellipse.Fill = new SolidColorBrush(Colors.Red);
                        break;
                    case ColorsEnum.GREEN:
                        ellipse.Fill = new SolidColorBrush(Colors.Green);
                        break;
                    case ColorsEnum.YELLOW:
                        ellipse.Fill = new SolidColorBrush(Colors.Yellow);
                        break;
                    case ColorsEnum.BLUE:
                        ellipse.Fill = new SolidColorBrush(Colors.Blue);
                        break;
                }

                ellipse.MouseDown += EllipseClicked;

                DrawingElements.Add(ellipse);
                UIGroupElement group = new UIGroupElement();
                group.Parent = ellipse;
                if (ellipseWindow.EllipseAddText)
                {
                    group.Child = DrawTextOnCanvas(mousePosition, ellipseWindow.TextMessage, 14, ellipseWindow.TextForeground,
                        0, ellipse.Height/2 - 10);
                    CanvasArea.Children.Add(group.Child);
                    Canvas.SetZIndex(group.Child, 30);
                }
                

                ellipse.RenderTransform = _transform;
                var inversed = _transform.Inverse;
                var newPoinst = inversed.Transform(mousePosition);

                Canvas.SetTop(ellipse, newPoinst.Y);
                Canvas.SetLeft(ellipse, newPoinst.X);

                uiGroupElements.Add(group);

                CanvasArea.Children.Add(ellipse);

            }
        }

        private void EllipseClicked(object sender, RoutedEventArgs e)
        {
            Label label = new Label();
            string text = "";
            bool isText = false;
            var ellipse = (Ellipse)sender;
            var group = uiGroupElements.FirstOrDefault(x => x.Parent == ellipse);
            if (group != null && group.Child != null)
            {
                label = (Label)group.Child;
                isText = true;
                text = label.Content.ToString();
                CanvasArea.Children.Remove(group.Child);
            }
            else
            {
                isText = false;
            }

            ellipseWindow = new EllipseWindow();
            ellipseWindow.SetValues(ellipse.Width, ellipse.Height, ellipse.StrokeThickness, ColorsEnum.BLACK, 
                text, isText);
            ellipseWindow.ShowDialog();
            Point point = new Point();
            point.X = Canvas.GetLeft(ellipse);
            point.Y = Canvas.GetTop(ellipse);
            var inversed = _transform.Inverse;
            var newPoinst = _transform.Transform(point);

            if(group != null)
            {
                uiGroupElements.Remove(group);
                DrawEllipse(newPoinst);
                DrawingElements.Remove(ellipse);
                CanvasArea.Children.Remove(ellipse);
            }

        }

        private void PanAndZoomCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsMouseMoved = true;
                Point mousePosition = _transform.Inverse.Transform(e.GetPosition(CanvasArea));
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                _transform.Matrix = translate.Value * _transform.Matrix;
                foreach (UIElement child in CanvasArea.Children)
                {
                    child.RenderTransform = _transform;
                }
            }

        }

        private void Undo(object sender, RoutedEventArgs e)
        {
            if(uiGroupElements.Count > 0)
            {
                var last = uiGroupElements[uiGroupElements.Count - 1];
                if(last.Parent != null)
                {
                    CanvasArea.Children.Remove(last.Parent);
                }
                if (last.Child != null)
                {
                    CanvasArea.Children.Remove(last.Child);
                }
            }
        }

        private void Redo(object sender, RoutedEventArgs e)
        {

        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            if (uiGroupElements.Count > 0)
            {
                foreach(var last in uiGroupElements)
                {
                    if (last.Parent != null)
                    {
                        CanvasArea.Children.Remove(last.Parent);
                    }
                    if (last.Child != null)
                    {
                        CanvasArea.Children.Remove(last.Child);
                    }
                }
               
            }
        }
    }
}
