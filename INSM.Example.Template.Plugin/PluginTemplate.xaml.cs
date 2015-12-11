using INSM.Template.Framework.v1;
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
using System.Windows.Threading;
using API = INSM.Template.Framework.v1;

namespace INSM.Example.Template.Plugin
{
    /// <summary>
    /// Template for receiving data from 
    /// </summary>
    public partial class PluginTemplate : UserControl, ITemplateControl
    {
        private ITemplate _Template;
        private IPlugIn _Plugin;
        private Dispatcher _Dispatcher;

        public PluginTemplate()
        {
            InitializeComponent();
            _Dispatcher = this.Dispatcher;

            _Template = new API.Template(null);

            _Plugin = _Template.GetPlugIn("Pickup");
            _Plugin.KeyValueReceived += _Plugin_KeyValueReceived;
        }

        private void _Plugin_KeyValueReceived(object sender, PlugInEventArgs e)
        {
            if (!_Dispatcher.CheckAccess())
            {
                //Switch thread to GUI thread and invoke
                _Dispatcher.Invoke(new Action(() => _Plugin_KeyValueReceived(sender, e)));
            }

            _Template.Debug(DebugLevel.High, DebugCategory.Information, "Got plugin event for plugin" + e.Name + " key: " + e.Key + " value: " + e.Value);

            try
            {
                bool value = Convert.ToBoolean(e.Value);
                switch (e.Key)
                {
                    case "switch1":
                        if (value)
                        {
                            Switch1.Fill = Brushes.LightGreen;
                        }
                        else
                        {
                            Switch1.Fill = Brushes.ForestGreen;
                        }
                        break;
                    case "switch2":
                        if (value)
                        {
                            Switch2.Fill = Brushes.LightGreen;
                        }
                        else
                        {
                            Switch2.Fill = Brushes.ForestGreen;
                        }
                        break;
                    default:
                        break;
                }
                _Template.SetState("Update", API.State.OK, "");
            }
            catch (Exception ex)
            {
                _Template.SetState("Update", API.State.Error, "Failed to update with data from plugin. " + ex.Message);
            }
        }

        public ITemplate TemplateInstance
        {
            get { return _Template; }
        }
    }
}
