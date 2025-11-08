using System;
using System.Collections.Generic;
using System.Linq;

namespace fluxel;

public static class GlobalStatistics
{
    public static int Online => ServerHost.Instance.OnlineStates?.AllOnline.Length ?? 0;

    public static IEnumerable<long> OnlineUsers
    {
        get
        {
            var ids = ServerHost.Instance.OnlineStates?.AllOnline ?? Array.Empty<long>();
            return ids.Append(0);
        }
    }
}
