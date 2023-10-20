using Newtonsoft.Json;
using System.Runtime.Serialization;
using VRChat.API.Model;
using xylOVRChat.Objects;
using xylOVRChat.SDK;

namespace xylOVRChat.Modules
{
    public class LocationNotifier
    {
        public static List<LocationNotifierObject.Player> WatchedPlayers { get; set; } = new List<LocationNotifierObject.Player>();
        private static string _watchedPlayersPath { get; set; } = "LocationNotifierData.json";
        public static void Start()
        {
            UpdateWatchedPlayers();
        }

        public static LocationNotifierObject.Player? RunChecks(string username)
        {
            foreach (var player in WatchedPlayers)
            {
                if (player.username == username)
                {
                    LimitedUser profile = VRChatApplication.VRAPISystem.GetUserByName(username);
                    if (profile != null)
                    {
                        player.limitedProfile = profile;
                        // TODO: Implement webhook notifications etc - xenosia


                        return player;
                    }
                }
            }
            return null;
        }

        public static void UpdateWatchedPlayers()
        {
            if (!System.IO.File.Exists(_watchedPlayersPath))
                return;

            using (StringReader stringReader = new StringReader(System.IO.File.ReadAllText(_watchedPlayersPath)))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
                {
                    WatchedPlayers = new JsonSerializer().Deserialize<List<LocationNotifierObject.Player>>(jsonReader);
                }
            }
        }

        public static void AddPlayer(LocationNotifierObject.Player player)
        {
            WatchedPlayers.Add(player);

            using (StreamWriter streamWriter = new StreamWriter(_watchedPlayersPath))
            {
                new JsonSerializer().Serialize(streamWriter, WatchedPlayers);
            }

            UpdateWatchedPlayers();
        }
        
    }
}
