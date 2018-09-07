using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace lod_test_task
{
    class Program
    {
        static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("You can't cook with no ingredients :C");
                return;
            }
            var request = args.Distinct().Aggregate("http://www.recipepuppy.com/api/?i=", (req, s) => s!= args.Distinct().Last() ? req + s + "," : req + s);
            Console.WriteLine(request);

            var reslist = new List<Result>();
            var minIngredientCount = Int32.MaxValue;
            for (int page = 1; page<11; ++page)
            {
                try
                {
                    var resp = GetData(request + "&p=" + page.ToString());
                    resp.Wait();
                    var currentMinIngredientCount = resp.Result.results.Min(y => y.ingredients.Split(',').Length);
                    if( minIngredientCount > currentMinIngredientCount)
                    {
                        minIngredientCount = currentMinIngredientCount;
                    }else if (minIngredientCount < currentMinIngredientCount)
                    {
                        continue;
                    }
                    reslist.AddRange(resp.Result.results.Where(x => x.ingredients.Split(',').Length == minIngredientCount));
                } 
                    catch (Exception e)
                {
                    break;
                }
            }

            Console.WriteLine("Here are the easiest recepies for your ingredient set:\n");
            foreach(var res in reslist)
            {
                Console.WriteLine(res.title + "\n" + res.ingredients + "\n" + res.href + "\n");
            }
        }

        static async Task<Rootobject> GetData(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) throw new Exception("Error 500");
            HttpContent content = response.Content;
            string jsonstr = await content.ReadAsStringAsync();
            var root = JsonConvert.DeserializeObject<Rootobject>(jsonstr);
            return root;
        }
    }
}
