using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PhantomTest.NET
{
    public static class TestRunner
    {
        private const string PhantomJs = "phantomjs.exe";
        private const string MochaPhantomJs = "mocha-phantomjs.coffee";
        private const string DefaultTestFile = "tests.html";
        private const string DefaultDirectory = "../../";

        private const string Start = "*START*";
        private const string Skip = "*SKIP*";
        private const string Pass = "*PASS*";
        private const string TestsComplete = "*COMPLETE*";
        private const string Failed = "*FAILED*";
        private const string Summary = "*SUMMARY*";
        private const string Time = "*TIME*";

        // List is the only format that gives me errors and duration...
        private const string Arguments = " {0} {1} list";

        public static IEnumerable<JavascriptTest> RunAllTests(string testsDirectory = null)
        {
            var phantomJs = new FileInfo((testsDirectory ?? DefaultDirectory) + PhantomJs);
            if (!phantomJs.Exists)
            {
                throw new InvalidOperationException("Could not find the phantomjs.exe file at: " + phantomJs.FullName);
            }

            var mochaPhantomJs = new FileInfo((testsDirectory ?? DefaultDirectory) + MochaPhantomJs);
            if (!mochaPhantomJs.Exists)
            {
                throw new InvalidOperationException("Could not find the mocha-phantomjs.coffee file at: " + mochaPhantomJs.FullName);
            }

            var testHtml = new FileInfo((testsDirectory ?? DefaultDirectory) + DefaultTestFile);
            if (!mochaPhantomJs.Exists)
            {
                throw new InvalidOperationException("Could not find the tests.html file at: " + testHtml.FullName);
            }

            var testData = ExecuteTestsAndReadLines(phantomJs, mochaPhantomJs, testHtml);

            var enumerator = testData.GetEnumerator();
            var count = 0;
            JavascriptTest current = null;
            List<JavascriptTest> errorred = new List<JavascriptTest>();
            List<JavascriptTest> results = new List<JavascriptTest>();

            // First section describes our tests
            while (enumerator.MoveNext())
            {
                string currentLine = enumerator.Current;

                if (currentLine.StartsWith(Start))
                {
                    continue;
                }

                if (currentLine.StartsWith(Pass) || currentLine.StartsWith(Skip) || currentLine.StartsWith(Failed) || currentLine.StartsWith(TestsComplete))
                {
                    if (current != null)
                    {
                        results.Add(current);   
                    }

                    current = new JavascriptTest() { Number = count };
                    count++;

                    if (currentLine.StartsWith(Pass))
                    {
                        currentLine = currentLine.Replace(Pass, string.Empty);
                        if (currentLine.Contains(Time))
                        {
                            var split = currentLine.Split('*');
                            currentLine = split[0];
                            double time;
                            if (double.TryParse(split[2].Replace("ms", string.Empty), out time))
                            {
                                current.Duration = time;
                            }
                        }

                        current.Passed = true;
                        current.Name = currentLine;
                        continue;
                    }

                    if (currentLine.StartsWith(Skip))
                    {
                        current.Skipped = true;
                        current.Name = currentLine.Replace(Skip, string.Empty);
                        continue;
                    }

                    if (currentLine.StartsWith(Failed))
                    {
                        currentLine = currentLine.Replace(Failed, string.Empty);
                        current.Name = currentLine.Substring(currentLine.IndexOf(')') + 1);
                        current.Error = string.Empty;
                        errorred.Add(current);
                        continue;
                    }

                    if (currentLine.StartsWith(TestsComplete))
                    {
                        break;
                    }
                }

                // If the test has passed we expect a time at some point...
                if (current.Passed)
                {
                    if (currentLine.Contains(Time))
                    {
                        var split = currentLine.Split('*');
                        currentLine = split[0];
                        double time;
                        if (double.TryParse(split[2].Replace("ms", string.Empty), out time))
                        {
                            current.Duration = time;
                        }
                    }
                }

                // At this point it is just more of the name...
                current.Name = current.Name + "\r\n" + currentLine;
            }

            // Do we have to continue grabbing stuff to work out the errors?
            if (errorred.Any())
            {
                current = null;
                string previousLine = null;
                // we want to skip the summary
                if(enumerator.MoveNext())
                {
                    previousLine = enumerator.Current;
                }

                while (enumerator.MoveNext())
                {
                    var currentLine = enumerator.Current;

                    if (currentLine.StartsWith(Failed))
                    {
                        int errorId = int.Parse(previousLine.Substring(0, previousLine.IndexOf(')'))) - 1;
                        current = errorred[errorId];
                        currentLine = currentLine.Replace(Failed, string.Empty).Replace(Time, string.Empty);
                        current.Error = currentLine;
                    }
                    else
                    {
                        if (current != null)
                        {
                            current.Error = current.Error + "\r\n" + currentLine;
                        }
                    }

                    previousLine = currentLine;
                }
            }

            return results;
        }

        private static IEnumerable<string> ExecuteTestsAndReadLines(FileInfo phantomJs, FileInfo mochajs, FileInfo tests)
        {
            var args = string.Format(Arguments, mochajs.Name, tests.Name);
            var startInfo = new ProcessStartInfo(phantomJs.FullName, args);
            startInfo.WorkingDirectory = mochajs.DirectoryName;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            var proc = Process.Start(startInfo);

            proc.WaitForExit();

            var errorMessage = proc.StandardError.ReadToEnd();
            var outputMessage = proc.StandardOutput.ReadToEnd();

            if (errorMessage != string.Empty)
            {
                throw new InvalidOperationException(errorMessage);
            }
            else
            {
                // We are using the list formatter... which gives us all the data
                // however the formatting is TERRIBLE :/
                return outputMessage
                    .Split('\n')
                    .Select(s => s.Replace("[0G", string.Empty)
                                  .Replace("[A[2K[2K[A[A", string.Empty)
                                  .Replace("[32m  ", string.Empty)
                                  .Replace("*", string.Empty)
                                  .Replace("Ô£ô[0m[90m ", Pass)
                                  .Replace("[A[2K[A[A", TestsComplete)
                                  .Replace("[90m    ", Start)
                                  .Replace("-[0m[36m ", Skip)
                                  .Replace("[0m", string.Empty)
                                  .Replace("[31m  ", Failed)
                                  .Replace("[91m  Ô£û[31m ", Summary)
                                  .Replace("[90m", Time)
                                  .Replace("[2K", string.Empty)
                                  .Trim())
                    .Where(s => !string.IsNullOrEmpty(s));
            }
        }

        //private const string Arguments = " mocha-phantomjs.coffee tests.html tap";
        //private static FileInfo _phantomjs = new FileInfo("../../phantomjs.exe");

        //public static IEnumerable TestCases
        //{
        //    get
        //    {
        //        return ParseTapData().ToList();
        //    }
        //}

        //private static IEnumerable<TestCaseData> ParseTapData()
        //{
        //    JavascriptTest error = null;
        //    IEnumerable<string> output = null;
        //    try
        //    {
        //        output = ExecuteTestsAndReadLines();
        //    }
        //    catch (Exception e)
        //    {
        //        error = new JavascriptTest() { Name = "Cannot run test runner", Error = e.Message + "\r\n" + e.StackTrace, Passed = false };
        //    }

        //    if (error != null)
        //    {
        //        // Return the error and stop the method
        //        yield return new TestCaseData(error).SetName(error.Name);
        //        yield break;
        //    }

        //    var enumerator = output.GetEnumerator();

        //    string currentLine;
        //    // We might have some errors coming up before the tap input
        //    while(true)
        //    {
        //        if(enumerator.MoveNext())
        //        {
        //            currentLine = enumerator.Current;
        //            if (currentLine.StartsWith("1.."))
        //            {
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            // We got to the end :(
        //            yield break;
        //        }
        //    }

        //    JavascriptTest current = null;
        //    // We are at the start of the tap tests
        //    while (enumerator.MoveNext())
        //    {
        //        currentLine = enumerator.Current;

        //        // Ignore comments
        //        if (currentLine.Trim().StartsWith("#"))
        //        {
        //            continue;
        //        }

        //        if (currentLine.StartsWith("ok") || currentLine.StartsWith("not ok"))
        //        {
        //            // We can send off the last test now
        //            if (current != null)
        //            {
        //                yield return new TestCaseData(current).SetName(current.Name);
        //            }

        //            // Set whether passed
        //            current = new JavascriptTest() { Error = string.Empty };
        //            current.Passed = currentLine.StartsWith("ok");

        //            currentLine = current.Passed ? currentLine.Substring(3) : currentLine.Substring(7);

        //            // Set the test number
        //            var spaceIndex = currentLine.IndexOf(' ');
        //            int currentTestNumber;
        //            if (!int.TryParse(currentLine.Substring(0, spaceIndex), out currentTestNumber))
        //            {
        //                error = new JavascriptTest() { Name = "Could not parse test number", Error = currentLine, Passed = false };
        //                yield return new TestCaseData(error).SetName(error.Name);
        //                yield break;
        //            }
        //            current.Number = currentTestNumber;

        //            // Set the name of the test
        //            var commentIndex = currentLine.IndexOf('#');
        //            current.Name = commentIndex == -1 ? currentLine.Substring(spaceIndex + 1) : currentLine.Substring(spaceIndex + 1, commentIndex);
        //        }
        //        else
        //        {
        //            // We are feeding error stuff
        //            if (current != null)
        //            {
        //                var commentIndex = currentLine.IndexOf('#');
        //                var errorData = commentIndex == -1 ? currentLine : currentLine.Substring(0, commentIndex);

        //                current.Error = current.Error + errorData + Environment.NewLine;
        //            }
        //        }
        //    }

        //    // We have to send off the last one
        //    if (current != null)
        //    {
        //        yield return new TestCaseData(current).SetName(current.Name);
        //    }
        //}

        //private static IEnumerable<string> ExecuteTestsAndReadLines()
        //{
        //    var startInfo = new ProcessStartInfo(_phantomjs.FullName, Arguments);
        //    startInfo.WorkingDirectory = _phantomjs.DirectoryName;
        //    startInfo.UseShellExecute = false;
        //    startInfo.RedirectStandardError = true;
        //    startInfo.RedirectStandardOutput = true;
        //    startInfo.CreateNoWindow = true;
        //    var proc = Process.Start(startInfo);

        //    proc.WaitForExit();

        //    var errorMessage = proc.StandardError.ReadToEnd();
        //    var outputMessage = proc.StandardOutput.ReadToEnd();

        //    if (errorMessage != string.Empty)
        //    {
        //        throw new InvalidOperationException(errorMessage);
        //    }
        //    else
        //    {
        //        outputMessage.Replace("\r", string.Empty);
        //        return outputMessage.Split('\n');
        //    }
        //}
    }
}
