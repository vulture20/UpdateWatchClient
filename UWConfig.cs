using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Serializable]
public class UWConfig
{
    public string serverIP { get; set; }
    public Int16 serverPort { get; set; }
    public Double timerInterval { get; set; }
    public Int32 timerRandom { get; set; }

    public UWConfig()
    {
        this.serverIP = "127.0.0.1";
        this.serverPort = 4584;
//                         Std   Min  Sek  ms
        this.timerInterval = 2 * 60 * 60 * 1000;
        this.timerRandom =            15 * 1000;
    }
}
