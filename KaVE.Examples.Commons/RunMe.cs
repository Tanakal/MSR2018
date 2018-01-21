/*
 * Copyright 2017 University of Zurich
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *    http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.IO;

namespace KaVE.Examples.Commons
{
    internal class RunMe
    {
        /*
	     * download the interaction data and unzip it somewhere, you should now
         * have a folder that includes a bunch of folders that have dates as
         * names and that contain .zip files.
	     */

        /* Prcess[] contains all the process we want to run on each Event encountered.
         * We can run 1 process, 2 process, or more.
         *I create 5 type of process who can be run:
         *  - EventCountProcess : find some data on the event in general.
         *  - CommandEventCountProcess : find some data the command event.
         *  - CodeNavigationProcess : find some data on the navigation of users ("how users navigate in the source code")
         *  - RefactoringTestFailProcess : find some data and correlation between the refactoring and the unit tests.
         *  - ChangeTestFailProcess : find some data and correlation between some parameters(refactoring, debugging, navigation, ...) and test Unit.
         *                            It's the general version of RefactoringTestFailProcess. It's the "Process" I uses to do some statisticals analyses.
         */

        public static void Main(string[] args)
        {
            string EventsDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName + "\\Events-170301";

            //Process[] _tasks = new Process[] {  new RefactoringTestFailProcess(), new ChangeTestFailProcess()};
            Process[] _tasks = new Process[] { new ChangeTestFailProcess()};

            new GettingStarted(EventsDir, _tasks).Run();
        }
    }
}
 