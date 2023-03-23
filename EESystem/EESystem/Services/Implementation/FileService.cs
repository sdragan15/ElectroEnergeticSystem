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
        private XmlDocument xmlDoc = new XmlDocument();
        private readonly string _filePath;
        private readonly ICalculationService _calculationService;

        public FileService(ICalculationService calculationService, string filePath)
        {
            _calculationService = calculationService;
            _filePath = filePath;
            xmlDoc.Load("Geographic.xml");
        }

        public void LoadNodesNetwork()
        {
            throw new NotImplementedException();
        }

        public List<SubstationEntity> LoadSubstationNetwork()
        {
            var result = new List<SubstationEntity>();

            XmlNodeList nodeList;

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {
                SubstationEntity sub = new SubstationEntity();
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);

                double noviX, noviY;
                _calculationService.ToLatLon(sub.X, sub.Y, 34, out noviY, out noviX);

                double canvasX, canvasY;
                _calculationService.CalculateCanvasCoords(noviX, noviY, out canvasX, out canvasY);
                sub.X = canvasX;
                sub.Y = canvasY;

                result.Add(sub);
            }

            return result;
        }
    }
}
