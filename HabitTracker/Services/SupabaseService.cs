using Supabase;

namespace HabitTracker.Services
{
    public static class SupabaseService
    {
        private static Client _client;

        public static Client Client => _client;

        public static async Task InitializeAsync()
        {
            if(_client != null) return;

            var url = "https://fkhmrfueypnrbkdvqiyn.supabase.co";
            var key = "sb_publishable_ckwd846nvtwV6oXPEuqq7w_QKFvhiQU";
        
            _client = new Client(url, key);
            await _client.InitializeAsync();
        }
    }
}