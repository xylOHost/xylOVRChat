using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace xylOVRChat.SDK
{
    public class Spotify
    {
        private static bool _disableThreads { get; set; } = false;
        private static string _currentArtist { get; set; } = string.Empty;
        private static string _currentSong { get; set; } = string.Empty;

        public static void Start()
        {
            new Thread(MonitorProcess).Start();
        }

        public static bool IsRunning()
        {
            if (Process.GetProcessesByName("Spotify").Count() > 0)
                return true;
            else
                return false;
        }

        public static Process? GetProcess()
        {
            if (IsRunning())
                return Process.GetProcessesByName("Spotify")[0];
            else 
                return null;
        }

        public static string GetArtist()
        {
            return _currentArtist;
        }

        public static string GetTrack()
        {
            return _currentSong;
        }

        public static void MonitorProcess()
        {
            while (!_disableThreads)
            {
                if (!IsRunning())
                {
                    Thread.Sleep(1000);
                    return;
                }

                Process spotifyProcess = GetProcess();

                if (spotifyProcess != null)
                {
                    string[] spotifyData = Regex.Split(spotifyProcess.MainWindowTitle, " - ");
                    _currentArtist = spotifyData[0];
                    _currentSong = spotifyData[1];
                }    
                Thread.Sleep(1750);
            }
        }
    }
}
