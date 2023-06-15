using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class RedditGameDealsScraper
{
    public static async Task Main(string[] args)
    {
        await ScrapeReddit();
    }

    // Make an asynchronous request to the Reddit /r/GameDeals page and get the most recent 100 game posts containing the words "free," "giveaway," or "100%."
    public static async Task<JArray> ScrapeReddit(HttpClient httpClient = null)
    {
        JArray gamePosts = new JArray();
        try
        {
            // Set up a handler to allow HTTP request redirection, cookie storage, and automatic compression.
            HttpClientHandler handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = new CookieContainer(),
            };

            // Create a new HttpClient object and set its default request headers to disguise the request as a user using Google Chrome.  This is required because it will return a 403 Blocked error otherwise.
            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

            // Send a GET request to the subreddit's API and obtain its JSON response.
            HttpResponseMessage response = await httpClient.GetAsync("https://www.reddit.com/r/GameDeals/new.json?limit=100");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            // Parse the JSON response with Newtonsoft.Json.
            JObject rss = JObject.Parse(responseBody);

            // Identify game posts based on their title containing the words "free," "giveaway," or "100%."
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

            // Create a new JSON file and write the qualifying game posts to it.
            File.WriteAllText("gamePosts.json", gamePosts.ToString());

            // Print a confirmation message to the console with the name of the output file.
            Console.WriteLine($"Done! Output saved to gamePosts.json.");
        }

        // Catch any exceptions thrown during the process and output them to the console.
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Exception Message: {0} ", e.Message);
        }

        return gamePosts;
    }
}
