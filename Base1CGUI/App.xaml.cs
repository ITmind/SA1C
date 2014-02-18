using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Text.RegularExpressions;
using System.Collections;
using NLog;
using SA1CService;
using Base1CGUI.ViewModel;
using Base1CGUI.View;

namespace Base1CGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Hashtable CommandLineArgs = new Hashtable();
        Logger logger = LogManager.GetCurrentClassLogger();
        //public delegate void CloseEventHandler(object sender, CloseEventArgs e);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //если нет аргументов то запустим GUI
            if (e.Args.Length == 0)
            {
                MainWindow window = new MainWindow();


                ServiceSA1C s = new ServiceSA1C();
                var viewModel = new MainWindowViewModel(s);

                viewModel.RequestClose += OnMainWindowRequestClose;

                window.DataContext = viewModel;

                window.Show();

            }
            //иначе отработаем аргументы
            else
            {
                ParseArguments(e.Args);
                EcecuteCmd();
            }
        }

        public void OnMainWindowRequestClose(object sender, CloseEventArgs e)
        {
            (sender as WorkspaceViewModel).RequestClose -= OnMainWindowRequestClose;
            Application.Current.MainWindow.Close();
            
        }

        private void ParseArguments(string[] Args)
        {
            string pattern = @"(?<argname>/\w+):(?<argvalue>\w+)";
            foreach (string arg in Args)
            {
                Match match = Regex.Match(arg, pattern);

                // If match not found, command line args are improperly formed.
                if (!match.Success)
                {
                    logger.Error("The command line arguments are improperly formed. Use /argname:argvalue.");
                    //throw new ArgumentException("The command line arguments are improperly formed. Use /argname:argvalue.");
                }

                // Store command line arg and value
                CommandLineArgs[match.Groups["argname"].Value] = match.Groups["argvalue"].Value;
            }
        }

        private void EcecuteCmd()
        {
            if (CommandLineArgs.ContainsKey("/setting"))
            {
                string settingName = CommandLineArgs["/setting"] as string;
                logger.Info("Запуск с праметром /setting:" + settingName);

                //загружаем настройки
                ServiceSA1C serviceSA1C = new ServiceSA1C();
                serviceSA1C.LoadSettings();
                //и запускаем обмен
                serviceSA1C.ExecuteExchange(settingName);
                //выходим из программы
                App.Current.Shutdown();
            }
        }

    }
}
