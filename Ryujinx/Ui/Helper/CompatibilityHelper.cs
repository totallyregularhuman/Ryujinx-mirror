﻿using Ryujinx.Common.Configuration;
using Ryujinx.Common.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Ryujinx.Ui.Helper
{
    public static class CompatibilityHelper
    {
        private const string _githubEndpointUrl = "https://api.github.com/";
        private static List<CompatibilityItem> _compatibilityItems = new List<CompatibilityItem>();
        private static bool _loaded = false;

        public async static void Load()
        {
            if (File.Exists(Path.Combine(AppDataManager.BaseDirPath, "compatibility_cache.json")))
            {
                string json = await File.ReadAllTextAsync(Path.Combine(AppDataManager.BaseDirPath, "compatibility_cache.json"));
                _compatibilityItems = JsonSerializer.Deserialize<List<CompatibilityItem>>(json);
            }

            _loaded = true;
        }

        public async static Task<bool> DownloadCompatibilityList()
        {
            int page = 0;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "ryujinx-game-list");

            while (true)
            {
                CompatibilityItem[] comp;

                try 
                { 
                     comp = JsonSerializer.Deserialize<CompatibilityItem[]>(await client.GetStringAsync($"{_githubEndpointUrl}repos/Ryujinx/Ryujinx-Games-List/issues?per_page=100&page={page}"));
                }
                catch
                {
                    return false; //403, Rate limit succeeded
                }

                if (comp.Length != 0)
                {
                    page++;
                    _compatibilityItems.AddRange(comp);
                }
                else
                {
                    saveCompatibility();
                    return true; //Everything Succeeded
                }
            }
        }

        private static void saveCompatibility() 
        {
            File.WriteAllText(Path.Combine(AppDataManager.BaseDirPath, "compatibility_cache.json"), JsonHelper.Serialize<List<CompatibilityItem>>(_compatibilityItems));
        }
        
        public static IList<CompatibilityLabel> GetLabel(string id)
        {
            id = id.ToUpper();

            while (!_loaded)
            {
                Thread.Sleep(100);
            }

            foreach (CompatibilityItem item in _compatibilityItems.ToArray())
            {
                if (item.title.Contains(id))
                {
                    List<CompatibilityLabel> labels = new List<CompatibilityLabel>();

                    foreach(CompatibilityLabel label in item.labels.OfType<CompatibilityLabel>().ToList())
                    {
                        string name = convertLabel(label.name);
                        if(!string.IsNullOrEmpty(name))
                        {
                            label.name = name;
                            labels.Add(label);
                        }
                    }
                    
                    return labels.ToArray();
                }
                
            }

            return null;
        }

        public static string GetIssueUrl(string id)
        {
            id = id.ToUpper();

            while (!_loaded)
            {
                Thread.Sleep(100);
            }

            foreach (CompatibilityItem item in _compatibilityItems.ToArray())
            {
                if (item.title.Contains(id))
                {
                        return item.html_url;
                }
                
            }

            return null;
        }

        private static string convertLabel(string labelName)
        {
            switch (labelName)
            {
                case "crash":
                    return "Crash";
                case "slow":
                    return "Slow";
                case "status-ingame":
                    return "Issues";
                case "status-nothing":
                    return "Nothing";
                case "status-playable":
                    return "Playable";
                case "ldn-works":
                    return "LDN works";
                case "ldn-partial":
                    return "LDN partially works";
                case "gpu":
                    return "Graphic Problems";
                case "deadlock":
                    return "Freeze";
                default:
                    return null;
            }
        }
    }

    public class CompatibilityItem
    {
        public string html_url { get; set; }
        public string title { get; set; }
        public IList<CompatibilityLabel> labels { get; set; }
    }

    public class CompatibilityLabel
    {
        public string name { get; set; }
    }

}
