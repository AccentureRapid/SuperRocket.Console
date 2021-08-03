using System;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using Abp.Events.Bus;
using Abp.BackgroundJobs;
using SuperRocket.Orchard.Job;

namespace AbpEfConsoleApp
{
    //Entry class of the test. It uses constructor-injection to get a repository and property-injection to get a Logger.
    public class Tester : ITransientDependency
    {
        public ILogger Logger { get; set; }

        private readonly IEventBus _eventBus;
        private readonly IBackgroundJobManager _backgroundJobManager;
        public Tester(
            IRepository<User, Guid> userRepository,
            IEventBus eventBus,
            IBackgroundJobManager backgroundJobManager
            )
        {
            _eventBus = eventBus;
            _backgroundJobManager = backgroundJobManager;

            Logger = NullLogger.Instance;
        }

        public void Run()
        {
            Logger.Debug("Started Tester.Run()");
            //_eventBus.Trigger
            _backgroundJobManager.Enqueue<TestJob, int>(1);

            _backgroundJobManager.Enqueue<DataMerger, int>(1);
            Logger.Debug("Finished Tester.Run()");
        }
    }
}