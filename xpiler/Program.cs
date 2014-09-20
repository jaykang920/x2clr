// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace xpiler
{
    class Program
    {
        public static int Main(string[] args)
        {
            var index = Xpiler.Options.Parse(args);
            if (index >= args.Length)
            {
                Console.WriteLine("error: at least one input path is required");
                return 2;
            }

            Xpiler xpiler = new Xpiler();
            while (index < args.Length)
            {
                xpiler.Process(args[index++]);
            }
            return (xpiler.Error ? 1 : 0);
        }
    }
}
