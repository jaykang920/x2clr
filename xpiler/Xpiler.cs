// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.IO;

namespace xpiler {
  class Xpiler {
    private static readonly Dictionary<string, IHandler> handlers;
    private static readonly Dictionary<string, IFormatter> formatters;

    private readonly Options options;
    private readonly IFormatter formatter;
    private readonly Stack<string> subDirs;
    private bool error;

    public static Dictionary<string, IFormatter> Formatters {
      get { return formatters; }
    }

    public Options Options {
      get { return options; }
    }

    public bool Error {
      get { return error; }
    }

    static Xpiler() {
      handlers = new Dictionary<string, IHandler>();
      handlers.Add(".xml", new XmlHandler());

      formatters = new Dictionary<string, IFormatter>();
      formatters.Add("cs", new CSharpFormatter());
    }

    public Xpiler(Options options) {
      this.options = options;
      formatter = formatters[options.Spec];
      subDirs = new Stack<string>();
      error = false;
    }

    public void Process(string path) {
      if (Directory.Exists(path)) {
        ProcessDir(path);
      } else if (File.Exists(path)) {
        ProcessFile(path);
      } else {
        Console.Error.WriteLine("{0} doesn't exist.", path);
        error = true;
      }
    }

    private void ProcessDir(string path) {
      Console.WriteLine("Directory {0}", Path.GetFullPath(path));
      DirectoryInfo di = new DirectoryInfo(path);
      FileSystemInfo[] entries = di.GetFileSystemInfos();
      foreach (FileSystemInfo entry in entries) {
        string pathname = Path.Combine(path, entry.Name);
        if ((entry.Attributes & FileAttributes.Directory) != 0) {
          if (options.Recursive) {
            subDirs.Push(entry.Name);
            ProcessDir(pathname);
            subDirs.Pop();
          }
        } else {
          ProcessFile(pathname);
        }
      }
    }

    private void ProcessFile(string path) {
      string filename = Path.GetFileName(path);
      string extension = Path.GetExtension(path);
      string outDir;
      if (options.OutDir == null) {
        outDir = Path.GetDirectoryName(path);
      } else {
        outDir = Path.Combine(options.OutDir, String.Join(
            Path.DirectorySeparatorChar.ToString(), subDirs.ToArray()));
      }
      IHandler handler;
      if (handlers.TryGetValue(extension.ToLower(), out handler) == false ||
          (!options.Force && formatter.IsUpToDate(path, outDir))) {
        return;
      }

      Console.WriteLine(filename);

      Document doc;
      if (handler.Handle(path, out doc) == false) {
        error = true;
      }
      if (error == true || doc == null) {
        return;
      }
      doc.Path = path;
      doc.OutDir = outDir;
      if (!Directory.Exists(outDir)) {
        Directory.CreateDirectory(outDir);
      }
      if (formatter.Format(doc) == false) {
        error = true;
      }
    }
  }
}
