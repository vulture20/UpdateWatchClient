using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

public class UWConfig : ISerializable
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
    protected UWConfig(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new System.ArgumentNullException("info");
        serverIP = (string)info.GetValue("serverIP ", typeof(string));
        serverPort = (Int16)info.GetValue("serverPort ", typeof(Int16));
        timerInterval = (Double)info.GetValue("timerInterval ", typeof(Double));
        timerRandom = (Int32)info.GetValue("timerRandom ", typeof(Int32));
    }

    [SecurityPermissionAttribute(SecurityAction.LinkDemand,
        Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new System.ArgumentNullException("info");
        info.AddValue("serverIP ", serverIP);
        info.AddValue("serverPort ", serverPort);
        info.AddValue("timerInterval ", timerInterval);
        info.AddValue("timerRandom ", timerRandom);
    }
}
