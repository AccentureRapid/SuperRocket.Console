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
using Abp.Events.Bus;
using Npgsql_Helper_;
using System.Data;
using AsyncIO.FileSystem;

namespace SuperRocket.Orchard.Job
{
    public class DataMerger : BackgroundJob<int>, ITransientDependency
    {
        private readonly IEventBus _eventBus;
        const string TagSeprator = @"@#gzg#";
        bool preferTitleToContent = Convert.ToBoolean(ConfigurationManager.AppSettings["PreferTitleToContent"]);
        public DataMerger(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }
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

            string databaseServer = ConfigurationManager.AppSettings["DatabaseServer"];
            string database = ConfigurationManager.AppSettings["Database"];
            string username = ConfigurationManager.AppSettings["Username"];
            string password = ConfigurationManager.AppSettings["Password"];

            string connectionString = string.Format(@"Server={0};Database={1};User Id={2};Password={3};",
                        databaseServer,
                        database,
                        username,
                        password
                        );//sslmode=disable;ssl=false;Protocol=3;Pooling=true;MaxPoolSize=30";
            Npgsql_Helper db = new Npgsql_Helper(connectionString);


            var client = new MinioClient(endPoint, accessKey, secretKey);
            var result = MinioHelper.ListObjects(client, bucket);

            Dictionary<string, string> fileNameMappings = new Dictionary<string, string>();

            if (result.Item1)
            {
                //var collector = result.Item2;
                //System.Console.WriteLine("Enumrate Minio objects start...");
                //List<Item> allObjects = new List<Item>();

                //collector.Subscribe(item =>
                //{
                //    System.Console.WriteLine(item.Key);
                //    //allObjects.Add(item);
                //    //var fileName = Path.Combine(dataFilesPath, DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + item.Key);
                //    //await MinioHelper.GetObjectAsync(client, bucket, item.Key, fileName);
                //    //fileNameMappings.Add(item.Key, fileName);
                //}, ex => System.Console.WriteLine($"OnError: {ex}"),
                //() => {
                //    Thread.Sleep(5000);
                //    System.Console.WriteLine("all data printed...");
                //});

                //System.Console.WriteLine($"Listed all objects in bucket {bucket} :  {allObjects.Count()}");
                //System.Console.WriteLine("Enumrate objects end...");
                //after collect all the mimio objects, query all the data

                // #### a simple query ####

                string sql = "SELECT article_url, article_title, article_content_path, article_tags, create_date FROM public.it_xueyuan_article_info";

                DataTable data = db.ExecuteDataset(CommandType.Text, sql)[0];
                System.Console.WriteLine("Total row count: " + data.Rows.Count.ToString());

                foreach (DataRow row in data.Rows)
                {
                    System.Console.WriteLine($"Acticle Url:  {row["article_url"]}, Title:{row["article_title"]}, Tags: {row["article_tags"]}");
                }

                // Write data to one txt file.
                WriteDataLine(data.Rows);
            }
            Thread.Sleep(5000);
            System.Console.WriteLine("{0} Test job completed with {1} counts successfully!", DateTime.Now.ToString(), number);
        }



        //Async method to be awaited
        public async void WriteDataLine(DataRowCollection rows, Dictionary<string, string> mappings)
        {
            var currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
            System.Console.WriteLine("Current Directory:" + currentDir);
            var dataFilesPath = Path.Combine(currentDir, "Out");
            System.Console.WriteLine("Output data file Full Path:" + dataFilesPath);
            var outputTitleFileName = Path.Combine(dataFilesPath, DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + "title.txt");
            var outputContentFileName = Path.Combine(dataFilesPath, DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + "content.txt");

            List<string> lines = new List<string>();
            List<string> contentLines = new List<string>();
            foreach (DataRow row in rows)
            {
                var article_url = row["article_url"].ToString();
                var article_title = row["article_title"].ToString();
                var article_tags = row["article_tags"].ToString();
                var article_content_path = row["article_content_path"].ToString();

                //get the //Files folder's certain file with datetime prefix
                string article_content_file = string.Empty;
                mappings.TryGetValue(article_content_path, out article_content_file);
                if (!string.IsNullOrEmpty(article_content_file))
                {
                    var fileFullName = Path.Combine(dataFilesPath, article_content_file);

                    //read content from fileFullName as conent

                    StringBuilder sb = new StringBuilder();
                    var tags = article_tags.Split(TagSeprator.ToCharArray()).Where( x => x != "");
                    var tagString = string.Join("|", tags);
                    sb.Append(tagString);
                    sb.Append(",");
                    if (preferTitleToContent)
                    {
                        sb.Append(article_title);
                    }
                    else
                    {
                        var text = await AsyncFile.ReadAllTextAsync(fileFullName);
                        sb.Append(text);
                    }

                    var line = sb.ToString();
                    lines.Add(line); 
                }
            }
            //算法|数据结构,连续子数组的元素之和最大值(tag1|tag2,title or tag1|tag2,article_content)
            if (preferTitleToContent) {
                MakeSureFileExists(outputTitleFileName);
                await AsyncFile.AppendAllLinesAsync(outputTitleFileName, lines);
                System.Console.WriteLine("data file with tag and title completed successfully!");
            }
            else {
                MakeSureFileExists(outputContentFileName);
                await AsyncFile.AppendAllLinesAsync(outputContentFileName, lines);
                System.Console.WriteLine("data file with tag and content completed successfully!");
            }
        }

        public async void WriteDataLine(DataRowCollection rows)
        {
            var currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
            System.Console.WriteLine("Current Directory:" + currentDir);
            var dataFilesPath = Path.Combine(currentDir, "Out");
            System.Console.WriteLine("Output data file Full Path:" + dataFilesPath);
            var outputTitleFileName = Path.Combine(dataFilesPath, DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + "title.txt");
            var outputContentFileName = Path.Combine(dataFilesPath, DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + "content.txt");

            List<string> lines = new List<string>();
            List<string> contentLines = new List<string>();
            foreach (DataRow row in rows)
            {
                var article_url = row["article_url"].ToString();
                var article_title = row["article_title"].ToString();
                var article_tags = row["article_tags"].ToString();
                var article_content_path = row["article_content_path"].ToString();

                //read content from fileFullName as conent

                StringBuilder sb = new StringBuilder();
                var tags = article_tags.Split(TagSeprator.ToCharArray()).Where(x => x != "");
                var tagString = string.Join("|", tags);
                sb.Append(tagString);
                sb.Append(",");
                sb.Append(article_title);

                var line = sb.ToString();
                lines.Add(line);
                
            }
            //算法|数据结构,连续子数组的元素之和最大值(tag1|tag2,title or tag1|tag2,article_content)
            MakeSureFileExists(outputTitleFileName);
            await AsyncFile.AppendAllLinesAsync(outputTitleFileName, lines);
            System.Console.WriteLine("data file with tag and title completed successfully!");
        }

        private static void MakeSureFileExists(string fileFullName)
        {
            FileInfo fileDestination = new FileInfo(fileFullName);
            if (!fileDestination.Exists)
            {
                using (FileStream stream = System.IO.File.Create(fileFullName))
                {

                }
            }
        }
    }
}
