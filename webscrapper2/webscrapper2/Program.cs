using AngleSharp.Dom;
using AngleSharp.Io;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.IO;
using CsvHelper;
using Newtonsoft.Json;
using CsvHelper.Configuration;
using System.Globalization;

class Program
{
    static void Main()
    {
        string option;
        do
        {
            // dit laat de opties zien voor de user
            Console.WriteLine("neem een optie:");
            Console.WriteLine("1. Scrape YouTube Videos");
            Console.WriteLine("2. Scrape ICTJob.be Jobs");
            Console.WriteLine("3. Scrape producten van een merk op coolblue");
            Console.Write("kies 1 of 2 of 3, druk anders op Enter om te stoppen: ");
            option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    ScrapeYouTube();
                    break;
                case "2":
                    ScrapeICTJob();
                    break;
                case "3":
                    ScrapeCoolblue();
                    break;
                default:
                    Console.WriteLine("verkeerd nummer!!");
                    break;
            }
        } while (option != "") ;
    }

    static void ScrapeYouTube()
    {
        // we vragen aan de user welke zoek term hij wil gebruiken 
        Console.Write("Geef YouTube zoek term: ");
        string searchTerm = Console.ReadLine(); //hier plaatsen het antwoord van de user in de string searchterm

        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("--headless"); // dit zorgt ervoor dat de website niet open springt

        string basePath = @"C:\Users\jacob\OneDrive\thomas more\2ITF\DevOps & security\"; //hier definieren we het pad voor de csv en json file
        string csvPath = Path.Combine(basePath, "youtube_results.csv");
        string jsonPath = Path.Combine(basePath, "youtube_results.json");
        //gebruiken chromedriver om te connecteren met de website
        using (IWebDriver driver = new ChromeDriver(chromeOptions))
        {
            // hier navigeren we naar de youtube search result
            driver.Navigate().GoToUrl($"https://www.youtube.com/results?search_query={searchTerm}&sp=CAI%253D");

            // wachten tot de pagina geladen is
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(d => d.Title.StartsWith(searchTerm, StringComparison.OrdinalIgnoreCase));

            // de gegevens ophalen van de videos
            var videoElements = driver.FindElements(By.CssSelector("ytd-video-renderer"));

            // hier maken we een list aan om data in op te slagen
            var videoDataList = new List<VideoData>();

            

            foreach (var videoElement in videoElements.Take(5)) // hier verzamelen we de data van de eerste 5 videos
            {
                var title = videoElement.FindElement(By.Id("video-title")).Text;
                var uploaderElement = videoElement.FindElement(By.CssSelector(".ytd-channel-name #text"));
                var uploader = uploaderElement.GetAttribute("innerText").Trim();

                var viewCount = videoElement.FindElement(By.CssSelector(".ytd-video-meta-block")).Text;
                var link = videoElement.FindElement(By.Id("video-title")).GetAttribute("href");

                var videoData = new VideoData //hier geven de data die we hebben opgehaald mee met de list
                {
                    Title = title,
                    Uploader = uploader,
                    ViewCount = viewCount,
                    Link = link
                };

                videoDataList.Add(videoData); //en hier voegen we de data toe
            }


            using (var writer = new StreamWriter(csvPath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) //hier schrijven we de data naar csv
            {
                Delimiter = ";" // Stel de scheidingslijn in op komma
            }))
            {
                csv.WriteRecords(videoDataList);
            }

            // Save to JSON
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(videoDataList, Formatting.Indented));


            // laten we de video details zien
            Console.WriteLine("\nTop 5 YouTube Video Details:");
            for (int i = 0; i < videoDataList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Titel: {videoDataList[i].Title}");
                Console.WriteLine($"   Uploader: {videoDataList[i].Uploader}");
                Console.WriteLine($"   Weergaven: {videoDataList[i].ViewCount}");
                Console.WriteLine($"   Link: {videoDataList[i].Link}");
                Console.WriteLine();
            }
        }

        Console.WriteLine("\ndruk op enter om te verlaten..."); //om terug naar vraag menu te gaan
        Console.ReadKey();
    }


    static void ScrapeICTJob()
    {
        Console.Write("geef de job in die je wil zoeken: ");
        string searchTerm = Console.ReadLine();

        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArgument("--headless");


        string basePath = @"C:\Users\jacob\OneDrive\thomas more\2ITF\DevOps & security\";
        string csvPath = Path.Combine(basePath, "ictjob_results.csv");
        string jsonPath = Path.Combine(basePath, "ictjob_results.json");


        using (var driver = new ChromeDriver(chromeOptions))
        {
            driver.Navigate().GoToUrl($"https://www.ictjob.be/nl/it-vacatures-zoeken?keywords={searchTerm}");

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            var jobElements = driver.FindElements(By.CssSelector(".search-item[itemtype='http://schema.org/JobPosting']")); //het juste css element vinden op de page

            var jobDataList = new List<JobData>();

            foreach (var jobElement in jobElements.Take(5))
            {
                var title = jobElement.FindElement(By.CssSelector(".job-title")).Text;
                var company = jobElement.FindElement(By.CssSelector(".job-company")).Text;
                var location = jobElement.FindElement(By.CssSelector(".job-location [itemprop='addressLocality']")).Text;
                var keyword = jobElement.FindElement(By.CssSelector(".job-keywords")).Text;
                var link = jobElement.FindElement(By.CssSelector("a.job-title")).GetAttribute("href");

                var jobData = new JobData
                {
                    Title = title,
                    Company = company,
                    Location = location,
                    Keyword = keyword,
                    Link = link
                };

                jobDataList.Add(jobData);
            }

            using (var writer = new StreamWriter(csvPath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            }))
            {
                csv.WriteRecords(jobDataList);
            }

            // Save to JSON
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(jobDataList, Formatting.Indented));

            // Print job details
            Console.WriteLine("\nTop 5 Job Details:");
            for (int i = 0; i < jobDataList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Title: {jobDataList[i].Title}");
                Console.WriteLine($"   Company: {jobDataList[i].Company}");
                Console.WriteLine($"   Location: {jobDataList[i].Location}");
                Console.WriteLine($"   Keywords: {jobDataList[i].Keyword}");
                Console.WriteLine($"   Link: {jobDataList[i].Link}");
                Console.WriteLine();
            }

            // Existing code...
            Console.WriteLine("\ndruk op enter om te verlaten...");
            Console.ReadKey();

        }
    }

    static void ScrapeCoolblue()
    {

        Console.Write("geef de merk naam in waar je iets over wil zoeken bv: samsung, apple, dyson, ... : ");
        string searchTerm = Console.ReadLine();

        string basePath = @"C:\Users\jacob\OneDrive\thomas more\2ITF\DevOps & security\";
        string csvPath = Path.Combine(basePath, "coolblue_results.csv");
        string jsonPath = Path.Combine(basePath, "coolblue_results.json");

        //geruiken chromedriver om te conntecteren met de website
        using (IWebDriver driver = new ChromeDriver())
        {
            driver.Navigate().GoToUrl($"https://www.coolblue.nl/{searchTerm}/filter");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            var productElements = driver.FindElements(By.CssSelector(".product-card__details"));

            var productList = new List<ProductData>();

            foreach (var productElement in productElements.Take(5)) 
            {
                var name = productElement.FindElement(By.CssSelector("div.product-card__title")).Text;
                var price = productElement.FindElement(By.CssSelector(".sales-price")).Text;
                var description = productElement.FindElement(By.CssSelector("div.product-card__highlights")).Text;
                var link = productElement.FindElement(By.CssSelector("div.product-card__title div.h3 a.link")).GetAttribute("href");

                var productData = new ProductData
                {
                    Name = name,
                    Price = price,
                    Description = description,
                    Link = link
                };

                productList.Add(productData);
            }
            using (var writer = new StreamWriter(csvPath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            }))
            {
                csv.WriteRecords(productList);
            }

    
            File.WriteAllText(jsonPath, JsonConvert.SerializeObject(productList, Formatting.Indented));

            // print de producten van uw merk
            Console.WriteLine("\nTop 5 Producten van uw merk:");
            for (int i = 0; i < productList.Count; i++)
            {
                Console.WriteLine($"{i + 1}. Name: {productList[i].Name}");
                Console.WriteLine($"   Price: {productList[i].Price}");
                Console.WriteLine($"   Description: {productList[i].Description}");
                Console.WriteLine($"   Link: {productList[i].Link}");
                Console.WriteLine();
            }

        
            Console.WriteLine("\ndruk op enter om te verlaten...");
            Console.ReadKey();
        }
    }



    class VideoData
    {
        public string Title { get; set; }
        public string Uploader { get; set; }
        public string ViewCount { get; set; }
        public string Link { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}\nUploader: {Uploader}\nView Count: {ViewCount}\nLink: {Link}\n---";
        }
    }
    class JobData
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Keyword { get; set; }
        public string Link { get; set; }

        public override string ToString()
        {
            return $"Title: {Title}\nCompany: {Company}\nLocation: {Location}\nKeywords: {string.Join(", ", Keyword)}\nLink: {Link}\n---";
        }
    }

    class ProductData
    {
        public string Name { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}\nPrice: {Price}\nDescription: {Description}\nLink: {Link}\n---";
        }
    }
}



