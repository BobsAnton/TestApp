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
        const string const_err_no_arg = "Error! No arguments!";
        const string const_err_invalid_arg = "Error! Invalid argument!";
        const string const_err_secur_exept = "Error! Security exception.";
        const string const_folder_name = "Test";
        const string const_err_folder_empty = "Error! The " + const_folder_name + " folder is empty.";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(const_err_no_arg);
                return;
            }
            var arg = args[0].Split(DirectoryInfoExtension.const_separator);
            if (arg.Length != 2)
            {
                Console.WriteLine(const_err_invalid_arg);
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
                Console.WriteLine(const_err_invalid_arg);
                return;
            }

            if (dir.FullName.Equals(Path.Combine(Environment.ExpandEnvironmentVariables("%SystemDrive%"), const_folder_name), StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine(const_err_invalid_arg);
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
                    Console.WriteLine(const_err_secur_exept);
                    return;
                }
            }
          
            try
            {
                if (Directory.GetDirectories(dir.FullName).Count() == 0)
                {
                    Console.WriteLine(const_err_folder_empty);
                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(const_err_secur_exept);
                return;
            }

            var path = dir.TryExtractPath(mask);

            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine(const_err_invalid_arg);
                return;
            }
            else
            {
                Console.WriteLine(path);
            }
        }
    }
}
