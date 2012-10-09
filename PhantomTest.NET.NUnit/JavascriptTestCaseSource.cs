using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhantomTest.NET
{
    public static class JavascriptTestCaseSource
    {
        public static IEnumerable TestCases
        {
            get
            {
                return TestCasesFromDirectory(TestDirectory);
            }
        }

        public static string TestDirectory { get; set; }

        public static IEnumerable<TestCaseData> TestCasesFromDirectory(string testDirectory = null)
        {
            var tests = TestRunner.RunAllTests(testDirectory);
            List<TestCaseData> testCases = new List<TestCaseData>();

            foreach (var test in tests)
            {
                var testCase = new TestCaseData(test).SetName(test.Name);

                if (test.Skipped)
                {
                    testCase.Ignore("This test was skipped in mocha");
                }

                testCases.Add(testCase);
            }

            return testCases;
        }
    }
}
