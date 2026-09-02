using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;


public record Author(string Login);
public record CommitAuthor(string Name, string Email, DateTimeOffset Date);
public record Commit(CommitAuthor Author, string Message);
public record Data(string Sha, Author Author, Commit Commit);

public record Repository(string Owner, string Name);

class Program
{   
    private static HttpClient _sharedClient = new()
    {
        BaseAddress = new Uri("https://api.github.com/"),
    };

    static public Repository ParseArgs(string[] args)
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
        
        Repository rep = new(parts[0], parts[1]);
        
        return rep;
    }

    static async Task<bool> RepositoryExists(HttpResponseMessage response)
    {
        return response.IsSuccessStatusCode;
    }
    static Data[] DeserealizeData(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        Data[]? data = JsonSerializer.Deserialize<Data[]>(json, options);
        if (data is null) throw  new NullReferenceException("Repository not found");
        
        return data;
    }

    static async Task<string> FetchRawData(HttpClient client, string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);
        
        if (!await RepositoryExists(response)) throw  new Exception("Repository not found");
        response.EnsureSuccessStatusCode();
        
        string? jsonResponse = await response.Content.ReadAsStringAsync();
        return jsonResponse;
    }
    
    static async Task<Data[]> GetRepositoryData(HttpClient client, Repository rep)
    {   
        string url = $"repos/{rep.Owner}/{rep.Name}/commits";
        string? rawJson = await FetchRawData(client, url);
        Data[]? data = DeserealizeData(rawJson);
        return data;
    }
    
    static async Task Main(string[] args)
    {   
        
        _sharedClient.DefaultRequestHeaders.Add("User-Agent", "dotnet");
      
        Repository? rep = ParseArgs(args);
        Data[]? data = await GetRepositoryData(_sharedClient, rep);
        
        Console.WriteLine(data.Length);
    }
}