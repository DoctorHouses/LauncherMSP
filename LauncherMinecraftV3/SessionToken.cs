using System;

namespace LauncherMinecraftV3
{
    [Serializable]
    public class SessionToken
    {
        public string Id { get; set; }
        public string PlayerName { get; set; }
        public string PlayerId { get; set; }
        public string ClientId { get; set; }

        public SessionToken()
        {
            Id = string.Empty;
            PlayerName = string.Empty;
            PlayerId = string.Empty;
            ClientId = string.Empty;
        }
    }
}
