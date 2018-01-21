# MSR2018

This is the C# project used to process and collect data from the Event provided to us.

In order to run it, you need to follow the next steps:

STEP 1 

Download or Clone the project on your device.
You should have a folder that contains some folders "KaVE.Examples.Commons" folder, "Results" folder, "Events" folder and files.
It's your root folder.

STEP 2

Download the dataset provided by MRS 2018 Mining challenge : http://www.kave.cc/datasets
In the "Interaction Data" part, the "Events (Mar 1, 2017)" dataset.
     
Unzip the "Events-170301.zip" in the the "Events" folder.
Now, you should have your "Events" folder that contains a lot of folder named by a date. These folders should contains some zip.
 
STEP 3

Check for the dependencies of this project.

Dependencies:
NuGet (C#):
    https://www.nuget.org/packages/KaVE.Commons
    



You can run the project!

  -In the RunMe.cs you can chose what process you want to run (by default, the process which will be run is "ChangeTestFailProcess", the process I used to do the statistical analyses).
  -The .txt output file will be stock in the "Results\RunUnitTest" folder.
  -You can read the "read-me" contains in the "Results" folder. it explains more the files and folder contains in the "Results" folder.

 
