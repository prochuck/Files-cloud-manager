using Files_cloud_manager.Client.Models.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Models.States
{
    public class ProgramListMemento
    {
        public List<ProgramDataModelDTO> ProgramsList { get; set; } = new List<ProgramDataModelDTO>();
    }
}
