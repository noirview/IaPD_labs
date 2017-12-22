using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NativeWifi;
using SimpleWifi;
using Wireless.Properties;

namespace Wireless
{
    public partial class WifiManagerForm : Form
    {
        private static Wifi _wifi;
        private List<AccessPoint> _accessPoints;

        private const int UpdateFrequencyInMillis = 10000;

        public WifiManagerForm()
        {
            InitializeComponent();
        }

        private void InitializeForm(object sender, EventArgs e)
        {
            _wifi = new Wifi();
            RefreshStatus();
        }

        private async void RefreshStatus()
        {
            // The UI Thread TaskScheduler instance
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            while (true)
            {
                var task = Task.Run(() =>
                {
                    RefreshAvailableWifiConnections();
                });
                await Task.Delay(UpdateFrequencyInMillis);
            }
        }

        private delegate void AddListViewItemsCallBack(List<ListViewItem> items);

        private void AddListViewItems(List<ListViewItem> items)
        {
            if (this.connectionListView.InvokeRequired)
            {
                var callBack = new AddListViewItemsCallBack(AddListViewItems);
                this.Invoke(callBack, new object[] { items });
            }
            else
            {
                connectionListView.Items.Clear();
                foreach (var item in items)
                {
                    connectionListView.Items.Add(item);
                }
            }
        }

        private void ConnectButtonClick(object sender, EventArgs e)
        {
            if (connectionListView.SelectedItems.Count > 0 && passwordLabel.Text.Length > 0)
            {
                var selectedItem = connectionListView.SelectedItems[0];
                var ap = (AccessPoint) selectedItem.Tag;

                statusLabel.Text = ConnectToWifi(ap, passwordTextBox.Text)
                    ? $"You connected successfully to the network {ap.Name}."
                    : $"Connection failed to the network {ap.Name}.";
            }
            else
            {
                statusLabel.Text = Resources.try_connect_warning_message;
            }
            passwordTextBox.Text = "";
        }

        private static bool ConnectToWifi(AccessPoint ap, string password)
        {
            var authRequest = new AuthRequest(ap) {Password = password};
            return ap.Connect(authRequest);
        }

        private void RefreshAvailableWifiConnections()
        {
            _accessPoints = _wifi.GetAccessPoints();

            var items = new List<ListViewItem>();
           
            foreach (var ap in _accessPoints)
            {
                var item = new ListViewItem(ap.Name);
                item.SubItems.Add(ap.MacAddress);
                item.SubItems.Add($"{ap.SignalStrength.ToString()} %");
                item.SubItems.Add(ap.AuthType);

                item.Tag = ap;
                items.Add(item);
            }
                    
          
            AddListViewItems(items);
        }
    }
}
