using PhantomTest.NET;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhantomTest.NET.Test
{
    [TestFixture]
    public class JavascriptTestsAttributeTests
    {
        [Test]
        [JavascriptTests("../../../PhantomTest.NET/")]
        public void RunJavascriptTests(JavascriptTest test)
        {
            test.Assert();
        }
    }
}
