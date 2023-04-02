using EESystem.Model;
using EESystem.Services.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace EESystem.Services.Implementation
{
    public class FileService : IFileService
    {
        private readonly double _canvasWidth;
        private readonly double _canvasHeight;
        private XmlDocument xmlDoc = new XmlDocument();
        private readonly string _filePath;
        private readonly ICalculationService _calculationService;

        public FileService(ICalculationService calculationService, string filePath, 
            double canvasWidth, double canvasHeight)
        {
            _calculationService = calculationService;
            _filePath = filePath;
            xmlDoc.Load("Geographic.xml");
            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
        }

        public List<NodeEntity> LoadNodesNetwork()
        {
            var result = new List<NodeEntity>();

            XmlNodeList nodeList;


            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
            foreach (XmlNode node in nodeList)
            {
                NodeEntity nodeobj = new NodeEntity();

                nodeobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeobj.Name = node.SelectSingleNode("Name").InnerText;
                nodeobj.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
                nodeobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);

                double noviX, noviY;
                double canvasX, canvasY;

                _calculationService.ToLatLon(nodeobj.X, nodeobj.Y, 34, out noviY, out noviX);

                _calculationService.CalculateCanvasCoords(noviX, noviY, out canvasX, out canvasY);

                nodeobj.X = canvasX * _canvasWidth;
                nodeobj.Y = canvasY * _canvasHeight;
                result.Add(nodeobj);
            }

            return result;
        }

        public List<SubstationEntity> LoadSubstationNetwork()
        {
            var result = new List<SubstationEntity>();

            XmlNodeList nodeList;

            double noviX, noviY;
            double canvasX, canvasY;

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {
                SubstationEntity sub = new SubstationEntity();
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);
                sub.Uid = Guid.NewGuid().ToString();
                _calculationService.ToLatLon(sub.X, sub.Y, 34, out noviY, out noviX);

                _calculationService.CalculateCanvasCoords(noviX, noviY, out canvasX, out canvasY);
                sub.X = canvasX * _canvasWidth;
                sub.Y = canvasY * _canvasHeight;

                result.Add(sub);
            }

            return result;
        }

        public List<LineEntity> LoadLinesNetwork()
        {
            var result = new List<LineEntity>();
            XmlNodeList lineList;

            lineList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode line in lineList)
            {
                LineEntity l = new LineEntity();
                l.FirstEnd = long.Parse(line.SelectSingleNode("FirstEnd").InnerText, CultureInfo.InvariantCulture);
                l.SecondEnd = long.Parse(line.SelectSingleNode("SecondEnd").InnerText, CultureInfo.InvariantCulture);

                //if(result.FirstOrDefault(x => x.FirstEnd == l.FirstEnd) != null && result.FirstOrDefault(x => x.SecondEnd == l.SecondEnd) != null)
                //{
                //    continue;
                //}

                //if (result.FirstOrDefault(x => x.FirstEnd == l.SecondEnd) != null && result.FirstOrDefault(x => x.SecondEnd == l.FirstEnd) != null)
                //{
                //    continue;
                //}

                //if (l.FirstEnd == l.SecondEnd)
                //    continue;

                result.Add(l);
            }

            return result;
        }

        public List<SwitchEntity> LoadSwitchesNetwork()
        {
            XmlNodeList nodeList;

            var result = new List<SwitchEntity>();

            double noviX, noviY;
            double canvasX, canvasY;

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
            foreach (XmlNode node in nodeList)
            {
                SwitchEntity switchobj = new SwitchEntity();
                switchobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText, CultureInfo.InvariantCulture);
                switchobj.Name = node.SelectSingleNode("Name").InnerText;
                switchobj.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
                switchobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);
                switchobj.Status = node.SelectSingleNode("Status").InnerText;

                _calculationService.ToLatLon(switchobj.X, switchobj.Y, 34, out noviY, out noviX);

                _calculationService.CalculateCanvasCoords(noviX, noviY, out canvasX, out canvasY);
                switchobj.X = canvasX * _canvasWidth;
                switchobj.Y = canvasY * _canvasHeight;
                switchobj.Uid = Guid.NewGuid().ToString();
                result.Add(switchobj);
            }

            return result;
        }
    }
}
