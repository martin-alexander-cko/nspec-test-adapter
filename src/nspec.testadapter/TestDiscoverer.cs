using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using NSpec.Domain;

namespace NSpec.TestAdapter
{
    [FileExtension(Constants.DllExtension)]
    [FileExtension(Constants.ExeExtension)]
    [DefaultExecutorUri(Constants.ExecutorUriString)]
    public class TestDiscoverer : ITestDiscoverer
    {
        /// <summary>
        /// Discovers the tests available from the provided containers (DLLs)
        /// </summary>
        /// <param name="sources">Collection of test containers (DLLs)</param>
        /// <param name="discoveryContext">Context in which discovery is being performed (run settings)</param>
        /// <param name="logger">Logger used to log messages.</param>
        /// <param name="discoverySink">Used to send testcases and discovery related events back to Discoverer manager.</param>
        public void DiscoverTests(
            IEnumerable<string> sources,
            IDiscoveryContext discoveryContext,
            IMessageLogger logger,
            ITestCaseDiscoverySink discoverySink)
        {
            logger.SendMessage(TestMessageLevel.Informational, $"DiscoverTests for sources: {string.Join(";", sources)}");

            foreach (var binaryPath in sources)
            {

                using (var diaSession = new DiaSession(binaryPath))
                {
                    logger.SendMessage(TestMessageLevel.Informational, $"Discovering tests in {binaryPath}");

                    // 1. Load the source assembly

                    Assembly assembly = null;
                    try
                    {
                        // if running in .NET Core
                        assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(binaryPath);

                        // if running in .NET Framework
                        // assembly = Assembly.LoadFrom(binaryPath);
                    }
                    catch (Exception exc)
                    {
                        logger.SendMessage(TestMessageLevel.Error, $"Failed to load assembly from {binaryPath}");
                        logger.SendMessage(TestMessageLevel.Error, exc.Message);
                    }
                    logger.SendMessage(TestMessageLevel.Warning, assembly == null ? "Assembly NULL" : "Assembly OK");

                    var testCases = new ExampleFinder().Find(binaryPath).Select(e => e.ToTestCase(binaryPath, diaSession, logger));

                    foreach (var testCase in testCases)
                    {
                        logger.SendMessage(TestMessageLevel.Informational, "Found TestCase --- BEGIN");
                        logger.SendMessage(TestMessageLevel.Informational, "FQDN: " + testCase.FullyQualifiedName);
                        logger.SendMessage(TestMessageLevel.Informational, "ExecutorUri: " + testCase.ExecutorUri);
                        logger.SendMessage(TestMessageLevel.Informational, "Source: " + testCase.Source);
                        logger.SendMessage(TestMessageLevel.Informational, "DisplayName: " + testCase.DisplayName);
                        logger.SendMessage(TestMessageLevel.Informational, "CodeFilePath: " + testCase.DisplayName);
                        logger.SendMessage(TestMessageLevel.Informational, "LineNumber: " + testCase.LineNumber);
                        logger.SendMessage(TestMessageLevel.Informational, "Found TestCase --- END");

                        discoverySink.SendTestCase(testCase);
                    }
                }
            }
        }
    }

    [ExtensionUri("executor://nspectestexecutor")]
    public class TestExecutor : ITestExecutor, IDisposable
    {

        public void Cancel()
        {

        }

        public void Dispose()
        {

        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Warning, "RunTests(tests) started");
            foreach (var test in tests)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Warning, $"Run {test.FullyQualifiedName}");
                frameworkHandle.RecordStart(test);
                var testOutcome = (test.FullyQualifiedName == "a.b.c.d.e") ?
                                TestOutcome.Passed
                                : TestOutcome.Failed;
                frameworkHandle.RecordResult(new TestResult(test) { Outcome = testOutcome });
            }
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Warning, "RunTests(sources) started");



            var dummyTestCase1 = new TestCase
            {
                FullyQualifiedName = "nspec.dummytest",
                DisplayName = "nspec Dummy Test",
                ExecutorUri = new Uri("executor://nspectestexecutor"),
                Source = sources.First()
            };

            var dummyTestCase2 = new TestCase
            {
                FullyQualifiedName = "nspec.dummytest2",
                DisplayName = "nspec Dummy Test2",
                ExecutorUri = new Uri("executor://nspectestexecutor"),
                Source = sources.First(),

            };

            var dummyTestCase3 = new TestCase
            {
                FullyQualifiedName = "a.b.c.d.e",
                DisplayName = "alpha beta gama",
                ExecutorUri = new Uri("executor://nspectestexecutor"),
                Source = sources.First()
            };

            var dummyTestCase4 = new TestCase
            {
                FullyQualifiedName = "ff",
                DisplayName = "ff",
                ExecutorUri = new Uri("executor://nspectestexecutor"),
                Source = sources.First()
            };

            var tests = new[] { dummyTestCase1, dummyTestCase2, dummyTestCase3 };

            foreach (var test in tests)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Warning,
                 $"Running {test.FullyQualifiedName}");
                frameworkHandle.RecordStart(test);
                var testOutcome = (test.FullyQualifiedName == "a.b.c.d.e") ?
                                TestOutcome.Passed
                                : TestOutcome.Failed;
                frameworkHandle.RecordEnd(test, TestOutcome.Passed);
            }

            frameworkHandle.RecordResult(new TestResult(dummyTestCase1) { Outcome = TestOutcome.Passed });
            frameworkHandle.RecordResult(new TestResult(dummyTestCase4) { Outcome = TestOutcome.Passed });


            frameworkHandle.SendMessage(TestMessageLevel.Warning, "RunTests(sources) over");

        }
    }
}
