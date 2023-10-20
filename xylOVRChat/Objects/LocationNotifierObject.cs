using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRChat.API.Model;

namespace xylOVRChat.Objects
{
    public class LocationNotifierObject
    {
        public class Player
        {
            public string userId { get; set; } = string.Empty;
            public string username { get; set; } = string.Empty;
            public string watchReason { get; set; } = string.Empty;
            public string webhook { get; set; } = string.Empty;
            public List<Tag> userTags { get; set; } = new List<Tag>() { Tag.None };
            public LimitedUser? limitedProfile { get; set; } = null;

            public enum Tag
            {
                None = 0,
                PersonOfInterest = 1,
                Peadophile = 2,
                Crasher = 3,
                Minor = 4,
                Friend = 5,
                Favorite = 6,
                COS_BOS = 7
            }
        }
    }
}
