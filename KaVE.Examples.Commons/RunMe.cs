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

namespace KaVE.Examples.Commons
{
    internal class RunMe
    {
        /*
	     * download the interaction data and unzip it somewhere, you should now
         * have a folder that includes a bunch of folders that have dates as
         * names and that contain .zip files.
	     */
        public const string EventsDir = "C:\\Users\\jimmyR\\Desktop\\coursJapon\\Mining challenge 2018\\Events-170301";
        

        public static void Main(string[] args)
        {
            Process[] _tasks = new Process[] {  new RefactoringTestFailProcess(), new ChangeTestFailProcess()};
            //Process[] _tasks = new Process[] { new ChangeTestFailProcess()};

            new GettingStarted(EventsDir, _tasks).Run();
        }
    }
}