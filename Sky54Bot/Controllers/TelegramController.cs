﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sky54Bot.DataAccesses;
using Sky54Bot.Models;
using Sky54Bot.Storages;
using Sky54Bot.Storages.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Fizzler.Systems.HtmlAgilityPack;

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

            return View(new SubscribesViewModel { EntityList = subscriptions != null ? subscriptions.ToList() : new List<SubscribeEntity>() });
        }

        [HttpGet("check")]
        public void Check()
        {
            var subscriptions = _dataAccess.SubscribesDataAccess.GetSubscribes();

            if (subscriptions != null && subscriptions.Length > 0)
            {
                var htmlStr = GetHtmlStr();

                string compareText = _dataAccess.SettingsDataAccess.ReadSetting("temp");

                var text = FormatText(htmlStr, ref compareText);
                if (!string.IsNullOrEmpty(text))
                {
                    _dataAccess.SettingsDataAccess.WriteSetting("temp", compareText);

                    foreach (var subscription in subscriptions)
                    {
                        _bot.SendTextMessageAsync(int.Parse(subscription.ChatId), text);
                    }
                }
            }
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
            else if (update.Message.Text.Equals("/now", StringComparison.OrdinalIgnoreCase))
            {
                NowCommand(update);
            }
            else if (update.Message.Text.Equals("/today", StringComparison.OrdinalIgnoreCase))
            {
                TodayCommand(update);
            }
            else if (update.Message.Text.Equals("/nextday", StringComparison.OrdinalIgnoreCase))
            {
                NextdayCommand(update);
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

        private static Dictionary<string, string> iconMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "partly_cloudy_none_night", "\U00002601" }, // Ночь, Переменная облачность, без осадков
            { "partly_cloudy_none_day", "\U0001F324" }, // День, Переменная облачность, без осадков
            { "mostly_cloudy_none_day", "\U0001F325" }, // День, Облачно, без осадков
            { "mostly_cloudy_none_night", "\U00002601" }, // Ночь, Облачно, без осадков
            { "partly_cloudy_light_rain_day", "\U000026C8" }, // День, Переменная облачность, небольшие дожди, гроза
            { "partly_cloudy_rainless_day", "\U0001F326" }, // День, Переменная облачность, без существенных осадков
            { "partly_cloudy_rain_day", "\U000026C8" }, // День, Переменная облачность, дождь
            { "partly_cloudy_rainless_night", "\U0001F327" }, // Ночь, Переменная облачность, без существенных осадков
            { "cloudy_rain_night", "\U000026C8" }, // Ночь, Пасмурно, дождь
            { "cloudy_rain_day", "\U000026C8" }, // День, Пасмурно, дождь
            { "cloudy_light_rain_day", "\U000026C8" }, // День, Пасмурно, дождь
            { "cloudy_none_night", "\U00002601" }, // Ночь, Пасмурно, без осадков
            { "cloudy_none_day", "\U0001F325" }, // День, Пасмурно, без осадков
            { "cloudy_rainless_night", "\U00002601" }, // Пасмурно, без существенных осадков
            { "sunshine_none_night", " \U00002609" }, // Ночь, Ясная погода, без осадков
            { "sunshine_none_day", "\U00002600" }, // День, Ясная погода, без осадков
            { "mostly_cloudy_rain_night", "\U0001F327" }, // Ночь, Облачно, дождь
            { "mostly_cloudy_rain_day", "\U0001F326" }, // День, Облачно, дождь
            { "mostly_cloudy_light_rain_night", "\U0001F327"}, // Ночь, Облачно, дождь
            { "mostly_cloudy_light_rain_day", "\U0001F327" }, // Облачно, небольшие дожди

            { "m-box__icon-wind_west", "\U00002192" },
            { "m-box__icon-wind_north", "\U00002191" },
            { "m-box__icon-wind_south", "\U00002193" },
            { "m-box__icon-wind_east", "\U00002190" },
            { "m-box__icon-wind_south_west", "\U00002199" },
            { "m-box__icon-wind_north_west", "\U00002196" },
            { "m-box__icon-wind_north_east", "\U00002197" },
            { "m-box__icon-wind_south_east", "\U00002198" },
            { "m-box__icon-wind_calm", "\U00002B58" }
        };

        private string GetHtmlStr()
        {
            var uri = new Uri("https://m.pogoda.ngs.ru/");
            var htmlStr = new WebClient().DownloadString(uri);

            return htmlStr;
        }

        private string FormatText(string htmlStr, ref string compareText)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlStr);

            /*foreach (HtmlNode node in html.DocumentNode.QuerySelectorAll("h3.r a"))
                Console.WriteLine(node.GetAttributeValue("href", null));*/


            var document = html.DocumentNode;

            var boxToaday = document.QuerySelector(".m-box-today");
            var icon = boxToaday.QuerySelector(".m-box__icon-big").GetAttributeValue("class", null);

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(icon))
            {
                var classes = icon.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string iconParsed = null;
                foreach (var @class in classes)
                {
                    if (iconMapping.ContainsKey(@class))
                    {
                        iconParsed = iconMapping[@class];
                        break;
                    }
                }

                icon = iconParsed;
            }

            var temp = boxToaday.QuerySelector(".m-box-today__temp").InnerText;
            if (!string.IsNullOrEmpty(temp))
                temp = temp.Replace("&minus;", "-");

            if (!string.IsNullOrEmpty(compareText))
            {
                if (string.Equals(temp, compareText, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            compareText = temp;

            var about = boxToaday.QuerySelectorAll(".m-box-today__about .m-box__text");



            /*sb.AppendLine("0263C \U0000263C");
            sb.AppendLine("02600\U00002600");
            sb.AppendLine("02609\U00002609");
            sb.AppendLine("02601\U00002601");
            sb.AppendLine("026C5\U000026C5");
            sb.AppendLine("026C8\U000026C8");
            sb.AppendLine("1F324\U0001F324");
            sb.AppendLine("1F325\U0001F325");
            sb.AppendLine("1F326\U0001F326");
            sb.AppendLine("1F327\U0001F327");
            sb.AppendLine("1F328\U0001F328");
            sb.AppendLine("1F329\U0001F329");
            sb.AppendLine("1F32A\U0001F32A");
            sb.AppendLine("02602\U00002602");
            sb.AppendLine("02614\U00002614");
            sb.AppendLine("1F4A7\U0001F4A7");
            sb.AppendLine("1F525\U0001F525");*/

            sb.Append($"Температура {temp}C \U0001F321");

            string feelInfo = null;
            string additionalInfo = null;

            foreach (var node in about)
            {
                var text = node.InnerText;
                if (!string.IsNullOrEmpty(text))
                {
                    if (text.IndexOf("ощущается", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        feelInfo = text;
                        if (!string.IsNullOrEmpty(feelInfo))
                            feelInfo = feelInfo.Replace("&minus;", "-");
                    }
                    else
                    {
                        additionalInfo = text;
                    }
                }
            }

            if (!string.IsNullOrEmpty(feelInfo))
            {
                sb.Append(System.Environment.NewLine);
                sb.Append($"{feelInfo}C \U0001F321");
            }

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                sb.Append(System.Environment.NewLine);

                sb.Append($"{additionalInfo}");
                if (!string.IsNullOrEmpty(icon))
                    sb.Append($" ( {icon} )");
            }

            var todayInfo = boxToaday.QuerySelector(".m-box-today__info");

            var wind = todayInfo.QuerySelector(".m-box__icon-wind");
            var windText = wind?.NextSibling.InnerText;

            var windIcon = wind?.GetAttributeValue("class", null);

            if (!string.IsNullOrEmpty(windIcon))
            {
                var classes = windIcon.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                string iconParsed = null;
                foreach (var @class in classes)
                {
                    if (iconMapping.ContainsKey(@class))
                    {
                        iconParsed = iconMapping[@class];
                        break;
                    }
                }

                windIcon = iconParsed;
            }

            if (!string.IsNullOrEmpty(windText))
            {
                sb.Append(System.Environment.NewLine);

                sb.Append($"Ветер {windText.Replace("&nbsp;", " ")}");

                if (!string.IsNullOrEmpty(windIcon))
                    sb.Append($" ( {windIcon} )");
            }

            var pressure = todayInfo.QuerySelector(".m-box__icon-pressure");
            var pressureText = pressure.NextSibling.InnerText;

            if (!string.IsNullOrEmpty(pressureText))
            {
                sb.Append(System.Environment.NewLine);
                sb.Append($"Давление {pressureText.Replace("&nbsp;", " ")} \U0001F550");
            }

            var humidity = todayInfo.QuerySelector(".m-box__icon-humidity");
            var humidityText = humidity.NextSibling.InnerText;

            if (!string.IsNullOrEmpty(humidityText))
            {
                sb.Append(System.Environment.NewLine);
                sb.Append($"Влажность {humidityText.Replace("&nbsp;", " ")} \U0001F4A7");
            }

            var todayDesc = boxToaday.QuerySelector(".m-box-today__desc");

            var todayDescLists = todayDesc.QuerySelectorAll(".m-box-today__desc-list");

            var todayDescListItems = todayDescLists.First().QuerySelectorAll(".m-box-today__desc-item");

            foreach (var item in todayDescListItems)
            {
                var span = item.ChildNodes.First();
                sb.Append(System.Environment.NewLine);
                sb.Append($"{span.InnerText}");
                sb.Append($" {span.NextSibling.InnerText.Replace("&minus;", "‒").Replace("&nbsp;", " ")}");
            }

            var magnetic = todayDesc.QuerySelector(".m-box__magnetic_status")?.InnerText;
            if (!string.IsNullOrEmpty(magnetic))
            {
                sb.Append(System.Environment.NewLine);
                sb.Append($"{magnetic.Replace("&nbsp;", " ")}");
            }

            var source = todayDesc.QuerySelector(".m-box__source")?.InnerText;
            if (!string.IsNullOrEmpty(source) && source.Contains("обновлен", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(System.Environment.NewLine);
                var text = source.Replace("&nbsp;", " ");

                text = text.Substring(text.IndexOf("обновлен", StringComparison.OrdinalIgnoreCase));

                sb.Append($"{text}");
            }

            return sb.ToString();
        }

        private string FormatTextToday(string htmlStr)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlStr);

            /*foreach (HtmlNode node in html.DocumentNode.QuerySelectorAll("h3.r a"))
                Console.WriteLine(node.GetAttributeValue("href", null));*/


            var document = html.DocumentNode;

            var sb = new StringBuilder();

            var boxTodays = document.QuerySelector(".m-box-weather .m-box-weather__list");

            if (boxTodays != null && boxTodays.ChildNodes.Count > 0)
            {
                var today = boxTodays.ChildNodes.First();

                var header = today.QuerySelectorAll(".m-box-weather__header").First()
                    .QuerySelectorAll(".m-box-weather__header-item").First().QuerySelector(".m-box__text").InnerHtml;
                if (!string.IsNullOrEmpty(header))
                {
                    sb.Append($"{header.Replace("<span class=\"js-weather-title\">сегодня,&nbsp;</span>", "сегодня, ").Replace("<br>", ", ").Replace("&nbsp;", " ")}");
                }

                var weathers = today.QuerySelectorAll(".m-box-weather__row .m-box-weather__cell");
                if (weathers != null)
                {
                    foreach (var node in weathers)
                    {
                        var nodeClasses = node.GetAttributeValue("class", null);
                        var isNext = false;
                        if (!string.IsNullOrEmpty(nodeClasses))
                        {
                            var classes = nodeClasses.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var @class in classes)
                            {
                                if (string.Equals(@class, "m-box-weather__cell_past", StringComparison.OrdinalIgnoreCase))
                                {
                                    isNext = true;
                                    break;
                                }
                            }
                        }

                        if (isNext) continue;

                        var texts = node.QuerySelectorAll(".m-box__text");
                        var icon = node.QuerySelector(".m-box__icon").GetAttributeValue("class", null);

                        if (!string.IsNullOrEmpty(icon))
                        {
                            var classes = icon.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            string iconParsed = null;
                            foreach (var @class in classes)
                            {
                                if (iconMapping.ContainsKey(@class))
                                {
                                    iconParsed = iconMapping[@class];
                                    break;
                                }
                            }

                            icon = iconParsed;
                        }
                        
                        var value = node.QuerySelector(".m-box-weather__value").InnerText;
                        if (!string.IsNullOrEmpty(value))
                            value = value.Replace("&minus;", "-");

                        sb.Append(System.Environment.NewLine);
                        sb.Append(System.Environment.NewLine);
                        sb.Append($"{repl(texts.First().InnerText.Replace("&nbsp;", " "))} {value.Replace("&nbsp;", " ")}°C \U0001F321");
                        if (!string.IsNullOrEmpty(icon))
                            sb.Append($" ( {icon} )");

                        sb.Append(System.Environment.NewLine);
                        sb.Append($"{texts.Last().InnerHtml.Replace("<br>", ", ").Replace("&nbsp;", " ").ToLower()}");
                    }
                }
            }
            
            return sb.ToString();
        }

        private string FormatTextNextday(string htmlStr)
        {
            var html = new HtmlDocument();
            html.LoadHtml(htmlStr);

            /*foreach (HtmlNode node in html.DocumentNode.QuerySelectorAll("h3.r a"))
                Console.WriteLine(node.GetAttributeValue("href", null));*/


            var document = html.DocumentNode;

            var sb = new StringBuilder();

            var boxTodays = document.QuerySelector(".m-box-weather .m-box-weather__list");

            if (boxTodays != null && boxTodays.ChildNodes.Count > 0)
            {
                var today = boxTodays.ChildNodes.First().NextSibling;

                var header = today.QuerySelectorAll(".m-box-weather__header").First()
                    .QuerySelectorAll(".m-box-weather__header-item").First().QuerySelector(".m-box__text").InnerHtml;
                if (!string.IsNullOrEmpty(header))
                {
                    sb.Append($"{header.Replace("<span class=\"js-weather-title\">завтра,&nbsp;</span>", "завтра, ").Replace("<br>", ", ").Replace("&nbsp;", " ")}");
                }

                var weathers = today.QuerySelectorAll(".m-box-weather__row .m-box-weather__cell");
                if (weathers != null)
                {
                    foreach (var node in weathers)
                    {
                        var nodeClasses = node.GetAttributeValue("class", null);
                        var isNext = false;
                        if (!string.IsNullOrEmpty(nodeClasses))
                        {
                            var classes = nodeClasses.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var @class in classes)
                            {
                                if (string.Equals(@class, "m-box-weather__cell_past", StringComparison.OrdinalIgnoreCase))
                                {
                                    isNext = true;
                                    break;
                                }
                            }
                        }

                        if (isNext) continue;

                        var texts = node.QuerySelectorAll(".m-box__text");
                        var icon = node.QuerySelector(".m-box__icon").GetAttributeValue("class", null);

                        if (!string.IsNullOrEmpty(icon))
                        {
                            var classes = icon.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            string iconParsed = null;
                            foreach (var @class in classes)
                            {
                                if (iconMapping.ContainsKey(@class))
                                {
                                    iconParsed = iconMapping[@class];
                                    break;
                                }
                            }

                            icon = iconParsed;
                        }

                        var value = node.QuerySelector(".m-box-weather__value").InnerText;
                        if (!string.IsNullOrEmpty(value))
                            value = value.Replace("&minus;", "-");

                        sb.Append(System.Environment.NewLine);
                        sb.Append(System.Environment.NewLine);
                        sb.Append($"{repl(texts.First().InnerText.Replace("&nbsp;", " "))} {value.Replace("&nbsp;", " ")}°C \U0001F321");
                        if (!string.IsNullOrEmpty(icon))
                            sb.Append($" ( {icon} )");

                        sb.Append(System.Environment.NewLine);
                        sb.Append($"{texts.Last().InnerHtml.Replace("<br>", ", ").Replace("&nbsp;", " ").ToLower()}");
                    }
                }
            }

            return sb.ToString();
        }


        private string repl(string s)
        {
            return s.Replace("ночь", "ночью").Replace("утро", "утром").Replace("день", "днем").Replace("вечер", "вечером");
        }

        private void NowCommand(Update update)
        {
            var htmlStr = GetHtmlStr();
            string compareText = null;
            var text = FormatText(htmlStr, ref compareText);
            if(!string.IsNullOrEmpty(text))
                _bot.SendTextMessageAsync(update.Message.Chat.Id, text);

            /*


                        // yields: [<p class="content">Fizzler</p>]
                        document.QuerySelectorAll(".content");

                        // yields: [<p class="content">Fizzler</p>,<p>CSS Selector Engine</p>]
                        document.QuerySelectorAll("p");

                        // yields empty sequence
                        document.QuerySelectorAll("body>p");

                        // yields [<p class="content">Fizzler</p>,<p>CSS Selector Engine</p>]
                        document.QuerySelectorAll("body p");

                        // yields [<p class="content">Fizzler</p>]
                        document.QuerySelectorAll("p:first-child");*/
        }

        private void TodayCommand(Update update)
        {
            var htmlStr = GetHtmlStr();
            var text = FormatTextToday(htmlStr);
            if (!string.IsNullOrEmpty(text))
                _bot.SendTextMessageAsync(update.Message.Chat.Id, text);
        }

        private void NextdayCommand(Update update)
        {
            var htmlStr = GetHtmlStr();
            var text = FormatTextNextday(htmlStr);
            if (!string.IsNullOrEmpty(text))
                _bot.SendTextMessageAsync(update.Message.Chat.Id, text);
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
                $"Hello! I'm Sky54Bot. Welcome!" + Environment.NewLine + Environment.NewLine +
                "Commands:" + Environment.NewLine +
                "/now - get current weather." + Environment.NewLine +
                "/today - get today weather." + Environment.NewLine +
                "/nextday - get nextday weather." + Environment.NewLine +
                "/subscribe - subscribe on bot." + Environment.NewLine +
                "/unsubscribe - unsubscribe on bot." + Environment.NewLine +
                "/status - status subscribe on bot." + Environment.NewLine + Environment.NewLine +
                "\U0001F609" + " Write your features in this chat.");
        }

        private void ProcessCallbackQuery(Update update)
        {
            
        }
    }
}
