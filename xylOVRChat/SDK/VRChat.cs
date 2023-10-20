using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;
using xylOVRChat.Modules;
using xylOVRChat.Objects;

namespace xylOVRChat.SDK
{
    public static class VRChatApplication
    {
        public static APISystem VRAPISystem { get; set; } = new APISystem();
        public static EventSystem VREventSystem { get; set; } = new EventSystem();

        public static List<string> PlayerList { get; set; } = new List<string>();

        public static void Initialize()
        {
            VRAPISystem.Start();
            VREventSystem.Start();

            VREventSystem.OnPlayerJoined += VREventSystem_OnPlayerJoined;
            VREventSystem.OnPlayerLeft += VREventSystem_OnPlayerLeft;
            VREventSystem.OnPlayerAvatarModeration += VREventSystem_OnPlayerAvatarModeration;
            VREventSystem.OnPlayerVoiceModeration += VREventSystem_OnPlayerVoiceModeration;
        }

        private static void VREventSystem_OnPlayerVoiceModeration(object? sender, VRC_Events.OnVoiceModeration e)
        {
            switch (e.moderationType)
            {
                case VRC_Events.OnVoiceModeration.ModerationType.Unmuted:
                    Console.WriteLine(e.displayName + " was unmuted");
                    break;
                case VRC_Events.OnVoiceModeration.ModerationType.Muted:
                    Console.WriteLine(e.displayName + " was muted");
                    break;
            }
        }

        private static void VREventSystem_OnPlayerAvatarModeration(object? sender, VRC_Events.OnAvatarModeration e)
        {
            switch (e.moderationType)
            {

                case VRC_Events.OnAvatarModeration.ModerationType.Safety:
                    Console.WriteLine(e.displayName + "'s avatar is now on safety settings");
                    break;
                case VRC_Events.OnAvatarModeration.ModerationType.Shown:
                    Console.WriteLine(e.displayName + "'s avatar is now fully shown");
                    break;
                case VRC_Events.OnAvatarModeration.ModerationType.Hidden:
                    Console.WriteLine(e.displayName + "'s avatar is now blocked");
                    break;
            }
        }

        private static void VREventSystem_OnPlayerJoined(object? sender, VRC_Events.OnPlayerJoined e)
        {
            if (!PlayerList.Contains(e.displayName))
                PlayerList.Add(e.displayName);
        }

        private static void VREventSystem_OnPlayerLeft(object? sender, VRC_Events.OnPlayerLeft e)
        {
            if (PlayerList.Contains(e.displayName))
                    PlayerList.Remove(e.displayName);
        }
    }

    public class EventSystem
    {
        private static bool _disableThreads { get; set; } = false;
        private static string _logDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow") + @"/VRChat/VRChat";
        private static FileInfo? _logFile { get; set; } = null;
        private static long _logFileCurrentLength { get; set; } = 0;


        public event EventHandler<VRC_Events.OnPlayerJoined> OnPlayerJoined = null!;
        public event EventHandler<VRC_Events.OnPlayerLeft> OnPlayerLeft = null!;
        public event EventHandler<VRC_Events.OnAvatarModeration> OnPlayerAvatarModeration = null!;
        public event EventHandler<VRC_Events.OnVoiceModeration> OnPlayerVoiceModeration = null!;

