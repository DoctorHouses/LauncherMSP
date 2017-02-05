using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherMinecraftV3
{
    [Serializable]
    public class SessionToken
    {
        public string ID { get; set; }
        public string PlayerName { get; set; }
        public string PlayerID { get; set; }
        public string ClientID { get; set; }

        public SessionToken()
        {
            ID = String.Empty;
            PlayerName = String.Empty;
            PlayerID = String.Empty;
            ClientID = String.Empty;
        }
    }
}
