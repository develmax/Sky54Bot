using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sky54Bot.DataAccesses;
using Sky54Bot.Storages;
using Telegram.Bot;

namespace Sky54Bot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Setup telegram client
            var accessToken = Configuration["Settings:accessToken"];

            var bot = new Telegram.Bot.TelegramBotClient(accessToken);
            
            // Set up webhook
            string webhookUrl = Configuration["Settings:webhookUrl"];
            int maxConnections = int.Parse(Configuration["Settings:maxConnections"]);

            bot.SetWebhookAsync(webhookUrl, maxConnections: maxConnections,
                allowedUpdates: new [] { Telegram.Bot.Types.Enums.UpdateType.Message, Telegram.Bot.Types.Enums.UpdateType.CallbackQuery});

            services.AddScoped<ITelegramBotClient>(client => bot);

            var storageConnectionString = Configuration["Settings:storageConnectionString"];
            var storage = new StorageAzure(storageConnectionString);

            services.AddScoped<IStorageAzure>(client => storage);
            services.AddScoped<IStorage, StorageAzureAdapter>();
            services.AddScoped<ISubscribesDataAccess, SubscribesDataAccess>();
            services.AddScoped<ISubscribesDataAccess, SubscribesDataAccess>();
            services.AddScoped<ISettingsDataAccess, SettingsDataAccess>();
            services.AddScoped<IDataAccess, DataAccess>();

            services.AddScoped<ISettings, Settings>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
