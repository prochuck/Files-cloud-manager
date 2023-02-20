using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Models
{
    class ProgramDataModel
    {
        public string PathToExe { get; set; }
        public string PathToData { get; set; }


        private string GetFullDataPath(string relativePath)
        {
            return $"{PathToData}/{relativePath}";
        }
    }
}
