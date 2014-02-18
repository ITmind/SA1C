using System;
using System.Collections.Generic;
using System.Linq;
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
using Base1CGUI.ViewModel;

namespace Base1CGUI.View
{
    /// <summary>
    /// Interaction logic for BaseConfig.xaml
    /// </summary>
    public partial class BaseConfigPage : UserControl
    {
        public BaseConfigPage()
        {
            InitializeComponent();
        }

        //по другому увы никак
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            BaseConfigViewModel vm = DataContext as BaseConfigViewModel;
            if(vm!=null)
                vm.baseInfo.SetPassword((sender as PasswordBox).Password);
        }
		
		void passwordBox1_GotFocus(object sender, RoutedEventArgs e)
		{
			//(sender as PasswordBox).
		}
		
		void passwordBox1_MouseEnter(object sender, MouseEventArgs e)
		{
			//passwordBox1.SelectAll();
		}
    }
}
