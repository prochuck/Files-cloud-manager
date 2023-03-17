using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Models.DTO
{
    [Serializable]
    internal class ProgramDataModelDTO
    {
        public ProgramDataModelDTO()
        {
        }
        public ProgramDataModelDTO(ProgramDataModel programDataModel)
        {
            PathToData = programDataModel.PathToData;
            PathToExe = programDataModel.PathToExe;
            GroupName = programDataModel.GroupName;
        }
        public ProgramDataModelDTO(string pathToExe, string pathToData, string groupName)
        {
            PathToExe = pathToExe;
            PathToData = pathToData;
            GroupName = groupName;
        }

        public string PathToExe { get; set; }
        public string PathToData { get; set; }
        public string GroupName { get; set; }


    }
}
