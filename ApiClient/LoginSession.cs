using System.Text.Json.Serialization;

namespace ApiClient
{
    public class LoginSession 
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        
        [JsonPropertyName("startup_data")]
        public StartupData Data { get; set; }

        public class StartupData
        {
            [JsonPropertyName("user")]
            public User UserInfo { get; set; }

            public class User
            {
                [JsonPropertyName("user_id")]
                public string Id { get; set; }

                [JsonPropertyName("name")]
                public string Name { get; set; }

                [JsonPropertyName("country_id")]
                public string CountryId { get; set; }

                [JsonPropertyName("email")]
                public string Email { get; set; }

                [JsonPropertyName("phone_number")]
                public string PhoneNumber { get; set; }
            }
        }
    }
}
