using Files_cloud_manager.Client.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.Models
{
    internal class ProgramsListModel
    {
				private ObservableCollection<ProgramDataModel> _programsList;
        public ReadOnlyObservableCollection<ProgramDataModel> ProgramsList;
        private ServerConnectionService _connectionService;

        public ProgramsListModel(ServerConnectionService connectionService)
        {
            _connectionService = connectionService;

            _programsList = new ObservableCollection<ProgramDataModel>(_connectionService);
            ProgramsList = new ReadOnlyObservableCollection<ProgramDataModel>(_programsList);
       
        }

    }
}
