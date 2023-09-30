using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xylOVRChat.Objects;

namespace xylOVRChat.Modules
{
    public class LocationNotifier
    {
        private static bool _initialized { get; set; } = false;

        private static string _notifyDatabaseLocation { get; set; } = $"Test.txt";
        private static List<LocationNotifierObject> _notificationSubjects { get; set; } = new List<LocationNotifierObject>();
        public static void RunOPJ(VRCEvents.OnPlayerJoined userData)
        {
            Initialize();

            foreach (var subject in _notificationSubjects)
            {
                if (subject.displayName == userData.displayName)
                {
                    if (subject.ShouldWebhook)
                    {
                        Console.WriteLine($"Found Marked Subject: {userData.displayName}");
                    }
                }
            }
        }

        public static void RunOPL(VRCEvents.OnPlayerLeft userData)
        {
            Initialize();

            foreach (var subject in _notificationSubjects)
            {
                if (subject.displayName == userData.displayName)
                {
                    if (subject.ShouldWebhook)
                    {
                        Console.WriteLine($"Lost Marked Subject: {userData.displayName}");
                    }
                }
            }
        }


        private static void Initialize()
        {
            if (_initialized)
                return;

            foreach (var line in File.ReadAllLines(_notifyDatabaseLocation).ToList())
            {

                _notificationSubjects.Add(new LocationNotifierObject() { displayName = line, ShouldWebhook = true, ShouldXSONotify = true });
            }

            _initialized = true;
        }

    }
}
