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
    public class DataProcessorJob : BackgroundJob<DataParameter>, ITransientDependency
    {
        public const string delimiter = @"\|,\|";
        /// <summary>
        /// This job will handle the passed file.
        /// </summary>
        /// <param name="dataFileFullPath"></param>
        public async override void Execute(DataParameter dataParameter)
        {
            /// 1.read all lines to a list with label, question
            var lines = await AsyncFile.ReadAllLinesAsync(dataParameter.SourceFileFullPath);
            DataItem dataItem = null;
            List<DataItem> list = new List<DataItem>();
            foreach (var line in lines)
            {
                //System.Console.WriteLine(System.Environment.NewLine);
                System.Console.WriteLine(line);
                var items = Regex.Split(line, delimiter, RegexOptions.IgnoreCase);
                //var items = line.Split(delimiter.ToCharArray());
                if (items.Length >= 3)
                {
                    dataItem = new DataItem();
                    dataItem.Label = items[1].ToString();
                    dataItem.Question = items[2].ToString();
                    list.Add(dataItem);
                }
            }
            //foreach (var item in list)
            //{
            //    System.Console.WriteLine(item.Label + "  " + item.Question);
            //}
            /// 2.write the list to a new txt file. Format e.g.  
            /// First line: label|,|ques
            /// Others:     news_car/other,news_car|,|4S店猫腻之选装！
            //var contents = Enumerable.Repeat("This is a test line.", 150).ToList();
            //var path = Path.Combine(appendAllLinesTestFolder, nameof(Default_LinesAppended));
            //Directory.CreateDirectory(appendAllLinesTestFolder);
            //File.WriteAllLines(path, contents);
            //await AsyncFile.AppendAllLinesAsync(path, contents);
            List<string> data = new List<string>();
            foreach (var item in list)
            {
                var line = item.Label + "|,|" + item.Question;
                data.Add(line);
            }

            await AsyncFile.AppendAllLinesAsync(dataParameter.DestinationFileFullPath, data);
            System.Console.WriteLine("Done");
        }
    }
}