using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

class Program
{   
    private static HttpClient sharedClient = new()
    {
        BaseAddress = new Uri("https://api.github.com/repos/dotnet/runtime/commits"),
    };
    

    static async Task GetData(HttpClient client)
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("/");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"{jsonResponse}\n");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"{e.Message}");
        }
    }
    
    static async Task Main(string[] args)
    {
        sharedClient.DefaultRequestHeaders.Add("User-Agent", "dotnet");
        await GetData(sharedClient);
    }
}