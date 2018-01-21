using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;
using System.IO;

namespace KaVE.Examples.Commons
{
    //this class collect some information about the navigation: the way developers navigate (keyboard shortcut, ...), when, ...
    //I decide to focus on the Result of Unit test and decide to abandon this reseach. 
    class CodeNavigationProcess : Process
    {
        Dictionary<string, int> NavigationTypeCount = new Dictionary<string, int>();
        Dictionary<int, string[]> NavigationTypeEnum = new Dictionary<int, string[]>();

        DateTimeOffset LastNavEDate;
        bool IsNullLastNavEDate = true;

        int _incrementalId = 0;

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
            var NavE = e as NavigationEvent;

            if (NavE != null)
            {
                //Navigation Global
                addTypeOfNavigationCount(NavE.TypeOfNavigation.ToString());

                //Navigation Details (one by one) -> for futures analyses

                string fullName = "unknow";
                string identifiers = "unknow";
                string TypeOfNavigation = NavE.TypeOfNavigation.ToString();

                if (!NavE.Location.IsUnknown)
                {
                    string[] nameAndIdentifierString = NavE.Location.ToString().Split(new string[] { " [" }, StringSplitOptions.None);

                    if(nameAndIdentifierString.Length == 1)
                    {
                        identifiers = nameAndIdentifierString[0];
                    }
                    else
                    {
                        if (nameAndIdentifierString[0]== "static")
                        {
                            string nameAndIdentifier = nameAndIdentifierString[2];
                            fullName = nameAndIdentifier.Split(new string[] { "]." }, StringSplitOptions.None)[1].Split('(')[0];
                            identifiers = nameAndIdentifier.Split(new string[] { "]." }, StringSplitOptions.None)[0];
                            if (fullName == "?")
                            {
                                fullName = "unknow";
                            }
                            if (identifiers == "?")
                            {
                                identifiers = "unknow";
                            }
                        }
                        else
                        {
                            string nameAndIdentifier = nameAndIdentifierString[1];
                            fullName = nameAndIdentifier.Split(new string[] { "]." }, StringSplitOptions.None)[1].Split('(')[0];
                            identifiers = nameAndIdentifier.Split(new string[] { "]." }, StringSplitOptions.None)[0];
                            if (fullName == "?")
                            {
                                fullName = "unknow";
                            }
                            if (identifiers == "?")
                            {
                                identifiers = "unknow";
                            }
                        }
                    }
                }

                //calcul time between this NavEvent and last one
                int timeBewteen;
                if (IsNullLastNavEDate) 
                {
                    IsNullLastNavEDate = false;
                    timeBewteen = 0;
                    
                }
                else
                {
                    timeBewteen = (int)(NavE.TriggeredAt.Value.Subtract(LastNavEDate).TotalMilliseconds);
                    TimeSpan t = NavE.TriggeredAt.Value.Subtract(LastNavEDate);
                }
                LastNavEDate = NavE.TriggeredAt.Value;

                addTypeOfNavigationEnum(TypeOfNavigation, fullName, identifiers, LastNavEDate.ToString(), NavE.ActiveDocument.Identifier);
            }
        }

        internal override void getResult(string percentage)
        {
            IsNullLastNavEDate = true;
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                string EventsDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Events-170301";
                StreamWriter sw = new StreamWriter("C:\\Users\\jimmyR\\Desktop\\coursJapon\\Mining challenge 2018\\result\\[Code Navigation] How do developers navigate the code base\\CountTypeOfNavigationCode.txt");
                sw.WriteLine("{0}", percentage);
                sw.WriteLine("TypeOfNavigation;number");

                foreach (KeyValuePair<string, int> navigationType in NavigationTypeCount)
                {
                    sw.WriteLine("{0};{1}", navigationType.Key, navigationType.Value);
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

            if(percentage == "85 / 85")
            {
                try
                {
                    //Pass the filepath and filename to the StreamWriter Constructor
                    StreamWriter sw = new StreamWriter(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + "\\Results\\RunNavigation\\EnumTypeOfNavigationCode.txt");
                    sw.WriteLine("TypeOfNavigation;FullName;Identifier;Time(ms);ActiveDocument");

                    foreach (KeyValuePair<int, string[]> navigationType in NavigationTypeEnum)
                    {
                        sw.WriteLine("{0};{1};{2};{3};{4}", navigationType.Value[0], navigationType.Value[1], navigationType.Value[2], navigationType.Value[3], navigationType.Value[4]);
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

        private void addTypeOfNavigationCount(string typeOfNavigation)
        {
            if (NavigationTypeCount.ContainsKey(typeOfNavigation))
            {
                NavigationTypeCount[typeOfNavigation] = NavigationTypeCount[typeOfNavigation] + 1;
            }
            else
            {
                NavigationTypeCount[typeOfNavigation] = 1;
            }
        }

        private void addTypeOfNavigationEnum(string typeOfNavigation, string fullName, string identifier, string time, string ActiveDocument)
        {
            _incrementalId += 1;
            NavigationTypeEnum.Add(_incrementalId, new string[] { typeOfNavigation, fullName, identifier, time, ActiveDocument });
        }

    }
}
