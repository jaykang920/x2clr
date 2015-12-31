﻿// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;

namespace xpiler
{
    class Xpiler
    {
        private static Dictionary<string, Handler> handlers;
        private static Dictionary<string, Formatter> formatters;
        private static Options options;

        private bool error;
        private Formatter formatter;
        private Stack<string> subDirs;

        public static Dictionary<string, Formatter> Formatters
        {
            get { return formatters; }
        }

        public static Options Options
        {
            get { return options; }
        }

        public bool Error
        {
            get { return error; }
        }

        static Xpiler()
        {
            options = new Options();

            handlers = new Dictionary<string, Handler>();
            handlers.Add(".xml", new XmlHandler());

            formatters = new Dictionary<string, Formatter>();
            formatters.Add("cs", new CSharpFormatter());
        }

        public Xpiler()
        {
            formatter = formatters[options.Spec];
            subDirs = new Stack<string>();
        }

        public void Process(string path)
        {
            if (Directory.Exists(path))
            {
                ProcessDir(path);
            }
            else if (File.Exists(path))
            {
                ProcessFile(path);
            }
            else
            {
                Console.Error.WriteLine("{0} doesn't exist.", path);
                error = true;
            }
        }

        private void ProcessDir(string path)
        {
            Console.WriteLine("Directory {0}", Path.GetFullPath(path));
            var di = new DirectoryInfo(path);
            var entries = di.GetFileSystemInfos();
            foreach (var entry in entries)
            {
                var pathname = Path.Combine(path, entry.Name);
                if ((entry.Attributes & FileAttributes.Directory) != 0)
                {
                    if (options.Recursive)
                    {
                        subDirs.Push(entry.Name);
                        ProcessDir(pathname);
                        subDirs.Pop();
                    }
                }
                else
                {
                    ProcessFile(pathname);
                }
            }
        }

        private void ProcessFile(string path)
        {
            var filename = Path.GetFileName(path);
            var extension = Path.GetExtension(path);
            string outDir;
            if (options.OutDir == null)
            {
                outDir = Path.GetDirectoryName(path);
            }
            else
            {
                outDir = Path.Combine(options.OutDir, String.Join(
                    Path.DirectorySeparatorChar.ToString(), subDirs.ToArray()));
            }
            Handler handler;
            if (handlers.TryGetValue(extension.ToLower(), out handler) == false ||
                (!options.Forced && formatter.IsUpToDate(path, outDir)))
            {
                return;
            }

            Console.WriteLine(filename);

            Document doc;
            if (handler.Handle(path, out doc) == false)
            {
                error = true;
            }
            if (error == true || doc == null)
            {
                return;
            }

            doc.BaseName = Path.GetFileNameWithoutExtension(path);

            if (!String.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            if (formatter.Format(doc, outDir) == false)
            {
                error = true;
            }
        }
    }
}
