using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Sky54Bot.Tasks
{
    public class ScheduleTask : ScheduledProcessor
    {
        public ScheduleTask(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
        }

        protected override string Schedule => "*/10 * * * *"; //Runs every 10 minutes

        public override Task ProcessInScope(IServiceProvider serviceProvider)
        {
            //Console.WriteLine("Processing starts here");
            var uri = new Uri("https://sky54bot.azurewebsites.net/api/telegram/check");
            var htmlStr = new WebClient().DownloadString(uri);

            return Task.CompletedTask;
        }
    }
}