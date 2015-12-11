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

/*
 * Example of the Template Property API
 * 
 * Properties are string based key/value persistent storage available to templates.
 * 
 * All properties are also sent to the server and is available from the server REST API.
 * 
 * If the SharedName is set, the property will be shared among all template on the player.
 * 
 * If it is not set it will be private to the template for each channel and playlist.
 * I e if a new version of the template is scheduled it will operate on a new property.
 * 
 */

namespace INSM.Example.Template.Property
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Template : UserControl, API.ITemplateControl
    {
        //Template instance
        private INSM.Template.Framework.v1.Template m_Template;

        //Template dataset parameter keys
        private const string PropertyKeyNameKey = "Key";
        private const string PropertySharedNameKey = "Shared name";

        //Dispatcher used to ensure operations in GUI thread
        private Dispatcher m_Dispatcher = null;

        private API.ITemplateProperty m_Property1;

        public Template()
        {
            InitializeComponent();

            //Remember the current dispatcher thread as the constructor is always called by the GUI thread
            m_Dispatcher = this.Dispatcher;

            //Create template with the template dataset parameters that this template uses and its default values.
            m_Template = new API.Template(new API.TemplateDataSet("Default template dataset", "",
                    new API.TemplateDataSetItem(API.TemplateDataSetItemType.Text, PropertyKeyNameKey, "Key name of property", "Test1"),
                    new API.TemplateDataSetItem(API.TemplateDataSetItemType.Text, PropertySharedNameKey, "Shared name of property", "")));

            //Register events

            //Loaded is emitted once when control is created and API is available.
            m_Template.TemplateLoaded += new API.LoadedEventHandler(Template_Loaded);

            //Playing is emitted when control is visible and template should show content.
            m_Template.TemplatePlaying += new API.PlayingEventHandler(Template_Playing);

            //TemplateDataSetChanged is emitted at any time when one or more parameters has changed value. 
            //Handle parameter changes gracefully. Keep used parameters in explicit private variables and compare with new values to avoid unnecessary restart of content that is not affected.
            m_Template.TemplateDataSet.TemplateDataSetChanged += new API.TemplateDataSetEventHandler(TemplateDataSet_TemplateDataSetChanged);

            //Unload is emitted once when control will be disposed.
            m_Template.TemplateUnload += new API.UnloadEventHandler(Template_Unload);

            buttonInc.Click += new RoutedEventHandler(buttonInc_Click);

            //Do not do initialization here that is dependent on the context API. Do that when the template is loaded.
        }

        public API.ITemplate TemplateInstance
        {
            get { return m_Template; }
        }

        private void Template_Playing(object sender, API.PlayingEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => CheckParameters());
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to play " + Environment.NewLine + ex.Message);
            }
        }

        private void Template_Loaded(object sender, API.LoadedEventArgs e)
        {
            try
            {
                Initialize();
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to initialize " + ex.Message);
            }
        }

        private void Template_Unload(object sender, API.UnloadEventArgs e)
        {
            try
            {
                Deinitialize();
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to unload " + ex.Message);
            }
        }

        private void TemplateDataSet_TemplateDataSetChanged(API.TemplateDataSetEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => CheckParameters());
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to change dataset " + ex.Message);
            }
        }

        private void Initialize()
        {
        }

        private void Deinitialize()
        {
            //Unregister events
            m_Template.TemplateLoaded -= new API.LoadedEventHandler(Template_Loaded);
            m_Template.TemplatePlaying -= new API.PlayingEventHandler(Template_Playing);
            m_Template.TemplateDataSet.TemplateDataSetChanged -= new API.TemplateDataSetEventHandler(TemplateDataSet_TemplateDataSetChanged);
            m_Template.TemplateUnload -= new API.UnloadEventHandler(Template_Unload);

            if (m_Property1 != null)
            {
                m_Property1.TemplatePropertyChanged -= new API.TemplatePropertyEventHandler(Property1_TemplatePropertyChanged);
            }
        }

        private void CheckParameters()
        {
            string newPropertyName = m_Template.TemplateDataSet.GetTemplateDataSetItemAsText(PropertyKeyNameKey);
            if (!string.IsNullOrEmpty(newPropertyName))
            {
                if (m_Property1 == null || !newPropertyName.Equals(m_Property1.Key, StringComparison.OrdinalIgnoreCase))
                {
                    m_Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "Property key changed to " + newPropertyName);

                    if (m_Property1 != null)
                    {
                        m_Property1.TemplatePropertyChanged -= new API.TemplatePropertyEventHandler(Property1_TemplatePropertyChanged);
                    }

                    m_Property1 = m_Template.GetTemplateProperty(newPropertyName);
                    m_Property1.TemplatePropertyChanged += new API.TemplatePropertyEventHandler(Property1_TemplatePropertyChanged);
                }

                string newPropertySharedName = m_Template.TemplateDataSet.GetTemplateDataSetItemAsText(PropertySharedNameKey);
                m_Property1.SharedName = newPropertySharedName;
            }
            else
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Property key name was not set");
            }
        }

        private void buttonInc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_Property1 != null)
                {
                    int i = 0;
                    if (string.IsNullOrEmpty(m_Property1.Value) || Int32.TryParse(m_Property1.Value, out i))
                    {
                        i++;
                        m_Property1.Value = Convert.ToString(i);
                        m_Property1.Save();

                        UpdateGUI();
                    }
                    else
                    {
                        m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Value was not an integer");
                    }
                }
                else
                {
                    m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Template property was not available");
                }
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed on increment" + ex.Message);
            }
        }

        private void Property1_TemplatePropertyChanged(object sender, API.TemplatePropertyEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => UpdateGUI());
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed on property changed" + ex.Message);
            }
        }

        private void UpdateGUI()
        {
            //Update the GUI

            //NOTE, in a real template a proper pattern should be used such as MVVM

            if (m_Property1 != null)
            {
                textBoxKey.Text = m_Property1.Key;
                if (m_Property1.IsShared)
                {
                    //Shared between all templates on the player
                    textBoxSharedName.Text = m_Property1.SharedName;
                }
                else
                {
                    //Property is private for this template in current channel and playlist
                    textBoxSharedName.Text = "<private>";
                }
                textBoxValue.Text = m_Property1.Value;

                //Update
                m_Dispatcher.Invoke(DispatcherPriority.Render, (Action)delegate() { });
            }
        }

        private void CheckExecuteOnGUIThread(Action action)
        {
            if (m_Dispatcher.CheckAccess())
            {
                //Invoke directly as this is the GUI thread
                action();
            }
            else
            {
                //Switch thread to GUI thread and invoke
                m_Dispatcher.Invoke(action);
            }
        }
    }

}
