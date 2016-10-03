using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // %SystemDrive%
            var systemDrive = Environment.ExpandEnvironmentVariables("%SystemDrive%") + @"\";

            const string testFolderName = @"Test";
            // The existence check of the %SystemDrive%/Test folder
            if (!Directory.Exists(systemDrive + testFolderName))
            {
                try
                {
                    // Create the %SystemDrive%/Test folder
                    var systemDriveFolder = new DirectoryInfo(systemDrive);
                    var testFolder = systemDriveFolder.CreateSubdirectory("Test");

                    // Create test folders inside %SystemDrive%/Test
                    testFolder.CreateSubdirectory(@"2015-09-15");
                    testFolder.CreateSubdirectory(@"2015-09-20");
                    testFolder.CreateSubdirectory(@"2015-10-10");
                    testFolder.CreateSubdirectory(@"2015-12-13");
                    testFolder.CreateSubdirectory(@"2016-05-17");
                    testFolder.CreateSubdirectory(@"41.1.102.0");
                    testFolder.CreateSubdirectory(@"41.1.103.0");
                    testFolder.CreateSubdirectory(@"41.1.104.0");
                    testFolder.CreateSubdirectory(@"41.1.104.1");
                    testFolder.CreateSubdirectory(@"41.1.105.0");
                }
                catch (System.Security.SecurityException)
                {
                    Console.WriteLine("Error! Security exception.");
                    return;
                }
            }

            // The DirectoryInfo object for %SystemDrive%\Test folder.
            var dir = new DirectoryInfo(systemDrive + testFolderName);
        }
    }
}
