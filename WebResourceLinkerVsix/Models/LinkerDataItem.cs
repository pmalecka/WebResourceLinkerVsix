using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebResourceLinkerVsix
{
    [Serializable]
    public class LinkerDataItem
    {
        public Guid WebResourceId { get; set; }
        public string SourceFilePath { get; set; }
    }
}