        public void Start()
        {
            _logFile = GetLogFile();

            using (FileStream fileStream = new FileStream(_logFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                _logFileCurrentLength = fileStream.Length;

            new Thread(MonitorLogFile).Start();
        }
         
        public void MonitorLogFile()
        {
            string currentFileContent = string.Empty;
            string previousFileContent = "previous content";
            while (!_disableThreads)
            {
                try
                {


                    using (FileStream fileStream = new FileStream(_logFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        fileStream.Seek(_logFileCurrentLength - 1, SeekOrigin.Begin);

                        using (StreamReader streamReader = new StreamReader(fileStream, Encoding.Default))
                        {
                            currentFileContent = streamReader.ReadToEnd();
                        }



                        if (currentFileContent != previousFileContent)
                        {
                            foreach (var line in currentFileContent.Replace(previousFileContent, "").Split('\n'))
                            {
                                Console.WriteLine(line);

                                if (line.Contains("OnPlayerJoined"))
                                {
                                    string displayName = Regex.Match(line, @"OnPlayerJoined (.+)").Groups[1].Value;
                                    if (displayName == string.Empty)
                                        continue;

                                    OnPlayerJoined?.Invoke(this, new VRC_Events.OnPlayerJoined() { dateTime = DateTime.Now, displayName = displayName });
                                }
                                if (line.Contains("OnPlayerLeft")) 
                                {

                                    string displayName = Regex.Match(line, @"OnPlayerLeft (.+)").Groups[1].Value;
                                    if (displayName == string.Empty)
                                        continue;

                                    OnPlayerLeft?.Invoke(this, new VRC_Events.OnPlayerLeft() { dateTime = DateTime.Now, displayName = displayName });
                                }
                                if (line.Contains("Joining wrld_"))
                                {
                                    // 2023.10.20 23:33:40 Log - [Behaviour] Joining wrld_4cf554b4-430c-4f8f-b53e-1f294eed230b:79786
                                }
                                if (line.Contains("Joining or Creating Room:"))
                                {
                                    //2023.10.20 23:33:40 Log - [Behaviour] Joining or Creating Room: The Black Cat
                                }
                                if (line.Contains("Successfully left room"))
                                {
                                    //2023.10.20 23:33:23 Log - [Behaviour] Successfully left room
                                }
                               
                                if (line.Contains("ModerationManager"))
                                {
                                    string moderationData = Regex.Match(line, @"\[ModerationManager\] (.+)").Groups[1].Value;

                                    if (line.ToLower().Contains("avatar"))
                                    {
                                        string displayName = moderationData.Split("avatar")[0].Replace(" ", "");
                                        string displayData = moderationData.ToLower().Split("avatar")[1];

                                        if (displayData.Contains("hidden"))
                                            OnPlayerAvatarModeration?.Invoke(this, new VRC_Events.OnAvatarModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRC_Events.OnAvatarModeration.ModerationType.Hidden });
                                        if (displayData.Contains("enabled"))
                                            OnPlayerAvatarModeration?.Invoke(this, new VRC_Events.OnAvatarModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRC_Events.OnAvatarModeration.ModerationType.Shown });
                                        if (displayData.Contains("safety"))
                                        {
                                            displayName = moderationData.Split("Avatar")[0].Replace(" ", "");
                                            OnPlayerAvatarModeration?.Invoke(this, new VRC_Events.OnAvatarModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRC_Events.OnAvatarModeration.ModerationType.Safety });
                                        }
                                    }
                                    if (line.ToLower().Contains("muted"))
                                    {
                                        try
                                        {
                                            string displayName = moderationData.Split(" is")[0];
                                            string displayData = moderationData.Split(" is")[1];

                                            if (displayData.Contains("now"))
                                                OnPlayerVoiceModeration?.Invoke(this, new VRC_Events.OnVoiceModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRC_Events.OnVoiceModeration.ModerationType.Muted });
                                            if (displayData.Contains("no longer"))
                                                OnPlayerVoiceModeration?.Invoke(this, new VRC_Events.OnVoiceModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRC_Events.OnVoiceModeration.ModerationType.Unmuted });

                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex);
                                        }
                                    }
                                    if (line.ToLower().Contains("Requesting block on"))
                                    {
                                        //2023.10.20 23:31:25 Log        -  [ModerationManager] Requesting block on $~??????~
                                    }
                                }
                            }

                            previousFileContent = currentFileContent;
                        }
                    }

                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    _logFile = GetLogFile();
                }
                
                Thread.Sleep(450);
            }
        }

        public FileInfo? GetLogFile()
        {
            foreach (var file in new DirectoryInfo(_logDirectory).GetFiles().OrderByDescending(x => x.LastWriteTime))
            {
                if (file.Name.EndsWith(".txt"))
                {
                    return file;
                }
            }
            return null;
        }
    }
    public class APISystem
    {
        private static string _userAgent { get; set; } = "xylovrchat/v0.0.1";
        private static string _apiKey { get; set; } = "JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26";

        private static Configuration _accountConfig { get; set; }
        private static AuthenticationApi _authenticationAPI { get; set; }
        private static FriendsApi _friendsAPI { get; set; }
        private static UsersApi _usersAPI { get; set; }

        public void Start()
        {
            string credentials = System.IO.File.ReadAllText($"F:\\xylO\\Program Sources\\VRChatAccount.txt");

            _accountConfig = new Configuration() { BasePath = "https://api.vrchat.cloud/api/1", Username = credentials.Split(':')[0], Password = credentials.Split(':')[1], UserAgent = _userAgent };

            _accountConfig.AddApiKey("apiKey", _apiKey);
            _accountConfig.DefaultHeaders.Add("Cookie", $"apiKey={_apiKey}; auth={credentials.Split(':')[2]}");

            _authenticationAPI = new AuthenticationApi(_accountConfig);

            _authenticationAPI.GetCurrentUser();

            _friendsAPI = new FriendsApi(_accountConfig);
            _usersAPI = new UsersApi(_accountConfig);

        }

        public LimitedUser GetUserByName(string username)
        {
           return _usersAPI.SearchUsers(username)[0];
        }

    }
}
