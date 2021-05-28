using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperRocket.Console
{
    public class DataParameter
    {
        public string SourceFileFullPath { get; set; }
        public string DestinationFileFullPath { get; set; }
    }

    public class DataFiles
    {
        public string SourceFileFullPath { get; set; }
        public List<FileInfo> SourceFiles { get; set; }
    }
}
