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
            XmlNodeList nodeList;

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                LineEntity l = new LineEntity();
                l.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText, CultureInfo.InvariantCulture);
                l.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText, CultureInfo.InvariantCulture);

                result.Add(l);
            }

            return result;
        }
    }
}
