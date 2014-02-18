using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using SA1CService;

namespace Base1CGUI.ViewModel
{
    /// <summary>
    /// The ViewModel for the application's main window.
    /// </summary>
    public class MainWindowViewModel : WorkspaceViewModel
    {
        #region Fields
                
        ServiceSA1C _serviceSA1C;
        Stack<object> _workspaces; 
        string _displayName;

        #endregion // Fields

        #region Constructor

        public MainWindowViewModel(ServiceSA1C serviceSA1C)
        {
            DisplayName = "SA1C";

            _serviceSA1C = serviceSA1C;
            _serviceSA1C.LoadSettings();
            _workspaces = null;
        }

        #endregion // Constructor

       
        #region Workspaces

		public override string DisplayName {
			get { return _displayName; }
			set { 
				_displayName = value;
				OnPropertyChanged("DisplayName");
			}
		}
        
        /// <summary>
        /// Returns the collection of available workspaces to display.
        /// A 'workspace' is a ViewModel that can request to be closed.
        /// </summary>
        public override object Workspaces
        {
            get
            {
                if (_workspaces == null)
                {
                    _workspaces = new Stack<object>();
                    AllBaseConfigViewModel allBaseVM = new AllBaseConfigViewModel(this, _serviceSA1C);
                    _workspaces.Push(allBaseVM);
                    
                }

                object currentWorkspace = _workspaces.Peek();
                this.Commands = (currentWorkspace as WorkspaceViewModel).Commands;
                (currentWorkspace as WorkspaceViewModel).Refresh();
                
                return currentWorkspace;
            }

            set
            {
                (value as WorkspaceViewModel).RequestClose += this.OnWorkspaceRequestClose;
                _workspaces.Push(value);
                base.OnPropertyChanged("Workspaces");
            }
        }

        public void OnWorkspaceRequestClose(object sender, CloseEventArgs e)
        {
            for (int i = 0; i < e.numPageForClose; i++)
            {
                WorkspaceViewModel workspace = _workspaces.Pop() as WorkspaceViewModel;
                workspace.RequestClose -= this.OnWorkspaceRequestClose;
                workspace.Dispose();
            }
           
            base.OnPropertyChanged("Workspaces");      
        }

        #endregion // Workspaces

       
    }
}