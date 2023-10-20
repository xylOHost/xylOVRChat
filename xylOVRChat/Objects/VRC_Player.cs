using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xylOVRChat.Modules;

namespace xylOVRChat.Objects
{
    public class VRC_Player
    {
        public string username { get; set; } = string.Empty;
        public bool isMuted { get; set; } = false;
        public VRC_Events.OnAvatarModeration.ModerationType avatarState { get; set; } = VRC_Events.OnAvatarModeration.ModerationType.Safety;
        public string note { get; set; } = string.Empty;
        public bool isKnownPlayer { get; set; } = false;

        public LocationNotifierObject.Player? locationNotifierProfile { get; set; } = null;

        public void CheckKnownPlayer()
        {
            var playerData = LocationNotifier.RunChecks(this.username);
            if (playerData != null)
            {
                isKnownPlayer = true;
                locationNotifierProfile = playerData;
            }
        }
    }
}
