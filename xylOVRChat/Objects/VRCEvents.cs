using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xylOVRChat.Objects
{
    public class VRCEvents
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
                Shown,
                Hidden,
                Safety
            }
        }
        public class OnVoiceModeration
        {
            public DateTime dateTime { get; set; } = DateTime.Now;
            public string displayName { get; set; } = string.Empty;
            public ModerationType moderationType { get; set; } = ModerationType.Unmuted;
            public enum ModerationType
            {
                Unmuted,
                Muted
                
            }
        }
    }
}
