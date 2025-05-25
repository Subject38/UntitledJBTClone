using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using UntitledJBTClone.Game.Models;

namespace UntitledJBTClone.Game.Services
{
    public class ProfileService
    {
        private readonly string connectionString;

        public ProfileService()
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UntitledJBTClone", "profiles.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Profiles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    CreatedAt TEXT NOT NULL,
                    LastPlayedAt TEXT NOT NULL,
                    TotalScore INTEGER DEFAULT 0,
                    PlayCount INTEGER DEFAULT 0,
                    Avatar TEXT DEFAULT 'default'
                )";
            command.ExecuteNonQuery();
        }

        public List<Profile> GetAllProfiles()
        {
            var profiles = new List<Profile>();

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Profiles ORDER BY LastPlayedAt DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                profiles.Add(new Profile
                {
                    Id = reader.GetInt32(0), // Id
                    Name = reader.GetString(1), // Name
                    CreatedAt = DateTime.Parse(reader.GetString(2)), // CreatedAt
                    LastPlayedAt = DateTime.Parse(reader.GetString(3)), // LastPlayedAt
                    TotalScore = reader.GetInt32(4), // TotalScore
                    PlayCount = reader.GetInt32(5), // PlayCount
                    Avatar = reader.GetString(6) // Avatar
                });
            }

            return profiles;
        }

        public Profile CreateProfile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Profile name cannot be empty");

            if (name.Length > 20)
                throw new ArgumentException("Profile name cannot exceed 20 characters");

            var profile = new Profile(name);

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Profiles (Name, CreatedAt, LastPlayedAt, TotalScore, PlayCount, Avatar)
                VALUES (@name, @createdAt, @lastPlayedAt, @totalScore, @playCount, @avatar)";

            command.Parameters.AddWithValue("@name", profile.Name);
            command.Parameters.AddWithValue("@createdAt", profile.CreatedAt.ToString("O"));
            command.Parameters.AddWithValue("@lastPlayedAt", profile.LastPlayedAt.ToString("O"));
            command.Parameters.AddWithValue("@totalScore", profile.TotalScore);
            command.Parameters.AddWithValue("@playCount", profile.PlayCount);
            command.Parameters.AddWithValue("@avatar", profile.Avatar);

            command.ExecuteNonQuery();

            // Get the ID of the inserted profile
            command.CommandText = "SELECT last_insert_rowid()";
            profile.Id = Convert.ToInt32(command.ExecuteScalar());

            return profile;
        }

        public void UpdateLastPlayed(int profileId)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Profiles SET LastPlayedAt = @lastPlayedAt WHERE Id = @id";
            command.Parameters.AddWithValue("@lastPlayedAt", DateTime.Now.ToString("O"));
            command.Parameters.AddWithValue("@id", profileId);

            command.ExecuteNonQuery();
        }

        public bool ProfileExists(string name)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Profiles WHERE Name = @name";
            command.Parameters.AddWithValue("@name", name);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        public void UpdateProfileName(int profileId, string newName)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Profiles SET Name = @name WHERE Id = @id";
            command.Parameters.AddWithValue("@name", newName);
            command.Parameters.AddWithValue("@id", profileId);

            command.ExecuteNonQuery();
        }

        public void DeleteProfile(int profileId)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Profiles WHERE Id = @id";
            command.Parameters.AddWithValue("@id", profileId);

            command.ExecuteNonQuery();
        }
    }
}