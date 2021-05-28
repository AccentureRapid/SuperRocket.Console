﻿using Abp.BackgroundJobs;
using Abp.Dependency;
using AsyncIO.FileSystem;
using SuperRocket.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SuperRocket.Orchard.Job
{
    public class DataProcessorJob : BackgroundJob<DataFiles>, ITransientDependency
    {
        public const string delimiter = @"\|,\|";
        /// <summary>
        /// This job will handle the passed file.
        /// </summary>
        /// <param name="dataFileFullPath"></param>
        public async override void Execute(DataFiles dataFiles)
        {
            List<Task> tasks = new List<Task>();
            List<string> allData = new List<string>();
            foreach (var file in dataFiles.SourceFiles)
            {
                //Task task = Task.Factory.StartNew
                Task<bool> task = await Task.Factory.StartNew(async () =>
                 {
                     Thread.Sleep(500);

                     System.Console.WriteLine(file.FullName);
                     var pathDestination = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Out", DateTime.UtcNow.ToString("yyyyMMddHHmmss") + file.Name + "_train.csv");
                     FileInfo fileDestination = new FileInfo(pathDestination);

                     if (!fileDestination.Exists)
                     {
                         using (FileStream stream = System.IO.File.Create(pathDestination))
                         { }
                     }

                    /// 1.read all lines to a list with label, question
                    var lines = await AsyncFile.ReadAllLinesAsync(file.FullName);
                     DataItem dataItem = null;
                     List<DataItem> list = new List<DataItem>();
                     foreach (var line in lines)
                     {
                         System.Console.WriteLine(line);
                         var items = Regex.Split(line, delimiter, RegexOptions.IgnoreCase);
                         if (items.Length >= 3)
                         {
                             dataItem = new DataItem();
                             dataItem.Label = items[1].ToString();
                             dataItem.Question = items[2].ToString();
                             list.Add(dataItem);
                         }
                     }

                    //1.multiple , should be to only one.
                    //2.if , is the last one, should remove it.
                    string pattern = @",{1,999}";
                     RegexOptions ops = RegexOptions.Singleline;
                     Regex reg = new Regex(pattern, ops);

                     List<string> data = new List<string>();
                     foreach (var item in list)
                     {
                         if (!string.IsNullOrEmpty(item.Label))
                         {
                            //1.multiple , should be to only one.
                            //2.if , is the last one, should remove it.
                            if (reg.IsMatch(item.Label))
                             {
                                 item.Label = reg.Replace(item.Label, ",");
                             }

                             if (item.Label.EndsWith(","))
                             {
                                 item.Label = item.Label.Substring(0, item.Label.Length - 1);
                             }

                             var line = item.Label + "|,|" + item.Question;
                             data.Add(line);
                         }
                     }

                     allData.AddRange(data);
                     await AsyncFile.AppendAllLinesAsync(pathDestination, data);
                     System.Console.WriteLine(file.FullName + " processed successfully！");
                     return true;
                 });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
            System.Console.WriteLine("All child data files processed successfully！");
            //output a merged file
            var mergerdFilePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Out", DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "merged_train.csv");
            AppendHeader(mergerdFilePath);
            await AsyncFile.AppendAllLinesAsync(mergerdFilePath, allData);
            System.Console.WriteLine("All data counts" + allData.Count().ToString());
            System.Console.WriteLine("All files processed successfully！");
        }

        private async void AppendHeader(string fileFullPathDestination)
        {
            List<string> header = new List<string>();
            header.Add("label" + "|,|" + "ques");
            await AsyncFile.AppendAllLinesAsync(fileFullPathDestination, header);
        }
    }
}
