using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhantomTest.NET
{
    public class JavascriptTest
    {
        public bool Skipped { get; set; }
        public bool Passed { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public double? Duration { get; set; }
        public string Error { get; set; }
    }
}
