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

namespace INSM.Example.Template.Command
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CommandTemplate : UserControl, API.ITemplateControl
    {
        private API.ITemplate _Template;

        public CommandTemplate()
        {
            InitializeComponent();

            _Template = new API.Template(null);
            _Template.TemplateCommand += _Template_TemplateCommand;
        }

        private void _Template_TemplateCommand(object sender, API.TemplateCommandEventArgs e)
        {
            if (e.TemplateCommand.Key == "CommandForMe")
            {
                string value = e.TemplateCommand.Value;

                //Do something

                e.TemplateCommand.ReturnResult("Some result to caller");
            }
        }

        public API.ITemplate TemplateInstance
        {
            get { return _Template; }
        }
    }
}
