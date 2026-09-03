using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;



public record Author(string Login);
public record CommitAuthor(string Name, string Email, DateTimeOffset Date);
public record CommitData(CommitAuthor Author, string Message);
public record Commit(string Sha, Author Author, [property: JsonPropertyName("commit")] CommitData CommitData);

public record Repository(string Owner, string Name);

class Program
{   
    public static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };
    
    private static HttpClient _sharedClient = new()
    {
        BaseAddress = new Uri("https://api.github.com/"),
    };

    public static Repository ParseArgs(string[] args)
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
    static Commit[] DeserializeData(string json)
    {
        var options = jsonSerializerOptions;
       
        Commit[]? commits = JsonSerializer.Deserialize<Commit[]>(json, options);
        if (commits is null) throw  new InvalidOperationException("Repository not found");
        
        return commits;
    }

    static async Task<string> FetchRawData(HttpClient client, string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);
        
        if (!await RepositoryExists(response)) throw  new Exception("Repository not found");
        response.EnsureSuccessStatusCode();
        
        string? jsonResponse = await response.Content.ReadAsStringAsync();
        return jsonResponse;
    }
    
    static async Task<List<Commit>> GetRepositoryData(HttpClient client, Repository rep)
    {   
        string url = $"repos/{rep.Owner}/{rep.Name}/commits";
        List<Commit> commits = new();

        int pageCount = 1;
        int maxPerPage = 100;
        
        Commit[]? page;
        do
        {
            string pagedUrl = $"{url}?per_page={maxPerPage}&page={pageCount}";
            string? rawJson = await FetchRawData(client, pagedUrl);
            page = DeserializeData(rawJson);
            
            if (page == null || page.Length == 0)
                break;
            
            commits.AddRange(page);
            pageCount++;
        } while (page.Length == maxPerPage);
        
        
        return  commits;
    }
    
    static async Task Main(string[] args)
    {   
        
        _sharedClient.DefaultRequestHeaders.Add("User-Agent", "dotnet");
        try
        {
            Repository? rep = ParseArgs(args);
            List<Commit>? commits = await GetRepositoryData(_sharedClient, rep);

            Console.WriteLine(commits.Count);
        }
        catch(Exception e)
        {
           Console.WriteLine(e.Message);
        }
        
    }
}