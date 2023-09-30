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
        public static void Initialize()
        {
            VRAPISystem.Start();
            VREventSystem.Start();

            VREventSystem.OnPlayerJoined += VREventSystem_OnPlayerJoined;
            VREventSystem.OnPlayerLeft += VREventSystem_OnPlayerLeft;
            VREventSystem.OnPlayerAvatarModeration += VREventSystem_OnPlayerAvatarModeration;
            VREventSystem.OnPlayerVoiceModeration += VREventSystem_OnPlayerVoiceModeration;
        }

        private static void VREventSystem_OnPlayerVoiceModeration(object? sender, VRCEvents.OnVoiceModeration e)
        {
            switch (e.moderationType)
            {
                case VRCEvents.OnVoiceModeration.ModerationType.Unmuted:
                    Console.WriteLine(e.displayName + " was unmuted");
                    break;
                case VRCEvents.OnVoiceModeration.ModerationType.Muted:
                    Console.WriteLine(e.displayName + " was muted");
                    break;
            }
        }

        private static void VREventSystem_OnPlayerAvatarModeration(object? sender, VRCEvents.OnAvatarModeration e)
        {
            switch (e.moderationType)
            {
                case VRCEvents.OnAvatarModeration.ModerationType.Safety:
                    Console.WriteLine(e.displayName + "'s avatar is now on safety settings");
                    break;
                case VRCEvents.OnAvatarModeration.ModerationType.Shown:
                    Console.WriteLine(e.displayName + "'s avatar is now fully shown");
                    break;
                case VRCEvents.OnAvatarModeration.ModerationType.Hidden:
                    Console.WriteLine(e.displayName + "'s avatar is now blocked");
                    break;
            }
        }

        private static void VREventSystem_OnPlayerJoined(object? sender, VRCEvents.OnPlayerJoined e)
        {
            Console.WriteLine(e.displayName + " has joined the instance");
        }

        private static void VREventSystem_OnPlayerLeft(object? sender, VRCEvents.OnPlayerLeft e)
        {
            Console.WriteLine(e.displayName + " has left the instance");
        }
    }

    public class EventSystem
    {
        private static bool _disableThreads { get; set; } = false;
        private static string _logDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow") + @"/VRChat/VRChat";
        private static FileInfo _logFile { get; set; } = null;
        private static long _logFileCurrentLength { get; set; } = 0;


        public event EventHandler<VRCEvents.OnPlayerJoined> OnPlayerJoined = null!;
        public event EventHandler<VRCEvents.OnPlayerLeft> OnPlayerLeft = null!;
        public event EventHandler<VRCEvents.OnAvatarModeration> OnPlayerAvatarModeration = null!;
        public event EventHandler<VRCEvents.OnVoiceModeration> OnPlayerVoiceModeration = null!;

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
                                //Console.WriteLine(line);
                                if (line.Contains("OnPlayerJoined"))
                                {
                                    string displayName = Regex.Match(line, @"OnPlayerJoined (.+)").Groups[1].Value;
                                    if (displayName == string.Empty)
                                        continue;

                                    OnPlayerJoined?.Invoke(this, new VRCEvents.OnPlayerJoined() { dateTime = DateTime.Now, displayName = displayName });
                                }
                                if (line.Contains("OnPlayerLeft"))
                                {

                                    string displayName = Regex.Match(line, @"OnPlayerLeft (.+)").Groups[1].Value;
                                    if (displayName == string.Empty)
                                        continue;

                                    OnPlayerLeft?.Invoke(this, new VRCEvents.OnPlayerLeft() { dateTime = DateTime.Now, displayName = displayName });
                                }
                                if (line.Contains("ModerationManager"))
                                {
                                    string moderationData = Regex.Match(line, @"\[ModerationManager\] (.+)").Groups[1].Value;

                                    if (line.ToLower().Contains("avatar"))
                                    {
                                        string displayName = moderationData.Split("avatar")[0].Replace(" ", "");
                                        string displayData = moderationData.ToLower().Split("avatar")[1];

                                        if (displayData.Contains("hidden"))
                                            OnPlayerAvatarModeration?.Invoke(this, new VRCEvents.OnAvatarModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRCEvents.OnAvatarModeration.ModerationType.Hidden });
                                        if (displayData.Contains("enabled"))
                                            OnPlayerAvatarModeration?.Invoke(this, new VRCEvents.OnAvatarModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRCEvents.OnAvatarModeration.ModerationType.Shown });
                                        if (displayData.Contains("safety"))
                                        {
                                            displayName = moderationData.Split("Avatar")[0].Replace(" ", "");
                                            OnPlayerAvatarModeration?.Invoke(this, new VRCEvents.OnAvatarModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRCEvents.OnAvatarModeration.ModerationType.Safety });
                                        }
                                    }
                                    if (line.ToLower().Contains("muted"))
                                    {
                                        string displayName = moderationData.Split(" is")[0];
                                        string displayData = moderationData.Split(" is")[1];

                                        if (displayData.Contains("now"))
                                            OnPlayerVoiceModeration?.Invoke(this, new VRCEvents.OnVoiceModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRCEvents.OnVoiceModeration.ModerationType.Muted });
                                        if (displayData.Contains("no longer"))
                                            OnPlayerVoiceModeration?.Invoke(this, new VRCEvents.OnVoiceModeration() { dateTime = DateTime.Now, displayName = displayName, moderationType = VRCEvents.OnVoiceModeration.ModerationType.Unmuted });

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

                
                Thread.Sleep(350);
            }
        }

        public FileInfo GetLogFile()
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

        public static LimitedUser GetUserByName(string username)
        {
           return _usersAPI.SearchUsers(username)[0];
        }

    }
}
