using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataGenNetCore
{
    public static class SampleData
    {
        public static IConfiguration Configuration { get; set; }
        public static List<string> Users()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("data/users.json");
            Configuration = builder.Build();
            string users = Configuration["Users"];            
            List<string> list = new List<string>(users.Split(","));
            return list;
        }
        public static List<TelcoMessage.Website> Websites()
        {
            string jsonString = File.ReadAllText("data/websites.json");
            var _websites = JsonConvert.DeserializeObject<List<TelcoMessage.Website>>(jsonString);
            return _websites;
        }

        public static List<TelcoMessage.DataSpecial> DataSpecials()
        {
            string jsonString = File.ReadAllText("data/dataspecials.json");
            var _dataspecials = JsonConvert.DeserializeObject<List<TelcoMessage.DataSpecial>>(jsonString);
            return _dataspecials;
        }
        public static List<TelcoMessage.Tower> Towers()
        {
            string jsonString = File.ReadAllText("data/towers.json");
            var _towers = JsonConvert.DeserializeObject<List<TelcoMessage.Tower>>(jsonString);
            return _towers;
        }
        public static List<TelcoMessage.Subscriber> Subscribers()
        {
            string jsonString = File.ReadAllText("data/subscribers.json");
            var _subscribers = JsonConvert.DeserializeObject<List<TelcoMessage.Subscriber>>(jsonString);
            return _subscribers;
        }
    }
}
