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
        public const char const_separator = '|';
        const string const_latest = "Latest";

        /// <summary>
        /// Returns the full path to the directory, which coincides or almost coincides with the mask parameter, 
        /// or returns an empty string on failure.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="mask">example: C:\Test|41.1.109.2 or C:\Test|2016-05-17</param>
        /// <returns>Path to the directory</returns>
        public static string TryExtractPath(this DirectoryInfo dir, string mask)
        {
            List<DateTime> dates;
            List<Version> versions;
            dir.GetLists(out dates, out versions);

            var path = string.Empty;
            
            DateTime date; 
            if (DateTime.TryParse(mask, out date))
            {
                path = dir.GetPath(date, dates);
                if (string.IsNullOrEmpty(path)) mask = const_latest;
            }
            else
            {
                Version version;
                if (Version.TryParse(mask, out version))
                {
                    path = dir.GetPath(version, versions);
                    if (string.IsNullOrEmpty(path)) mask = const_latest;
                }
            }

            if (mask.Equals(const_latest)) path = dir.Latest(dates, versions);
            return path;
        }

        /// <summary>
        /// Tries to convert folder names in the directory "dir" to the instances of DateTime or Version.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="dates">List of the instances of the DateTime class</param>
        /// <param name="versions">List of the instances of the Version class</param>
        private static void GetLists(this DirectoryInfo dir, out List<DateTime> dates, out List<Version> versions)
        {
            dates = new List<DateTime>();
            versions = new List<Version>();
            var foldersNames = Directory.GetDirectories(dir.FullName).Select(fn => dir.RemovePrefix(fn));
            foreach (var foldersName in foldersNames)
            {
                DateTime date;
                if (DateTime.TryParse(foldersName, out date))
                {
                    dates.Add(date);
                }
                else
                {
                    Version version;
                    if (Version.TryParse(foldersName, out version)) versions.Add(version);
                }
            }
        }

        /// <summary>
        /// Returns the full path to the directory, which coincides or almost coincides with the date parameter, 
        /// or returns an empty string on failure.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="date">The instance of the DateTime class</param>
        /// <param name="dates">List of the instances of the DateTime class</param>
        /// <returns>Path to the directory</returns>
        private static string GetPath(this DirectoryInfo dir, DateTime date, List<DateTime> dates)
        {
            var matches = Directory.GetDirectories(dir.FullName).Where(fn => date.CompareWith(dir.RemovePrefix(fn)));
            if (matches.Count() != 0) return matches.First();

            if (dates.Count != 0 && dates.Where(d => d < date).OrderByDescending(d => d).Count() != 0)
            {
                var prevDate = dates.Where(d => d < date).OrderByDescending(d => d).First();
                matches = Directory.GetDirectories(dir.FullName).Where(fn => prevDate.CompareWith(dir.RemovePrefix(fn)));
                if (matches.Count() != 0) return matches.First();
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns the full path to the directory, which coincides or almost coincides with the version parameter, 
        /// or returns an empty string on failure.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="version">The instance of the Version class</param>
        /// <param name="versions">List of the instances of the Version class</param>
        /// <returns>Path to the directory</returns>
        private static string GetPath(this DirectoryInfo dir, Version version, List<Version> versions)
        {
            var matches = Directory.GetDirectories(dir.FullName).Where(fn => version.CompareWith(dir.RemovePrefix(fn)));
            if (matches.Count() != 0) return matches.First();

            if (versions.Count != 0 && versions.Where(v => v < version).OrderByDescending(v => v).Count() != 0)
            {
                var prevVersion = versions.Where(v => v < version).OrderByDescending(v => v).First();
                matches = Directory.GetDirectories(dir.FullName).Where(fn => prevVersion.CompareWith(dir.RemovePrefix(fn)));
                if (matches.Count() != 0) return matches.First();
            }
            return string.Empty;
        }

        /// <summary>
        /// Compares the instance of the DateTime with the string.
        /// </summary>
        /// <param name="date">The instance of the DateTime class</param>
        /// <param name="foldersName">String in the DateTime format (example: 2016-05-18)</param>
        /// <returns></returns>
        private static bool CompareWith(this DateTime date, string foldersName)
        {
            DateTime nextDate;
            if (DateTime.TryParse(foldersName, out nextDate) && nextDate.Equals(date))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Compares the instance of the Version with the string.
        /// </summary>
        /// <param name="version">The instance of the Version class</param>
        /// <param name="foldersName">String in the Version format (example: 41.1.105.0)</param>
        /// <returns></returns>
        private static bool CompareWith(this Version version, string foldersName)
        {
            Version nextVersion;
            if (Version.TryParse(foldersName, out nextVersion) && nextVersion.Equals(version))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the full path to the most latest directory.
        /// </summary>
        /// <param name="dir">The instance of the DirectoryInfo class</param>
        /// <param name="dates">List of the instances of the DateTime class</param>
        /// <param name="versions">List of the instances of the Version class</param>
        /// <returns>Path to the directory</returns>
        private static string Latest(this DirectoryInfo dir, List<DateTime> dates, List<Version> versions)
        {
            if (dates.Count != 0) return dir.GetPath(DateTime.MaxValue, dates);
            if (versions.Count != 0) return dir.GetPath(new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue), versions);
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
            return arg.Remove(0, (dir.FullName + const_separator).Count());
        }
    }
}
