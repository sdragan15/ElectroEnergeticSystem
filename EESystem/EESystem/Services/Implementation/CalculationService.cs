using EESystem.Model;
using EESystem.Services.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EESystem.Services.Implementation
{
    public class CalculationService : ICalculationService
    {
        private int count = 0;
        private readonly int _resolution;
        private readonly double _substationWidth = 3;
        private readonly double _nodeWidth = 3;
        private readonly double _switchWidth = 3;
        private double maxWidth;
        private double maxHeight;

        private Stopwatch stopwatch = new Stopwatch();
        private Stopwatch stopwatch2 = new Stopwatch();

        private List<Coordinates> intersection = new List<Coordinates>();

        private List<List<Coordinates>> allPaths = new List<List<Coordinates>>();

        public CalculationService(int resolution, double substationWidth, double nodeWidth)
        {
            _resolution = resolution;
            _substationWidth = substationWidth;
            _nodeWidth = nodeWidth;
            maxWidth = 1200 / _resolution;
            maxHeight = 800 / _resolution;
        }


        public void CalculateCanvasCoords(double x, double y, out double newX, out double newY)
        {
            //newY = (x - 45.19) * 2000;
            //newX = Math.Abs(y - 19.95) * 2000;
            newX = (x - 19.70) * (1 / (19.98 - 19.70));     // 19.98 je max latituda, 19.74 min.
            newY = Math.Abs((y - 45.329) * (1 / (45.349 - 45.19)));     // Formula eksperimentalno izvucena
        }

        public List<NodeEntity> CalculateNodesCoordByResolution(List<NodeEntity> nodes)
        {
            var result = new List<NodeEntity>();

            foreach (var item in nodes)
            {
                double tempX = item.X;
                double tempY = item.Y;

                double coordX = Math.Floor(tempX / _resolution);
                double coordY = Math.Floor(tempY / _resolution);
                item.X = Math.Floor(tempX / _resolution) * _resolution - _nodeWidth / 2;
                item.Y = Math.Floor(tempY / _resolution) * _resolution - _nodeWidth / 2;

                if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), item))
                {
                    result.Add(item);
                    continue;
                }

                int radius = 0;
                bool added = false;
                while (radius <= 100)
                {
                    radius++;

                    tempX = coordX - radius;
                    tempY = coordY - radius;
                    for (int i = 0; i <= radius* 2; i++)
                    {
                        tempX += i;
                        
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _nodeWidth / 2,
                            Y = tempY * _resolution - _nodeWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                    tempX = coordX + radius;
                    tempY = coordY - radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempY += i;
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _nodeWidth / 2,
                            Y = tempY * _resolution - _nodeWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                    tempX = coordX + radius;
                    tempY = coordY + radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempX -= i;
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _nodeWidth / 2,
                            Y = tempY * _resolution - _nodeWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                    tempX = coordX - radius;
                    tempY = coordY + radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempY -= i;
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _nodeWidth / 2,
                            Y = tempY * _resolution - _nodeWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                }
            }

            return result;
        }

        
        public List<SubstationEntity> CalculateSubstaionCoordByResolution(List<SubstationEntity> substations)
        {
            //var result = new List<SubstationEntity>();

            //foreach(var item in substations)
            //{
            //    double x = item.X;
            //    double y = item.Y;

            //    item.X = Math.Floor(x / _resolution) * _resolution - _substationWidth / 2;
            //    item.Y = Math.Floor(y / _resolution) * _resolution - _substationWidth / 2;

            //    if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), item))
            //    {
            //        result.Add(item);
            //        continue;
            //    }

            //    int radius = 1;
            //    //while (true)
            //    //{

            //    //}
            //}

            //return result;

            var result = new List<SubstationEntity>();

            foreach (var item in substations)
            {
                double tempX = item.X;
                double tempY = item.Y;

                double coordX = Math.Floor(tempX / _resolution);
                double coordY = Math.Floor(tempY / _resolution);
                item.X = Math.Floor(tempX / _resolution) * _resolution - _nodeWidth / 2;
                item.Y = Math.Floor(tempY / _resolution) * _resolution - _nodeWidth / 2;

                if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), item))
                {
                    result.Add(item);
                    continue;
                }

                int radius = 0;
                bool added = false;
                while (radius <= 100)
                {
                    radius++;

                    tempX = coordX - radius;
                    tempY = coordY - radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempX += i;

                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _nodeWidth / 2,
                            Y = tempY * _resolution - _nodeWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                    tempX = coordX + radius;
                    tempY = coordY - radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempY += i;
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _nodeWidth / 2,
                            Y = tempY * _resolution - _nodeWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                    tempX = coordX + radius;
                    tempY = coordY + radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempX -= i;
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _nodeWidth / 2,
                            Y = tempY * _resolution - _nodeWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                    tempX = coordX - radius;
                    tempY = coordY + radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempY -= i;
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _nodeWidth / 2,
                            Y = tempY * _resolution - _nodeWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                }
            }

            return result;
        }

        public Dictionary<long, long> SetNodePairs(List<NodeEntity> nodes, List<LineEntity> lines)
        {
            var result = new Dictionary<long, long>();

            foreach (var line in lines)
            {
                var firstNode = nodes.Where(x => x.Id == line.FirstEnd).FirstOrDefault();
                var secondNode = nodes.Where(x => x.Id == line.SecondEnd).FirstOrDefault();

                if (firstNode != null && secondNode != null)
                {
                    if(!result.ContainsKey(firstNode.Id) && !result.ContainsKey(secondNode.Id))
                    {
                        result.Add(firstNode.Id, secondNode.Id);
                    }
                }
            }

            return result;
        }

        private bool LineContains(List<LineEntity> lines, NodeEntity node)
        {
            return lines.Where(x => x.FirstEnd == node.Id || x.SecondEnd == node.Id).FirstOrDefault() != null;
        }


        public void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        private bool ContainsCoord(List<PowerEntity> entities, PowerEntity entity)
        {
            return entities.Where(x => x.X == entity.X && x.Y == entity.Y).FirstOrDefault() != null ? true : false;
        }

        public List<Coordinates> CalculateEdgeCoordsBFS(int[,] matrix, Coordinates start, Coordinates end, double width)
        {
            double startX = start.X;
            double startY = start.Y;

            Dictionary<Coordinates, Coordinates> path = new Dictionary<Coordinates, Coordinates>();
            Queue<Coordinates> edges = new Queue<Coordinates>();

            int tempX = (int)Math.Floor((start.X + width / 2) / _resolution);
            int tempY = (int)Math.Floor((start.Y + width / 2) / _resolution);

            var tempEnd = new Coordinates();

            tempEnd.X = (int)Math.Floor((end.X + width / 2) / _resolution);
            tempEnd.Y = (int)Math.Floor((end.Y + width / 2) / _resolution);

            edges.Enqueue(new Coordinates()
            {
                X = tempX,
                Y = tempY
            });

            var points = new List<Coordinates>();

            stopwatch.Start();
            BfsAlgorithm(matrix, edges, path, tempEnd);
            stopwatch.Stop();

            if (path.Keys.FirstOrDefault(x => x.X == tempEnd.X && x.Y == tempEnd.Y) != null)
            {
                var temp = tempEnd;
                points.Add(new Coordinates()
                {
                    X = (temp.X) * _resolution,
                    Y = (temp.Y) * _resolution
                });
                matrix[(int)temp.X, (int)temp.Y] = 2;

                while (true)
                {
                    temp = path[path.Keys.FirstOrDefault(x => x.X == temp.X && x.Y == temp.Y)];
                    points.Add(new Coordinates()
                    {
                        X = (temp.X) * _resolution,
                        Y = (temp.Y) * _resolution
                    });

                    matrix[(int)temp.X, (int)temp.Y] = 2;

                    if (path.Keys.FirstOrDefault(x => x.X == temp.X && x.Y == temp.Y) == null)
                        break;

                    if (temp.X == tempX && temp.Y == tempY)
                        break;
                }

                allPaths.Add(points);
            }
            else
            {
                points = CalculateEdgeCoords(new Coordinates(start.X, start.Y), new Coordinates(end.X, end.Y));

                var valid = true;
                for (int i = 0; i < points.Count(); i++)
                {
                    tempX = (int)Math.Floor((points[i].X + width / 2) / _resolution);
                    tempY = (int)Math.Floor((points[i].Y + width / 2) / _resolution);

                    bool top = false;
                    bool left = false;
                    bool right = false;
                    bool bottom = false;

                    bool topSec = false;
                    bool leftSec = false;
                    bool rightSec = false;
                    bool bottomSec = false;

                    if (matrix[tempX, tempY] == 2 && i > 0 && i < points.Count() - 1)
                    {

                        for (int k = -1; k <= 1; k += 2)
                        {
                            if (points[i + k].X == points[i].X && points[i + k].Y > points[i].Y)
                                bottom = true;
                            if (points[i + k].X == points[i].X && points[i + k].Y < points[i].Y)
                                top = true;
                            if (points[i + k].X < points[i].X && points[i + k].Y == points[i].Y)
                                left = true;
                            if (points[i + k].X > points[i].X && points[i + k].Y == points[i].Y)
                                right = true;
                        }

                        var newPaths = allPaths.Where(x => x.Contains(points[i])).ToList();
                        foreach (var item in newPaths)
                        {
                            var coord = item.FirstOrDefault(x => x.X == points[i].X && x.Y == points[i].Y);

                            if (coord != null)
                            {
                                var id = item.IndexOf(coord);
                                if (id > 0 && id < item.Count() - 1)
                                {
                                    for (int k = -1; k <= 1; k += 2)
                                    {
                                        if (item[id + k].X == item[id].X && item[id + k].Y > item[id].Y)
                                            bottomSec = true;
                                        if (item[id + k].X == item[id].X && item[id + k].Y < item[id].Y)
                                            topSec = true;
                                        if (item[id + k].X < item[id].X && item[id + k].Y == item[id].Y)
                                            leftSec = true;
                                        if (item[id + k].X > item[id].X && item[id + k].Y == item[id].Y)
                                            rightSec = true;
                                    }

                                    if (top ^ topSec && right ^ rightSec && left ^ leftSec && bottom ^ bottomSec)
                                    {
                                        intersection.Add(new Coordinates(item[id].X, item[id].Y));
                                    }
                                }
                            }
                        }
                    }

                    if (tempX < 0 || tempY < 0 || tempX > 300 || tempY > 240)
                    {
                        valid = false;
                        break;
                    }
                    matrix[tempX, tempY] = 2;
                }



                if (!valid)
                {
                    points = new List<Coordinates>();
                }

                if(points.Count > 0)
                    allPaths.Add(points);
            }

            for(int i=0; i<300; i++)
            {
                for(int j=0; j<240; j++)
                {
                    if(matrix[i,j] != 2)
                        matrix[i,j] = 0;
                }
            }


            count = 0;
            return points;
        }

        private void BfsAlgorithm(int[,] matrix, Queue<Coordinates> edges, Dictionary<Coordinates, Coordinates> path, Coordinates end)
        {
            count = 0;
            while (true)
            {
                count++;
                //if (count > 100)
                //    return;

                //if(count > 4000)
                //{
                //    printMatrix(matrix);
                //}
                //printMatrix(matrix);


                Coordinates edge;
                if (edges.Count() > 0)
                    edge = edges.Dequeue();
                else
                    return;

                int tempX = (int)edge.X;
                int tempY = (int)edge.Y;

                if (tempX - 1 < 0 || tempY - 1 < 0 || tempX + 1 > 300 || tempY + 1 > 240)
                    continue;

                if (edge.X == end.X && edge.Y == end.Y)
                {
                    matrix[tempX, tempY] = 1;
                    var newEdge = new Coordinates()
                    {
                        X = tempX,
                        Y = tempY
                    };
                    path[newEdge] = edge;
                    edges.Enqueue(newEdge);
                    return;
                }

                if (tempX < 300 && matrix[tempX + 1, tempY] == 0)
                {
                    matrix[tempX + 1, tempY] = 1;
                    var newEdge = new Coordinates()
                    {
                        X = tempX + 1,
                        Y = tempY
                    };
                    path[newEdge] = edge;
                    edges.Enqueue(newEdge);
                }

                if (tempY < 240 && matrix[tempX, tempY + 1] == 0)
                {
                    matrix[tempX, tempY + 1] = 1;
                    var newEdge = new Coordinates()
                    {
                        X = tempX,
                        Y = tempY + 1
                    };
                    path[newEdge] = edge;
                    edges.Enqueue(newEdge);
                }

                if (tempX > 0 && matrix[tempX - 1, tempY] == 0)
                {
                    matrix[tempX - 1, tempY] = 1;
                    var newEdge = new Coordinates()
                    {
                        X = tempX - 1,
                        Y = tempY
                    };
                    path[newEdge] = edge;
                    edges.Enqueue(newEdge);
                }

                if (tempY > 0 && matrix[tempX, tempY - 1] == 0)
                {
                    matrix[tempX, tempY - 1] = 1;
                    var newEdge = new Coordinates()
                    {
                        X = tempX,
                        Y = tempY - 1
                    };
                    path[newEdge] = edge;
                    edges.Enqueue(newEdge);
                }
            }
            
        }

        private void printMatrix(int[,] matrix)
        {
            using (StreamWriter writer = new StreamWriter("matrix.txt"))
            {
                for (int i = 0; i < maxWidth; i++)
                {
                    writer.WriteLine();
                    for (int j = 0; j < maxHeight; j++)
                    {
                        writer.Write(matrix[i, j]);
                    }
                }
            }
        }

        public List<Coordinates> CalculateEdgeCoords(Coordinates start, Coordinates end)
        {
            var result = new List<Coordinates>();

            var tempX = start.X;
            var tempY = start.Y;

            result.Add(new Coordinates()
            {
                X = tempX + _nodeWidth/2,
                Y = tempY + _nodeWidth/2
            });

            for(int i=0; i<=1; i++)
            {
                double coordX = tempX;
                double coordY = tempY;

                if (tempX != end.X)
                {
                    coordX = tempX;
                    coordY = tempY;
                    while (coordX != end.X)
                    {
                        result.Add(new Coordinates()
                        {
                            X = coordX + _nodeWidth / 2,
                            Y = coordY + _nodeWidth / 2
                        });

                        if (coordX < end.X)
                            coordX++;
                        else
                            coordX--;
                    }

                }
                else if (tempY != end.Y)
                {
                    coordX = tempX;
                    coordY = tempY;
                    while (coordY != end.Y)
                    {
                        result.Add(new Coordinates()
                        {
                            X = coordX + _nodeWidth / 2,
                            Y = coordY + _nodeWidth / 2
                        });

                        if (coordY < end.Y)
                            coordY++;
                        else
                            coordY--;
                    }

                }

                tempX = coordX;
                tempY = coordY;

                result.Add(new Coordinates()
                {
                    X = tempX + _nodeWidth / 2,
                    Y = tempY + _nodeWidth / 2
                });
            }

            return result;
        }

        public List<SwitchEntity> CalculateSwitchesCoordByResolution(List<SwitchEntity> switches)
        {
            var result = new List<SwitchEntity>();

            foreach (var item in switches)
            {
                double tempX = item.X;
                double tempY = item.Y;

                double coordX = Math.Floor(tempX / _resolution);
                double coordY = Math.Floor(tempY / _resolution);
                item.X = Math.Floor(tempX / _resolution) * _resolution - _switchWidth / 2;
                item.Y = Math.Floor(tempY / _resolution) * _resolution - _switchWidth / 2;

                if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), item))
                {
                    result.Add(item);
                    continue;
                }

                int radius = 0;
                bool added = false;
                while (radius <= 100)
                {
                    radius++;

                    tempX = coordX - radius;
                    tempY = coordY - radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempX += i;

                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _switchWidth / 2,
                            Y = tempY * _resolution - _switchWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                    tempX = coordX + radius;
                    tempY = coordY - radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempY += i;
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _switchWidth / 2,
                            Y = tempY * _resolution - _switchWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                    tempX = coordX + radius;
                    tempY = coordY + radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempX -= i;
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _switchWidth / 2,
                            Y = tempY * _resolution - _switchWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                    tempX = coordX - radius;
                    tempY = coordY + radius;
                    for (int i = 0; i <= radius * 2; i++)
                    {
                        tempY -= i;
                        var coord = new PowerEntity()
                        {
                            X = tempX * _resolution - _switchWidth / 2,
                            Y = tempY * _resolution - _switchWidth / 2
                        };

                        if (!ContainsCoord(result.Cast<PowerEntity>().ToList(), coord))
                        {
                            item.X = coord.X;
                            item.Y = coord.Y;
                            result.Add(item);
                            added = true;
                            break;
                        }
                    }
                    if (added) break;

                }
            }

            return result;
        }

        public List<Coordinates> GetInersections()
        {
            return intersection;
        }

        public Dictionary<long, long> SetSwitchesPairs(List<SwitchEntity> switches, List<LineEntity> lines)
        {
            var result = new Dictionary<long, long>();

            foreach (var line in lines)
            {
                var firstNode = switches.Where(x => x.Id == line.FirstEnd).FirstOrDefault();
                var secondNode = switches.Where(x => x.Id == line.SecondEnd).FirstOrDefault();

                if (firstNode != null && secondNode != null)
                {
                    if (!result.ContainsKey(firstNode.Id) && !result.ContainsKey(secondNode.Id))
                    {
                        result.Add(firstNode.Id, secondNode.Id);
                    }
                }
            }

            return result;
        }

        public long GetElapsedTime(int type)
        {
            switch (type)
            {
                case 0: return stopwatch.ElapsedMilliseconds;
                case 1: return stopwatch2.ElapsedMilliseconds;
                default: return 0;
            }
            
        }
    }
}
