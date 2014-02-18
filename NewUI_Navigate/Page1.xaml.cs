using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using SA1CService;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NewUI_Navigate
{
	/// <summary>
	/// Interaction logic for Page1.xaml
	/// </summary>
	public partial class Page1 : Page
	{
        ServiceSA1C _serviceSA1C;
        BaseConfigElemen CurrentConfig;

		public Page1()
		{
			InitializeComponent();
		}

        public ObservableCollection<BaseConfigElemen> AllConfigs { get; private set; }

        void CreateAllBaseConfig()
        {
            List<BaseConfigElemen> all =
                (from cust in _serviceSA1C.config.basesConfig
                 select new BaseConfigElemen(_serviceSA1C)).ToList();

            //foreach (BaseConfigElemen cvm in all)
            //    cvm.PropertyChanged += this.OnCustomerViewModelPropertyChanged;

            this.AllConfigs = new ObservableCollection<BaseConfigElemen>(all);
            this.AllConfigs.CollectionChanged += this.OnCollectionChanged;
        }

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (BaseConfigElemen custVM in e.NewItems)
                    custVM.PropertyChanged += this.OnCustomerViewModelPropertyChanged;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (BaseConfigElemen custVM in e.OldItems)
                    custVM.PropertyChanged -= this.OnCustomerViewModelPropertyChanged;
        }

        void OnCustomerViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsSelected")
            {
                return;
            }

            if ((sender as BaseConfigElemen).IsSelected)
            {
                CurrentConfig = (BaseConfigElemen)sender;
            }
            //(sender as BaseConfigViewModel).VerifyPropertyName(IsSelected);
        }
	}
}