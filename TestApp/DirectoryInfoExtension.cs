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
        private static Version maxVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
        private static Version minVersion = new Version(0, 0, 0, 0);

        /// <summary>
        /// Returns the full path to the directory, which coincides or almost coincides with the arg parameter, 
        /// or returns an empty string on failure.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="arg">Input parameter (example: C:\Test|41.1.109.2)</param>
        /// <returns>Path to the directory</returns>
        public static string GetPath(this DirectoryInfo dir, string arg)
        {
            // Check arg.
            if (!arg.Contains(dir.FullName + "|")) return string.Empty;
            // Remove [C:\Test|] -> arg = 41.1.101.0
            arg = dir.RemovePrefix(arg);
            if (arg.Length == 0) return string.Empty;

            // Search for the path to the folder.
            var foldersNames = Directory.GetDirectories(dir.FullName);
            if (foldersNames.Where(fn => fn.Contains(arg)).Count() != 0)
                return foldersNames.Where(fn => fn.Contains(arg)).First();

            // Search for the path to the "latest" folder.
            var obj = dir.ExtractObject(arg);
            return (obj != null) ? dir.Latest(obj) : string.Empty;
        }


        /// <summary>
        /// Looking for the most similar directory or the most latest directory.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="obj">The instance of the DateTime or Version class</param>
        /// <returns>Path to the directory</returns>
        private static string Latest(this DirectoryInfo dir, object obj)
        {
            var mask = (obj is DateTime?) ?
                ((DateTime)obj).ToString("yyyy-MM-dd") :
                ((Version)obj).ToString();

            var split = (obj is DateTime?) ? '-' : '.';

            var splitIndex = mask.Length - 1;
            var splitNumber = mask.Split(split).Count();
            for (int i = 0; i < splitNumber; i++)
            {
                splitIndex = mask.LastIndexOf(split, splitIndex - 1);

                // Search for the similar folders.
                mask = mask.Remove(splitIndex + 1) + '*';
                var dirs = Directory.GetDirectories(dir.FullName, mask).Where(d => d.Contains(split)).ToArray();
                if (dirs.Count() == 0)
                {
                    // No similar folders.
                    // Expand the search range.
                    continue;
                }

                // Search for the previous version.
                var nameDir = dir.GetPrevVersion(dirs, obj);
                if (nameDir != string.Empty)
                {
                    // Success.
                    return string.Format(@"{0}\{1}", dir.FullName, nameDir);
                }
            }

            // Failure -> Return the path to the "latest" folder.
            if (obj is DateTime)
            {
                return ((DateTime)obj != DateTime.MaxValue) ? dir.Latest(DateTime.MaxValue) : dir.Latest(maxVersion);          
            }
            if (obj is Version)
            {
                return ((Version)obj != maxVersion) ? dir.Latest(maxVersion) : dir.Latest(DateTime.MaxValue);
            }                      
            return string.Empty;
        }

        /// <summary>
        /// Returns the name of the directory (no higher than the current version) 
        /// from the list of similar directories or an empty string on failure.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="similarDirs">List of similar catalogs</param>
        /// <param name="obj">The instance of the DateTime or Version class</param>
        /// <returns>If successful, it returns the name of the directory</returns>
        private static string GetPrevVersion(this DirectoryInfo dir, string[] similarDirs, object obj)
        {
            // DateTime.
            if (obj is DateTime)
            {
                var currentDate = (DateTime)obj;
                var prevDate = DateTime.MinValue;

                foreach (var sdir in similarDirs)
                {
                    var date = dir.ExtractObject(dir.RemovePrefix(sdir));
                    if (date == null) continue;
                    var nextDate = (DateTime)date;
                    if (nextDate < currentDate && nextDate > prevDate)
                    {
                        prevDate = nextDate;
                    }
                }

                if (prevDate != DateTime.MinValue)
                {
                    return prevDate.ToString("yyyy-MM-dd");
                }               
            }
            // Version.
            else
            {
                var currentVersion = (Version)obj;
                var prevVersion = minVersion;

                foreach (var sdir in similarDirs)
                {
                    var version = dir.ExtractObject(dir.RemovePrefix(sdir));
                    if (version == null) continue;
                    var nextVersion = (Version)version;
                    if (nextVersion < currentVersion && nextVersion > prevVersion)
                    {
                        prevVersion = nextVersion;
                    }
                }

                if (prevVersion != minVersion)
                {
                    return prevVersion.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Removes the prefix characters (example: C:\Test|2016-05-18 -> 2016-05-18)
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="arg">example: C:\Test|2016-05-18</param>
        /// <returns>example: 2016-05-18</returns>
        private static string RemovePrefix(this DirectoryInfo dir, string arg)
        {
            return arg.Remove(0, (dir.FullName + "|").Count());
        }

        /// <summary>
        /// Attempts to create an instance of the DateTime or Version class from a string.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="arg">A string in DateTime format or Version format</param>
        /// <returns>The instance of the DateTime or Version class or null on failure</returns>
        private static object ExtractObject(this DirectoryInfo dir, string arg)
        {
            object obj = null;
            try
            {
                obj = DateTime.Parse(arg);
            }
            catch (FormatException) { }

            try
            {
                obj = Version.Parse(arg);
            }
            catch (Exception) { }

            return obj;
        }
    }
}
