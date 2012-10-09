using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhantomTest.NET
{
    [AttributeUsage(AttributeTargets.Method)]
    public class JavascriptTestsAttribute : TestCaseSourceAttribute
    {
        public JavascriptTestsAttribute()
            : base(typeof(JavascriptTestCaseSource), "TestCases")
        {
        }

        public JavascriptTestsAttribute(string testsDirectory)
            : base(typeof(JavascriptTestCaseSource), "TestCases")
        {
            JavascriptTestCaseSource.TestDirectory = testsDirectory;
        }
    }
}
