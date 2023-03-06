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

        public ProgramsListModel()
        {
            _programsList=new ObservableCollection<ProgramDataModel>();
            ProgramsList = new ReadOnlyObservableCollection<ProgramDataModel>(_programsList);
        }



        private 

    }
}
