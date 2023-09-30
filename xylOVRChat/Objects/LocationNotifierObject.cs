using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xylOVRChat.Objects
{
    public class LocationNotifierObject
    {
        public string displayName { get; set; } = string.Empty;

        public bool ShouldWebhook { get; set;  } = false;
        public bool ShouldXSONotify { get; set; } = false;
        
    }
}
