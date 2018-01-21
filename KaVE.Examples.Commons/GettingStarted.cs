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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Utils.Collections;
using KaVE.Commons.Utils.IO.Archives;

/**
 * This class let use browsing all the 11M event, deserializing theim and launching a "Process" on each of theim.
 * this class only browse events and did not any analyses. "Process class" do the analyses.
 */
namespace KaVE.Examples.Commons
{
    internal class GettingStarted
    {
        private readonly string _eventsDir;
        private Process[] _tasks;

        public GettingStarted(string eventsDir, Process[] tasks)
        {
            _eventsDir = eventsDir;
            _tasks = tasks;
        }

        public void Run()
        {
            Console.Write("looking (recursively) for events in folder {0}\n", Path.GetFullPath(_eventsDir));

            /*
             * Each .zip that is contained in the eventsDir represents all events
             * that we have collected for a specific user, the folder represents the
             * first day when the user uploaded data.
             */
            var userZips = FindUserZips();
            var ZipIterator = 0;
            foreach (var userZip in userZips)
            {
                ZipIterator += 1;
                Console.Write("\n#### processing user zip: {0} #####\n", userZip);

                // open the .zip file ...
                using (IReadingArchive ra = new ReadingArchive(Path.Combine(_eventsDir, userZip)))
                {
                    // ... and iterate over content.
                    while (ra.HasNext())
                    {
                        /*
                         * within the userZip, each stored event is contained as a
                         * single file that contains the Json representation of a
                         * subclass of IDEEvent.
                         */
                        try
                        {
                            var e = ra.GetNext<IDEEvent>();

                            // the events can then be processed individually
                            foreach (Process task in _tasks) task.process(e);                               
                        }
                        catch(System.InvalidOperationException e)
                        {

                            Console.WriteLine(e.Message);
                        }
                        catch (Newtonsoft.Json.JsonReaderException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("not tested error!" + e.Message);
                        }
                    }
                }

                //getThe result for this actual zip and stock it in the .txt Result file.
                foreach (Process task in _tasks) task.getResult(ZipIterator + " / " + userZips.Count);
            }
        }

        /*
             * will recursively search for all .zip files in the eventsDir. The paths
             * that are returned are relative to the eventsDir.
             */
        public ISet<string> FindUserZips()
        {
            var prefix = _eventsDir.EndsWith("\\") ? _eventsDir : _eventsDir + "\\";
            var zips = Directory.EnumerateFiles(_eventsDir, "*.zip", SearchOption.AllDirectories)
                .Select(f => f.Replace(prefix, ""));
            return Sets.NewHashSetFrom(zips);
        }
       
    }
}