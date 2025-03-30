using MemoryCardGameMAP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;

namespace MemoryCardGameMAP.Services
{
    public class UserService
    {
        private readonly string _usersFilePath;

        public UserService(string usersFilePath = "users.json")
        {
            _usersFilePath = usersFilePath;
        }

        public List<User> GetAllUsers()
        {
            if (!File.Exists(_usersFilePath))
                return new List<User>();

            string json = File.ReadAllText(_usersFilePath);
            return string.IsNullOrEmpty(json)
                ? new List<User>()
                : JsonSerializer.Deserialize<List<User>>(json);
        }

        public void AddUser(User user)
        {
            var users = GetAllUsers();

            // Check if username already exists
            if (users.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"User '{user.Username}' already exists");

            users.Add(user);
            SaveUsers(users);
        }

        public void DeleteUser(string username)
        {
            var users = GetAllUsers();
            var userToRemove = users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (userToRemove != null)
            {
                users.Remove(userToRemove);
                SaveUsers(users);

                // Delete user game saves and statistics
                DeleteUserGameSaves(username);
                DeleteUserStatistics(username);
            }
        }

        private void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_usersFilePath, json);
        }

        private void DeleteUserGameSaves(string username)
        {
            // Implementation to delete user's saved games
            string saveFilePath = $"{username}_save.json";
            if (File.Exists(saveFilePath))
                File.Delete(saveFilePath);
        }

        private void DeleteUserStatistics(string username)
        {
            // Implementation would depend on how you store statistics
            // This is just a placeholder
        }
    }
}
