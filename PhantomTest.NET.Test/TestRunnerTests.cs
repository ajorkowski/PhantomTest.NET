using System;
using NUnit.Framework;

namespace PhantomTest.NET.Test
{
    [TestFixture]
    public class TestRunnerTests
    {
        [Test]
        public void RunsWhenValid()
        {
            var tests = TestRunner.RunAllTests("../../../PhantomTest.NET/");
        }
    }
}
