using Abp.BackgroundJobs;
using Abp.Dependency;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel;
using SuperRocket.Console;

namespace SuperRocket.Orchard.Job
{
    public class DataMerger : BackgroundJob<int>, ITransientDependency
    {
        public async override void Execute(int number)
        {
            //1.download minio data files to local directory  //Files
            var currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
            System.Console.WriteLine("Current Directory:" + currentDir);
            var dataFilesPath = Path.Combine(currentDir, "Files");
            System.Console.WriteLine("Minio Files Full Path:" + dataFilesPath);
            DirectoryInfo folder = new DirectoryInfo(dataFilesPath);

            string endPoint = ConfigurationManager.AppSettings["EndPoint"];
            string accessKey = ConfigurationManager.AppSettings["AccessKey"];
            string secretKey = ConfigurationManager.AppSettings["ScecretKey"];
            string bucket = ConfigurationManager.AppSettings["Bucket"];

            var client = new MinioClient(endPoint, accessKey, secretKey);
            var result = MinioHelper.ListObjects(client, bucket);

            if (result.Item1)
            {
                var collector = result.Item2;
                System.Console.WriteLine("Enumrate Minio objects start...");
                List<Item> allObjects = new List<Item>();

                collector.Subscribe(async item =>
                {
                    System.Console.WriteLine(item.Key);
                    allObjects.Add(item);
                    var fileName = Path.Combine(dataFilesPath, item.Key);
                    await MinioHelper.GetObjectAsync(client, bucket, item.Key, fileName);
                }, ex => System.Console.WriteLine($"OnError: {ex}"),
                () => {
                    System.Console.WriteLine($"Listed all objects in bucket {bucket}\n :  {allObjects.Count()}");
                    System.Console.WriteLine("Enumrate objects end...");
                });
            }

            Thread.Sleep(5000);
            System.Console.WriteLine("{0} Test job completed with {1} counts successfully!", DateTime.Now.ToString(), number);
        }
    }
}
