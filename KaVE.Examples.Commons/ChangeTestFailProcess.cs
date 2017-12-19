using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Model.Events.VisualStudio;
using KaVE.Commons.Model.Events.TestRunEvents;
using System.IO;

namespace KaVE.Examples.Commons
{
    class ChangeTestFailProcess : Process
    {
        Dictionary<string, string[]> TestCases = new Dictionary<string, string[]>();
        Dictionary<int, string[]> TestResult = new Dictionary<int, string[]>();

        int _numberEditChanges = 0;
        int _numberSolutionChanges = 0;
        int _numberCompletionChanges = 0;
        int _numberDebugerStart = 0;
        int _numberDebugerDesignPhase = 0;
        int _TimeBreakpointDebuging = 0;

        int _testCaseId = 0;

        DateTimeOffset _lastTerminatedAt = DateTimeOffset.MinValue;

        internal override void process(CompletionEvent e)
        {
            if(e.TerminatedState == TerminationState.Applied && e.Selections.Count > 0)
            {
                _numberCompletionChanges += e.Selections.Count;
            }
        }

        internal override void process(CommandEvent ce)
        {
            //not used
        }

        internal override void processBasic(IDEEvent e)
        {
            var EditE = e as EditEvent;
            var TestRunE = e as TestRunEvent;
            var SolutionE = e as SolutionEvent;
            var DebugE = e as DebuggerEvent;

            //analyse of EditionEvent (we will take into consideration the number of changes for all edit).
            if (DebugE != null)
            {
                if (DebugE.Mode == DebuggerMode.Design) _numberDebugerDesignPhase++;

                if (DebugE.Mode == DebuggerMode.Break) _TimeBreakpointDebuging += (int)(DebugE.TerminatedAt.Value.Subtract(DebugE.TriggeredAt.Value).TotalMilliseconds);

                if (_lastTerminatedAt != DateTimeOffset.MinValue && (int)DebugE.TriggeredAt.Value.Subtract(_lastTerminatedAt).TotalSeconds > 0) _numberDebugerStart++;
                _lastTerminatedAt = DebugE.TerminatedAt.Value;

            }

            //analyse of EditionEvent (we will take into consideration the number of changes for all edit).
            if (EditE != null)
            {
                _numberEditChanges += EditE.NumberOfChanges;
            }

            //analyse of SolutionEvent (If a project or item name is change, we count it. If a solutionItem or projectItem are removed, we count it)
            //The goals is to determine if more project and item are changed (name, removed, ...) more testCase pass from Success to Failed.
            else if (SolutionE != null)
            {
                if(SolutionE.Action == SolutionAction.RenameSolutionItem || SolutionE.Action == SolutionAction.RenameSolution || SolutionE.Action == SolutionAction.RenameProjectItem || SolutionE.Action == SolutionAction.RenameProject || SolutionE.Action == SolutionAction.RemoveSolutionItem || SolutionE.Action == SolutionAction.RemoveProjectItem || SolutionE.Action == SolutionAction.RemoveProject)
                {
                    _numberSolutionChanges += 1;
                }
            }

            else if (TestRunE != null)
            {
                foreach (KeyValuePair<string, string[]> test in TestCases) 
                {
                    int v0 = int.Parse(test.Value[0]);
                    int v1 = int.Parse(test.Value[1]);
                    int v2 = int.Parse(test.Value[2]);
                    int v3 = int.Parse(test.Value[3]);
                    int v4 = int.Parse(test.Value[4]);
                    int v5 = int.Parse(test.Value[5]);

                    test.Value[0] = (v0 + _numberSolutionChanges).ToString();

                    test.Value[1] = (v1 + _numberEditChanges).ToString();

                    test.Value[2] = (v2 + _numberCompletionChanges).ToString();

                    test.Value[3] = (v3 + _numberDebugerStart).ToString();
                    test.Value[4] = (v4 + _numberDebugerDesignPhase).ToString();
                    test.Value[5] = (v5 + _TimeBreakpointDebuging).ToString();
                }

                for (int i = 0; i < TestRunE.Tests.Count; i++)
                {
                    if (TestCases.ContainsKey(TestRunE.Tests.ElementAt(i).TestMethod.Name))
                    {
                        //Test : Success -> Failed
                        if (TestRunE.Tests.ElementAt(i).Result.ToString() == "Failed" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][3] == "Success")
                        {
                            _testCaseId += 1;
                            TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][2], "0" });
                        }
                        //Test : Failed -> Success
                        else if (TestRunE.Tests.ElementAt(i).Result.ToString() == "Success" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][3] == "Failed")
                        {
                            _testCaseId += 1;
                            TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][2], "2" });
                        }
                        //Test : Failed -> Failed   OR  Success -> Success
                        else if ((TestRunE.Tests.ElementAt(i).Result.ToString() == "Failed" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][3] == "Failed") || (TestRunE.Tests.ElementAt(i).Result.ToString() == "Success" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][3] == "Success"))
                        {
                            _testCaseId += 1;
                            TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][2], "1" });
                        }

                        TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Name][3] = TestRunE.Tests.ElementAt(i).Result.ToString();
                    }
                    else
                    {
                        TestCases.Add(TestRunE.Tests.ElementAt(i).TestMethod.Name, new string[] { "0", "0", "0", "0", "0", "0", TestRunE.Tests.ElementAt(i).Result.ToString() });
                    }
                }
                _numberSolutionChanges = 0;
                _numberEditChanges = 0;
                _numberCompletionChanges = 0;
                _numberDebugerStart = 0;
                _numberDebugerDesignPhase = 0;
                _TimeBreakpointDebuging = 0;
                _lastTerminatedAt = DateTimeOffset.MinValue;
            }
        }

        internal override void getResult(string percentage, bool NextUserNew)
        {
            if (NextUserNew)
            {
                TestCases = new Dictionary<string, string[]>();
                _numberSolutionChanges = 0;
                _numberEditChanges = 0;
                _numberCompletionChanges = 0;
                _numberDebugerStart = 0;
                _numberDebugerDesignPhase = 0;
                _TimeBreakpointDebuging = 0;
                _lastTerminatedAt = DateTimeOffset.MinValue;
            }

            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter("C:\\Users\\jimmyR\\Desktop\\coursJapon\\Mining challenge 2018\\result\\[Refactoring] Does refactoring lead to more failed tests\\AllParameters.txt");
                sw.WriteLine("NumberSolutionEvent,NumberEditChanges,NumberCompletionChanges,NumberDebugerStart,NumberDebugerDesignPhase,TimeDebugingBreakPOint,State");

                foreach (KeyValuePair<int, string[]> Test in TestResult)
                {
                    sw.WriteLine("{0},{1},{2},{3},{4},{5},{6}", Test.Value[0], Test.Value[1], Test.Value[2], Test.Value[3], Test.Value[4], Test.Value[5], Test.Value[6]);
                }

                //Close the file
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Process over!");
            }
        }
    }
}
