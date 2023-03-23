using Files_cloud_manager.Client.Commands;
using Files_cloud_manager.Client.Extentions;
using Files_cloud_manager.Client.Models;
using Files_cloud_manager.Client.Services;
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
        public ProgramDataViewModel SelectedProgram
        {
            get { return _selectedProgram; }
            set
            {
                _selectedProgram = value;
                OnPropertyChanged(nameof(SelectedProgram));
            }
        }

        public ICommand CreateProgramDataCommand { get; private set; }
        public ICommand LogIn { get; private set; }

        private ProgramsListModel _model;

        private IDialogService _dialogService;
        private IProgramListCaretaker _programListCaretaker;


        public ProgramListViewModel(ProgramsListModel model, IProgramListCaretaker programListCaretaker, IDialogService dialogService)
        {
            _dialogService = dialogService;
            _model = model;
            _programListCaretaker = programListCaretaker;

            _model.SetMemento(_programListCaretaker.Memento);

            ProgramsList = new ObservableCollection<ProgramDataViewModel>();

            CreateProgramDataCommand = new Command(e => CreateProgramData(), null);

            LogIn = new Command(e => _dialogService.ShowLoginDialog()) ;

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
                    _model.CreateProgramData(
                        groupCreationViewModel.PathToExe,
                        groupCreationViewModel.PathToData,
                        groupCreationViewModel.GroupName
                        )
                    ));

                _programListCaretaker.Memento = _model.CreateMemento();
                _programListCaretaker.SaveToFile();
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