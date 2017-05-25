using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace StateMachineCommentsReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IReaderModel _model = new ReaderModel(new StandardStateMachineReader());

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Window mv = new MainWindow();
            mv.DataContext = new ReaderModelView(_model);
            mv.Show();
        }
    }
}
