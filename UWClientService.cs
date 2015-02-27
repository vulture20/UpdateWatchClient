using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.Net.Sockets;
using System.Timers;

namespace UpdateWatch_Client
{
    class UWClientService : ServiceBase
    {
        //                         Std   Min  Sek  ms
        const double timerInterval = 2 * 60 * 60 * 1000;
        const Int32 timerRandom =         5 * 60 * 1000;

        private static System.Timers.Timer timer1;
        Random random = new Random();

        public UWClientService()
        {
            this.ServiceName = "UpdateWatch-Client";
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            timer1 = new System.Timers.Timer(timerInterval + random.Next(timerRandom));
            timer1.Elapsed += new ElapsedEventHandler(OnTimer1);
            timer1.AutoReset = true;
            timer1.Enabled = true;
//            timer1.Start();
            
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }

        private static void OnTimer1(object source, ElapsedEventArgs e)
        {

        }
    }
}
