using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Base1CGUI.ViewModel
{
    public static class ControlBehaviours
    {
        public static readonly DependencyProperty DoubleClickCommand = EventBehaviourFactory.CreateCommandExecutionEventBehaviour(
            Control.MouseDoubleClickEvent, 
            "DoubleClickCommand", 
            typeof (ControlBehaviours));

        public static void SetDoubleClickCommand(Control o, ICommand command)
        {
            o.SetValue(DoubleClickCommand, command);
        }

        public static void GetDoubleClickCommand(Control o)
        {
            o.GetValue(DoubleClickCommand);
        }
    }
}
