using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xylOVRChat.SDK;

namespace xylOVRChat.Modules
{
    public class TestModule
    {
        public static void RunGetUserData(string displayName)
        {
            Thread.Sleep(1000);
           var user = APISystem.GetUserByName(displayName);

            Console.WriteLine($"{user.Id}:{displayName}:{user.CurrentAvatarImageUrl}");
        }
    }
}
