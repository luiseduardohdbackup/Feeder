using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Feeder
{
    [Serializable]
    public class RssItem
    {
        public string Title { get; set; }
        public string PubDate { get; set; }
        public string Creator { get; set; }
        public string Link { get; set; }
    }
}
