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
using API = INSM.Template.Framework.v1;

namespace INSM.Example.Template.SurveyTouch
{
    /// <summary>
    /// Template that collects user feedback and sends it to the server.
    /// The data is numeric so the AMS will aggregate the data on regions in time segments.
    /// </summary>
    public partial class SurveyTemplate : UserControl, ITemplateControl
    {
        private ITemplate _Template;

        private ITemplateProperty _HappyProperty;
        private ITemplateProperty _NeutralProperty;
        private ITemplateProperty _SadProperty;

        public SurveyTemplate()
        {
            InitializeComponent();

            _Template = new API.Template(null);
            _HappyProperty = _Template.GetTemplateProperty("Happy");
            _NeutralProperty = _Template.GetTemplateProperty("Neutral");
            _SadProperty = _Template.GetTemplateProperty("Sad");
        }

        private void HappyButton_Click(object sender, RoutedEventArgs e)
        {
            Increment(_HappyProperty);
        }

        private void NeutralButton_Click(object sender, RoutedEventArgs e)
        {
            Increment(_NeutralProperty);
        }

        private void SadButton_Click(object sender, RoutedEventArgs e)
        {
            Increment(_SadProperty);
        }

        private void Increment(ITemplateProperty property)
        {
            try 
            {
                int value = 0;
                if (!string.IsNullOrEmpty(property.Value))
                {
                    if (!Int32.TryParse(property.Value, out value))
                    {
                        _Template.SetState("Increment", API.State.Error, "Failed to read value " + property.Value);
                    }
                }

                property.Value = Convert.ToString(value + 1);
                property.Save();

                _Template.SetState("Increment", API.State.OK, "");
            }
            catch(Exception ex)
            {
                _Template.SetState("Increment", API.State.Error, "Failed to increment. "  + ex.Message);
            }
        }

        public ITemplate TemplateInstance
        {
            get { return _Template; }
        }
    }
}
