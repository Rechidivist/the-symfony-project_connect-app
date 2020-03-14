using System;
using Topshelf;

namespace SiteConnectorService
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main()
        { 
            var rc = HostFactory.Run(x =>
            {
                x.Service<Service>(s =>
                {
                    s.ConstructUsing(name => new Service());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();

                x.UnhandledExceptionPolicy = Topshelf.Runtime.UnhandledExceptionPolicyCode.LogErrorOnly;

                x.SetDescription("Сервис обновляющий инфу с сайта");
                x.SetDisplayName("The HardWorker SERIAL <-> SITE interface");
                x.SetServiceName("HardWorker");

                x.OnException((ex) =>
                {
                    Console.WriteLine("Exception occured Main : " + ex.ToString());
                    Console.ReadKey();
                });
            });                                                            

            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode()); 
            Environment.ExitCode = exitCode;
        }

    }
}
