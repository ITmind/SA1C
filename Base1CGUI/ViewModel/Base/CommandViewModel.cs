using System;
using System.Windows.Input;

namespace Base1CGUI.ViewModel
{
    /// <summary>
    /// Represents an actionable item displayed by a View.
    /// </summary>
    public class CommandViewModel : ViewModelBase
    {
        bool _isEnabled;

        public CommandViewModel(string displayName, ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            base.DisplayName = displayName;
            this.Command = command;
            _isEnabled = true;
        }

        public CommandViewModel(string displayName, ICommand command, bool isEnabled)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            base.DisplayName = displayName;
            this.Command = command;
            _isEnabled = isEnabled;
        }

        public ICommand Command { get; private set; }

        public bool IsEnabled {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                base.OnPropertyChanged("IsEnabled");
            }
        }
    }
}