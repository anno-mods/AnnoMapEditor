using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor
{
    public static class Extensions
    {
        public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (predicate.Invoke(item))
                    return index;
                index++;
            }

            return -1;
        }
    }

    internal class Utils
    {
        /// <summary>
        /// Delete directory, fallback to delete files only.
        /// Return false if at least one file couldn't be deleted.
        /// </summary>
        /// <param name="path"></param>
        public static bool TryDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch
            {
                return TryDeleteAllFiles(path);
            }
            return true;
        }

        private static bool TryDeleteAllFiles(string path)
        {
            bool successful = true;
            string[]? files = null;
            try
            {
                files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            }
            catch { }

            if (files is null)
                return false;

            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    successful = false;
                }
            }
            return successful;
        }
    }
}
