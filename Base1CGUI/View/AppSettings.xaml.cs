/*
 * Сделано в SharpDevelop.
 * Пользователь: Кулик
 * Дата: 09.03.2011
 * Время: 8:51
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Base1CGUI.ViewModel;

namespace Base1CGUI.View
{
	/// <summary>
	/// Interaction logic for AppSettings.xaml
	/// </summary>
	public partial class AppSettings : UserControl
	{
		public AppSettings()
		{
			InitializeComponent();
		}
		
		//по другому увы никак
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            AppSettingsViewModel vm = DataContext as AppSettingsViewModel;
            if(vm!=null)
                vm.Email.SetPassword((sender as PasswordBox).Password);
        }
	}
}