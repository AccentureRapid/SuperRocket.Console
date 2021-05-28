using System;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Castle.Core.Logging;
using Abp.Events.Bus;
using Abp.BackgroundJobs;
using SuperRocket.Orchard.Job;
using System.IO;
using SuperRocket.Console;
using System.Threading;
using System.Collections.Generic;

namespace AbpEfConsoleApp
{
    //Entry class of the test. It uses constructor-injection to get a repository and property-injection to get a Logger.
    public class Tester : ITransientDependency
    {
        public ILogger Logger { get; set; }

        private readonly IRepository<User, Guid> _userRepository;
        private readonly IEventBus _eventBus;
        private readonly IBackgroundJobManager _backgroundJobManager;
        public Tester(
            IRepository<User, Guid> userRepository,
            IEventBus eventBus,
            IBackgroundJobManager backgroundJobManager
            )
        {
            _userRepository = userRepository;
            _eventBus = eventBus;
            _backgroundJobManager = backgroundJobManager;

            Logger = NullLogger.Instance;
        }

        public void Run()
        {
            Logger.Debug("Started Tester.Run()");

            //_eventBus.Trigger
            //1.get data file from in folder
            //取得控制台应用程序的根目录方法
            //方法1、Environment.CurrentDirectory 取得或设置当前工作目录的完整限定路径
            //方法2、AppDomain.CurrentDomain.BaseDirectory 获取基目录，它由程序集冲突解决程序用来探测程序集
            //Sample output
            //Current Directory:E:\project\SuperRocket.Console\SuperRocket.Console\bin\Debug\
            //Input Data Full Path:E:\project\SuperRocket.Console\SuperRocket.Console\bin\Debug\In
            //E:\project\SuperRocket.Console\SuperRocket.Console\bin\Debug\In\data.txt
            var currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine("Current Directory:" + currentDir);
            var inputFullPath = Path.Combine(currentDir, "In");
            Console.WriteLine("Input Data Full Path:" + inputFullPath);
            DirectoryInfo folder = new DirectoryInfo(inputFullPath);
            var files = folder.GetFiles("*.txt");
            List<FileInfo> dataFiles = new List<FileInfo>();
            dataFiles.AddRange(files);

            if (files.Length > 0)
            {
                _backgroundJobManager.Enqueue<DataProcessorJob, DataFiles>(new DataFiles
                {
                    SourceFileFullPath = inputFullPath,
                    SourceFiles = dataFiles
                });
            }
            else
            {
                System.Console.WriteLine("No data files found.");
            }
            
            //foreach (FileInfo file in folder.GetFiles("*.txt"))
            //{
            //    Console.WriteLine(file.FullName);
            //    var pathDestination = Path.Combine(currentDir, "Out", DateTime.UtcNow.ToString("yyyyMMddHHmmss") + file.Name + "_train.csv");
            //    FileInfo fileDestination = new FileInfo(pathDestination);


            //    if (!fileDestination.Exists)
            //    {
            //        using (FileStream stream = System.IO.File.Create(pathDestination))
            //        {

            //        }
            //    }

            //    _backgroundJobManager.Enqueue<DataProcessorJob, DataParameter>(new DataParameter
            //    { 
            //      SourceFileFullPath = file.FullName,
            //      DestinationFileFullPath = pathDestination
            //    });

            //    Thread.Sleep(2000);
            //}

            _backgroundJobManager.Enqueue<TestJob, int>(1);

          

            Logger.Debug("Finished Tester.Run()");
        }
    }
}