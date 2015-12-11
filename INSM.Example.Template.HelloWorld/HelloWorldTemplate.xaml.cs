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
using INSM.Template.Framework.v1;

namespace INSM.Template.Example.HelloWorld
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class HelloWorldTemplate : UserControl, ITemplateControl
    {
        private ITemplate _Template;

        public HelloWorldTemplate()
        {
            InitializeComponent();

            _Template = new INSM.Template.Framework.v1.Template(null);

            _Template.TemplateDataSet.TemplateDataSetChanged2 += TemplateDataSet_TemplateDataSetChanged2;
        }

        private void TemplateDataSet_TemplateDataSetChanged2(object sender, TemplateDataSetEventArgs e)
        {
            this.myLabel.Content = e.TemplateDataSet.GetTemplateDataSetItemAsText("MyText");
        }

        public ITemplate TemplateInstance
        {
            get { return _Template; }
        }
    }
}
