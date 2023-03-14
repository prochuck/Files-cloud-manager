using Files_cloud_manager.Client.Commands;
using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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

        public ICommand CreateProgramDataCommand { get; private set; }

        private ProgramsListModel _model;



        public ProgramListViewModel(ProgramsListModel model)
        {
            _model = model;
            ProgramsList = new ObservableCollection<ProgramDataViewModel>();

            CreateProgramDataCommand = new Command(e => CreateProgramData(), null);

            foreach (var item in _model.ProgramsList)
            {
                ProgramsList.Add(new ProgramDataViewModel(item));
            }
        }



        public void CreateProgramData()
        {
            _model.UpdateFileGroups();
            GroupCreationView groupCreationView = new GroupCreationView(_model.FileGroups.ToList());
            if (groupCreationView.ShowDialog() is bool res && res)
            {
                GroupCreationViewModel groupCreationViewModel = groupCreationView.DataContext as GroupCreationViewModel;
                try
                {
                    _model.CreateNewProgramData(groupCreationViewModel.PathToExe, groupCreationViewModel.PathToData, groupCreationViewModel.GroupName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
