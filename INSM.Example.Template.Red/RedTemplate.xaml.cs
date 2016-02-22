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

namespace INSM.Example.Template.Red
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class RedTemplate : UserControl, API.ITemplateControl
    {
        //Template instance
        private INSM.Template.Framework.v1.Template m_Template;

        public RedTemplate()
        {
            InitializeComponent();

            m_Template = new INSM.Template.Framework.v1.Template(null);
        }

        public API.ITemplate TemplateInstance
        {
            get { return m_Template; }
        }
    }
}
