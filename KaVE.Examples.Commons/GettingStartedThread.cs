﻿/*
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
using KaVE.Commons.Utils.Concurrency;

/**
 * Simple example that shows how the dataset can be opened, all users identified,
 * and all contained events deserialized.
 */
namespace KaVE.Examples.Commons
{
    internal class GettingStartedThread
    {
        private readonly string _eventsDir;
        private Process[] _tasks;

        public GettingStartedThread(string eventsDir, Process[] tasks)
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
                Task[] taskThread = taskThread = new Task[0];
                int taskNum = 0;
                ZipIterator += 1;
                Console.Write("\n#### processing user zip: {0} #####\n", userZip);

                // open the .zip file ...
                using (IReadingArchive ra = new ReadingArchive(Path.Combine(_eventsDir, userZip)))
                {

                    // ... and iterate over content.
                    while (ra.HasNext())
                    {
                        taskThread = new Task[ra.Count * _tasks.Length];
                        /*
                         * within the userZip, each stored event is contained as a
                         * single file that contains the Json representation of a
                         * subclass of IDEEvent.
                         */
                        try
                        {

                            var e = ra.GetNext<IDEEvent>();
                            foreach (Process task in _tasks)
                            {
                                taskThread[taskNum] = (new Task(() => { task.process(e); }));
                                taskThread[taskNum].Start();
                                ++taskNum;
                            }

                            // the events can then be processed individually                              
                        }
                        catch (System.InvalidOperationException e)
                        {

                            Console.WriteLine("unknow error!" + e.Message);
                        }
                        catch (Newtonsoft.Json.JsonReaderException e)
                        {
                            Console.WriteLine("unknow error!" + e.Message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("not tested error!" + e.Message);
                        }
                    }
                }

                //Task.WaitAll(taskThread);

                if (ZipIterator == userZips.Count)
                {
                    foreach (Process task in _tasks) task.getResult(ZipIterator + " / " + userZips.Count, true);
                }
                else
                {
                    if (userZips.ElementAt(ZipIterator).Split('\\')[0] == userZips.ElementAt(ZipIterator - 1).Split('\\')[0])
                    {
                        foreach (Process task in _tasks) task.getResult(ZipIterator + " / " + userZips.Count, false);
                    }
                    else
                    {
                        foreach (Process task in _tasks) task.getResult(ZipIterator + " / " + userZips.Count, true);
                    }
                }
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

        /*
             * if you review the type hierarchy of IDEEvent, you will realize that
             * several subclasses exist that provide access to context information that
             * is specific to the event type.
             * 
             * To access the context, you should check for the runtime type of the event
             * and cast it accordingly.
             * 
             * As soon as I have some more time, I will implement the visitor pattern to
             * get rid of the casting. For now, this is recommended way to access the
             * contents.
             */


    }
}