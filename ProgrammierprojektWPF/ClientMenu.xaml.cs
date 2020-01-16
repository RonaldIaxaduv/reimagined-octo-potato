using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Communication;
using System.Threading.Tasks;

namespace ProgrammierprojektWPF
{
    /// <summary>
    /// Interaction logic for ClientMenu.xaml
    /// </summary>
    public partial class ClientMenu : Window, IDisposable
    {
        private uint msgBufSz;
        private uint MessageBufferSize
        {
            get { return msgBufSz; }
            set
            {
                if (value > 0)
                {
                    msgBufSz = value;
                    lbMessages_ItemsChanged(lbChatMessages.Items, null);
                }
            }
        }
        private List<string> userList = new List<string>(); //used to get the username from the item selected in lbUsers
        public Client wrapper;

        public ClientMenu()
        {
            InitializeComponent();
            tblVersion.Text = "Version Number: " + VersionNumber.get();
            ((INotifyCollectionChanged)lbChatMessages.Items).CollectionChanged += lbMessages_ItemsChanged;
        }
        public ClientMenu(Client wrapper) : this()
        {
            this.wrapper = wrapper;
        }


        public async Task updateUserList(List<string> onlineUsers)
        {
            lbUsers.Items.Clear(); userList.Clear();
            foreach (string username in onlineUsers) //clients can only display other online clients. including all offline clients would probably be a violation of privacy anyway
            { lbUsers.Items.Add(ListBoxUserItem.generate(lbUsers.FontSize, username, true)); userList.Add(username); }
        }
        public void addChatMessage(ChatMessage msg)
        {
            lbChatMessages.Items.Add(ListBoxChatItem.generate(msg.sender, msg));
        }
        

        //controls
        private async void cmdWhisper_Click(object sender, RoutedEventArgs e)
        {
            if (lbUsers.SelectedIndex < 0)
            { MessageBox.Show("Select the user whom you'd like to message.", "No User Selected", MessageBoxButton.OK, MessageBoxImage.Error); }
            else
            {
                if (tbMessage.Text == "")
                { MessageBox.Show("Please enter a message to send.", "No Message Entered", MessageBoxButton.OK, MessageBoxImage.Error); }
                else
                {
                    cmdWhisper.IsEnabled = false;
                    cmdGlobalMessage.IsEnabled = false;
                    string recipient = userList[lbUsers.SelectedIndex];
                    string msg = tbMessage.Text;
                    tbMessage.Text = "";
                    lbUsers.SelectedIndex = -1; //unselect user
                    await wrapper.requestWhisperChatMessage(recipient, msg);
                    cmdWhisper.IsEnabled = true;
                    cmdGlobalMessage.IsEnabled = true;
                }
            }
        }
        private async void cmdGlobalMessage_Click(object sender, RoutedEventArgs e)
        {
            if (tbMessage.Text == "")
            { MessageBox.Show("Please enter a message to send to all users (bottom-left box).", "No Message Entered", MessageBoxButton.OK, MessageBoxImage.Error); }
            else
            {
                cmdWhisper.IsEnabled = false;
                cmdGlobalMessage.IsEnabled = false;
                string msg = tbMessage.Text;
                tbMessage.Text = "";
                await wrapper.requestBroadcastChatMessage(msg);
                cmdWhisper.IsEnabled = true;
                cmdGlobalMessage.IsEnabled = true;
                lbUsers.SelectedIndex = -1; //unselect user
            }
        }

        private void cmdLogout_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void tbBuffer_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                uint input = uint.Parse(tbBuffer.Text);
                MessageBufferSize = input;
            }
            catch (Exception)
            { tbBuffer.Text = "1000"; }
        }

        private void lbMessages_ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            while (((ItemCollection)sender).Count > MessageBufferSize)
            { ((ItemCollection)sender).RemoveAt(0); }
            ((ItemCollection)sender).MoveCurrentToLast();
            if (lbChatMessages.Items.Count > 0)
            { lbChatMessages.ScrollIntoView(lbChatMessages.Items.GetItemAt(lbChatMessages.Items.Count - 1)); }
        }


        //shutdown
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try { wrapper.sender.Close(); } catch (Exception) { }
            Dispose(); //sever connection
            MainWindow newMainWindow = new MainWindow();
            newMainWindow.Show();
        }
        public void Dispose()
        {
            wrapper = null;
        }
    }
}
