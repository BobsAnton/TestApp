using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestApp
{
    public static class DirectoryInfoExtension
    {
        /// <summary>
        /// Returns the full path to the directory, which coincides or almost coincides with the arg parameter, 
        /// or returns an empty string on failure.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="arg">Input parameter (example: C:\Test|41.1.109.2)</param>
        /// <returns>Path to the directory</returns>
        public static string GetPath(this DirectoryInfo dir, string arg)
        {
            var path = string.Empty;
            path = dir.TryExtractPath(arg, patternDate, '-');
            if (path == string.Empty)
            {
                path = dir.TryExtractPath(arg, patternVersion, '.');
            }
            return path;
        }


        /// <summary>
        /// Date format template.
        /// </summary>
        private const string patternDate = @"(\d{4})\-(0\d|1[012])\-([0-2]\d|3[01])";

        /// <summary>
        /// Version format template.
        /// </summary>
        private const string patternVersion = @"[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+";

        /// <summary>
        /// Extracts the directory path or an empty string on failure.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="arg">Input parameter (example: C:\Test|41.1.109.2)</param>
        /// <param name="pattern">Format template</param>
        /// <param name="split">Separator of the format</param>
        /// <returns>Path to the directory</returns>
        private static string TryExtractPath(this DirectoryInfo dir, string arg, string pattern, char split)
        {
            var patternPath = string.Format(@"^{0}\|{1}$", dir.FullName.Replace(@"\", @"\\"), pattern);
            if (Regex.IsMatch(arg, patternPath))
            {
                var matches = Regex.Matches(arg, patternPath);
                foreach (var march in matches)
                {
                    // Remove [C:\Test|] -> arg = 41.1.101.0
                    arg = march.ToString().Remove(0, dir.LenghtOfPrefix());
                    var foldersNames = Directory.GetDirectories(dir.FullName);
                    return (foldersNames.Where(fn => fn.Contains(arg)).Count() != 0) ?
                        foldersNames.Where(fn => fn.Contains(arg)).First() :
                        dir.Latest(arg, split);
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Looking for the most similar directory or the most latest directory.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="arg">Input parameter (example: 41.1.101.0)</param>
        /// <param name="split">Separator of the format</param>
        /// <returns>Path to the directory</returns>
        private static string Latest(this DirectoryInfo dir, string arg, char split)
        {
            var splitIndex = arg.Length - 1;
            var splitNumber = arg.Split(split).Count();
            for (int i = 0; i < splitNumber; i++)
            {
                splitIndex = arg.LastIndexOf(split, splitIndex - 1);

                var currentVersion = int.MinValue;
                try
                {
                    // Get the current version number.
                    currentVersion = (arg[arg.Length - 1] == '*') ?
                        int.Parse(arg.Remove(arg.Length - 2).Remove(0, splitIndex + 1)) :
                        int.Parse(arg.Remove(0, splitIndex + 1));
                }
                catch (Exception e)
                {
                    if (e is OverflowException || e is FormatException)
                    {
                        return string.Empty;
                    }
                    throw;
                }

                // Search for the similar folders.
                arg = arg.Remove(splitIndex + 1) + '*';
                var dirs = Directory.GetDirectories(dir.FullName, arg).Where(d => d.Contains(split)).ToArray();
                if (dirs.Count() == 0)
                {
                    // No similar folders.
                    // Expand the search range.
                    continue;
                }

                // Search for the previous version.
                var nameDir = dir.GetPrevVersion(dirs, arg, split, currentVersion);
                if (nameDir != string.Empty)
                {
                    // Success.
                    return string.Format(@"{0}\{1}", dir.FullName, nameDir);
                }
            }

            // Failure -> Return the path to the "latest" folder.
            if (dir.ForldersExist(patternDate))
            {
                arg = string.Format(@"{0}|{1}", dir.FullName, DateTime.MaxValue.ToString("yyyy-MM-dd"));
                return dir.GetPath(arg);
            }
            else if (dir.ForldersExist(patternVersion))
            {
                arg = string.Format(@"{0}|{1}", dir.FullName, string.Format("{0}.{1}.{2}.{3}", int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue));
                return dir.GetPath(arg);
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the name of the directory (no higher than the current version) 
        /// from the list of similar directories or an empty string on failure.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="similarDirs">List of similar catalogs</param>
        /// <param name="arg">Mask of search similar catalogs (example: 41.1.*)</param>
        /// <param name="split">Separator of the format</param>
        /// <param name="current">The original version number</param>
        /// <returns>If successful, it returns the name of the directory</returns>
        private static string GetPrevVersion(this DirectoryInfo dir, string[] similarDirs, string arg, char split, int current)
        {
            var next = int.MinValue;
            var prev = int.MinValue;
            var sub = string.Empty;

            // To save the zero in the version name (example: 2015-09-09) 
            var strPrev = string.Empty;

            foreach (var similarDir in similarDirs)
            {
                // temp = next + sub
                // Example: remove [C:\Test|41.1.*] -> temp = 103.0
                var temp = similarDir.Remove(0, dir.LenghtOfPrefix() + arg.Length - 1);

                try
                {
                    // next - the next number of the version in the cycle
                    // Example: remove [.0] -> next = 103
                    next = (temp.Contains(split)) ? int.Parse(temp.Remove(temp.IndexOf(split))) : int.Parse(temp);
                }
                catch (Exception e)
                {
                    if (e is OverflowException || e is FormatException)
                    {
                        continue;
                    }
                    throw;
                }

                if (next == prev)
                {
                    // Necessary to update the sub variable if next = prev
                    // Example: sub = .0
                    sub = DeleteNumPrefix(split, temp);
                }

                if (next < current && next > prev)
                {
                    // prev - the number of the version that need to find
                    prev = next;
                    // To save the zero in the version name (example: 2015-09-09) 
                    strPrev = (temp.Contains(split)) ? temp.Remove(temp.IndexOf(split)) : temp;
                    // Necessary to update the sub
                    sub = DeleteNumPrefix(split, temp);
                }
            }

            if (prev != int.MinValue)
            {
                // Success.
                // Return the full name of the found directory.
                // Example: prefix = 41.1., prev = 104, sub = .1
                var prefix = arg.Remove(arg.Length - 1);
                return prefix + strPrev + sub;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Deletes all characters from a string to a separator character (example: 103.0 -> .0)
        /// </summary>
        /// <param name="split">Separator of the format</param>
        /// <param name="str">Input string</param>
        /// <returns></returns>
        private static string DeleteNumPrefix(char split, string str)
        {
            return (str.Contains(split)) ? str.Remove(0, str.IndexOf(split)) : "";
        }

        /// <summary>
        /// Returns the number of prefix characters (example: C:\Test| -> 8)
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <returns>The number of prefix characters</returns>
        private static int LenghtOfPrefix(this DirectoryInfo dir)
        {
            return (dir.FullName + "|").Count();
        }

        /// <summary>
        /// Checks for the existence of the directories that satisfy to the template.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="pattern">The search template</param>
        /// <returns>true - directories exist, false - do not exist</returns>
        private static bool ForldersExist(this DirectoryInfo dir, string pattern)
        {
            var result = false;
            var p = string.Format(@"^{0}\\{1}$", dir.FullName.Replace(@"\", @"\\"), pattern);
            var dirs = Directory.GetDirectories(dir.FullName);
            foreach (var subdir in dirs)
            {
                if (Regex.IsMatch(subdir, p))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}
