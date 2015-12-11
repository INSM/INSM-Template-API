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
 * Example of the playback related template API
 * 
 * The platform may emit prepare and release events in some circumstances but it should not be expected.
 * 
 */

namespace INSM.Example.Template.Media
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MediaTemplate : UserControl, API.ITemplateControl
    {
        //Template instance
        private INSM.Template.Framework.v1.Template m_Template;

        //Template dataset parameter keys
        private const string MediaFileKey = "MyFile";

        //Template file to play from the platform (identified by m_Filename)
        private API.ITemplateFile m_TemplateFile = null;
        private int m_PlaybackTicket = 0;

        //Dispatcher used to ensure operations in GUI thread
        private Dispatcher m_Dispatcher = null;

        //Variables to handle internal state
        private string m_PlayingFilename = null;
        private bool m_PlayRequested = false;

        public MediaTemplate()
        {
            InitializeComponent();

            //Remember the current dispatcher thread as the constructor is always called by the GUI thread
            m_Dispatcher = this.Dispatcher;

            //Define the default template parameters and their default vaules
            API.TemplateDataSet defaultTemplateDataSet = new API.TemplateDataSet("Default template dataset", "",
                    new API.TemplateDataSetItem(API.TemplateDataSetItemType.MediaFile, MediaFileKey, "Media file", ""));

            //Define the template capabilities
            API.TemplateCapabilities capabilities = new API.TemplateCapabilities() 
            {
                TransitionMode = API.TransitionMode.CrossTransition,
                SupportedTranstions = new string[] { "Fade" }
            };

            //Create template with the template dataset parameters that this template uses and its default values.
            m_Template = new INSM.Template.Framework.v1.Template(defaultTemplateDataSet, capabilities);
            //Register events

            //Loaded is emitted once when control is created and API is available.
            m_Template.TemplateLoaded += new API.LoadedEventHandler(Template_Loaded);

            //Prepare is optionally emitted at any time to let template prepare its resources. 
            m_Template.Prepare += new API.PrepareEventHandler(Template_Prepare);

            //Playing is emitted when control is visible and template should show content.
            m_Template.TemplatePlaying += new API.PlayingEventHandler(Template_Playing);

            //Paused is emitted when playback should stop. Control might still be visible.
            m_Template.TemplatePaused += new API.PausedEventHandler(Template_Paused);

            //Stopping is emitted when control will be hidden and template is expecting transition to stop showing content.
            m_Template.TemplateStopping += new API.StoppingEventHandler(Template_Stopping);

            //Stopped is emitted when control is hidden and template is expecting to stop showing content.
            m_Template.TemplateStopped += new API.StoppedEventHandler(Template_Stopped);

            //Release is optionally emitted to request the template to release additional resources.
            m_Template.Release += new API.ReleaseEventHandler(Template_Release);

            //PreviewRequested is emitted when preview content should be rendered in the control for a snapshot to be generated. Control will not be visible and playing.
            //m_Template.TemplatePreviewRequested += new API.PreviewRequestEventHandler(Template_PreviewRequested);

            //TemplateDataSetChanged is emitted at any time when one or more parameters has changed value. 
            //Handle parameter changes gracefully. Keep used parameters in explicit private variables and compare with new values to avoid unnecessary restart of content that is not affected.
            m_Template.TemplateDataSet.TemplateDataSetChanged += new API.TemplateDataSetEventHandler(TemplateDataSet_TemplateDataSetChanged);

            //Unload is emitted once when control will be disposed.
            m_Template.TemplateUnload += new API.UnloadEventHandler(Template_Unload);

            //Do not do more initialization here. Do that in Initialize() instead when the context API is available.
        }

        public API.ITemplate TemplateInstance
        {
            get { return m_Template; }
        }

        private void Template_Loaded(object sender, API.LoadedEventArgs e)
        {
            try
            {
                m_Template.SetState(API.State.Initializing, "Initializing MediaTemplate");

                Initialize();
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to initialize" + Environment.NewLine + ex.ToString());
                m_Template.MediaEnded();
            }
        }

        private void Template_Prepare(object sender, API.PrepareEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => PrepareResources());
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to prepare resources" + Environment.NewLine + ex.ToString());
                m_Template.MediaEnded();
            }
        }

        private void Template_Playing(object sender, API.PlayingEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => StartPlay(e.Transition, e.Duration, e.Tuning));
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to play" + Environment.NewLine + ex.ToString());
                m_Template.MediaEnded();
            }
        }

        private void Template_Stopping(object sender, API.StoppingEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => StopPlay(e.Transition, e.Duration, e.Tuning));
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to stop" + Environment.NewLine + ex.ToString());
                m_Template.MediaEnded();
            }
        }

        private void Template_Stopped(object sender, API.StoppedEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => StopPlay());
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to stop" + Environment.NewLine + ex.ToString());
                m_Template.MediaEnded();
            }
        }

        private void Template_Paused(object sender, API.PausedEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => PausePlay());
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to pause" + Environment.NewLine + ex.ToString());
                m_Template.MediaEnded();
            }
        }

        private void Template_Release(object sender, API.ReleaseEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => ReleaseResources());
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to release resources" + Environment.NewLine + ex.ToString());
                m_Template.MediaEnded();
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
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to unload" + Environment.NewLine + ex.ToString());
                m_Template.MediaEnded();
            }
        }

        private void TemplateDataSet_TemplateDataSetChanged(API.TemplateDataSetEventArgs e)
        {
            try
            {
                CheckExecuteOnGUIThread(() => CheckDataSetChange(e.TemplateDataSet));
            }
            catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to change dataset" + Environment.NewLine + ex.ToString());
                m_Template.MediaEnded();
            }
        }

        private void Initialize()
        {
            //TODO do onetime initialization
        }

        private void Deinitialize()
        {
            StopPlay(null, 0, 0);

            //Unregister events
            m_Template.TemplateLoaded -= new API.LoadedEventHandler(Template_Loaded);
            m_Template.Prepare -= new API.PrepareEventHandler(Template_Prepare);
            m_Template.TemplatePlaying -= new API.PlayingEventHandler(Template_Playing);
            m_Template.TemplatePaused -= new API.PausedEventHandler(Template_Paused);
            m_Template.TemplateStopping -= new API.StoppingEventHandler(Template_Stopping);
            m_Template.TemplateStopped -= new API.StoppedEventHandler(Template_Stopped);
            m_Template.Release -= new API.ReleaseEventHandler(Template_Release);
            //m_Template.TemplatePreviewRequested -= new API.PreviewRequestEventHandler(Template_PreviewRequested);
            m_Template.TemplateDataSet.TemplateDataSetChanged -= new API.TemplateDataSetEventHandler(TemplateDataSet_TemplateDataSetChanged);
            m_Template.TemplateUnload -= new API.UnloadEventHandler(Template_Unload);
        }

        private void PrepareResources()
        {
            // Get the media filename
            m_TemplateFile = m_Template.TemplateDataSet.GetTemplateDataSetItemAsFile(MediaFileKey);
            if (m_TemplateFile != null)
            {
                if (m_TemplateFile.FileName != m_PlayingFilename)
                {
                    //We don't have enought data in the datset yet. 
                    //It will soon be updated in TemplateDataSet_TemplateDataSetChanged event
                    m_TemplateFile.TemplateFileDeleted += m_TemplateFile_TemplateFileDeleted;

                    if (m_TemplateFile.IsAvailable)
                    {
                        SetMedia();
                    }
                    else
                    {
                        m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "File " + m_TemplateFile.Name + " not present, downloading on demand as file " + m_TemplateFile.FileName);

                        m_Template.SetState("Prepare", API.State.Initializing, "Downloading file");

                        m_TemplateFile.DownloadFileFinished += m_TemplateFile_DownloadFileFinished;
                        m_TemplateFile.DownloadFileProgress += m_TemplateFile_DownloadFileProgress;
                        m_TemplateFile.DownloadAsync();
                    }
                }
            }
        }

        private void m_TemplateFile_TemplateFileDeleted(object sender, API.TemplateFileEventArgs e)
        {
            if (e.TemplateFile.FileName == m_PlayingFilename)
            {
                StopPlay();
                ReleaseResources();
            }
        }

        private void m_TemplateFile_DownloadFileProgress(object sender, API.FileProgressEventArgs e)
        {
            if (e.State == API.TransferStatus.Aborted ||
                e.State == API.TransferStatus.Failed)
            {
                m_Template.SetState("Prepare", API.State.Error, "Download failed");
            }
            else if (e.State == API.TransferStatus.InProgress ||
                e.State == API.TransferStatus.Verifying)
            {
                m_Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "Downloading file " + e.TemplateFile.Name + " " + e.State.ToString() + " " + e.Progress * 100 + " %");
            }
        }

        private void m_TemplateFile_DownloadFileFinished(object sender, API.TemplateFileEventArgs e)
        {
            m_Template.SetState("Prepare", API.State.OK, "Download done");
        }

        private void SetMedia()
        {
            if (!string.IsNullOrEmpty(m_TemplateFile.FileName))
            {
                mediaElement1.LoadedBehavior = MediaState.Manual;
                mediaElement1.UnloadedBehavior = MediaState.Manual;
                mediaElement1.Source = null;
                mediaElement1.Source = new Uri(m_TemplateFile.FileName, UriKind.RelativeOrAbsolute);
                mediaElement1.ScrubbingEnabled = true;

                m_PlayingFilename = m_TemplateFile.FileName;

                m_Template.SetState("Prepare", API.State.Initializing, "Prepared");

                m_Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "Prepared file " + m_TemplateFile.Name);
            }
            else
            {
                m_Template.SetState("Prepare", API.State.Initializing, "Not yet ready to prepared");

                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "File " + m_TemplateFile.Name + " was not found at " + m_TemplateFile.FileName + " on prepare");
            }
        }

        private void ReleaseResources()
        {
            //this.Content = null;
            m_PlayingFilename = null;
        }

        private void CheckDataSetChange(API.ITemplateDataSet templateDataSet)
        {
            API.ITemplateFile templateFile = templateDataSet.GetTemplateDataSetItemAsFile(MediaFileKey);
            if(templateFile != null)
            {
                if (m_TemplateFile == null || !m_TemplateFile.FileName.Equals(templateFile.FileName, StringComparison.OrdinalIgnoreCase))
                {
                    //Media filename has changed
                    m_Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "Media file changed to " + templateFile.FileName);

                    if (m_PlayRequested)
                    {
                        m_Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "Restarting media");

                        //Restart play of new file
                        StopPlay();
                        ReleaseResources();
                        PrepareResources();
                        StartPlay(null, 0, 0);
                    }
                }
            }
        }

        private void StartPlay(string transition, double duration, double tuning)
        {
            m_PlayRequested = true;

            PrepareResources();

            if (!string.IsNullOrEmpty(m_PlayingFilename))
            {
                if (!string.IsNullOrEmpty(transition))
                {
                    if (transition.Equals("Fade", StringComparison.OrdinalIgnoreCase))
                    {
                        //TODO fade in for <duration> seconds
                    }
                }

                mediaElement1.Play();

                //Explicitly report to playlog with reason
                m_PlaybackTicket = m_Template.BeginFilePlayback(m_TemplateFile, "Play");

                m_Template.SetState("Play", API.State.OK, "Playing");

                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Information, "Playing file " + m_TemplateFile.Name);
            }
            else
            {
                m_Template.SetState("Play", API.State.Initializing, "Not ready to play");
            }
        }

        private void StopPlay(string transition, double duration, double tuning)
        {
            if (!string.IsNullOrEmpty(transition))
            {
                if (transition.Equals("Fade", StringComparison.OrdinalIgnoreCase))
                {
                    //TODO fade out for <duration> seconds

                    return;
                }
            }

            //Default do nothing if transition is not recognized
            StopPlay();
        }

        private void StopPlay()
        {
            m_PlayRequested = false;

            if (m_TemplateFile != null)
            {
                mediaElement1.Stop();

                //Explicitly report to playlog
                m_Template.EndFilePlayback(m_PlaybackTicket);

                m_Template.SetState("Play", API.State.Warning, "Stopped");

                m_Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "Stop file " + m_TemplateFile.Name);
            }
        }

        private void PausePlay()
        {
            m_PlayRequested = false;

            mediaElement1.Pause();

            //Explicitly report to playlog
            m_Template.EndFilePlayback(m_PlaybackTicket);

            m_Template.SetState("Play", API.State.Warning, "Paused");
        }

        private void GeneratePreview()
        {
            //TODO generate preview in control
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

