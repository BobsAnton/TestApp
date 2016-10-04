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
            if (args.Length == 0)
            {
                Console.WriteLine("Error! No arguments!");
                return;
            }
            var arg = args[0].Split('|');
            if (arg.Length != 2)
            {
                Console.WriteLine("Error! Invalid argument!");
                return;
            }

            DirectoryInfo dir;
            var mask = arg[1];
            try
            {
                // The DirectoryInfo object for %SystemDrive%\Test folder.
                dir = new DirectoryInfo(arg[0]);
            }
            catch (Exception)
            {
                Console.WriteLine("Error! Invalid argument!");
                return;
            }

            if (dir.FullName != Environment.ExpandEnvironmentVariables("%SystemDrive%") + @"\Test")
            {
                Console.WriteLine("Error! Invalid argument!");
                return;
            }

            // The existence check of the %SystemDrive%/Test folder
            if (!Directory.Exists(dir.FullName))
            {
                try
                {
                    // Create test folders inside %SystemDrive%/Test
                    dir.CreateSubdirectory(@"2015-09-15");
                    dir.CreateSubdirectory(@"2015-09-20");
                    dir.CreateSubdirectory(@"2015-10-10");
                    dir.CreateSubdirectory(@"2015-12-13");
                    dir.CreateSubdirectory(@"2016-05-17");
                    dir.CreateSubdirectory(@"41.1.102.0");
                    dir.CreateSubdirectory(@"41.1.103.0");
                    dir.CreateSubdirectory(@"41.1.104.0");
                    dir.CreateSubdirectory(@"41.1.104.1");
                    dir.CreateSubdirectory(@"41.1.105.0");
                }
                catch (System.Security.SecurityException)
                {
                    Console.WriteLine("Error! Security exception.");
                    return;
                }
            }
          
            try
            {
                if (Directory.GetDirectories(dir.FullName).Count() == 0)
                {
                    Console.WriteLine("The {0} folder is empty.", dir.FullName);
                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Error! Security exception.");
                return;
            }

            var path = dir.TryExtractPath(mask);

            if (path == string.Empty)
            {
                Console.WriteLine("Error! Invalid argument!");
                return;
            }
            else
            {
                Console.WriteLine(path);
            }
        }
    }
}
