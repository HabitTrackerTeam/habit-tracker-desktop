using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    public static class LocalAccountService
    {
        // bezpieczna sciezka w systemie do zapisu:
        private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HabitTracker");
        private static readonly string FilePath = Path.Combine(FolderPath, "users.json");

        //odczytywanie zapisanych kont
        public static List<SavedAccount> LoadSavedAccounts()
        {
            if (!File.Exists(FilePath))
                return new List<SavedAccount>(); 
            try
            {
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<SavedAccount>>(json) ?? new List<SavedAccount>();
            }
            catch
            {
                return new List<SavedAccount>();
            }
        }

        //zapis kont lokalnie
        public static void SaveAccount(SavedAccount account)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);

            var accounts = LoadSavedAccounts();
            
            accounts.RemoveAll(a => a.Email.Equals(account.Email, StringComparison.OrdinalIgnoreCase));
            
            accounts.Add(account);
            string json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        
        //obsluga przycisku do zapomnienia konta
        public static void RemoveAccount(string email)
        {
            if (!File.Exists(FilePath)) return;

            var accounts = LoadSavedAccounts();
            accounts.RemoveAll(a => a.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            
            string json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}