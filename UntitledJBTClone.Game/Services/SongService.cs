using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UntitledJBTClone.Game.Models;

namespace UntitledJBTClone.Game.Services
{
    public class SongService
    {
        private readonly string songsPath;
        private List<SongInfo> songs = new();

        public SongService(string songsPath = "songs")
        {
            this.songsPath = songsPath;
            ScanSongs();
        }

        public List<SongInfo> GetAllSongs() => songs;

        public SongInfo GetSongByIndex(int index)
        {
            if (index >= 0 && index < songs.Count)
                return songs[index];
            return null;
        }

        private void ScanSongs()
        {
            songs.Clear();

            if (!Directory.Exists(songsPath))
                return;

            // Recursively scan all directories for .memon files
            var memonFiles = Directory.GetFiles(songsPath, "*.memon", SearchOption.AllDirectories);

            foreach (var memonFile in memonFiles)
            {
                try
                {
                    var songInfo = LoadSongInfo(memonFile);
                    if (songInfo != null)
                        songs.Add(songInfo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading song from {memonFile}: {ex.Message}");
                }
            }

            // Sort songs by title
            songs = songs.OrderBy(s => s.MemonData.Metadata.Title).ToList();
        }

        private SongInfo LoadSongInfo(string memonPath)
        {
            var memonData = MemonParser.ParseMemonFile(memonPath);
            var folderPath = Path.GetDirectoryName(memonPath);

            var songInfo = new SongInfo
            {
                FolderPath = folderPath,
                MemonPath = memonPath,
                MemonData = memonData
            };

            // Find audio file
            if (!string.IsNullOrEmpty(memonData.Metadata.Audio))
            {
                var audioPath = Path.Combine(folderPath, memonData.Metadata.Audio);
                var normalizedAudioPath = Path.GetFullPath(audioPath);
                if (File.Exists(normalizedAudioPath))
                    songInfo.AudioPath = normalizedAudioPath;
            }
            else
            {
                // Look for common audio file extensions
                var audioExtensions = new[] { ".ogg", ".mp3", ".wav", ".m4a" };
                foreach (var ext in audioExtensions)
                {
                    var audioFiles = Directory.GetFiles(folderPath, $"*{ext}");
                    if (audioFiles.Length > 0)
                    {
                        songInfo.AudioPath = Path.GetFullPath(audioFiles[0]);
                        break;
                    }
                }
            }

            // Find jacket file
            if (!string.IsNullOrEmpty(memonData.Metadata.Jacket))
            {
                var jacketPath = Path.Combine(folderPath, memonData.Metadata.Jacket);
                var normalizedJacketPath = Path.GetFullPath(jacketPath);
                if (File.Exists(normalizedJacketPath))
                    songInfo.JacketPath = normalizedJacketPath;
            }
            else
            {
                // Look for common image file extensions
                var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp" };
                foreach (var ext in imageExtensions)
                {
                    var imageFiles = Directory.GetFiles(folderPath, $"*{ext}");
                    if (imageFiles.Length > 0)
                    {
                        songInfo.JacketPath = Path.GetFullPath(imageFiles[0]);
                        break;
                    }
                }
            }

            return songInfo;
        }

        public void RefreshSongs()
        {
            ScanSongs();
        }
    }
}