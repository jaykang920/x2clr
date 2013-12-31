// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

namespace x2
{
    // Work-around for missing Action<> delegates in .NET 2.0
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}
