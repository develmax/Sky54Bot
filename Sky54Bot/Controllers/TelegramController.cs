using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sky54Bot.DataAccesses;
using Sky54Bot.Models;
using Sky54Bot.Storages;
using Sky54Bot.Storages.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sky54Bot.Controllers
{
    [Route("api/[controller]")]
    public class TelegramController : Controller
    {
        ITelegramBotClient _bot;
        private IDataAccess _dataAccess;
        private ISettings _settings;
        private IConfiguration _configuration;
        public TelegramController(ITelegramBotClient bot, IDataAccess dataAccess,
            ISettings settings, IConfiguration configuration) {
            _bot = bot;
            _dataAccess = dataAccess;
            _settings = settings;
            _configuration = configuration;
        }

        [HttpGet("subscribes")]
        public IActionResult Subscribes()
        {
            var subscriptions = _dataAccess.SubscribesDataAccess.GetSubscribes();

            return View(new SubscriptionsViewModel { EntityList = subscriptions != null ? subscriptions.ToList() : new List<SubscribeEntity>() });
        }

        [HttpGet("settings")]
        public IActionResult Settings()
        {
            var settings = _dataAccess.SettingsDataAccess.GetSettings();

            return View(new SettingsViewModel { EntityList = settings != null ? settings.ToList() : new List<SettingEntity>() });
        }


        // GET api/telegram/update/{token}
        [HttpPost("update/{token}")]
        public void Update([FromRoute] string token, [FromBody] Telegram.Bot.Types.Update update)
        {
            try
            {
                var accessToken = _configuration["Settings:accessToken"];
                if (token != accessToken) return;

                if (update == null) return;

                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
                {
                    if (update.CallbackQuery != null && !string.IsNullOrEmpty(update.CallbackQuery.Data) &&
                        update.CallbackQuery.Message != null && !string.IsNullOrEmpty(update.CallbackQuery.Message.Text))
                    {
                        ProcessCallbackQuery(update);
                    }
                }
                else if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    if (update.Message != null && !string.IsNullOrEmpty(update.Message.Text))
                    {
                        ProcessMessage(update);
                    }
                }

            }
            catch (Exception e)
            {
                _bot.SendTextMessageAsync(update.Message.Chat.Id,
                    $"Exception: {e.ToString()}");
                //throw;
            }
        }

        private void ProcessMessage(Update update)
        {
            if (update.Message.Text.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                StartCommand(update);
            }
            else if (update.Message.Text.Equals("/subscribe", StringComparison.OrdinalIgnoreCase))
            {
                SubscribeCommand(update);
            }
            else if (update.Message.Text.Equals("/unsubscribe", StringComparison.OrdinalIgnoreCase))
            {
                UnSubscribeCommand(update);
            }
            else if (update.Message.Text.Equals("/stop", StringComparison.OrdinalIgnoreCase))
            {
                StopCommand(update);
            }
            else if (update.Message.Text.Equals("/status", StringComparison.OrdinalIgnoreCase))
            {
                SubscribeStatusCommand(update);
            }
            else if (update.Message.Text.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
            {
                AdminCommand(update);
            }
            else
            {
                ProcessPlanText(update);
            }
        }

        private void ProcessPlanText(Update update)
        {
            if (_settings.AdminId.HasValue)
            {
                _bot.SendTextMessageAsync(update.Message.Chat.Id, $"Your text translate to Sky54Bot Admin.");

                _bot.SendTextMessageAsync(_settings.AdminId.Value,
                    $"Query text by user " + update.Message.Chat.Username + " (" +
                    (update.Message.Chat.FirstName + " " + update.Message.Chat.LastName).Trim() + ")" +
                    ": " + update.Message.Text);
            }
        }

        private void AdminCommand(Update update)
        {
            _settings.AdminId = update.Message.Chat.Id;
            _settings.AdminName = update.Message.Chat.Username;

            _bot.SendTextMessageAsync(update.Message.Chat.Id,
                $"Set admin is " + update.Message.Chat.Username + ".");
        }

        private void SubscribeStatusCommand(Update update)
        {
            var status = _dataAccess.SubscribesDataAccess.SubscribeStatus(update.Message.Chat.Id.ToString());
            if (status)
            {
                _bot.SendTextMessageAsync(update.Message.Chat.Id, $"You are subscribed.");
            }
            else
            {
                _bot.SendTextMessageAsync(update.Message.Chat.Id, $"You are unsubscribed.");
            }
        }

        private void StopCommand(Update update)
        {
            _dataAccess.SubscribesDataAccess.UnSubscribe(update.Message.Chat.Id.ToString());

            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"I unsubscribe you.");
            _bot.SendTextMessageAsync(update.Message.Chat.Id, $"Bye. I will see you later.");
        }

        private void UnSubscribeCommand(Update update)
        {
            var status = _dataAccess.SubscribesDataAccess.SubscribeStatus(update.Message.Chat.Id.ToString());

            if (!status)
            {
                _bot.SendTextMessageAsync(update.Message.Chat.Id, $"You are already unsubscribed.");
            }
            else
            {
                _dataAccess.SubscribesDataAccess.UnSubscribe(update.Message.Chat.Id.ToString());
                _bot.SendTextMessageAsync(update.Message.Chat.Id, $"I unsubscribe you.");
                _bot.SendTextMessageAsync(update.Message.Chat.Id, $"Bye. I will see you later.");
            }
        }

        private void SubscribeCommand(Update update)
        {
            var status = _dataAccess.SubscribesDataAccess.SubscribeStatus(update.Message.Chat.Id.ToString());

            if (status)
            {
                _bot.SendTextMessageAsync(update.Message.Chat.Id, $"You are already subscribed.");
            }
            else
            {
                _dataAccess.SubscribesDataAccess.Subscribe(update.Message.Chat.Id.ToString(), 
                    update.Message.Chat.Username + " (" +
                    (update.Message.Chat.FirstName + " " + update.Message.Chat.LastName).Trim() + ")");
                _bot.SendTextMessageAsync(update.Message.Chat.Id, $"You subscribe success.");
            }
        }

        private void StartCommand(Update update)
        {
            _bot.SendTextMessageAsync(update.Message.Chat.Id,
                $"Hello! I am Sky54Bot. Welcome!" + Environment.NewLine + Environment.NewLine +
                "Commands:" + Environment.NewLine +
                "/subscribe - Subscribe on updates." + Environment.NewLine +
                "/unsubscribe - Unsubscribe on updates." + Environment.NewLine +
                "/status - Status subscribe on updates." + Environment.NewLine + Environment.NewLine +
                "\U0001F609" + " Write your features in this chat.");
        }

        private void ProcessCallbackQuery(Update update)
        {
            
        }
    }
}
