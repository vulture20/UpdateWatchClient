﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace UpdateWatch_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            System.ServiceProcess.ServiceBase.Run(new UWClientService());
        }
    }
}
