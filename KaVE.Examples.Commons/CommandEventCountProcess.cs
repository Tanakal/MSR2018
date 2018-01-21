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
    //In the same way than the EventCountProcess, this class count the number of different command for each CommandEvent
    //With the EventCountProcess, I see that CommandEvent represent 80% of all the event. For this reason, i decides to browse these CommandEvent
    //The result show me trivial things: the most used commands are "openFile", "save File", "home", "refresh", "unknow command", ...
    //I decides to don't use CommandEvent in the rest of the analyses cause of the poor information we have about it.
    class CommandEventCountProcess : Process
    {
        Dictionary<string, int> commandNumber = new Dictionary<string, int>();

        internal override void process(CommandEvent ce)
        {
            addCommand(ce.CommandId);
            Console.Write("found a CommandEvent\n");
        }

        internal override void process(CompletionEvent e)
        {
            //do nothing
            Console.Write("found a CompletionEvent\n");
        }

        internal override void processBasic(IDEEvent e)
        {
            //do nothing
            Console.Write("found a tierce Event\n");
        }

        private void addCommand(string command)
        {
            if(command[0] != '{')
            {
                if (commandNumber.ContainsKey(command))
                {
                    commandNumber[command] = commandNumber[command] + 1;
                }
                else
                {
                    commandNumber[command] = 1;
                }
            }
            else
            {
                Console.Write("found a doublon\n");
            }
        }

        internal override void getResult(string percentage)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Results\\RunCommand\\commandCount.txt");
                sw.WriteLine("{0}", percentage);
                sw.WriteLine("CommandName;number");

                foreach (KeyValuePair<string, int> commandCount in commandNumber)
                {
                    sw.WriteLine("{0};{1}", commandCount.Key, commandCount.Value);
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
