using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace joke_program_challenge
{


    public class GetJoke
    {
        public string joke { get; set; }
        public int total_pages { get; set; }
        public int total_jokes { get; set; }
    }

    public class GetJokeQuery
    {
        public int current_page { get; set; }
        public int next_page { get; set; }
        public int limit { get; set; }
        public int total_pages { get; set; }
        public int total_jokes { get; set; }

        public List<JokeResults> results { get; set; }
    }

    public class JokeResults
    {
        public string joke { get; set; }
    }


    class Program
    {


        static void Main(string[] args)
        {
            // Menu:
            bool showMenu = true;
            while (showMenu)
            {
                showMenu = MainMenu();
            }

        }

        private static bool MainMenu()
        {
            Console.Clear();
            Console.WriteLine("***Searching Jokes Program:***");
            Console.WriteLine("1) Type '1' for fetching a single random joke.");
            Console.WriteLine("2) Type '2' for any other term for fetching up to 30 jokes containing the term.");
            Console.WriteLine("3) Exit");
            Console.Write("\r\nSelect an option: ");


            switch (Console.ReadLine())
            {
                case "1":
                    GetRandomJoke().Wait();
                    Console.ReadLine();
                    return true;
                case "2":
                    Console.WriteLine("Enter a term to search: ");
                    string input = Console.ReadLine();
                    if (Regex.Match(input, @"^\w+", RegexOptions.IgnoreCase).Success)
                    {
                        SearchJoke(input).Wait();
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Please provide term to search that includes at least 2 characters");
                    }
                    return true;
                case "3":
                    return false;
                default:
                    return true;
            }
        }



        private static void addGroupedJokesToLists(string item, List<string> shortJokes_10words, List<string> mediumJokes_20words, List<string> longJokes_20wordsPlus)
        {
            int wrd, l;
            l = 0;
            wrd = 1;

            /* loop till end of string */
            while (l <= item.Length - 1)
            {
                /* check whether the current character is white space or new line or tab character*/
                if (item[l] == ' ' || item[l] == '\n' || item[l] == '\t')
                {
                    wrd++;
                }

                l++;
            }
            //Console.Write("Total number of words in the string is : {0}\n", wrd);

            // Short (<10 words), Medium (<20 words), Long (>= 20 words)
            if (wrd < 10)
            {
                shortJokes_10words.Add(item);
                // Console.WriteLine(" short word ---" + item);
                // Console.Write("Total number of words in the string is : {0}\n", wrd);
            }
            if (wrd >= 10 && wrd < 20)
            {
                mediumJokes_20words.Add(item);
                // Console.WriteLine(" Medium word --->>" + item);
                // Console.Write("Total number of words in the string is : {0}\n", wrd);
            }
            if (wrd >= 20)
            {
                //Console.WriteLine("LARGE " + item);
                longJokes_20wordsPlus.Add(item);
            }

        }

        // Function to fetch a single random joke:
        // ----------------------------------------------------------------------------------------
        private static async Task GetRandomJoke()
        {
            const string BaseUrl = "https://icanhazdadjoke.com";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "special agent 007");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                HttpResponseMessage response = await client.GetAsync(BaseUrl);
                var contentString = await response.Content.ReadAsStringAsync();
                var contentJson = JsonConvert.DeserializeObject<GetJoke>(contentString);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Content is object && response.Content.Headers.ContentType.MediaType == "application/json")
                    {
                        Console.WriteLine("** Fetching random joke:\n\t-> {0}", contentJson.joke);
                        Console.WriteLine("\n");
                    }
                    else
                    {
                        Console.WriteLine("should not be here");
                    }
                }
                else
                {
                    Console.WriteLine("Internal server Error");
                }
            }
        }



        // Function to search 30 jokes based on user input term:
        // ----------------------------------------------------------------------------------------
        private static async Task SearchJoke(string userInput)
        {
            var BaseUrl = $"https://icanhazdadjoke.com/search?term={userInput}&limit=30";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "special agent 007");
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                HttpResponseMessage response = await client.GetAsync(BaseUrl);
                var contentString = await response.Content.ReadAsStringAsync();
                var contentJson = JsonConvert.DeserializeObject<GetJokeQuery>(contentString);

                if (response.IsSuccessStatusCode)
                {
                    if (response.Content is object && response.Content.Headers.ContentType.MediaType == "application/json")
                    {

                        Console.WriteLine("***Number of Jokes: " + contentJson.results.Count);
                        //int counter = 0;

                        //Grouping:  Short (<10 words), Medium (<20 words), Long (>= 20 words)
                        List<string> shortJokes_10words = new List<string>();
                        List<string> mediumJokes_20words = new List<string>();
                        List<string> longJokes_20wordsPlus = new List<string>();


                        foreach (var item in contentJson.results)
                        {
                            // Console.WriteLine(item.joke);  //JokeResults

                            // Emphashize the search/matching term in the sentence:
                            var userInput_emphasised = $"<{userInput}>";
                            string output = Regex.Replace(item.joke, userInput, userInput_emphasised, RegexOptions.IgnoreCase);
                            // Console.WriteLine(output);

                            //Grouping by length of words:
                            addGroupedJokesToLists(output, shortJokes_10words, mediumJokes_20words, longJokes_20wordsPlus);
                        }


                        // Printing the results of grouped words:

                        Console.WriteLine("**Number of short words Jokes (10 words): " + shortJokes_10words.Count);
                        foreach (string word in shortJokes_10words)
                        {
                            Console.WriteLine("\t" + word);
                        }

                        Console.WriteLine("**Number of medium words Jokes (20 words): " + mediumJokes_20words.Count);
                        foreach (string word in mediumJokes_20words)
                        {
                            Console.WriteLine("\t" + word);
                        }

                        Console.WriteLine("**Number of large words Jokes (+20 words): " + longJokes_20wordsPlus.Count);
                        foreach (string word in longJokes_20wordsPlus)
                        {
                            Console.WriteLine("\t" + word);
                        }

                        Console.WriteLine("\n--- End ---\n");


                    }
                    else
                    {
                        Console.WriteLine("should not be here");
                    }
                }
                else
                {
                    Console.WriteLine("Internal server Error");
                }
            }

        }










    }
}
