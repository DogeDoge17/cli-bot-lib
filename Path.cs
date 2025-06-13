using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace cli_bot
{
    public class Path
    {
        private readonly string _path;

        public static char GoodSep { get { return System.OperatingSystem.IsWindows() ? '\\' : '/'; } }
        public static char BadSep { get { return System.OperatingSystem.IsWindows() ? '/' : '\\'; } }

        public Path(string path)
        {
            if(GoodSep != '/')
                _path = path.Replace(BadSep, GoodSep).TrimStart('\\', '/').TrimEnd('\\', '/');
            else
                _path = path.Replace(BadSep, GoodSep).TrimEnd('\\', '/');
        }
        public Path()
        {
            _path = string.Empty;
        }

        public static implicit operator string(Path p) => p._path;


        public static implicit operator Path(string s) => new Path(s);


        public static Path operator /(Path left, Path right)
        {
            return new Path((left._path.TrimEnd('\\', '/') + GoodSep + right._path.TrimStart('\\', '/')).Replace(BadSep, GoodSep));        
        }

        public override string ToString()
        {
            return _path;
        }
        public Path? ParentPath
        {
            get
            {
                string? parent = GetDirectoryName(_path);
                return parent != null ? new Path(parent) : null;
            }
        }

        public Path? DirectoryPath
        {
            get
            {
                if (Extension == string.Empty)
                    return new(_path);

                string? parent = GetDirectoryName(_path);
                return parent != null ? new Path(parent) : null;
            }
        }

        public string Extension
        {
            get
            {
                return System.IO.Path.GetExtension(_path);
            }
        }

        public static Path Assembly
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public bool Contains(string path)
        {
            return _path.Contains(path);
        }

        public Path RelativeTo(Path basePath)
        {
            Uri baseUri = new Uri(basePath._path.EndsWith(GoodSep)
                                  ? basePath._path
                                  : basePath._path + GoodSep);
            Uri targetUri = new Uri(_path);
            Uri relativeUri = baseUri.MakeRelativeUri(targetUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
            return new Path(relativePath.Replace(BadSep, GoodSep));
        }

        public string FileName => GetFileName(_path);

        public static string? GetDirectoryName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            path = path.Trim();

            // Handle special cases for root or single-character paths.
            if (path.Length == 1 && (path == "\\" || path == "/"))
            {
                return null;
            }

            // Normalize slashes for consistency.
            path = path.Replace(BadSep, GoodSep);



            int lastSlashIndex = path.LastIndexOf(GoodSep);

            // If there are no slashes, the path has no directory component.
            if (lastSlashIndex == -1)
            {
                return null;
            }

            // If the last slash is at the beginning (e.g., "C:\"), handle it specially.
            if (lastSlashIndex == 0 || (lastSlashIndex == 2 && path[1] == ':'))
            {
                return path.Substring(0, lastSlashIndex + 1);
            }

            // Return the substring up to (but not including) the last slash.
            return path.Substring(0, lastSlashIndex);
        }

        public static string GetFileName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));
            }

            path = path.Trim();

            // Normalize slashes for consistency.
            path = path.Replace(BadSep, GoodSep);



            int lastSlashIndex = path.LastIndexOf(GoodSep);

            // If there are no slashes, the entire path is the file name.
            if (lastSlashIndex == -1)
            {
                return path;
            }

            // Return the substring after the last slash.
            return path.Substring(lastSlashIndex + 1);
        }

    }
}
