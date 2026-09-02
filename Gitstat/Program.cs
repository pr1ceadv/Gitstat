using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;


public record Author(string Login);
public record CommitAuthor(string Name, string Email, string Date);
public record Commit(CommitAuthor Author, string Message);
public record Data(string Sha, Author Author, Commit Commit);

public record Repository(string Owner, string Repo);

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
    
    static async Task<string> GetRepositoryData(HttpClient client, Repository rep)
    {
        string jsonResponse = "";
        try
        {   
            string owner = rep.Owner;
            string repo = rep.Repo;
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
        var rep = ParseArgs(args);
        var jsonResponse = await GetRepositoryData(_sharedClient, rep);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        
        Data[]? data = JsonSerializer.Deserialize<Data[]>(jsonResponse, options);
        Console.WriteLine(data.Length);
    }
}