using Files_cloud_manager.Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files_cloud_manager.Client.ViewModels
{
    internal class ProgramListViewModel : ViewModelBase, IDisposable
    {
        public ObservableCollection<ProgramDataViewModel> ProgramsList { get; private set; }

        private ProgramDataViewModel _selectedProgram;

        public ProgramDataViewModel SelectedProgram
        {
            get { return _selectedProgram; }
            set
            {
                if (_selectedProgram is not null)
                {
                    _selectedProgram.CancelOperations();
                }
                _selectedProgram = value;
                OnPropertyChanged(nameof(SelectedProgram));
            }
        }


        private ProgramsListModel _model;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ProgramListViewModel(ProgramsListModel model)
        {
            _model = model;
            ProgramsList = new ObservableCollection<ProgramDataViewModel>();

            foreach (var item in _model.ProgramsList)
            {
                ProgramsList.Add(new ProgramDataViewModel(item));
            }
        }
       
        public void Dispose()
        {

        }
    }
}
