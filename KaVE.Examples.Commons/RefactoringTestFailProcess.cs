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
    //this class collect the informations/events/parameters related to EditEvent only.
    //We can launch this class int the RunMe.cs to collect more informations about refactoring. I did it, but not show it 
    //in the paper.
    class RefactoringTestFailProcess : Process
    {
        Dictionary<string, string[]> TestCases = new Dictionary<string, string[]>();
        Dictionary<int, string[]> TestResult = new Dictionary<int, string[]>(); 

        int _numberEdit = 0;
        int _numberChanges = 0;
        int _sizeChanges = 0;
        int _testCaseId = 0;

        internal override void process(CompletionEvent e)
        {
            //do nothing
        }

        internal override void process(CommandEvent ce)
        {
            //do nothing
        }

        internal override void processBasic(IDEEvent e)
        {
            var EditE = e as EditEvent;
            var TestRunE = e as TestRunEvent;

            if (EditE != null)
            {
                _numberEdit += 1;
                _numberChanges += EditE.NumberOfChanges;
                _sizeChanges += EditE.SizeOfChanges;
            }

            else if (TestRunE != null)
            {
                foreach (KeyValuePair<string, string[]> test in TestCases) //adding of all the editEvent since the last testEvent
                {
                    int v1 = int.Parse(test.Value[0]);
                    int v2 = int.Parse(test.Value[1]);
                    int v3 = int.Parse(test.Value[2]);

                    test.Value[0] = (v1 + _numberEdit).ToString();
                    test.Value[1] = (v2 + _numberChanges).ToString();
                    test.Value[2] = (v3 + _sizeChanges).ToString();
                }

                for (int i = 0; i < TestRunE.Tests.Count; i++)
                {
                    if (TestCases.ContainsKey(TestRunE.Tests.ElementAt(i).TestMethod.Identifier))
                    {
                        if (TestRunE.Tests.ElementAt(i).Result.ToString() != "Error")
                        {
                            //Test : Success -> Failed
                            if (TestRunE.Tests.ElementAt(i).Result.ToString() == "Failed" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3] == "Success")
                            {
                                _testCaseId += 1;
                                TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][2], "0" });
                            }
                            //Test : Failed -> Success
                            else if (TestRunE.Tests.ElementAt(i).Result.ToString() == "Success" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3] == "Failed")
                            {
                                _testCaseId += 1;
                                TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][2], "2" });
                            }
                            //Test : Failed -> Failed   OR  Success -> Success
                            else if ((TestRunE.Tests.ElementAt(i).Result.ToString() == "Failed" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3] == "Failed") || (TestRunE.Tests.ElementAt(i).Result.ToString() == "Success" && TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3] == "Success"))
                            {
                                _testCaseId += 1;
                                TestResult.Add(_testCaseId, new string[] { TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][0], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][1], TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][2], "1" });
                            }
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][0] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][1] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][2] = "0";
                            TestCases[TestRunE.Tests.ElementAt(i).TestMethod.Identifier][3] = TestRunE.Tests.ElementAt(i).Result.ToString();
                        }
                    }
                    else
                    {
                        TestCases.Add(TestRunE.Tests.ElementAt(i).TestMethod.Identifier, new string[] { "0", "0", "0", TestRunE.Tests.ElementAt(i).Result.ToString() });
                    }
                }
                _numberEdit = 0;
                _numberChanges = 0;
                _sizeChanges = 0;
            }
        }

        internal override void getResult(string percentage)
        {
            TestCases = new Dictionary<string, string[]>();
            _numberEdit = 0;
            _numberChanges = 0;
            _sizeChanges = 0;

            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Results\\RunUnitTest\\refactoringTest.txt");
                sw.WriteLine("NumberEdit,NumberChanges,SizeChanges,State");

                foreach (KeyValuePair<int, string[]> Test in TestResult)
                {
                    sw.WriteLine("{0},{1},{2},{3}", Test.Value[0], Test.Value[1], Test.Value[2], Test.Value[3]);
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
