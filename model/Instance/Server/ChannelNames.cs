using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class ChannelNames
    {
        public static string GetClientsChannel()
        {
            return "Clients";
        }

        public static string GetMasterServerChannel()
        {
            return "Master";
        }

        public static string GetGameServerChannel(int id)
        {
            return string.Format("GS{0}", id);
        }
    }
}
