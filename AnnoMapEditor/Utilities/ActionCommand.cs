using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnnoMapEditor.Utilities
{
    public class ActionCommand : ICommand
    {
        private readonly Action<object?> _action;

        public ActionCommand(Action<object?> action)
        {
            _action = action;
        }

        public void Execute(object? parameter)
        {
            _action(parameter);
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
