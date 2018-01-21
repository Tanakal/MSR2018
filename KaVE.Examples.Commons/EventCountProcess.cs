using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaVE.Examples.Commons
{
    //This class count the number of events for ech differents events.
    //It was a  priliminary work, just to train me to browse the event and to have first analyses.
    //It allow me to see what event happend the most, ...
    internal class EventCountProcess : Process
    {
        Dictionary<string, int> eventsNumber = new Dictionary<string, int>();


        internal override void process(CommandEvent ce)
        {
            addEvent("CommandEvent");
            Console.Write("found a CommandEvent (id: {0})\n", ce.CommandId);
        }

        internal override void process(CompletionEvent e)
        {
            var snapshotOfEnclosingType = e.Context2.SST;
            var enclosingTypeName = snapshotOfEnclosingType.EnclosingType.FullName;

            addEvent("CompletionEvent");
            Console.Write("found a CompletionEvent (was triggered in: {0})\n", enclosingTypeName);
        }

        internal override void processBasic(IDEEvent e)
        {
            var eventType = e.GetType().Name;
            var triggerTime = e.TriggeredAt ?? DateTime.MinValue;

            addEvent(eventType);
            Console.Write("found an {0} that has been triggered at: {1})\n", eventType, triggerTime);
        }

        private void addEvent(string eventName)
        {
            if (eventsNumber.ContainsKey(eventName))
            {
                eventsNumber[eventName] = eventsNumber[eventName] + 1;
            }
            else
            {
                eventsNumber[eventName] = 1;
            }
        }

        internal override void getResult(string percentage)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Results\\RunEvent\\eventsCount.txt");
                sw.WriteLine("{0}", percentage);
                sw.WriteLine("EventName;number");

                foreach (KeyValuePair<string, int> eventCount in eventsNumber)
                {
                    sw.WriteLine("{0};{1}", eventCount.Key, eventCount.Value);
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
