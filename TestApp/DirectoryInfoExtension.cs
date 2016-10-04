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
                if (path == string.Empty) mask = "Latest";
            }
            else
            {
                Version version;
                if (Version.TryParse(mask, out version))
                {
                    path = dir.GetPath(version, versions);
                    if (path == string.Empty) mask = "Latest";
                }
            }

            if (mask == "Latest") path = dir.Latest(dates, versions);
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
            var foldersNames = Directory.GetDirectories(dir.FullName);
            foreach (var foldersName in foldersNames.Select(fn => fn.Remove(0, (dir.FullName + '|').Count())))
            {
                if (foldersName.Contains('-'))
                {
                    DateTime date;
                    if (DateTime.TryParse(foldersName, out date)) dates.Add(date);
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
            if (dates.Contains(date)) return string.Format(@"{0}|{1}", dir.FullName, date.ToString("yyyy-MM-dd"));
            if (dates.Count != 0 && dates.Where(d => d < date).OrderByDescending(d => d.Date).Count() != 0)
                return string.Format(@"{0}|{1}", dir.FullName, dates.Where(d => d < date).OrderByDescending(d => d).First().ToString("yyyy-MM-dd"));
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
            if (versions.Contains(version)) return string.Format(@"{0}|{1}", dir.FullName, version);
            if (versions.Count() != 0 && versions.Where(v => v < version).OrderByDescending(v => v).Count() != 0)
                return string.Format(@"{0}|{1}", dir.FullName, versions.Where(v => v < version).OrderByDescending(v => v).First());
            return string.Empty;
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
    }
}
