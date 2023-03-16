using Files_cloud_manager.Client.Commands;
using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.Services.Interfaces;
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

        private IDialogService _dialogService;

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



        public ProgramListViewModel(ProgramsListModel model,IDialogService dialogService)
        {
            _dialogService=dialogService;
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

            GroupCreationViewModel groupCreationViewModel = _dialogService.ShowGroupCreationDialog(_model.FileGroups.ToList());
            if (groupCreationViewModel is null)
            {
                return;
            }
            try
            {
                ProgramsList.Add(new ProgramDataViewModel(
                    _model.CreateNewProgramData(
                        groupCreationViewModel.PathToExe,
                        groupCreationViewModel.PathToData,
                        groupCreationViewModel.GroupName
                        )
                    ));

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        public void Dispose()
        {
        }
    }
}