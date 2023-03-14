using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Files_cloud_manager.Client.Commands
{
    internal class AsyncCommand : ICommand
    {
        private Func<object?, Task> _execute;
        private Func<object?, bool>? _canExecute;
        private Action<string?>? _errorMessageCallBack;
        private bool _isExecuting = false;

        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        public bool IsExecuting
        {
            get
            {
                return _isExecuting;
            }
            private set
            {
                _isExecuting = value;
                CanExecuteChanged?.Invoke(this, new EventArgs());
            }
        }

        public AsyncCommand(Func<object?, Task> command, Func<object?, bool>? canExecute, Action<string?>? callBack)
        {
            _execute = command;
            _canExecute = canExecute;
            _errorMessageCallBack = callBack;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return !IsExecuting && ((_canExecute is null) || _canExecute(parameter));
        }

        public async void Execute(object? parameter)
        {
            await _semaphoreSlim.WaitAsync();
            IsExecuting = true;
            try
            {
                await ExecuteAsync(parameter);
                if (_errorMessageCallBack is not null)
                    _errorMessageCallBack(null);
            }
            catch (Exception e)
            {
                if (_errorMessageCallBack is not null)
                    _errorMessageCallBack(e.Message);
            }
            finally
            {
                IsExecuting = false;
                _semaphoreSlim.Release();
            }
        }

        public virtual Task ExecuteAsync(object? parameter)
        {
            return _execute(parameter);
        }
    }
}
