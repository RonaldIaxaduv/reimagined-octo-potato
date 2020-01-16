using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Threading;

namespace ProgrammierprojektWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void cmdConnectFour_Click(object sender, RoutedEventArgs e)
        {
            //tbd next milestone
        }

        private void cmdChomp_Click(object sender, RoutedEventArgs e)
        {
            //tbd next milestone
        }

        private void cmdServer_Click(object sender, RoutedEventArgs e)
        {
            //using (Server s = new Server())
            //{
            //    s.startServer().GetAwaiter().GetResult();
            //}
            Server s = new Server();
            s.startServer();
            Close();
        }

        private void cmdClient_Click(object sender, RoutedEventArgs e)
        {
            //using (Client c = new Client())
            //{
            //    //c.startClient().GetAwaiter().GetResult();
            //    //await c.startClient().ConfigureAwait(false);
            //    await c.startClient();
            //}

            //var newWindowThread = new Thread(new ThreadStart(async () =>
            //{
            //    Client c = new Client();
            //    //c.myWindow.Show();
            //    //System.Windows.Threading.Dispatcher.Run();
            //    await c.startClient().ConfigureAwait(false);
            //    //await c.startClient();
            //}));
            //newWindowThread.SetApartmentState(ApartmentState.STA);
            ////newWindowThread.IsBackground = true;
            //newWindowThread.IsBackground = false;
            //newWindowThread.Start();
            //Console.WriteLine(newWindowThread.IsAlive);

            Client c = new Client();
            c.startClient();
            Close();

            //Close();
        }

        private void cmdQuit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
