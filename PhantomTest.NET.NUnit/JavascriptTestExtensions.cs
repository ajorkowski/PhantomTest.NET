using NU = NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhantomTest.NET
{
    public static class JavascriptTestExtensions
    {
        public static void Assert(this JavascriptTest test)
        {
            NU.Assert.True(test.Passed, test.Error);
        }
    }
}
