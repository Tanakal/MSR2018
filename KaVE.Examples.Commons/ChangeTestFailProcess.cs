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
    //this class collect the informations/events/parameters presented in the paper (all the 7 parameters). 
    //It's the class to launch on the RunMe.cs if you want to retrieve the same informations I used for the analyses.
    class ChangeTestFailProcess : Process
    {
        Dictionary<string, string[]> TestCases = new Dictionary<string, string[]>();
        Dictionary<int, string[]> TestResult = new Dictionary<int, string[]>();
        List<string> DocumentName = new List<string>();

        int _numberEditChanges = 0;
        int _numberSolutionChanges = 0;
        int _numberCompletionChanges = 0;
        int _numberDebugerStart = 0;
        int _numberDebugerDesignPhase = 0;
        int _TimeBreakpointDebuging = 0;
        int _numberOfNavigation = 0;

        int _testCaseId = 0;

        DateTimeOffset _lastTerminatedAt = DateTimeOffset.MinValue;

        internal override void process(CompletionEvent e)
        {
            if(e.TerminatedState == TerminationState.Applied && e.Selections.Count > 0)
            {
                _numberCompletionChanges += e.Selections.Count;
            }

            if (e.ActiveDocument != null && e.ActiveDocument.Identifier != null)
            {
                addDocumentName(e.ActiveDocument.Identifier);
            }     
        }

        internal override void process(CommandEvent ce)
        {
            if (ce.ActiveDocument != null && ce.ActiveDocument.Identifier != null)
            {
                addDocumentName(ce.ActiveDocument.Identifier);
            } 
        }

        internal override void processBasic(IDEEvent e)
        {
            if(e.ActiveDocument!= null && e.ActiveDocument.Identifier != null)
            {
                addDocumentName(e.ActiveDocument.Identifier);
            }
            
            var EditE = e as EditEvent;
            var TestRunE = e as TestRunEvent;
            var SolutionE = e as SolutionEvent;
            var DebugE = e as DebuggerEvent;
            var NavE = e as NavigationEvent;

            //analyse of Debugging event (we will take into consideration the design phase, the time into breackpoint and the number of breackpoint).
            if (DebugE != null)
            {
                if (DebugE.Mode == DebuggerMode.Design) _numberDebugerDesignPhase++;

                if (DebugE.Mode == DebuggerMode.Break) _TimeBreakpointDebuging += (int)(DebugE.TerminatedAt.Value.Subtract(DebugE.TriggeredAt.Value).TotalMilliseconds);

                if (_lastTerminatedAt != DateTimeOffset.MinValue && (int)DebugE.TriggeredAt.Value.Subtract(_lastTerminatedAt).TotalSeconds > 0) _numberDebugerStart++;
                _lastTerminatedAt = DebugE.TerminatedAt.Value;

            }

            //analyse of EditionEvent (we will take into consideration the number of changes for all edit).
            else if (EditE != null)
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

            else if (NavE != null)
            {
                //analyse of all navigation (we count the number of navigation).
                _numberOfNavigation += 1;
            }

            //analyse of Test Event (it's here we stock the results of other parameters between two state/result of a unit test).
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
                    int v6 = int.Parse(test.Value[6]);

                    test.Value[0] = (v0 + _numberSolutionChanges).ToString();

                    test.Value[1] = (v1 + _numberEditChanges).ToString();

                    test.Value[2] = (v2 + _numberCompletionChanges).ToString();

                    test.Value[3] = (v3 + _numberDebugerStart).ToString();
                    test.Value[4] = (v4 + _numberDebugerDesignPhase).ToString();
                    test.Value[5] = (v5 + _TimeBreakpointDebuging).ToString();

                    test.Value[6] = (v6 + _numberOfNavigation).ToString();
                }

                for (int i = 0; i < TestRunE.Tests.Count; i++)
                {
                    if (TestCases.ContainsKey(TestRunE.Tests.ElementAt(i).TestMethod.Identifier))
                    {

                        if (TestRunE.Tests.ElementAt(i).Result.ToString() != "Error")
                        {
                            //Test : Success -> Failed
                            if (TestRunE.Tests.ElementAt(i).Result.ToString() == "Failed" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][7] == "Success")
                            {
                                _testCaseId += 1;
                                TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][2], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][4], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][5], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][6], DocumentName.Count.ToString(), "0" });
                            }
                            //Test : Failed -> Success
                            else if (TestRunE.Tests.ElementAt(i).Result.ToString() == "Success" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][7] == "Failed")
                            {
                                _testCaseId += 1;
                                TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][2], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][4], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][5], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][6], DocumentName.Count.ToString(), "3" });
                            }
                            //Test : Failed -> Failed   OR  Success -> Success
                            else if ((TestRunE.Tests.ElementAt(i).Result.ToString() == "Failed" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][7] == "Failed") )
                            {
                                _testCaseId += 1;
                                TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][2], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][4], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][5], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][6], DocumentName.Count.ToString(), "1" });
                            }

                            else if ((TestRunE.Tests.ElementAt(i).Result.ToString() == "Success" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][7] == "Success"))
                            {
                                _testCaseId += 1;
                                TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][2], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][4], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][5], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][6], DocumentName.Count.ToString(), "2" });
                            }

                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][0] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][1] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][2] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][4] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][5] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][6] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][7] = TestRunE.Tests.ElementAt(i).Result.ToString();
                        }
                    }
                    else
                    {
                        TestCases.Add(TestRunE.Tests.ElementAt(i).TestMethod.Identifier, new string[] { "0", "0", "0", "0", "0", "0", "0", TestRunE.Tests.ElementAt(i).Result.ToString() });
                    }
                }
                _numberSolutionChanges = 0;
                _numberEditChanges = 0;
                _numberCompletionChanges = 0;
                _numberDebugerStart = 0;
                _numberDebugerDesignPhase = 0;
                _TimeBreakpointDebuging = 0;
                _lastTerminatedAt = DateTimeOffset.MinValue;
                _numberOfNavigation = 0;
            }
        }

        internal override void getResult(string percentage)
        {
            _numberSolutionChanges = 0;
            _numberEditChanges = 0;
            _numberCompletionChanges = 0;
            _numberDebugerStart = 0;
            _numberDebugerDesignPhase = 0;
            _TimeBreakpointDebuging = 0;
            _lastTerminatedAt = DateTimeOffset.MinValue;
            _numberOfNavigation = 0;
            TestCases = new Dictionary<string, string[]>();
            DocumentName = new List<string>();

            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Results\\RunUnitTest\\FINAL-Result.txt");
                sw.WriteLine("NumberSolutionEvent,NumberEditChanges,NumberCompletionChanges,NumberDebugerStart,NumberDebugerDesignPhase,TimeDebugingBreakPOint,numberOfNavigation,DocumentActive,State");

                foreach (KeyValuePair<int, string[]> Test in TestResult)
                {
                    sw.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}", Test.Value[0], Test.Value[1], Test.Value[2], Test.Value[3], Test.Value[4], Test.Value[5], Test.Value[6], Test.Value[7], Test.Value[8]);
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

        private void addDocumentName(string ActiveDocument)
        {
            //we don't know the file Yet -> we add it in the list
            if (DocumentName.IndexOf(ActiveDocument) == -1)
            {
                DocumentName.Add(ActiveDocument);
            }    
        }
    }
}
