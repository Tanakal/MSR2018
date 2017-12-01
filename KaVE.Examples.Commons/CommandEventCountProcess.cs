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

        internal override void getResult(string userZip)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter("C:\\Users\\jimmyR\\Desktop\\coursJapon\\Mining challenge 2018\\commandCount.txt");
                sw.WriteLine("{0}", userZip);
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
