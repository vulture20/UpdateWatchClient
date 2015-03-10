using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;
using WUApiLib;

namespace UpdateWatch_Client
{
    public partial class UWClientService : ServiceBase
    {
        //                         Std   Min  Sek  ms
//        const double timerInterval = 2 * 60 * 60 * 1000;
//        const double timerInterval =      1 * 60 * 1000;
//        const Int32 timerRandom =             15 * 1000;
//        const string serverIP = "192.168.116.200";
//        const Int16 serverPort = 4584;

        private static System.Timers.Timer timer1 = new System.Timers.Timer();
        private static Random random = new Random();
        private static Thread th = new Thread(new ThreadStart(handleUpdates));
        private static UWConfig config = new UWConfig();
        private static EventLog eventLog = new EventLog("Application");

        public UWClientService()
        {
            this.ServiceName = "UpdateWatch-Client";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        private static void getConfig() {
            if (!File.Exists(@"UWConfig.xml"))
            {
                try
                {
                    eventLog.WriteEntry("No config file found. Creating new one...", EventLogEntryType.Warning);

                    FileStream fileStream = new FileStream(@"UWConfig.xml", FileMode.CreateNew);
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(UWConfig));

                    xmlSerializer.Serialize(fileStream, config);

                    fileStream.Close();
                }
                catch (Exception ex)
                {
                    eventLog.WriteEntry("Errot at creating the config file: " + ex.InnerException.Message, EventLogEntryType.Error);
                    if (Program.console)
                    {
                        Console.WriteLine("Create Config: " + ex.Message);
                    }
                    else
                        throw;
                }
            }
            try
            {
                eventLog.WriteEntry("Reading config file...", EventLogEntryType.Information);

                StreamReader sr = new StreamReader(@"UWConfig.xml", true);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UWConfig));

                config = (UWConfig)xmlSerializer.Deserialize(sr);

                sr.Close();
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry("Error at reading the config file: " + ex.InnerException.Message, EventLogEntryType.Error);
                if (Program.console)
                {
                    Console.WriteLine("Read config: " + ex.Message);
                }
                else
                    throw;
            }
        }

        public static void initializeService()
        {
            timer1.Interval = config.timerInterval + random.Next(config.timerRandom);
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer1);
            timer1.AutoReset = true;
            timer1.Enabled = true;
            timer1.Start();

            eventLog.Source = "UpdateWatch-Client";

            getConfig();

            th.Start();
        }

        protected override void OnStart(string[] args)
        {
            eventLog.WriteEntry("Starting service...", EventLogEntryType.Information);
            initializeService();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("Stopping service...", EventLogEntryType.Information);
            timer1.Enabled = false;
            timer1.Stop();
            th.Abort();

            base.OnStop();
        }

        protected override void OnPause()
        {
            eventLog.WriteEntry("Pausing service...", EventLogEntryType.Information);
            timer1.Enabled = false;

            base.OnPause();
        }

        protected override void OnContinue()
        {
            eventLog.WriteEntry("Continuing service...", EventLogEntryType.Information);
            timer1.Enabled = true;

            base.OnContinue();
        }

        private static void handleUpdates()
        {
            List<UWUpdate.WUpdate> updateList = new List<UWUpdate.WUpdate>();
            UpdateSession uSession = new UpdateSession();
            IUpdateSearcher uSearcher = uSession.CreateUpdateSearcher();

            uSearcher.Online = false;
            try
            {
                ISearchResult sResult = uSearcher.Search("IsInstalled=0 And IsHidden=0 And Type='Software'");
//                ISearchResult sResult = uSearcher.Search("IsInstalled=1 And IsHidden=0");

                eventLog.WriteEntry("Searched for Updates: " + sResult.Updates.Count + " found", EventLogEntryType.Information);
                if (Program.console)
                    Console.WriteLine("Found " + sResult.Updates.Count + " Updates:");

                foreach (IUpdate update in sResult.Updates)
                {
                    string KBArticles = "";
                    UWUpdate.WUpdate wUpdate = new UWUpdate.WUpdate();
                    wUpdate.Description = update.Description;
                    wUpdate.ReleaseNotes = update.ReleaseNotes;
                    wUpdate.SupportUrl = update.SupportUrl;
                    wUpdate.Title = update.Title;
                    wUpdate.UpdateID = update.Identity.UpdateID;
                    wUpdate.RevisionNumber = update.Identity.RevisionNumber;
                    wUpdate.isMandatory = update.IsMandatory;
                    wUpdate.isUninstallable = update.IsUninstallable;
                    foreach (string KBArticle in update.KBArticleIDs)
                    {
                        KBArticles += KBArticle + ", ";
                    }
                    wUpdate.KBArticleIDs = KBArticles.TrimEnd((','), (' '));
                    wUpdate.MsrcSeverity = update.MsrcSeverity;
                    wUpdate.Type = (Int32)update.Type;
                    updateList.Add(wUpdate);

                    if (Program.console)
                        Console.WriteLine(update.Title);
                }
                try
                {
                    TcpClient c = new TcpClient(config.serverIP, config.serverPort);
                    Stream networkStream = c.GetStream();

                    UWUpdate.clSendData sendData = new UWUpdate.clSendData()
                    {
                        dnsName = System.Net.Dns.GetHostName(),
                        machineName = System.Environment.MachineName,
                        osVersion = System.Environment.OSVersion.ToString(),
                        tickCount = System.Environment.TickCount,
                        updateCount = sResult.Updates.Count,
                        wUpdate = updateList
                    };
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(UWUpdate.clSendData));

                    StringBuilder sb = new StringBuilder();
                    StringWriter sw = new StringWriter(sb);
                    xmlSerializer.Serialize(sw, sendData);
                    Console.WriteLine(sw.GetStringBuilder().ToString());

                    xmlSerializer.Serialize(networkStream, sendData);
                    networkStream.Flush();

                    c.Close();
                }
                catch (Exception ex)
                {
                    eventLog.WriteEntry("Couldn't send the data: " + ex.InnerException.Message, EventLogEntryType.Error);
                    if (Program.console)
                        Console.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry("Couldn't search for updates: " + ex.InnerException.Message, EventLogEntryType.Error);
                if (Program.console)
                {
                    Console.WriteLine(ex.ToString());
                }
                else
                {
                    throw;
                }
            }
        }

        private static void OnTimer1(object source, ElapsedEventArgs e)
        {
            th.Abort();
            th = new Thread(new ThreadStart(handleUpdates));
            th.Start();
        }
    }
}
