using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMP.Model
{
    public class Media
    {
        public string FileName { get; set; }
        public int Duration { get; set; }
        public bool isPlaying { get; set; }
    }
}
