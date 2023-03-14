using Files_cloud_manager.Client.Commands;
using Files_cloud_manager.Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace Files_cloud_manager.Client.ViewModels
{
    internal class ProgramDataViewModel : ViewModelBase, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ProgramDataModel _model;


        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        public string PathToExe
        {
            get { return _model.PathToExe; }
            set
            {
                _model.PathToExe = value;
                OnPropertyChanged(nameof(PathToExe));
            }
        }

        public string PathToData
        {
            get { return _model.PathToData; }
            set
            {
                _model.PathToData = value;
                OnPropertyChanged(nameof(PathToData));
            }
        }

        public string GroupName
        {
            get { return _model.GroupName; }
        }

        private bool _isSyncFromClient;
        public bool IsSyncFromClient
        {
            get { return _isSyncFromClient; }
            set
            {
                _isSyncFromClient = value;
                OnPropertyChanged(nameof(IsSyncFromClient));
            }
        }



        string _errorText;
        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                _errorText = value;
                OnPropertyChanged(nameof(ErrorText));
            }
        }



        public ICommand SyncFilesAsyncCommand { get; private set; }

        public ICommand CompareFilesCommand { get; private set; }

        public ICommand RollBackCommand { get; private set; }

        public ReadOnlyObservableCollection<FileDifferenceModel> Files
        {
            get
            {
                return _model.FileDifferences;
            }
        }

        public ProgramDataViewModel(ProgramDataModel model)
        {
            _model = model;

            SyncFilesAsyncCommand = new AsyncCommand(async e =>
            {
                if (IsSyncFromClient)
                {
                    await SyncFilesAsync(SyncDirection.FromClient).ConfigureAwait(false);
                }
                else
                {
                    await SyncFilesAsync(SyncDirection.FromServer).ConfigureAwait(false);
                }
            }, null, e => ErrorText = e);
            CompareFilesCommand = new AsyncCommand(async e =>
            {
                await CompareLocalFilesToServerAsync().ConfigureAwait(false);
                OnPropertyChanged(nameof(SyncFilesAsyncCommand));
            }, null, e => ErrorText = e);

            RollBackCommand = new AsyncCommand(async e => await _model.TryRollBackSyncByNameAsync(), null, null);

        }

        public void CancelOperations()
        {
            CancellationTokenSource.Cancel();
        }

        public async Task SyncFilesAsync(SyncDirection syncDirection)
        {
            await _model.SynchronizeFilesAsync(syncDirection, CancellationTokenSource.Token).ConfigureAwait(false);
        }

        public async Task CompareLocalFilesToServerAsync()
        {
            await _model.CompareLocalFilesToServerAsync(CancellationTokenSource.Token).ConfigureAwait(false);
        }
        public void Dispose()
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }
    }
}
