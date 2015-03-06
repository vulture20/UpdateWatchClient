using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;

namespace UpdateWatch_Client
{
    [RunInstaller(true)]
    public partial class UWClientInstaller : Installer
    {
        private ServiceProcessInstaller processInstaller;
        private ServiceInstaller serviceInstaller;

        public UWClientInstaller()
        {
            EventLogInstaller installer = FindInstaller(this.Installers);
            if (installer != null)
                installer.Log = "UpdateWatch-Client";

            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = "UpdateWatch-Client"; // must match UWClientService.ServiceName
            serviceInstaller.DisplayName = "UpdateWatch-Client";
            serviceInstaller.Description = "Client-Komponente des Update-Managementtools UpdateWatch";
            processInstaller.Account = ServiceAccount.LocalSystem;

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }

        private EventLogInstaller FindInstaller(InstallerCollection installers)
        {
            foreach (Installer installer in installers)
            {
                if (installer is EventLogInstaller)
                    return (EventLogInstaller)installer;

                EventLogInstaller eventLogInstaller = FindInstaller(installer.Installers);
                if (eventLogInstaller != null)
                    return eventLogInstaller;
            }
            return null;
        }
    }
}