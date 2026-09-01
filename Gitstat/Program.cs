using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;


public class Author
{   
    
    public string login { get; set; }
   
}
public class CommitAuthor
{
    public string name { get; set; }
    public string email { get; set; }
    public string  date { get; set; }
}

public class Commit
{
    public CommitAuthor  author { get; set; }
    public string  message { get; set; }
    
}

public class Data
{
    public string Sha { get; set; }
    public Commit commit { get; set; }
    public Author author { get; set; }
    
    public override string ToString()
    {
        return $"Sha: {Sha}\nAuthor: {author?.login}";
    }
}
class Program
{   
    private static HttpClient _sharedClient = new()
    {
        BaseAddress = new Uri("https://api.github.com/"),
    };

    static public (string owner, string repo) ParseArgs(string[] args)
    {
        if (args.Length != 1)
            throw new ArgumentException("Usage: gitstat <owner>/<repo>");

        string[] parts = args[0].Split('/');
        
        if (parts.Length != 2 || 
            string.IsNullOrWhiteSpace(parts[0]) || 
            string.IsNullOrWhiteSpace(parts[1]))
        {
            throw new ArgumentException("Repository must be in format <owner>/<repo>");
        }
        
        return (parts[0], parts[1]);
    }
    
    static async Task<string> GetRepositoryData(HttpClient client, string owner, string repo)
    {
        string jsonResponse = "";
        try
        {   
            string BaseUrl = client.BaseAddress.ToString();
            string url = $"repos/{owner}/{repo}/commits";
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            jsonResponse = await response.Content.ReadAsStringAsync();
            
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"{e.Message}");
        }
        return jsonResponse;
    }
    
    static async Task Main(string[] args)
    {   
        
        _sharedClient.DefaultRequestHeaders.Add("User-Agent", "dotnet");
        var (owner,  repo) = ParseArgs(args);
        var jsonResponse = await GetRepositoryData(_sharedClient, owner, repo );

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        
        Data[]? data = JsonSerializer.Deserialize<Data[]>(jsonResponse, options);
        Console.WriteLine(data.Length);
    }
}