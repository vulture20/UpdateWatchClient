using System.ComponentModel;
using System.ServiceProcess;

[RunInstaller(true)]
public class UWClientInstaller : ServiceInstaller
{
    private ServiceProcessInstaller processInstaller;
    private ServiceInstaller serviceInstaller;

    public UWClientInstaller()
    {
        processInstaller = new ServiceProcessInstaller();
        serviceInstaller = new ServiceInstaller();

        processInstaller.Account = ServiceAccount.LocalSystem;
        serviceInstaller.StartType = ServiceStartMode.Automatic;
        serviceInstaller.ServiceName = "UpdateWatch-Client"; // must match UWClientService.ServiceName

        Installers.Add(processInstaller);
        Installers.Add(serviceInstaller);
    }
}
