using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Net.Sockets;
using System.Timers;
using WUApiLib;

namespace UpdateWatch_Client
{
    class UWClientService : ServiceBase
    {
        //                         Std   Min  Sek  ms
        const double timerInterval = 2 * 60 * 60 * 1000;
        const Int32 timerRandom =         5 * 60 * 1000;
        const String serverIP = "127.0.0.1";
        const Int16 serverPort = 4584;

        private static System.Timers.Timer timer1;
        private static Random random = new Random();

        public UWClientService()
        {
            this.ServiceName = "UpdateWatch-Client";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        public static void initializeService()
        {
            timer1 = new System.Timers.Timer(timerInterval + random.Next(timerRandom));
            timer1.Elapsed += new ElapsedEventHandler(OnTimer1);
            timer1.AutoReset = true;
            timer1.Enabled = true;
//            timer1.Start();

            handleUpdates();
        }

        protected override void OnStart(string[] args)
        {
            initializeService();

            base.OnStart(args);
        }

        protected override void OnStop()
        {
            timer1.Enabled = false;

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
            UpdateSession uSession = new UpdateSession();
            IUpdateSearcher uSearcher = uSession.CreateUpdateSearcher();
            uSearcher.Online = false;
            try
            {
                ISearchResult sResult = uSearcher.Search("IsInstalled=0 And IsHidden=0 And Type='Software'");
                if (Program.console)
                {
                    Console.WriteLine("Found " + sResult.Updates.Count + " Updates:");
                    foreach (IUpdate update in sResult.Updates)
                    {
                        Console.WriteLine(update.Title);
                    }
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
            handleUpdates();
        }
    }
}
