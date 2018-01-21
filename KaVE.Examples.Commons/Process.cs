using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Model.Events.TestRunEvents;
using KaVE.Commons.Model.Events.UserProfiles;
using KaVE.Commons.Model.Events.VersionControlEvents;
using KaVE.Commons.Model.Events.VisualStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaVE.Examples.Commons
{
    //it's the abstract class, the origin of the other Process class.
    //each other process class implement some research on the 11M event. 
    //the type of research is explaned directly on the top of each ....Process.cs

    internal abstract class Process
    {
        internal void process(IDEEvent e)
        {
            var ce = e as CommandEvent;
            var compE = e as CompletionEvent;

            /*this is all the Event we can use*/

            //var IDEStateE = e as IDEStateEvent;
            //var WindowE = e as WindowEvent;
            //var EditE = e as EditEvent;
            //var ActivityE = e as ActivityEvent;
            //var NavE = e as NavigationEvent;
            //var DocE= e as DocumentEvent;
            //var DebugE = e as DebuggerEvent;
            //var SolutionE = e as SolutionEvent;
            //var BuildE= e as BuildEvent;
            //var SystemE = e as SystemEvent;
            //var TestRunE = e as TestRunEvent;
            //var VersionControlE = e as VersionControlEvent;
            //var UserProfileE= e as UserProfileEvent;
            //var FindE = e as FindEvent;

            if (ce != null) process(ce);
            else if (compE != null) process(compE);

            else processBasic(e);
        }

        internal abstract void process(CommandEvent ce);

        internal abstract void process(CompletionEvent e);

        internal abstract void processBasic(IDEEvent e);

        internal abstract void getResult(string percentage);
    }
}
