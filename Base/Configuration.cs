using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Base
{
    internal class Configuration
    {
        public static string Browser { get; set; }

        public static string DownloadDirectory { get; set; }

        public Configuration()
        {
            JsonConfigurationProvider jsonConfigurationProvider = new JsonConfigurationProvider(new JsonConfigurationSource());
            
            DownloadDirectory = Path.Combine(Path.GetTempPath(), "epam_downloads", Guid.NewGuid().ToString());
            Directory.CreateDirectory(DownloadDirectory);

            jsonConfigurationProvider.TryGet("browser", out var browser);
            Browser = browser;
        }
    }
}
