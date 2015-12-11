using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using API = INSM.Template.Framework.v1;

namespace INSM.Example.Template.Mono
{
    /// <summary>
    /// Example of a template independent of rendering framework (such as WPF).
    /// This type of template must take case of all rendering itself.
    /// On windows an Attribute will be available for the parent window handler.
    /// </summary>
    public class NoGUITemplate : API.ITemplateControl
    {
        //Template instance
        private INSM.Template.Framework.v1.Template m_Template;

        //Template dataset parameter keys
        private const string MediaFileKey = "MediaFile";

        //Template file to play from the platform (identified by m_Filename)
        private API.ITemplateFile m_TemplateFile = null;
        private int m_PlaybackTicket = 0;

        //Variables to handle internal state
        private bool m_Prepared = false;
        private bool m_PlayRequested = false;

        public NoGUITemplate()
        {
            //Define the default template parameters and their default vaules
            API.TemplateDataSet defaultTemplateDataSet = new API.TemplateDataSet("Default template dataset", "",
                    new API.TemplateDataSetItem(API.TemplateDataSetItemType.MediaFile, MediaFileKey, "Media file", ""));

            //Define the template capabilities
            API.TemplateCapabilities capabilities = new API.TemplateCapabilities()
            {
                TransitionMode = API.TransitionMode.NoTransition
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
            m_Template.TemplateDataSet.TemplateDataSetChanged2 += new API.TemplateDataSetEventHandler2(TemplateDataSet_TemplateDataSetChanged);

            //Unload is emitted once when control will be disposed.
            m_Template.TemplateUnload += new API.UnloadEventHandler(Template_Unload);

            //New file is available
            m_Template.TemplateFileAdded += new API.TemplateFileEventHandler(Template_TemplateFileAdded);

            //File is no longer available
            m_Template.TemplateFileDeleted += new API.TemplateFileEventHandler(Template_TemplateFileDeleted);

            //Do not do more initialization here. Do that in Initialize() instead when the context API is available.
        }

        private void Template_TemplateFileDeleted(object sender, API.TemplateFileEventArgs e)
        {
            m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Information, "File deleted " + e.TemplateFile.Name);
            Console.WriteLine("File deleted " + e.TemplateFile.Name);
        }

        private void Template_TemplateFileAdded(object sender, API.TemplateFileEventArgs e)
        {
            m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Information, "File added " + e.TemplateFile.Name);
            Console.WriteLine("File added " + e.TemplateFile.Name);
        }

        public API.ITemplate TemplateInstance
        {
            get { return m_Template; }
        }

        private void Template_Loaded(object sender, API.LoadedEventArgs e)
        {
            try
            {
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Information, "Mono template loaded");

                m_Template.SetState(API.State.Initializing, "Initializing MonoTemplate");

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
                PrepareResources();
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
                StartPlay(e.Transition, e.Duration, e.Tuning);
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
                StopPlay(e.Transition, e.Duration, e.Tuning);
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
                StopPlay();
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
                PausePlay();
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
                ReleaseResources();
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

        //private void Template_PreviewRequested(object sender, API.PreviewRequestEventArgs e)
        //{
        //    try
        //    {
        //        GeneratePreview();
        //        e.PreviewRequestFinished = true;
        //    }
        //    catch (Exception ex) //Always catch exceptions in template calls. If not the template might restart in a new process.
        //    {
        //        m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "Failed to generate preview" + Environment.NewLine + ex.ToString());
        //        //No need to exit
        //    }
        //}

        private void TemplateDataSet_TemplateDataSetChanged(object sender, API.TemplateDataSetEventArgs e)
        {
            try
            {
                CheckDataSetChange(e.TemplateDataSet);

                //Make a call
                int files = m_Template.GetFiles().Count();
                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Information, "Player has " + files + " files available");
                Console.WriteLine("Player has " + files + " files available");
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

            Console.WriteLine("Initialize");
        }

        private void Deinitialize()
        {
            Console.WriteLine("Deinitialize");

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
            m_Template.TemplateDataSet.TemplateDataSetChanged2 -= new API.TemplateDataSetEventHandler2(TemplateDataSet_TemplateDataSetChanged);
            m_Template.TemplateUnload -= new API.UnloadEventHandler(Template_Unload);
        }

        private void PrepareResources()
        {
            if (!m_Prepared)
            {
                // Get the media filename
                m_TemplateFile = m_Template.TemplateDataSet.GetTemplateDataSetItemAsFile(MediaFileKey);
                if (m_TemplateFile != null)
                {
                    //We don't have enought data in the datset yet. 
                    //It will soon be updated in TemplateDataSet_TemplateDataSetChanged event

                    if (!m_TemplateFile.IsAvailable)
                    {
                        m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "File " + m_TemplateFile.Name + " not present, downloading on demand as file " + m_TemplateFile.FileName);
                        m_TemplateFile.Download();
                    }

                    if (!string.IsNullOrEmpty(m_TemplateFile.FileName))
                    {
                        Console.WriteLine("Prepare");

                        m_Prepared = true;

                        m_Template.SetState(API.State.Initializing, "Prepared");

                        m_Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "Prepared file " + m_TemplateFile.Name);
                    }
                    else
                    {
                        m_Prepared = false;

                        m_Template.SetState(API.State.Initializing, "Not yet ready to prepared");

                        m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Error, "File " + m_TemplateFile.Name + " was not found at " + m_TemplateFile.FileName + " on prepare");
                    }
                }
                else
                {
                    m_Prepared = false;
                }
            }
        }

        private void ReleaseResources()
        {
            m_Prepared = false;
        }

        private void CheckDataSetChange(API.ITemplateDataSet templateDataSet)
        {
            API.ITemplateFile templateFile = templateDataSet.GetTemplateDataSetItemAsFile(MediaFileKey);
            if (templateFile != null)
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

            if (m_Prepared)
            {
                if (!string.IsNullOrEmpty(transition))
                {
                    if (transition.Equals("Fade", StringComparison.OrdinalIgnoreCase))
                    {
                        //TODO fade in for <duration> seconds
                    }
                }

                Console.WriteLine("Play");

                //Explicitly report to playlog with reason
                m_PlaybackTicket = m_Template.BeginFilePlayback(m_TemplateFile, "Play");

                m_Template.SetState(API.State.OK, "Playing");

                m_Template.Debug(API.DebugLevel.High, API.DebugCategory.Information, "Playing file " + m_TemplateFile.Name);
            }
            else
            {
                m_Template.SetState(API.State.Initializing, "Not ready to play");
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
                Console.WriteLine("Stop");

                //Explicitly report to playlog
                m_Template.EndFilePlayback(m_PlaybackTicket);

                m_Template.SetState(API.State.Warning, "Stopped");

                m_Template.Debug(API.DebugLevel.Low, API.DebugCategory.Information, "Stop file " + m_TemplateFile.Name);
            }
        }

        private void PausePlay()
        {
            m_PlayRequested = false;

            Console.WriteLine("Pause");

            //Explicitly report to playlog
            m_Template.EndFilePlayback(m_PlaybackTicket);

            m_Template.SetState(API.State.Warning, "Paused");
        }

        private void GeneratePreview()
        {
            Console.WriteLine("Generate preview");

            //TODO generate preview
        }
    }
}
