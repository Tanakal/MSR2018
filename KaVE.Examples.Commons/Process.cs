using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaVE.Examples.Commons
{
    internal abstract class Process
    {
        internal void process(IDEEvent e)
        {
            var ce = e as CommandEvent;
            var compE = e as CompletionEvent;

            if (ce != null) process(ce);
            else if (compE != null) process(compE);
            else processBasic(e);
        }

        internal abstract void process(CommandEvent ce);

        internal abstract void process(CompletionEvent e);

        internal abstract void processBasic(IDEEvent e);

        internal abstract void getResult(string userZip);
    }
}
