using System;
using System.Windows.Input;

namespace YNOV_Password.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (_canExecute == null)
                return true;

            if (parameter is T t)
                return _canExecute(t);
            
            // Si T est string et parameter est null, traiter comme chaîne vide
            if (typeof(T) == typeof(string) && parameter == null)
                return _canExecute((T)(object)"");
                
            // Pour les types référence nullable
            if (parameter == null && !typeof(T).IsValueType)
                return _canExecute(default(T)!);
                
            return false;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T t)
            {
                _execute(t);
            }
            else if (typeof(T) == typeof(string) && parameter == null)
            {
                _execute((T)(object)"");
            }
            else if (parameter == null && !typeof(T).IsValueType)
            {
                _execute(default(T)!);
            }
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object? parameter)
        {
            _execute();
        }

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
