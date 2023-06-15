using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        await ScrapeReddit();
    }

    static async Task ScrapeReddit()
    {
        JArray gamePosts = new JArray();
        try
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = new CookieContainer(),
            };

            // Send HTTP request to the Reddit /r/GameDeals page
            HttpClient httpClient = new HttpClient(handler);

            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

            HttpResponseMessage response = await httpClient.GetAsync("https://www.reddit.com/r/GameDeals/new.json?limit=100");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            JObject rss = JObject.Parse(responseBody);

            foreach (var post in rss["data"]["children"])
            {
                string title = (string)post["data"]["title"];
                string url = (string)post["data"]["url"];

                if (title.ToLower().Contains("free") || title.ToLower().Contains("giveaway") || title.ToLower().Contains("100%"))
                {
                    JObject postDetails = new JObject();
                    postDetails["title"] = title;
                    postDetails["url"] = url;

                    gamePosts.Add(postDetails);
                }
            }

            File.WriteAllText("gamePosts.json", gamePosts.ToString());
            Console.WriteLine("Done!");
            Console.WriteLine("Output saved to gamePosts.json");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message: {0} ", e.Message);
        }
    }
}
