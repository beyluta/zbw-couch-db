using Newtonsoft.Json;
using System.Text;

namespace zbw_couch_db_demo
{
    public class Program
    {
        private static string RemoteUrl = String.Empty;
        private static string Credentials = String.Empty;
        public enum CrudOp
        {
            READ = 0,
            WRITE = 1,
            DELETE = 2
        }

        public static void Main(string[] args)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔══════════════════════════════════════╗");
                Console.WriteLine("║              COUCH DB UI             ║");
                Console.WriteLine("╠══════════════════════════════════════╣");
                Console.WriteLine("║   This database stores:              ║");
                Console.WriteLine("║     - Title                          ║");
                Console.WriteLine("║     - Genre                          ║");
                Console.WriteLine("║     - Rating                         ║");
                Console.WriteLine("║     - Release Date                   ║");
                Console.WriteLine("║   Powered by CouchDB (API requests)  ║");
                Console.WriteLine("╠══════════════════════════════════════╣");
                Console.WriteLine("║   Press 'r' to read all documents    ║");
                Console.WriteLine("║   Press 'w' to write a new entry     ║");
                Console.WriteLine("║   Press 'd' to delete by ID & rev    ║");
                Console.WriteLine("╚══════════════════════════════════════╝");
                Console.ResetColor();
                ConsoleKeyInfo c = Console.ReadKey();
                CrudOp operation = SelectMode(c.KeyChar);
                SelectOperation(operation);
                Console.ReadKey();
                Console.Clear();
            }
        }

        public static CrudOp SelectMode(char mode)
        {
            return mode switch
            {
                'r' => CrudOp.READ,
                'w' => CrudOp.WRITE,
                'd' or _ => CrudOp.DELETE,
            };
        }

        public static void SelectOperation(CrudOp operation)
        {
            switch (operation)
            {
                default:
                case CrudOp.READ:
                    Read();
                    break;
                case CrudOp.WRITE:
                    Write();
                    break;
                case CrudOp.DELETE:
                    Delete();
                    break;
            }
        }
        public static string DefaultReadLine(string message)
        {
            Console.Write($"\n{message}");
            return Console.ReadLine() ?? "n/a";
        }

        public static void AuthRequest(ref HttpClient client)
        {
            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(Credentials));
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
        }

        public static async Task<int> GetDocCount()
        {
            HttpClient client = new HttpClient();
            AuthRequest(ref client);
            var response = await client.GetAsync($"{RemoteUrl}/_all_docs?=limit=0");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json)!;
            client.Dispose();
            return result.total_rows;
        }

        public static async void Read()
        {
            int count = await GetDocCount();
            Console.WriteLine($"\nDatabase Store Contains {count} documents");
            if (count <= 0) return;

            HttpClient client = new HttpClient();
            AuthRequest(ref client);
            HttpResponseMessage response = await client.GetAsync($"{RemoteUrl}/_all_docs?include_docs=true");
            string result = await response.Content.ReadAsStringAsync();
            dynamic docs = JsonConvert.DeserializeObject(result)!;

            foreach (var row in docs.rows)
            {
                var doc = row.doc;
                Console.WriteLine($"_id: {doc._id}");
                foreach (var property in doc)
                {
                    if (property.Name != "_id")
                    {
                        Console.WriteLine($"{property.Name}: {property.Value}");
                    }
                }
                Console.WriteLine();
            } 

            client.Dispose();
        }

        public static async void Write()
        {
            string title = DefaultReadLine("Entry Title: ");
            string genre = DefaultReadLine("Entry Genre: ");
            string rating = DefaultReadLine("Entry Rating: ");
            string releaseDate = DefaultReadLine("Entry Release Date: ");

            var data = new
            {
                title,
                genre,
                rating,
                releaseDate
            };


            HttpClient client = new HttpClient();
            AuthRequest(ref client);
            string json = JsonConvert.SerializeObject(data);
            StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(RemoteUrl, jsonContent);
            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
            client.Dispose();
        }

        public static async void Delete()
        {
            string id = DefaultReadLine("Document Id: ");
            string rev = DefaultReadLine("Document Rev: ");

            HttpClient client = new HttpClient();
            AuthRequest(ref client);
            HttpResponseMessage response = await client.DeleteAsync($"{RemoteUrl}/{id}?rev={rev}");
            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);
            client.Dispose();
        }
    }
}