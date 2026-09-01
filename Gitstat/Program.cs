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
    

    static async Task<string> GetData(HttpClient client)
    {
        string jsonResponse = "";
        
        try
        {   
            string BaseUrl = client.BaseAddress.ToString();
            string url = $"{BaseUrl}repos/dotnet/runtime/commits";
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
        
        var jsonResponse = await GetData(_sharedClient);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        
        Data[]? data = JsonSerializer.Deserialize<Data[]>(jsonResponse, options);
        Console.WriteLine(data.Length);
    }
}