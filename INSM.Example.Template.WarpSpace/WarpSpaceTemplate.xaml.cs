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
using API = INSM.Template.Framework.v1;

namespace INSM.Example.Template.WarpSpace
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class WarpSpaceTemplate : UserControl, API.ITemplateControl
    {
        private API.ITemplate _Template;
        private API.IWarpSpace _WarpSpace;

        public WarpSpaceTemplate()
        {
            InitializeComponent();
            _Template = new API.Template(null);
            _Template.TemplateDataSet.TemplateDataSetChanged2 += TemplateDataSet_TemplateDataSetChanged2;

        }

        private void TemplateDataSet_TemplateDataSetChanged2(object sender, API.TemplateDataSetEventArgs e)
        {
            string masterIP = e.TemplateDataSet.GetTemplateDataSetItemAsText("MasterIP");

            if (_WarpSpace == null)
            {
                //Assume only one template per player
                _WarpSpace = _Template.GetWarpSpace("MyWarpSpaceChannel", masterIP);
                _WarpSpace.PeerRegistered += _WarpSpace_PeerRegistered;
                _WarpSpace.MessageReceived += _WarpSpace_MessageReceived;
            }
        }

        private void _WarpSpace_PeerRegistered(object sender, API.PeerRegisteredEventArgs e)
        {
            _Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "New peer was registered from " + e.IP + " " + e.ID);
        }

        private void _WarpSpace_MessageReceived(object sender, API.PeerMessageReceivedEventArgs e)
        {
            _Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "WarpSpace message received from " + e.IP + " " + e.Id + " key: " + e.Key + " " + e.Value);

            e.Reply = "SomeReturnValue";
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _WarpSpace.SendMessageToPeers("MyKey", "MyValue");
        }

        public API.ITemplate TemplateInstance
        {
            get { return _Template; }
        }
    }
}
