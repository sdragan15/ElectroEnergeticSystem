using EESystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace EESystem.Services.Interface
{
    public interface IFileService
    {
        public List<SubstationEntity> LoadSubstationNetwork();
        public List<NodeEntity> LoadNodesNetwork();
        public List<LineEntity> LoadLinesNetwork();
        public List<SwitchEntity> LoadSwitchesNetwork();
    }
}
