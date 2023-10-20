using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xylOVRChat.Objects
{
    public class VRC_Events
    {
        public class OnPlayerJoined
        {
            public DateTime dateTime { get; set; } = DateTime.Now;
            public string displayName { get; set; } = string.Empty;
        }

        public class OnPlayerLeft
        {
            public DateTime dateTime { get; set; } = DateTime.Now;
            public string displayName { get; set; } = string.Empty;
        }

        public class OnAvatarModeration
        {
            public DateTime dateTime { get; set; } = DateTime.Now;
            public string displayName { get; set; } = string.Empty;
            public ModerationType moderationType { get; set; } = ModerationType.Safety;
            public enum ModerationType
            {
                Shown = 0,
                Hidden = 1,
                Safety = 2
            }
        }

        public class OnVoiceModeration
        {
            public DateTime dateTime { get; set; } = DateTime.Now;
            public string displayName { get; set; } = string.Empty;
            public ModerationType moderationType { get; set; } = ModerationType.Unmuted;
            public enum ModerationType
            {
                Unmuted = 0,
                Muted = 1
            }
        }
    }
}
