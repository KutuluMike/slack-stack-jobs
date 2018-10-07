// Copyright (c)Mike Edenfield <kutulu@kutulu.org>. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SlackStackJobs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .UseEnvironment("Development")
                .ConfigureWebJobs(jobs =>
                {
                    jobs.AddAzureStorageCoreServices()
                        .AddAzureStorage()
                        .AddCosmosDB();
                })
                .ConfigureAppConfiguration(config =>
                {
                    config.AddCommandLine(args);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddConsole();
                })
                .UseConsoleLifetime();

            using (var host = builder.Build())
            {
                await host.RunAsync();
            }
        }
    }
}
