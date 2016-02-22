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

namespace INSM.Example.Template.Green
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class GreenTemplate : UserControl, API.ITemplateControl
    {
        private INSM.Template.Framework.v1.Template m_Template;

        public GreenTemplate()
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
