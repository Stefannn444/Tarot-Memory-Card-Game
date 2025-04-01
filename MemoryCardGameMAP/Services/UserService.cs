using MemoryCardGameMAP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

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

                DeleteUserGameSaves(username);
            }
        }

        private void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_usersFilePath, json);
        }

        private void DeleteUserGameSaves(string username)
        {
            try
            {
                string saveFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SavedGames", $"{username}.json");
                if (File.Exists(saveFile))
                {
                    File.Delete(saveFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete saved game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateUser(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Username))
                return;

            var users = GetAllUsers();

            var existingUser = users.FirstOrDefault(u =>
                u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase));

            if (existingUser != null)
            {
                int index = users.IndexOf(existingUser);
                users[index] = user;

                SaveUsers(users);
            }
        }
    }
}
