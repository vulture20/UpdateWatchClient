using System;
using System.Collections.Generic;
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

        public UWClientService()
        {
            this.ServiceName = "UpdateWatch-Client";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        public static void initializeService()
        {
            timer1.Interval = config.timerInterval + random.Next(config.timerRandom);
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer1);
            timer1.AutoReset = true;
            timer1.Enabled = true;
            timer1.Start();

            try
            {
                FileStream fileStream = new FileStream(@"UWConfig.xml", FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(UWConfig));

                config = (UWConfig)xmlSerializer.Deserialize(fileStream);

                fileStream.Close();
            }
            catch { }

            th.Start();
        }

        protected override void OnStart(string[] args)
        {
            initializeService();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            timer1.Enabled = false;
            timer1.Stop();
            th.Abort();

            base.OnStop();
        }

        protected override void OnPause()
        {
            timer1.Enabled = false;

            base.OnPause();
        }

        protected override void OnContinue()
        {
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
                    if (Program.console)
                        Console.WriteLine(ex.ToString());
                }
            }
            catch (Exception ex)
            {
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
