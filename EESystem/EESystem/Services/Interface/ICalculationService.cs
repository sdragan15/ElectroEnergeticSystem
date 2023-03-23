using EESystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EESystem.Services.Interface
{
    public interface ICalculationService
    {
        public void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude);
        public void CalculateCanvasCoords(double x, double y, out double newX, out double newY);
        public List<SubstationEntity> CalculateSubstaionCoordByResolution(List<SubstationEntity> substations);
        public List<NodeEntity> CalculateNodesCoordByResolution(List<NodeEntity> nodes);
    }
}
