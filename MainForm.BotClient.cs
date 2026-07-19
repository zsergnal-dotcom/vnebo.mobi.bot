using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using vnebo.mobi.bot.Libs;
using vnebo.mobi.bot.Properties;

namespace vnebo.mobi.bot
{
    public partial class MainFormAll
    {
        private Dictionary<string, string> GetBotSettings(int botId) =>
            settings.GetSett($"USER_{botId}");

        private HttpClient CreateBotHttpClient(Dictionary<string, string> accountSettings, out HttpClientHandler handler)
        {
            handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true
            };

            HttpClient client = HelpMethod.HttpManager(accountSettings["LOGIN"], handler);

            try
            {
                client.BaseAddress = new Uri("https://" + accountSettings["SERVER"]);
            }
            catch (Exception ) { }
            return client;
        }

        private async Task<string> EnsureAuthorizedAsync(
            int botId,
            HttpClient client,
            Dictionary<string, string> accountSettings,
            bool logMailRuHintOnFailure = false,
            bool logAuthFailure = true)
        {
            string result = await HelpMethod.Get("/home", client);
            if (!result.Contains("Мой профиль"))
            {
                HelpMethod.StatusLog("Авторизация...", botId, this, Resources.auth);
                HelpMethod.Log("Авторизация ", botId, this);
                result = await BotEngine.Authorization(
                    accountSettings["LOGIN"],
                    accountSettings["PASSWORD"],
                    client,
                    accountSettings["SERVER"]);
            }

            if (!result.Contains("Мой профиль") && logAuthFailure)
            {
                HelpMethod.Log("Ошибка авторизации ", botId, this);
                if (logMailRuHintOnFailure)
                {
                    LogMailRuLoginHint(botId, accountSettings);
                }
            }

            return result;
        }

        private void LogMailRuLoginHint(int botId, Dictionary<string, string> accountSettings)
        {
            if (accountSettings["SERVER"] != "mm.vnebo.mobi")
            {
                return;
            }

            if (accountSettings["LOGIN"].IndexOf("@") == -1 || accountSettings["LOGIN"].IndexOf(".") == -1)
            {
                HelpMethod.Log("Login должен быть в формате login@servermail.ru ", botId, this);
                HelpMethod.Log("Например: Zserg@mail.ru ", botId, this);
            }
        }

        private void RunAuthorizedAction(
            int botId,
            Func<HttpClient, Dictionary<string, string>, string, Task> action,
            bool setStartFlag = false,
            bool logMailRuHintOnFailure = false,
            bool disposeClientOnAuthFailure = false)
        {
            if (setStartFlag)
            {
                Start[$"{botId}"] = true;
            }

            Dictionary<string, string> accountSettings = GetBotSettings(botId);
            HttpClient client = CreateBotHttpClient(accountSettings, out HttpClientHandler handler);

            Task.Run(async () =>
            {
                string authResult = await EnsureAuthorizedAsync(botId, client, accountSettings, logMailRuHintOnFailure);
                if (!authResult.Contains("Мой профиль"))
                {
                    if (disposeClientOnAuthFailure)
                    {
                        client.Dispose();
                    }
                    return;
                }

                await action(client, accountSettings, authResult);
            });
        }
    }
}
