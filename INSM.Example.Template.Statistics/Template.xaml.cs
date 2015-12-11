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
 * Example of the Statitics API
 * 
 * Statistics are (non shared) numerical properties.
 * 
 * Statistics is a utility class to conveniently record statistics on actions.
 * The utility will record number of actions and their durations.
 * 
 * The properties are stored in a format suitable for statistics aggregation on server
 * and display using statistics webmodules.
 * 
 * The data is time and region segmented and available from the player and server REST APIs.
 * 
 * As for all properties, choose the keys wisely as they are all kept in memory. A key name should 
 * represent an distinct action that is expected to be used over a long period of time.
 * 
 * Specifically the number of key names should not grow to many. If you have specific needs for your 
 * statistics consider condensing them in a XML/JSON structure and save as one normal property or use 
 * an external system for statistics storage.
 * 
 */

namespace INSM.Example.Template.Statistics
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Template : UserControl, API.ITemplateControl
    {
        //Template instance
        private INSM.Template.Framework.v1.Template m_Template;

        //Template dataset parameter key for defining name of statistics key. I.e. Product name
        private const string StatsKeyNameKey1 = "Product 1 name";

        //Dispatcher used to ensure operations in GUI thread
        private Dispatcher m_Dispatcher = null;

        private API.Statistics m_Statistics1;
        private string m_StatisticsKey1;

        public Template()
        {
            InitializeComponent();

            //Remember the current dispatcher thread as the constructor is always called by the GUI thread
            m_Dispatcher = this.Dispatcher;

            //Create template with the template dataset parameters that this template uses and its default values.
            m_Template = new API.Template(new API.TemplateDataSet("Default template dataset", "",
                    new API.TemplateDataSetItem(API.TemplateDataSetItemType.Text, StatsKeyNameKey1, "Key name of statistics property", "StatTest1")));

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

            buttonBegin.Click += new RoutedEventHandler(buttonBegin_Click);
            buttonEnd.Click += new RoutedEventHandler(buttonEnd_Click);

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
            m_Statistics1 = new INSM.Template.Framework.v1.Statistics(m_Template);
        }

        private void Deinitialize()
        {
            //Unregister events
            m_Template.TemplateLoaded -= new API.LoadedEventHandler(Template_Loaded);
            m_Template.TemplatePlaying -= new API.PlayingEventHandler(Template_Playing);
            m_Template.TemplateDataSet.TemplateDataSetChanged -= new API.TemplateDataSetEventHandler(TemplateDataSet_TemplateDataSetChanged);
            m_Template.TemplateUnload -= new API.UnloadEventHandler(Template_Unload);
        }

        private void CheckParameters()
        {
            string newStatisticsKey = m_Template.TemplateDataSet.GetTemplateDataSetItemAsText(StatsKeyNameKey1);
            if (!string.IsNullOrEmpty(newStatisticsKey))
            {
                m_StatisticsKey1 = newStatisticsKey;
                CheckExecuteOnGUIThread(() => UpdateGUI());
            }
            else
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Statistics key was not set");
            }
        }

        private void buttonBegin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                m_Statistics1.BeginAction(m_StatisticsKey1);
                buttonBegin.IsEnabled = false;
                buttonEnd.IsEnabled = true;
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed on increment" + ex.Message);
            }
        }

        private void buttonEnd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                m_Statistics1.EndAction(m_StatisticsKey1);
                buttonBegin.IsEnabled = true;
                buttonEnd.IsEnabled = false;
                UpdateGUI();
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed on increment" + ex.Message);
            }
        }

        private void UpdateGUI()
        {
            //Update the GUI

            //NOTE, in a real template a proper pattern should be used such as MVVM
            if (!string.IsNullOrEmpty(m_StatisticsKey1))
            {
                textBoxKey.Text = m_StatisticsKey1;
                textBoxValue.Text = m_Template.GetTemplateProperty(m_StatisticsKey1).Value;
            }
            else
            {
                textBoxKey.Text = "<key not set>";
                textBoxValue.Text = "<value not set>";
            }

            //Update
            m_Dispatcher.Invoke(DispatcherPriority.Render, (Action)delegate() { });
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
