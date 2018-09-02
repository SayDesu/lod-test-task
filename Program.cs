﻿using System;
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
            string request = "http://www.recipepuppy.com/api/?i=";
            string last = args.Last();
            foreach (string ingredient in args)
            {
                request += ingredient;
                if (!ingredient.Equals(last))
                {
                    request += ",";
                }
            }
            List<Result> reslist = new List<Result>();
            int min_ingredients_count = int.MaxValue;
            for (int page = 1; page<11; ++page)
            {
                try
                {
                    var resp = GetData(request + "&p=" + page.ToString());
                    if (resp.Status == TaskStatus.Faulted) break;
                    resp.Wait();
                    var rootobj = resp.Result;
                    foreach (Result res in rootobj.results)
                    {
                        var splited = res.ingredients.Split(',');
                        if (splited.Length < min_ingredients_count)
                        {
                            min_ingredients_count = splited.Length;
                        }
                        reslist.Add(res);
                    }
                } catch (Exception e)
                {
                    break;
                }
            }

            Console.WriteLine("Here are the easiest recepies for your ingredient set:\n");
            foreach(Result res in reslist)
            {
                if (res.ingredients.Split(',').Length == min_ingredients_count)
                {
                    Console.WriteLine(res.title + "\n" + res.ingredients + "\n" + res.href + "\n");
                }
            }
        }

        async static Task<Rootobject> GetData(string url)
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