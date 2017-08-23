using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Events.Bus.Exceptions;
using Abp.Events.Bus.Handlers;
using Castle.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperRocket.Console.EventHandler
{
    /// <summary>
    /// If any job exception throwed, exit the application with exit code = 1
    /// </summary>
    public class BackgroundJobExceptionEventHandler : IEventHandler<AbpHandledExceptionData>, ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager;


        public ILogger Logger { get; set; }
        public BackgroundJobExceptionEventHandler(IBackgroundJobManager backgroundJobManager)
        {
            _backgroundJobManager = backgroundJobManager;

            Logger = NullLogger.Instance;
        }

        public void HandleEvent(AbpHandledExceptionData eventData)
        {
            Logger.Error(string.Format("Job error occured : {0}", eventData.Exception.InnerException.StackTrace));
            //Exit with error = 1
            Environment.Exit(1);
        }
    }

}
