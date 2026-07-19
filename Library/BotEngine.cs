using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using vnebo.mobi.bot.Library;
using vnebo.mobi.bot.Properties;

namespace vnebo.mobi.bot.Libs
{

    internal class BotEngine
    {
        const int FEATURE_DISABLE_NAVIGATION_SOUNDS = 21;
        const int SET_FEATURE_ON_PROCESS = 0x00000002;
        const string DEF_PASS = "1qaz@WSX";
        // Patch: no-op comment to ensure patch context alignment
        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(int FeatureEntry,
                                                      [MarshalAs(UnmanagedType.U4)] int dwFlags,
                                                      bool fEnable);
        private const string page1 = @"

<!DOCTYPE html>
<html>
<head><meta http-equiv=""X-UA-Compatible"" content=""IE=11"">
<link rel=""stylesheet"" type=""text/css"" href=""%SERVER%/images/style.css"">
</head>
<body>
<form id=""form1"" runat=""server"">
<div class=""lvlPrgbr"">
<div class=""prgbr"">
<div class=""prg"" style=""width:%LVL_PRC%%;""></div>
</div>
</div>
<div class=""nfl ny"">
<div class=""snow"">
<span class=""amount""><strong>
<span class=""user""><span><span>%NAME% %CUP%</span></span></span><span class=""minor""></span></strong></span>
<div class=""ln m5""></div>
%CITY%

<div class=""small m5 rstat"">
<span class=""fll"">
<img src=""%SERVER%/images/icons/star.png"" width=""16"" height=""16""> Уровень: <strong class=""white"">%LEVEL%</strong>
</span>

<span class=""flr"">
<img src=""%SERVER%/images/icons/hd_nebo.png"" width=""16"" height=""16""> Этажей: <strong class=""white"">%FLOOR%</strong>
</span>
<div class=""clb""></div>
<div><br/>%KOL_MAN%</div>
<div><br/>
<img src=""%SERVER%/images/icons/mn_iron.png"" width=""16"" height=""16"">%COIN% 
<img src=""%SERVER%/images/icons/mn_gold.png"" width=""16"" height=""16"">%BAKS% 
<img src=""%SERVER%/images/icons/key.png"" width=""16"" height=""16"">%KEY%
</div>
</div>
</div>
</div>

%VYR%
<div>%PQ%</div>
<div>%TQ%</div>
<div>%PK%</div>
<div>%ACTION%</div>
   </form>
</body>
</html>
";

       // private static readonly IniFiles settings = new IniFiles();
        static void DisableClickSounds()
        {
            CoInternetSetFeatureEnabled(FEATURE_DISABLE_NAVIGATION_SOUNDS,
                                        SET_FEATURE_ON_PROCESS,
                                        true);
        }

        public static async Task<string> Authorization(string Login, string Password, HttpClient client, string server)
        {
            string AuthorizationResult;
            switch (server)
            {
                case "odkl.vnebo.mobi":
                    AuthorizationResult = await AuthorizationOK(Login, Password, client);
                    break;
                case "mm.vnebo.mobi":
                    AuthorizationResult = await AuthorizationMail(Login, Password, client);
                    break;
                case "fs.vnebo.mobi":
                    AuthorizationResult = await AuthorizationFoto(Login, Password, client);
                    break;
                default:
                    AuthorizationResult = await AuthorizationNebo(Login, Password, client);
                    break;
            }
            return AuthorizationResult;
        }
        /// <summary>
        /// Метод, который авторизуется в игре.
        /// </summary>
        /// <param name="Login">Логин пользователя.</param>
        /// <param name="Password">Пароль пользователя.</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        public static async Task<string> AuthorizationNebo(string Login, string Password, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                
                // Отправляем запрос на страницу логина
                string result = await HelpMethod.Get("/login", client, cancellationToken);
                // Парсим скрытое поле
                string hidden_input = new Regex("<input type=\"hidden\" name=\"(.*?)\" id=").Match(result).Groups[1].Value;
                string url = new Regex("action=\"(.*?)\" id=").Match(result).Groups[1].Value;

                // Генерируем POST запрос
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    {hidden_input, "" },
                    {"login", Login },
                    {"password", Password },
                    { ":submit", "Вход" }
                };

                // Отправляем запрос
                return await HelpMethod.Post(url, parameters, client);

            }
            catch (Exception ex)
            {
                Logger.Write("ex autoriz=" + ex);
                return "error";
            }
        }
        [STAThread]
        public static async Task<string> AuthorizationOK(string Login, string Password, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                string url = "https://m.ok.ru/cdk/st.cmd/main/st.redirect/%252Fdk/st.intended/on/_prevCmd/appLauncher/tkn/3756";
                //url = "https://m.ok.ru/cdk/st.cmd/main/st.lgi/KjDP5uqfAqlL/_prevCmd/loginForHref/tkn/2372";
                string res = await HelpMethod.Get(url, client);
                string findstr = new Regex("action=\"(.*?)\" ").Match(res).Groups[1].Value.Replace("&amp;", "&");
                Dictionary<string, string> formData = new Dictionary<string, string>
                {
                     { "fr.login", Login },
                     { "fr.password", Password },
                     { "button_login", "Войти" },
                     { "fr.posted", "set" },
                     { "fr.needCaptcha", "" }
                };
                 res=await HelpMethod.Post(url + findstr, formData, client);
 
                findstr = new Regex("<div class=\"block  registration_head block-text\">(.*?)</div>").Match(res).Groups[1].Value;
                if (findstr.Length > 0) {
                    string label1 = new Regex("<h1.*?>(.*?)</h1>").Match(res).Groups[1].Value;
                    string label2 = "";
                   /* foreach (Match match in Regex.Matches(res, "<div class=\"block \">(.+?)</div>", RegexOptions.Singleline | RegexOptions.Multiline))
                    {
                        label2+= match.Groups[1].Value+" \n";
                    }*/
                    string txtBut = new Regex("name=\"confirmPhone\" value=\"(.*?)\" type=\"submit\"").Match(res).Groups[1].Value;
                    //Dictionary<string, string> account_settings = settings.GetSett($"USER_{BotID}");
                    // Web Confir =  new Web(account_settings,res);
                    findstr = new Regex("action=\"(.*?)\" ").Match(res).Groups[1].Value.Replace("&amp;", "&");
                    formData = new Dictionary<string, string>
                {
                     { "rfr.posted", "set" },
                     { "confirmPhone", "Отправить код" }
                };
                    res = await HelpMethod.Post(url + findstr, formData, client);
                    foreach (Match match in Regex.Matches(res, "<div class=\"block \">(.+?)</div>", RegexOptions.Singleline | RegexOptions.Multiline))
                    {
                        label2 += match.Groups[1].Value + " \n";
                    }

                    Confirum Confir = new Confirum(label1,label2,txtBut);
                    string code = "";
                    if (Confir.ShowDialog() == DialogResult.OK) {
                        //"rfr.smsCode"
                        code = Confir.code;
                        formData = new Dictionary<string, string>
                {
                     { "rfr.smsCode", code },
                     { "confirmPhone", "Отправить код" },
                     { "rfr.posted", "set"}
                };
                        findstr = new Regex("action=\"(.*?)\" ").Match(res).Groups[1].Value.Replace("&amp;", "&");
                        res = await HelpMethod.Post(url + findstr, formData, client);
    /*                    TextWriter tw2 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "logok.html");
                        tw2.Write(res);
                        tw2.Close();*/

                    }
                }

                res = await HelpMethod.Get("https://m.ok.ru/game/nebo", client);
                findstr = new Regex("src=\"(.{8}odkl.vnebo.mobi.*?)\" data-").Match(res).Groups[1].Value.Replace("&amp;", "&");
                res = await HelpMethod.Get(findstr, client);

                return res;
            }
            catch (Exception ex)
            {
                Logger.Write("ex autoriz OK=" + ex);
                return "error";
            }
        }
        public static async Task<string> AuthorizationMail(string Login, string Password, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
 
                HttpClientHandler hand = new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    UseCookies = true
                };
                string url;
                string res;
                url = "https://connect.mail.ru/oauth/authorize?client_id=702652&response_type=code&redirect_uri=http://m.my.mail.ru/apps/702652";
                res = await HelpMethod.Get(url, client, cancellationToken);

                string findstr = new Regex("name=\"Page\" value=\"(.*?)\"/>").Match(res).Groups[1].Value.Replace("&amp;", "&");
                if (findstr.Length > 0) //если требуется авторизация
                {
                    string[] strArray = Login.Split('@');
                    url = "https://auth.mail.ru/cgi-bin/auth";

                    Dictionary<string, string> formData = new Dictionary<string, string>
                {
                     { "Login", strArray[0] },
                     { "Password", Password },
                     { "page", findstr },
                     { "Domain", strArray[1] },
                     { "submit", "" }
                                     };
                    //// Отправляем запрос
                    res = await HelpMethod.Post(url, formData, client);
                     HelpMethod.SaveCooc(Login, hand);
                }
                url = "https://m.my.mail.ru/apps/702652";
                res = await HelpMethod.Get(url, client, cancellationToken);

                url = "https://m.my.mail.ru/cgi-bin/app/mobile_redirect?app_id=702652";
                res = await HelpMethod.Get(url, client);
                return res;
            }
            catch (Exception ex)
            {
                Logger.WriteError("ex autoriz mail=" + ex);
                return "error";
            }
        }


        public static async Task<string> AuthorizationFoto(string Login, string Password, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            try
            {
                HttpClientHandler hand = new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    UseCookies = true,
                    PreAuthenticate=true,
                    UseDefaultCredentials=true
                };
                string url;
                string res;
                url = "https://m.fotostrana.ru/games/nebo/entrance/";

                res = await HelpMethod.Get(url, client);
                

                if (!res.Contains("Мой профиль"))
                {
                     url = "https://fotostrana.ru/signup/login/";
                    url = "https://m.fotostrana.ru/start/main/login/";
                    //url = "https://fotostrana.ru/signup/login/?redirect_url=%2Fapp%2Fnebo%2F";
                    res = await HelpMethod.Get(url, client);
                   /* TextWriter tw0 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "fs res do login.html");
                    tw0.Write(res);
                    tw0.Close();*/
                    
                    string findstr = new Regex("name=\"csrftkn\" value=\"(.*?)\"").Match(res).Groups[1].Value.Replace("&amp;", "&");
                    //url = "https://fotostrana.ru/signup/signup/auth/";
                    url = "https://m.fotostrana.ru/signup/login/?id=14";
                    Dictionary<string, string> formData = new Dictionary<string, string>
                {
                     { "user_email", Login },
                     { "user_password", Password },
                     { "csrftkn", findstr },
                     { "submit", "1" },
                     { "tk", "" }//,
                        //{"submitted","1" }
                                     };
                   /* Console.WriteLine("login = "+Login+" pass = "+Password+" csr = "+findstr);*/
                    //// Отправляем запрос
                    res = await HelpMethod.Post(url, formData, client);
                   /* TextWriter tw1 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "fs res login.html");
                    tw1.Write(res);
                    tw1.Close();*/

                    if (res.Contains("Пожалуйста, введите символы на картинке"))
                    {
                        string findstr_img = "https://m.fotostrana.ru" + new Regex("img alt=\"\" src=\"(.*?)\"").Match(res).Groups[1].Value.Replace("&amp;", "&");
                        Confirum Confir = new Confirum("1", "Введите код", "Отправить",findstr_img);
                        Logger.Write("csr = " + findstr_img);
                        string code = "";
                        if (Confir.ShowDialog() == DialogResult.OK)
                        {
                            findstr = new Regex("name=\"csrftkn\" value=\"(.*?)\"").Match(res).Groups[1].Value.Replace("&amp;", "&");
                            //"rfr.smsCode"
                            code = Confir.code;
                            formData = new Dictionary<string, string>
                {
                      { "user_email", Login },
                     { "user_password", Password },
                     { "csrftkn", findstr },
                     { "captcha_response_field", code },
                     { "tk", "" },
                        {"submit","1" }
                };
                            Logger.Write("csr = " + findstr + " code = " + code + " login = " + Login + " password = " + Password);
                            url = "https://m.fotostrana.ru/signup/login/";
                            res = await HelpMethod.Post(url, formData, client);
                           /* TextWriter tw2c = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "fs res code.html");
                            tw2c.Write(res);
                            tw2c.Close();*/

                        }
                    }
                        HelpMethod.SaveCooc(Login, hand);

 
                    url = "https://m.fotostrana.ru/games/nebo/";
                    res = await HelpMethod.Get(url, client);
                  /*  TextWriter tw2 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "fs res.html");
                    tw2.Write(res);
                    tw2.Close();*/

                    url = "https://m.fotostrana.ru/games/nebo/entrance/";
                    res = await HelpMethod.Get(url, client);
                   /* TextWriter tw3 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "fs res2.html");
                    tw3.Write(res);
                    tw3.Close();*/

                    return res;
                }
                else { return res; }
            }
            catch (Exception ex)
            {
                Logger.WriteError("ex autoriz foto=" + ex);
                return "error";
            }
        }
        /// <summary>
        /// Метод, который собирает выручку с этажей.
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        public static async Task CollectCoins(int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Собираем выручку...", BotID, Form, Resources.thinking);
            // Идём на страницу сбора выручки
            string tek_str = "/floors/5";
            string result = await HelpMethod.Get(tek_str, client, cancellationToken);

            // Если есть проданный товар
            if (result.Contains("Собрать выручку!"))
            {
                HelpMethod.StatusLog("Собираем выручку...", BotID, Form, Resources.st_sold);

                // Общая сумма профита и количество этажей на которых собрано
                int floorCount = 0;

                // Запускаем цикл
                foreach (Match match in Regex.Matches(result, "href=\"(.{40,60}floorPanel.*?)\"", RegexOptions.Singleline | RegexOptions.Multiline))
                {
                     // Парсим первую ссылку этажа и первый профит с этажа
                    // href="./5?139-1.-floors-0-floorPanel-state-action"
                    

                    // Прибавляем количество этажей

                    if (floorCount > 200) { break; }
                    // Забираем выручку
                    result = await HelpMethod.Get(match.Groups[1].Value, client, cancellationToken);
                    // cancellationToken check preserved to allow cooperative cancellation
                    if (cancellationToken.IsCancellationRequested || !MainFormAll.Start[$"{BotID}"]) { return; }
                    floorCount++;
                }
                

                // Логируем
                if (floorCount > 0) { HelpMethod.Log($"Этажей, на которых собрана выручка: {floorCount}", BotID, Form); }
            }


        }
        public static async Task CollectBaraban(int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            //HelpMethod.StatusLog("Собираем выручку...", BotID, Form, Resources.thinking);
            // Идём на страницу сбора выручки
            string result = await HelpMethod.Get("/baraban", client, cancellationToken);

            // Если есть проданный товар
            if (result.Contains("baraban/r/1"))
            {
                HelpMethod.StatusLog("Крутим барабан...", BotID, Form, Resources.thinking);
                result = await HelpMethod.Get("/baraban/r/1", client, cancellationToken);
                if (result.Contains("<span>X1</span>"))
                {
                    string url2 = new Regex("<a class=\"link small m5\" href=\"(.*?)\">").Match(result).Groups[1].Value;
                    result = await HelpMethod.Get(url2, client, cancellationToken);

                }
                // Общая сумма профита и количество этажей на которых собрано
                int barab = 0;

                // Запускаем цикл
                do
                {
                    // Парсим первую ссылку этажа и первый профит с этажа
                    // string ost= new Regex("<span class=\"white\">([0-9]{1,3})</span>").Match(result).Groups[1].Value;
                    string url = new Regex("<a class=\"tdu\" href=\"(.*?)\">").Match(result).Groups[1].Value;

                    // Прибавляем количество этажей

                    if (barab > 200) { break; }
                    HelpMethod.StatusLog($"Крутим барабан: {barab}", BotID, Form, Resources.thinking);
                    // Забираем выручку
                    await HelpMethod.Get(url, client, cancellationToken);
                    if (!MainFormAll.Start[$"{BotID}"]) { return; }
                    barab++;
                    result = await HelpMethod.Get("/baraban/r/1", client, cancellationToken);
                }
                while (result.Contains("Достать шар за ")|| result.Contains("Достать 10 шаров за "));

                // Логируем
                if (barab > 1) { HelpMethod.Log($"Достали шаров: {barab}", BotID, Form); }
            }


        }

        public static async Task CollectFootball(int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            //HelpMethod.StatusLog("Собираем выручку...", BotID, Form, Resources.thinking);
            // Идём на страницу сбора выручки
            string result = await HelpMethod.Get("/football", client, cancellationToken);
            int i = 0;

            // Если есть проданный товар
            if (result.Contains("-buttonsBlock-")|| result.Contains("-getPrizeLink")|| result.Contains("-nextLink"))
            {
                Random rnd = new Random();
                
        
                //href="./football?1512-1.-buttonsBlock-leftLink&amp;action=1784445205779">
                //href="./football?0-25.-buttonsBlock-rightLink&amp;action=1784445205779&amp;action=1784446004916"><img
                
                do
                {
                    int rl = rnd.Next(1, 3);
                    string url2 = new Regex("href=\"(.{10,70}-buttonsBlock-.*?)\">.*?href=\"(.{10,70}-buttonsBlock-.*?)\">", RegexOptions.Singleline | RegexOptions.Multiline).Match(result).Groups[rl].Value;
                    i++;
                    if (url2.Length > 0) { result = await HelpMethod.Get(url2, client, cancellationToken); } 
                    //<a href="./football?21-1.-nextLink&amp;p=1&amp;k=2&amp;action=1784448694172">Далее</a>
                    if(result.Contains("-nextLink"))
                    {
                        string url3 = new Regex("<a href=\"(.{10,70}-nextLink.*?)\">").Match(result).Groups[1].Value;
                        await HelpMethod.Get(url3, client, cancellationToken);
                        result = await HelpMethod.Get("/football", client, cancellationToken);
                    }
                    //<a href="./football?18-1.-getPrizeLink&amp;p=1&amp;k=2&amp;action=1784448352647">Получить награду!</a>
                    if (result.Contains("-getPrizeLink"))
                    {
                        string url3 = new Regex("<a href=\"(.{10,70}-getPrizeLink.*?)\">").Match(result).Groups[1].Value;
                        await HelpMethod.Get(url3, client, cancellationToken);
                        result = await HelpMethod.Get("/football", client, cancellationToken);
                    }
                    HelpMethod.StatusLog($"Пинаем мячи...{i}", BotID, Form, Resources.thinking);

                } while (result.Contains("-buttonsBlock-") || result.Contains("-getPrizeLink") || result.Contains("-nextLink"));
                
                if (result.Contains("<span>X1</span>"))
                {
                    string url2 = new Regex("<a class=\"link small m5\" href=\"(.*?)\">").Match(result).Groups[1].Value;
                    result = await HelpMethod.Get(url2, client, cancellationToken);

                }
                // Общая сумма профита и количество этажей на которых собрано
    
           }


        }

        /// <summary>
        /// Метод, который берет задания коллекции
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        public static async Task GetCollection(int BotID,  MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Идем в коллекции...", BotID, Form, Resources.thinking);
            string url = "/wicket/bookmarkable/ru.overmobile.towers.wicket.pages.guild.collections.GuildCollectionsPage";
            // Идём на страницу коллекций href="./ru.overmobile.towers.wicket.pages.guild.collections.GuildCollectionsPage"
            string result = await HelpMethod.Get(url, client, cancellationToken);
                        // Если кнопка получить или сдать коллекцию
            if (result.Contains("Получить награду!") || result.Contains("Получить задание"))
            {
                HelpMethod.StatusLog("Заходим в коллекцию ", BotID, Form, Resources.st_sold);
                //находим ссылку кнопки
                result = await HelpMethod.ClickGreenButton(result, "Получить награду!"+ "Получить задание", client,url);

               
                // Логируем
                if (result.Contains("Получить задание"))
                {
                    HelpMethod.Log("Взяли коллекцию ", BotID, Form);
                }
                else 
                { 
                    HelpMethod.Log("Получили награду за коллекцию "+result, BotID, Form); 
                }
            }
            HelpMethod.StatusLog("Проверяем перс.коллекцию...", BotID, Form, Resources.thinking);
            url = "city/coll/my";
            //проверим наличие награды за перс.коллекцию
            result = await HelpMethod.Get(url, client, cancellationToken);
            //пройдемся по всем кнопкам
            result = await HelpMethod.ClickGreenButton(result, "Получить награду",client,url);
            if (result.Length > 0) { HelpMethod.Log("Получили персональную награду за коллекции "+result, BotID, Form); }
            
        }
        /// <summary>
        /// Метод, который  запускает инвов
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        public static async Task StartInv(int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Проверяем инвов...", BotID, Form, Resources.thinking);

            string result = await HelpMethod.Get("/boss/start", client, cancellationToken);
            result = await HelpMethod.ClickGreenButton(result, "Начать переговоры", client, "/boss/start");
            if (result.Contains("Начать переговоры")) { HelpMethod.Log("Запустили инвов", BotID, Form); }
               
        }
        static async Task CheckInv(string res, int BotID, HttpClient client, MainFormAll mf, System.Threading.CancellationToken cancellationToken = default)
        {
            if (res.Contains("Начать переговоры")) { await BotEngine.HelpInv(BotID, mf, client, true); }
        }

        public static async Task HelpInv(int BotID, MainFormAll Form, HttpClient client, bool help, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Проверяем запущены ли инвы...", BotID, Form, Resources.thinking);
            string nagr="";
            string tmp;
            string result = await HelpMethod.Get("/home", client, cancellationToken);
            result = await HelpMethod.ClickGreenButton(result, "Начать переговоры", client,"/home");
            if (result.Equals("Начать переговоры"))
            {
                HelpMethod.StatusLog("переходим к инвам ", BotID, Form, Resources.st_sold);

                result = await HelpMethod.Get("/boss", client, cancellationToken);
                if (!result.Contains("Осталось <span"))
                {
                    int i = 0;
                    do
                    {
                        i++;
                        if (i > 1300) { break; }
                        tmp = await HelpMethod.ClickGreenButton(result, "", client,"/boss");
                        if (tmp.StartsWith("Ваша")) nagr = tmp;
                        await HelpMethod.RandomDelay(500,900);
                        result = await HelpMethod.Get("/boss", client, cancellationToken);
                        if (!MainFormAll.Start[$"{BotID}"]) { return; }
                        if (!help) return;
                    } while (!result.Contains("Осталось "));


                } else {
                   /* TextWriter tw2 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "loginv.html");
                    tw2.Write(result);
                    tw2.Close();*/

                    HelpMethod.Log("не успели на инвы :( ", BotID, Form); }
                // Логируем
                HelpMethod.Log("Помогли с инвами ", BotID, Form);
            }
            result = await HelpMethod.Get("/boss", client, cancellationToken);
            tmp = await HelpMethod.ClickGreenButton(result, "Получить награду!", client, "/boss/start");
            if (tmp.StartsWith("Ваша")) nagr = tmp;
            if(nagr.Length>0) HelpMethod.Log(nagr, BotID, Form);
        }
        /// <summary>
        /// Метод, который  берет задания сундуков
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        public static async Task<bool> GetSund(int BotID, MainFormAll Form, bool nagrada, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Проверяем сундуки...", BotID, Form, Resources.thinking);
            string url = "/city/box/quests";
            // Идём на страницу сундуков
            string result = await HelpMethod.Get(url, client, cancellationToken);
            if (result.Contains("Получить награду!"))
            {
                HelpMethod.StatusLog("Получаем награду за сундуки ", BotID, Form, Resources.st_sold);
                await HelpMethod.ClickGreenButton(result, "Получить награду!", client,url);
                // Логируем
                HelpMethod.Log("Получили награду за сундуки", BotID, Form);
            }
            // Если есть проданный товар
            if (result.Contains("Завершить!"))
            {
                HelpMethod.StatusLog("Сдаем сундук ", BotID, Form, Resources.st_sold);
                await HelpMethod.ClickGreenButton(result, "Завершить!", client,url);
                // Логируем
                HelpMethod.Log("Сдали сундук", BotID, Form);
                return true;
            }

            if (result.Contains("Получить задание")&&nagrada)
            {
                HelpMethod.StatusLog("Берем сундук ", BotID, Form, Resources.st_sold);
                await HelpMethod.ClickGreenButton(result, "Получить задание", client,url);
                HelpMethod.Log("Взяли сундук", BotID, Form);
            }
            return false;
        }

        /// <summary>
        /// Метод, который выкладывает товар на этажах.
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        public static async Task SellGoods(int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Анализ ситуации...Выкладываем товар", BotID, Form, Resources.thinking);
            string tek_str = "/floors/3";
            string result = await HelpMethod.Get(tek_str, client, cancellationToken);

            // Если есть доставленный товар
            if (result.Contains("Выложить товар"))
            {
                HelpMethod.StatusLog("Выкладываем товар...", BotID, Form, Resources.st_stocked);

                int floorCount = 0;

                // Запускаем цикл
                foreach (Match match in Regex.Matches(result, "<a class=\"tdu\" href=\"(.{40,60}floorPanel.*?)\"", RegexOptions.Singleline | RegexOptions.Multiline))
                {
                    // Парсим первую ссылку этажа и первый профит с этажа
                    // href="./5?139-1.-floors-0-floorPanel-state-action"

                    
                    // Прибавляем количество этажей

                    if (floorCount > 200) { break; }
                    // Забираем выручку
                    result = await HelpMethod.Get(match.Groups[1].Value, client, cancellationToken);
                    if (cancellationToken.IsCancellationRequested || !MainFormAll.Start[$"{BotID}"]) { return; }
                    floorCount++;
                    HelpMethod.StatusLog($"Выкладываем товар... {floorCount}", BotID, Form, Resources.st_stocked);
                }

                // Логируем
                if (floorCount > 0) { HelpMethod.Log($"Этажей, на которых выложен товар: {floorCount}", BotID, Form); }
            }
        }

        /// <summary>
        /// Метод, который закупает товар на этажах.
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        public static async Task BuyGoods(int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Анализ ситуации... Закупаем товар", BotID, Form, Resources.thinking);

            // Идём на страницу закупки товара
            string tek_str = "/floors/2";
            string result = await HelpMethod.Get(tek_str, client, cancellationToken);
            await CheckInv(result, BotID, client, Form);
            // Если можно закупить товар
            if (result.Contains("Закупить товар"))
            {
                HelpMethod.StatusLog("Закупаем товар...", BotID, Form, Resources.st_empty_plus);

                // Количество этажей
                int floorCount = 0;

                // Запускаем цикл
                int i = 0;
                do
                {
                    i++;
                    if (i > 300) { break; }
                    // Парсим ссылку первого этажа href="../floor/121576119"
                    Match url_floor = new Regex("floor/([0-9]*?)\">").Match(result);

                    // Составляем ссылку на этаж
                    string url = $"/floor/{url_floor.Groups[1].Value}";
                    if (url.Length > 0)
                    {
                        // Переходим на этаж
                        result = await HelpMethod.Get(url, client, cancellationToken);
                        await CheckInv(result, BotID, client, Form, cancellationToken);
                        // Переменная для ссылки на закупку товара
                        string url_purchase=  new Regex("href=\"(.*?floorPanel-product.*?)\"").Match(result).Groups[1].Value;
                        HelpMethod.StatusLog($"Закупаем товар...{i}", BotID, Form, Resources.st_empty_plus);
                        // Проверяем где нужно закупить, приоритет 3 - 2 - 1
                        // href="./121536485?79-1.-floorPanel-productA-emptyState-action-link"
                        // href = "./121463046?103-1.-floorPanel-productB-emptyState-action-link"
                        /*    if (result.Contains("productC"))
                            {
                                url_purchase = new Regex("wicket:interface=:[0-9]*?:floorPanel:productC:emptyState:action:link::ILinkListener::").Match(result).Value;
                            }
                            else if (result.Contains("productB"))
                            {
                                url_purchase = new Regex("wicket:interface=:[0-9]*?:floorPanel:productB:emptyState:action:link::ILinkListener::").Match(result).Value;
                            }
                            else
                            {
                                url_purchase = new Regex("wicket:interface=:[0-9]*?:floorPanel:productA:emptyState:action:link::ILinkListener::").Match(result).Value;
                            }*/

                        if (url_purchase.Length > 0)
                        {
                            // Прибавляем количество этажей
                            floorCount++;

                            // Закупаем товар на этаже
                            result = await HelpMethod.Get($"{url_purchase}", client, cancellationToken);
                        }
                    }
                    if (cancellationToken.IsCancellationRequested || !MainFormAll.Start[$"{BotID}"]) { return; }
                }
                while (result.Contains("Закупить товар"));

                // Логируем
                if (floorCount > 0)
                {
                    HelpMethod.Log($"Этажей, на которых закуплен товар: {floorCount}", BotID, Form);
                }
            }
        }

        /// <summary>
        /// Метод, который развозит посетителей в лифте.
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        public static async Task Lift(int BotID, MainFormAll Form, HttpClient client, int ot, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Анализ ситуации... Идем в лифт", BotID, Form, Resources.thinking);
            
            // Проверяем лифт
            string result = await HelpMethod.Get("/lift", client, cancellationToken);
            await CheckInv(result, BotID,client,Form);
            // Если есть посетители
            if (result.Contains("Поднять лифт")|| result.Contains("Получить чаевые"))
            {
                 int visitors_count = 0;
                int i = 0;
                // Запускаем цикл подъёма
                do
                {
                    i++;
                    if (i > 600) { break; }
                    string kol= new Regex("kol=([0-9]{1,3})").Match(result).Groups[1].Value;
                    if (kol.Length > 0) { if (HelpMethod.ToInt(kol) > 0) { HelpMethod.Log($"Слишком маленький интервал повторов: {ot} Было {kol} попыток преодолеть 'Слишком быстро'", BotID, Form); } }
                    //if (!ost.Equals("")) {
                        HelpMethod.StatusLog("Поднимаем лифт " + visitors_count, BotID, Form, Resources.tb_lift);
                   // }
                    
                    // Переменная в которой будет храниться ссылка (поднять лифт или получить чаевые)
                    string url= new Regex("href=\"(.{0,36}lift.*?)\">").Match(result).Groups[1].Value;
                    if (url.Length > 0)
                    {
                        result = await HelpMethod.Get2(url, client, ot, cancellationToken);
                        await CheckInv(result, BotID, client, Form, cancellationToken);
                        if (result.Contains("<b>Главная</b>")) { result = await HelpMethod.Get("/lift", client, cancellationToken); }
                        if (url.Contains("tipsLink")) { visitors_count++; }

                    }
                    // no-op patch: alignment insertion
                    if (cancellationToken.IsCancellationRequested || !MainFormAll.Start[$"{BotID}"]) { return; }
                }
                while (result.Contains("Поднять лифт") || result.Contains("Получить чаевые"));

                // Логируем
                if (visitors_count > 0) { HelpMethod.Log($"Доставили посетителей: {visitors_count}", BotID, Form); }
            }
        }
        public static async Task Check_val(int BotID, MainFormAll Form, HttpClient client, string url)
        {
            HelpMethod.StatusLog("Получаем награды стрелы любви", BotID, Form, Resources.thinking);

            // Проверяем Кубок
            string result = await HelpMethod.Get(url, client);

            //Match but = new Regex("<a style=\"margin: 5px auto;\" href=\"(.*?doSnowBallLink.*?)\">Бросать</a>").Match(result);
            //string urlc = but.Groups[1].Value.Replace("&amp;", "&");

            int i = 0;
            do
            {

                i++;
                if (i > 50) { return; }
               
                url = new Regex("<a style=\"margin: 5px auto;\" href=\"(.*?doSnowBallLink.*?)\">Бросать</a>").Match(result).Groups[1].Value;
                if (url.Length > 0)
                {
                    result = await HelpMethod.Get($"{url}", client);
                   
                }
                if (!MainFormAll.Start[$"{BotID}"]) { return; }
            } while (result.Contains("Бросать") && !result.Contains("закончились стрелы"));

        }
        public static async Task Cup(int BotID, MainFormAll Form, HttpClient client, string url)
        {
            HelpMethod.StatusLog("Получаем награды..."+url, BotID, Form, Resources.thinking);
            
            // Проверяем Кубок
            string result = await HelpMethod.Get(url, client);

            Match but = new Regex("<a class=\".{0,40}btng.{0,40}\" href=\"(.*?)\">(.*?)</a>").Match(result);
            string urlc = but.Groups[1].Value.Replace("&amp;", "&");
            string log = but.Groups[2].Value;
            if (urlc.Length > 0&&!log.Equals("Начать переговоры") && !log.Equals("принять"))
            {
                result=await HelpMethod.Get(urlc, client);
                string priz = "";

                if (result.Contains("Ваш подарок")|| result.Contains("Ваша награда")) 
                {
                    priz = new Regex("[ка]:(.+?)<div class=\"nfl m5\"", RegexOptions.Singleline | RegexOptions.Multiline).Match(result).Groups[1].Value;
                    priz = Regex.Replace(priz, @"<(.|\n)*?>"," ");
                    priz = Regex.Replace(priz, @"\r?\n", string.Empty);
                    priz = Regex.Replace(priz, "  ", " ");

                }

                if (url.Equals("/lobby")) {

                    if (log.Equals("Получить задание")) {
                        priz = new Regex("Ваша награда:(.+?)</span>", RegexOptions.Singleline | RegexOptions.Multiline).Match(result).Groups[1].Value;
                        priz = Regex.Replace(priz, @"<(.|\n)*?>", "");
                        priz = Regex.Replace(priz, @"\r?\n", string.Empty);
                        priz = Regex.Replace(priz, "  ", " ");
                        log = "Получили задание VIP посетителя: "+priz; 
                    
                    }else{ log = "Получили награду VIP посетителя! Ваш приз " + priz; }}
                if (url.Equals("/cup")) { log = "Получили кубок"; }
                if (url.Equals("/cup/tournament")) { log = "Получили кубок чемпионов"; }
                // Логируем
                if (log.Length > 0) { HelpMethod.Log(log, BotID, Form); }

            }
        }

        /// <summary>
        /// Метод, который забирает выполненные ежедневные задания.
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        public static async Task<string> PersQuests(int BotID, MainFormAll Form, HttpClient client, bool go, System.Threading.CancellationToken cancellationToken = default)
        {
            if(go)
            HelpMethod.StatusLog("Заходим в перс.задания...", BotID, Form, Resources.thinking);
            DateTime now = DateTime.UtcNow.AddHours(3);
                //текущий день недели
            // Переходим на страницу заданий
            string result = await HelpMethod.Get("/quests", client); 

                            //В воскресение сдадим только 7 заданий
                if(result.Contains("Выполнено 7 заданий")&& now.DayOfWeek == DayOfWeek.Sunday) { go = false; }
                if (now.DayOfWeek == DayOfWeek.Saturday) { go = false; }
                // Если есть выполненные задания
                if (result.Contains("Получить награду") && go)
                {
                    HelpMethod.StatusLog("Забираем ежедневные награды...", BotID, Form, Resources.quests);
                    //await HelpMethod.ClickGreenButton(result, "Получить награду!", client);
                    // Переменные для хранения монет и баксов
                    string blok;
                //<div><b>Товар на витрине</b></div>
                //<a class="btng btn60" href="./quests?265-1.-completedQuests-4-quest-getAwarLink">Получить награду!</a>
                foreach (Match match in Regex.Matches(result, "<div><b>(.+?)</b></div>.+?<a class=\".{0,40}btng.{0,40}\" href=\"(.*?)\">Получить награду!</a>", RegexOptions.Singleline | RegexOptions.Multiline))
                    {
                        if (now.DayOfWeek == DayOfWeek.Sunday && match.Groups[1].Value.Equals("Легкие деньги")) continue;
                        result = await HelpMethod.Get(match.Groups[2].Value, client);

                        if (result.Contains("Награда"))
                        {
                            blok = HelpMethod.GetNagrada(result);
                            HelpMethod.Log("Получили награду за перс.задание "+match.Groups[1].Value +" Ваша награда: \n"+ blok, BotID, Form);
                        }
                        if (result.Contains("Выполнено 7 заданий") && now.DayOfWeek == DayOfWeek.Sunday) { break; }
                    }
                }
                string aktQuest = "";
                result = await HelpMethod.Get("/quests", client);
            string nameQuest;
            foreach (Match match in Regex.Matches(result, "<div class=\"nfl\">(.*?)</div><div>", RegexOptions.Singleline | RegexOptions.Multiline))
                {

                    if (match.Groups[1].Value.Length > 0)
                    {
                        if (!match.Groups[1].Value.Contains("Получить награду"))
                        {
                        nameQuest = new Regex("<b>(.+?)</b>", RegexOptions.Singleline | RegexOptions.Multiline).Match(match.Groups[1].Value).Groups[1].Value;
                        if(nameQuest.Length>0)
                            aktQuest += new Regex("<b>(.+?)</b>", RegexOptions.Singleline | RegexOptions.Multiline).Match(match.Groups[1].Value).Groups[1].Value + ", ";
                        }
                    }
                }
                if (aktQuest.Length > 2)
                {
                    aktQuest = aktQuest.Remove(aktQuest.Length - 2);
                }
                 return aktQuest;
            
            //return "Суббота, перс.задания не выполняем";
        }
        /// <summary>
        /// Метод, который забирает выполненные городские задания.
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        /// <param name="go">true - сдавать задания, false - только посмотреть какие</param>
        public static async Task<string> CityQuests(int BotID, MainFormAll Form, HttpClient client,bool go, System.Threading.CancellationToken cancellationToken = default)
        {
            
            HelpMethod.StatusLog("заходим в горзадания", BotID, Form, Resources.thinking);
            DateTime now = DateTime.Now;
            
          //  if (!go)
           // {
                // Переходим на страницу заданий
                string result = await HelpMethod.Get("/city/quests", client);
               
                // Если есть выполненные задания и есть команда сдавать задания и не воскресение, то нажимаем зелёную кнопку
                if (result.Contains("Получить награду")&&go&& now.DayOfWeek != DayOfWeek.Sunday)
                {
                    HelpMethod.StatusLog("Забираем городские награды...", BotID, Form, Resources.quests);
                //await HelpMethod.ClickGreenButton(result, "Получить награду", client,"city quest");
                
                Match res = new Regex("<strong class=\"sale\">(.*?)</strong>.*?<a class=\".{0,40}btng.{0,40}\" href=\"(.*?)\">Получить награду!</a>", RegexOptions.Singleline | RegexOptions.Multiline).Match(result);
                    result = await HelpMethod.Get(res.Groups[2].Value, client);
                result = HelpMethod.GetNagrada(result);
                    // Логируем
                    HelpMethod.Log("Забрали награду за гор.задание "+ res.Groups[1].Value+" Награда:\n"+ result, BotID, Form);
                return null;
                }
                result = await HelpMethod.Get("/home", client);
                if (result.Contains("city/quests")) //есть гор.задание на главной странице
            {
                    result = await HelpMethod.Get("/city/quests", client); //переходим к гор.заданиям
                //если выполнено, то не передаем как активное
                if (result.Contains("Получить награду")) { return null; }
                //найдем какое
                string blok = new Regex("<div class=\"nfl\">(.{90,150})<div class=\"qstPrgbr", RegexOptions.Singleline | RegexOptions.Multiline).Match(result).Groups[1].Value;
                    if (blok.Length > 0) { blok = new Regex("<strong class=\"sale\">(.*?)</strong>", RegexOptions.Singleline | RegexOptions.Multiline).Match(blok).Groups[1].Value;
                    
                        
                        return blok; }
                } else { return null; }
            return null;
           // } return "Воскресение, задания не сдаем!";
        }
        /// <summary>
        /// Метод, который выселяет жителей из гостиницы.
        /// </summary>
        /// <param name="hostel_url">Ссылка на гостиницу.</param>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="client">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        /// <param name="hostel_evict_less_9">Выселять жителей ниже 9 уровня.</param>
        /// <param name="hostel_evict_minus">Выселять жителей со знаком (-).</param>
        /// <param name="hostel_evict_plus">Выселять жителей со знаком (+).</param>
        public static async Task HostelEvict(string hostel_url, int BotID, MainFormAll Form,HttpClient client, bool hostel_evict_less_9 = false, bool hostel_evict_minus = false, bool hostel_evict_plus = false, System.Threading.CancellationToken cancellationToken = default)
        {
            if (hostel_url.Length > 0)
            {
                
                HelpMethod.StatusLog("Проверяем гостиницу...", BotID, Form, Resources.man_minus);

                // Заходим в гостиницу
                string result = await HelpMethod.Get(hostel_url, client);

                // Раскрываем список жителей в гостинице, если это нужно
                if (result.Contains("Раскрыть список"))
                {
                    // Парсим ссылку, для раскрытие списка href="./68037603?102-1.-floorPanel-expandResidentsLink"
                    // new Regex("<span class=\".*?\">([0-9])</span>").Match(li).Groups[1].Value;
                    string expandResidentsLink = new Regex("href=\"(.*?expandResidentsLink)\"").Match(result).Groups[1].Value;

                    // Проверяем то что ссылка не пустая и раскрываем список
                    if (expandResidentsLink.Length > 0)
                    {
                        result = await HelpMethod.Get(expandResidentsLink, client);
                    }
                }

                // Навсякий случай проверим
                if (result.Contains("Свернуть список"))
                {
                    // Переменные
                    int human_evict_count = 0;

                    // Запускаем цикл выселения
                    foreach (object item in new Regex("<li>.*?</li>", RegexOptions.Singleline).Matches(result))
                    {
                        string li = item.ToString();
                        
                        // Если пустая ячейка, пропускаем
                        if (li.Contains("Свободное место"))
                        {
                            continue;
                        }

                        // Парсим уровень жителя
                        string level = new Regex("<span class=\".*?\">([0-9])</span>").Match(li).Groups[1].Value;
                        // Парсим (-)
                        string major = new Regex("<span class=\"major\">(.*?)</span>").Match(li).Groups[1].Value;
                        // Парсим (+)
                        string amount = new Regex("<span class=\"amount\">(.*?)</span>").Match(li).Groups[1].Value;
                        // Парсим ссылку на жителя href="../human/11447548016?1=1"
                        string human_url = new Regex("href=\"(.*?/human/[0-9]*.*?)\">").Match(li).Groups[1].Value;

                        // Если важные переменные не пустые
                        if (level.Length > 0 & human_url.Length > 0)
                        {
                            // Если выполняем любое из этих действий, оповещаем через статус
                            if (HelpMethod.ToInt(level) < 9 & hostel_evict_less_9 || major.Length > 0 & hostel_evict_minus || amount.Length > 0 & hostel_evict_plus)
                            {
                                HelpMethod.StatusLog("Выселяем жителей..."+ human_evict_count.ToString(), BotID, Form, Resources.man_minus);
                            }

                            // Житель меньше 9 уровня и включена опция "Выселять жителей ниже 9 уровня" и у жителя нет знака (+)
                            if (HelpMethod.ToInt(level) < 9 & hostel_evict_less_9 & amount.Length == 0)
                            {
                                // Переходим на страницу жителя
                                result = await HelpMethod.Get(human_url, client);
                        await CheckInv(result, BotID, client, Form, cancellationToken);
                                // Парсим ссылку на выселенения
                                string evictLink = new Regex("href=\"(.*?)\">Выселить").Match(result).Groups[1].Value.Replace("&amp;", "&");

                                // Если ссылка на выселенения не пустая
                                if (evictLink.Length > 0)
                                {
                                    // Выселяем жителя
                                    await HelpMethod.Get(evictLink, client);

                                    // Прибавляем к общему количеству выселенных жителей
                                    human_evict_count++;
                                }
                            }
                            // Если житель со знаком (-) и включена опция "Выселять со знаком (-)"
                            else if (major.Length > 0 & hostel_evict_minus)
                            {
                                // Переходим на страницу жителя
                                result = await HelpMethod.Get(human_url, client);
                        await CheckInv(result, BotID, client, Form, cancellationToken);
                                // Парсим ссылку на выселенения
                                string evictLink = new Regex("href=\"(.*?)\">Выселить").Match(result).Groups[1].Value.Replace("&amp;", "&");

                                // Если ссылка на выселенения не пустая
                                if (evictLink.Length > 0)
                                {
                                    // Выселяем жителя
                                     await HelpMethod.Get(evictLink, client);

                                    // Прибавляем к общему количеству выселенных жителей
                                    human_evict_count++;
                                }
                            }
                            // Если житель со знаком (+) и включена опция "Выселять со знаком (+)"
                            else if (amount.Length > 0 & hostel_evict_plus)
                            {
                                // Переходим на страницу жителя
                                result = await HelpMethod.Get(human_url, client);
                                await CheckInv(result, BotID, client, Form, cancellationToken);
                                // Парсим ссылку на выселенения
                                string evictLink = new Regex("href=\"(.*?)\">Выселить").Match(result).Groups[1].Value.Replace("&amp;", "&");

                                // Если ссылка на выселенения не пустая
                                if (evictLink.Length > 0)
                                {
                                    // Выселяем жителя
                                    _ = await HelpMethod.Get(evictLink, client);

                                    // Прибавляем к общему количеству выселенных жителей
                                    human_evict_count++;
                                }
                            }
                        }
                    }
                    result = await HelpMethod.Get(hostel_url,client);
                    if (result.Contains("Свернуть список"))
                    {
                        // Парсим ссылку, для раскрытие списка href="./68037603?932-1.-floorPanel-collapseResidentsLink"
                        string expandResidentsLink = new Regex("\"(.*?collapseResidentsLink.*?)\"").Match(result).Value;

                        // Проверяем то что ссылка не пустая и сворачиваем список
                        if (expandResidentsLink.Length > 0)
                        {
                            await HelpMethod.Get(expandResidentsLink, client);
                        }
                    }
                    // Если выселенных больше 0
                    if (human_evict_count > 0)
                    {
                        HelpMethod.Log($"Выселили жителей: {human_evict_count}", BotID, Form);
                    }
                }
            }
        }

    
  /*      public static async Task BusinessTournament(int BotID, MainForm Form, HttpClient client)
        {
            HelpMethod.StatusLog("Анализ ситуации... Идем в бизнес-турнир", BotID, Form, Resources.thinking);

            // Проверяем бизнес турнир
            string result = await HelpMethod.Get("/inspectors", client);

            // Если можно получить награду
            if (result.Contains("Получить награду"))
            {
                HelpMethod.StatusLog("Получаем награду за бизнес турнир...", BotID, Form, Resources.chart_pie);
                await HelpMethod.ClickGreenButton(result, "Получить награду!", client,"buisness");
                HelpMethod.Log($"Получили награду за бизнес турнир.", BotID, Form);
                    
            }
        }*/

        public static async Task GoLift15(int BotID, MainFormAll Form, HttpClient client, int ot, int kol=1, bool change=false, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Идем в лифт", BotID, Form, Resources.thinking);
            //развезём имеющихся
            await Lift(BotID, Form, client,ot);
            HelpMethod.StatusLog("Зовем посетителей", BotID, Form, Resources.thinking);
            string result = await HelpMethod.Get("/lift", client);
            string url = new Regex("href=\"(.*?activateLiftLink.*?)\">").Match(result).Groups[1].Value;
            //если правильно распарсили, зовем за 2 бакса
            if (url.Length > 0) { result = await HelpMethod.Get(url, client); HelpMethod.StatusLog("Позвали за 2 бакса", BotID, Form, Resources.thinking); } else { HelpMethod.Log($"Нет ссылки для развоза посетителей", BotID, Form); return; }
            url = new Regex("href=\"(.*?processLiftAll.*?)\">").Match(result).Groups[1].Value;
            //если правильно распарсили, развозим за 1 бакс
            if (url.Length > 0) { result = await HelpMethod.Get(url, client); HelpMethod.StatusLog("развезли за 1 бакс", BotID, Form, Resources.thinking); } else { HelpMethod.Log($"Нет ссылки для развоза посетителей", BotID, Form); return; }
            //если сейчас режис стоит за 75 баксов, меняем на 15
            if (result.Contains("X25")) { url = new Regex("href=\"(.*?changeMode.*?)\">").Match(result).Groups[1].Value; result = await HelpMethod.Get(url, client); HelpMethod.StatusLog("Поменяли на 15 баксов", BotID, Form, Resources.thinking); }
            //кликаем сколько надо за 15    
            for (int i = 0; i < kol; i++)
            {
                url = new Regex("href=\"(.*?doLifterLink.*?)\">").Match(result).Groups[1].Value;
                if (url.Length > 0) 
                { 
                    result = await HelpMethod.Get(url, client);
                    await CheckInv(result, BotID, client, Form, cancellationToken);
                    HelpMethod.StatusLog($"Выполнили за 15 {i+1} раз(а)", BotID, Form, Resources.thinking);
                    //если стоит флаг менять на 75, то меняем после 5 раз по 15
                    if (change&&i>=4) 
                    {
                        string urlCg= new Regex("href=\"(.*?changeMode.*?)\">").Match(result).Groups[1].Value;
                        if (urlCg.Length > 0) { result = await HelpMethod.Get(urlCg, client); HelpMethod.StatusLog("Сменили развоз на за 75", BotID, Form, Resources.thinking); } else { HelpMethod.Log($"Нет ссылки для развоза посетителей", BotID, Form); return; }
                    }
                } 
                else { HelpMethod.Log($"Нет ссылки для развоза посетителей", BotID, Form); return; }
            }
        }
        public static async Task BuyPiarMark( int BotID, MainFormAll Form, HttpClient client, string pok, System.Threading.CancellationToken cancellationToken = default)
        {
            string log, url,tek_url;
            
            if (pok == "piar") { tek_url = "vendor/buff/2"; log = "пиар"; } else { tek_url = "vendor/buff/1"; log = "маркетинг"; }
            HelpMethod.StatusLog($"Пытаемся приобрести {log}...", BotID, Form, Resources.man_plus);
            string result = await HelpMethod.Get(tek_url, client);
            await CheckInv(result, BotID, client, Form, cancellationToken);
            //result= new Regex("<div class=\"nfl\" style=\"text-align:left;\">(.*?)</div>.?</div>.?</div>").Match(result).Groups[1].Value.Replace(" &amp;", "&");
            if (!result.Contains("<span class=\"buff small\">Осталось:")) {
               url = new Regex("<a class=\"tdu\" href=\"(.*?buy4Link.*?)\">").Match(result).Groups[1].Value.Replace("&amp;", "&");
                if (pok == "piar")
                { url = new Regex("<a class=\"tdu\" href=\"(.*?buy4Link.*?)\">").Match(result).Groups[1].Value.Replace("&amp;", "&"); }
                    await HelpMethod.Get(url, client);

               HelpMethod.Log($"Купили {log} ", BotID, Form);
            }

        }
            /// <summary>
            /// Метод, который нанимает более опытных жителей на работу.
            /// </summary>
            /// <param name="hostel_url">Ссылка на гостиницу.</param>
            /// <param name="BotID">Идентификатор бота (вкладки).</param>
            /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
            /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
            public static async Task HumanJobs(string hostel_url, int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            
            // Если ссылка на гостиницу не пустая

            if (hostel_url.Length > 0)
            {
                HelpMethod.StatusLog("Ищем опытных работников...", BotID, Form, Resources.man_plus);

                // Идём в гостиницу
                string result = await HelpMethod.Get(hostel_url, client);

                // Парсим все HTML-Блоки жителей
                MatchCollection human_list = new Regex("<li>.*?</li>", RegexOptions.Singleline).Matches(result);

                // Переменная для общего хранения устроенных на работу
                int humans_job_found = 0;

                // Перебираем каждого жителя гостиницы
                foreach (object human in human_list)
                {
                    
                    // Если у пользователя есть знак (+)
                    if (human.ToString().Contains("(+)"))
                    {

                        HelpMethod.StatusLog("Пытаемся нанять опытных работников...", BotID, Form, Resources.man_plus);
                        //<a class="tdu" href="https://nebo.mobi/floor/68037603?7348-1.-floorPanel-residents-0-residentPanel-sendToWorkLink">устроить на работу</a>
                            string url = new Regex("<a class=\"tdu\" href=\"(.*?)\">устроить на ").Match(human.ToString()).Groups[1].Value;
                            if (url.Length > 0) {
                            await HelpMethod.Get(url, client);
                            continue;

                        }

                            // Парсим ссылку на жителя
                            string human_path = new Regex("href=\"(.*?human/.*?)\">").Match(human.ToString()).Groups[1].Value;

                        // Переходим на жителя
                        result = await HelpMethod.Get(human_path, client);

                        // Парсим ссылку кнопки "Найти работу"
                        string find_job_path = new Regex("href=\"(.*?)\">Найти").Match(result).Groups[1].Value;
                        string human_level = new Regex("<strong>([0-9])</strong>").Match(result).Groups[1].Value;

                        // Переходим на поиск работы
                        result = await HelpMethod.Get(find_job_path, client);
                        await CheckInv(result, BotID, client, Form, cancellationToken);
                        // Парсим ссылку кнопки "Устроить на работу"
                        string get_job_path = new Regex(@"href=""(.*?)"">устроить на работу").Match(result).Groups[1].Value;

                        // Если есть кнопка "Устроить на работу"
                        if (get_job_path.Length > 0)
                        {
                            // Устраиваем на работу
                            await HelpMethod.Get(get_job_path, client);

                            // Прибавляем к количеству найденых работу жителей
                            humans_job_found++;
                        }
                        else
                        {
                            // Парсим все html-блоки этажей
                            MatchCollection floor_list = new Regex("<li>.*?</li>", RegexOptions.Singleline).Matches(result);

                            // Перебираем этажи
                            foreach (object floor in floor_list)
                            {
                                if (floor.ToString().Contains("Работа мечты, но мест нет"))
                                {
                                    // Парсим ссылку на этаж
                                    string floor_url = new Regex(@"<a class=""flhdr"" href=""(.*?)"">").Match(result).Groups[1].Value;

                                    // Переходим на этаж
                                    result = await HelpMethod.Get(floor_url, client);

                                    // Парсим HTML-Блоки жителей этажа
                                    MatchCollection floor_humans_list = new Regex(@"<li class=""\w{2}"">(.*?)</li>", RegexOptions.Singleline).Matches(result);

                                    // Перебираем жителей этажа
                                    foreach (object floor_human in floor_humans_list)
                                    {
                                        // Парсим уровень и ссылку на жителя
                                        string floor_human_level = new Regex(@"<span class=""\w{2}"">([0-9])</span>").Match(floor_human.ToString()).Groups[1].Value;
                                        string floor_human_path = new Regex(@"<a class=""btn"" href=""(.*?)"">").Match(floor_human.ToString()).Groups[1].Value;

                                        // Если житель не счастливый
                                        if (!floor_human.ToString().Contains("sml_happy"))
                                        {
                                            // Переходим на жителя
                                            result = await HelpMethod.Get(floor_human_path, client);

                                            // Парсим ссылку на увольнение
                                            string human_dismiss_path = new Regex(@"<a class=""btnw"" href=""(.*?)"">Уволить</a>").Match(result).Groups[1].Value;

                                            // Пробуем уволить
                                            result = await HelpMethod.Get($"{human_dismiss_path}", client);

                                            // Если уволили
                                            if (result.Contains("Уволена из") || result.Contains("Уволен из"))
                                            {
                                                // Получаем ссылку на этаж с которого был уволен житель
                                                string floor_dismiss_path = new Regex(@"<a class=""\w{2}"" href=""(.*?)""><span>(.*?)</span></a>").Match(result).Groups[1].Value;

                                                // Переходим на этаж с которого уволили
                                                result = await HelpMethod.Get($"{floor_dismiss_path}", client);

                                                // Парсим ссылку на поиск нового работника 
                                                string floor_id = new Regex(@"<a class=""btn"" href=""(.*?)"">").Match(result).Groups[1].Value;

                                                // Переходим нанимать жителя
                                                result = await HelpMethod.Get($"{floor_id}", client);

                                                // Парсим ссылку кнопки "принять на работу", но если она работа мечты
                                                string dream_job_path = new Regex(@"<img src=""/images/icons/sml_happy\.png"" alt="""" height=""16"" width=""16""/> <a class=""tdu"" href=""\.\./\.\./(.*?)"">принять на работу</a>").Match(result).Groups[1].Value;

                                                // Нанимаем
                                                result = await HelpMethod.Get($"/{dream_job_path}", client);

                                                // Прибавляем к количеству найденых работу жителей
                                                humans_job_found++;

                                                // Выходим из цикла
                                                break;
                                            }
                                            // Если не смогли
                                            else
                                            {
                                                // Выходим из цикла, т.к смысла нет пытаться, т.к скорее всего товар закупается
                                                break;
                                            }
                                        }
                                        // Иначе житель счатливый
                                        else
                                        {
                                            // Если уровень нового жителя больше или равен предыдущего
                                            if (HelpMethod.ToInt(human_level) > HelpMethod.ToInt(floor_human_level))
                                            {
                                                // Переходим на жителя
                                                result = await HelpMethod.Get(floor_human_path, client);

                                                // Парсим ссылку на увольнение
                                                string human_dismiss_path = new Regex(@"href=""(.*?)"">Уволить").Match(result).Groups[1].Value;

                                                // Пробуем уволить
                                                result = await HelpMethod.Get($"{human_dismiss_path}", client);

                                                // Если уволили
                                                if (result.Contains("Уволена из") || result.Contains("Уволен из"))
                                                {
                                                    // Получаем ссылку на этаж с которого был уволен житель
                                                    string floor_dismiss_path = new Regex(@"<a class=""\w{2}"" href=""(.*?)""><span>(.*?)</span></a>").Match(result).Groups[1].Value;

                                                    // Переходим на этаж с которого уволили
                                                    result = await HelpMethod.Get($"/{floor_dismiss_path}", client);

                                                    // Парсим ссылку на поиск нового работника 
                                                    string floor_id = new Regex(@"<a class=""btn"" href=""(.*?)"">").Match(result).Groups[1].Value;

                                                    // Переходим нанимать жителя
                                                    result = await HelpMethod.Get(floor_id, client);

                                                    // Парсим ссылку кнопки "принять на работу", но если она работа мечты
                                                    string dream_job_path = new Regex(@"src=""/images/icons/sml_happy\.png"" alt="""" height=""16"" width=""16""/> <a class=""tdu"" href=""(.*?)"">принять на работу</a>").Match(result).Groups[1].Value;

                                                    // Нанимаем
                                                    result = await HelpMethod.Get($"{dream_job_path}", client);

                                                    // Прибавляем к количеству найденых работу жителей
                                                    humans_job_found++;

                                                    // Выходим из цикла
                                                    break;
                                                }
                                                // Если не смогли
                                                else
                                                {
                                                    // Выходим из цикла, т.к смысла нет пытаться, т.к скорее всего товар закупается
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    // Выходим из этого цикла, т.к этаж нашли
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                if (humans_job_found > 0)
                {
                    HelpMethod.Log($"Наняли новых работников: {humans_job_found}", BotID, Form);
                }
            }
        }

        /// <summary>
        /// Метод, который открывает новый этажи.
        /// </summary>
        /// <param name="Result">Исходный код ответа.</param>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        public static async Task FloorOpen( int BotID, MainFormAll Form, HttpClient client)
        {
            string Result = await HelpMethod.Get("/home",client);
            // Общее количество открытых этажей
            int floor_open_count = 0;

            // Запускаем цикл открытией этажей
            int i = 0;
            if (Result.Contains("Открыть этаж"))
            {
                do
                {
                    i++;
                    if (i > 30) { break; }
                    
                    // Парсим ссылку на открытие этажа
                    string floor_open_url = new Regex("<a class=\"tdu\" href=\"(.*?)\">Открыть этаж!").Match(Result).Groups[1].Value;

                    // Если спарсенная ссылка не пустая
                    if (floor_open_url.Length > 0)
                    {
                        HelpMethod.StatusLog("Открываем новые этажи...", BotID, Form, Resources.st_builded);

                        // Открываем этаж
                        Result = await HelpMethod.Get($"/{floor_open_url}", client);

                        // Прибавляем общее количество открытых этажей
                        floor_open_count++;
                    }
                    else { Logger.WriteError(floor_open_url + Result);return; }
                    if (!MainFormAll.Start[$"{BotID}"]) { return; }
                }
                while (Result.Contains("Открыть этаж!"));
            }
            if (Result.Contains("Начать строительство"))
            {
                i = 0;
                do
                {
                    i++;
                    if (i > 30) { break; }
                    string floor_open_url = new Regex("<a class=\"tdu\" href=\"(.*?)\">Начать строительство").Match(Result).Groups[1].Value;
                    if (floor_open_url.Length == 0) { Logger.WriteError(floor_open_url + Result); return; }
                    Result = await HelpMethod.Get($"/{floor_open_url}", client);
                    floor_open_url = new Regex("<a href=\"(.*?floorLink.*?)\" class=").Match(Result).Groups[1].Value;
                    if (floor_open_url.Length == 0) { Logger.WriteError("2"+floor_open_url + Result); return; }
                    await HelpMethod.Get($"/{floor_open_url}", client);
                    Result = await HelpMethod.Get("/home", client);
                    if (!MainFormAll.Start[$"{BotID}"]) { return; }
                }
                while (Result.Contains("Начать строительство"));
            }
            // Логируем
            if (floor_open_count > 0)
            {
                HelpMethod.Log($"Открыли этажей: {floor_open_count}", BotID, Form);
            }
        }
        public static async Task UgrFloor (int BotID, MainFormAll Form, HttpClient client)
        {
            HelpMethod.StatusLog("Улучшаем этажи", BotID, Form, Resources.thinking);
            string res = await HelpMethod.Get("/home", client);
            //Match item = new Regex(@"<a href=""(floor/0/\d+)"">.*?<span>.<img alt(.*?)</span>").Match(res);
            string url;
            string resUpgr;
            foreach (Match item in Regex.Matches(res, @"<a href=""(floor/0/\d+)"">.*?<span>.<img alt(.*?)</span>", RegexOptions.Singleline | RegexOptions.Multiline))
            {
                
                if (item.Groups[2].Value.Contains("-"))
                {
                    url = item.Groups[1].Value;
                    do
                    {
                        
                        resUpgr = await HelpMethod.Get(url, client);
                        url = new Regex("<a class=\"tdu\" href=\"(.*?)\">.*?</a>").Match(resUpgr).Groups[1].Value;
                        
                        if(resUpgr.Contains("У Вас не хватает")) { HelpMethod.Log("Нет баксов на улучшение этажей", BotID, Form); return; }
                        if (!MainFormAll.Start[$"{BotID}"]) { return; }
                    } while (resUpgr.Contains("Улучшить за"));
                }
            }

        }

        public static async Task OpenNY(int BotID, string url_a, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Заходим за билетами", BotID, Form, Resources.thinking);
            string res = await HelpMethod.Get(url_a, client, cancellationToken);
            string url;
            string url2;
            do
            {
                url =  new Regex(@"<a href=""(\?.*?AllLink.*?)"">.*?все</a>").Match(res).Groups[1].Value;
                //debug url
                res = await HelpMethod.Get(url, client, cancellationToken);
                await CheckInv(res, BotID, client, Form);
                if (!MainFormAll.Start[$"{BotID}"]) { return; }
            } while (res.Contains("все</a>"));

            foreach (Match item in Regex.Matches(res, @"<a class=""tdu"" href=""(.*?floor.*?)"">(.*?)</a>"))
            {
                    url = url_a + item.Groups[1].Value;
                
                if (url.Length>0&&(item.Groups[2].Value.Equals("Обменять!") || item.Groups[2].Value.Equals("Добавить в очередь") || item.Groups[2].Value.Equals("Забрать!")))
                {
                    res=await HelpMethod.Get(url, client, cancellationToken);
                    await CheckInv(res, BotID, client, Form, cancellationToken);
                    url2 = url_a + new Regex(@"<a class=""tdu"" href=""[\./]*?(\?.*?product.*?)"">(.*?)</a>").Match(res).Groups[1].Value;
                    await HelpMethod.Get(url2, client, cancellationToken);
                    //debug url2
                }
                
            }

        }

        public static async Task OpenLavka(int BotID, string url, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            
            HelpMethod.StatusLog("Тратим в лавке/огороде", BotID, Form, Resources.thinking);
            

            string cena;
            string kol;

            string url2;
            int num;
            Random rnd = new Random();
            //string url2;
            //url =  new Regex(@"<div class="""">\n<a href=""(.*?)"">\n<img class=""logo", RegexOptions.Singleline).Match(res).Groups[1].Value;

            url2 = url;
            //debug url2
            string res = await HelpMethod.Get(url, client, cancellationToken);
            int i = 0;
            do
            {
                
                Match[] matches = Regex.Matches(res, @"<a class=""tdn"" href=""(.*?openLink.*?)"">")
                       .Cast<Match>()
                       .ToArray();
                if (matches.Length > 0)
                {
                    num = rnd.Next(0, matches.Length);
                    res = await HelpMethod.Get(matches[num].Groups[1].Value, client, cancellationToken);
                }
                if (res.Contains("Установить"))
                {
                    url = new Regex("<a class=\".{0,40}btng.{0,40}\" href=\"(.*?)\">Установить</a>").Match(res).Groups[1].Value;
                    await HelpMethod.Get(url, client, cancellationToken);
                    res = await HelpMethod.Get(url2, client, cancellationToken);
                }
                if (res.Contains("улучшить"))
                {
                    url = new Regex("<a class=\".{0,40}btng.{0,40}\" href=\"(.*?)\">улучшить</a>").Match(res).Groups[1].Value;
                    res = await HelpMethod.Get(url, client, cancellationToken);
                }
                if (res.Contains("Продолжить"))
                {
                    url = new Regex("<a class=\".{0,40}btng.{0,40}\" href=\"(.*?)\">Продолжить</a>").Match(res).Groups[1].Value;
                    res = await HelpMethod.Get(url, client, cancellationToken);
                }
                if (res.Contains("Следующая задача"))
                {
                    url = new Regex("<a class=\".{0,40}btng.{0,40}\" href=\"(.*?)\">Следующая задача</a>").Match(res).Groups[1].Value;
                    res = await HelpMethod.Get(url, client, cancellationToken);
                }
                cena = new Regex(@"за .*?/>(\d+)\s").Match(res).Groups[1].Value.Replace("&#039;","").Replace("'","");
                if (HelpMethod.ToInt(cena) == 0) { HelpMethod.StatusLog("нет цены!", BotID, Form, Resources.thinking); break; }
                res = res.Replace("&#039;", "");
                kol = new Regex(@"У вас: .*?n>(\d+?)</s").Match(res).Groups[1].Value;
                i++;
                if (i > 40) { return; }
                HelpMethod.StatusLog("Осталось попыток - " + Decimal.Round(HelpMethod.ToInt(kol) / HelpMethod.ToInt(cena)).ToString(), BotID, Form, Resources.thinking);
                //HelpMethod.Log("У нас "+kol+" цена "+cena+" Осталось попыток - " + Decimal.Round(HelpMethod.ToInt(kol) / HelpMethod.ToInt(cena)).ToString(), BotID, Form);
                if (!MainFormAll.Start[$"{BotID}"]) { return; }
            } while (HelpMethod.ToInt(kol)/HelpMethod.ToInt(cena)>=1&&i<40);
            url = new Regex("href=\"(tower/id/[0-9]*.?)\"><span>").Match(res).Groups[1].Value;
                    res = await HelpMethod.Get(url, client, cancellationToken);
            if (res.Contains("Улучшить"))
            {
                url = new Regex("<a class=\".{0,40}btng.{0,40}\" href=\"(.*?)\">Улучшить</a>").Match(res).Groups[1].Value;
                await HelpMethod.Get(url, client, cancellationToken);
            }

        }
        public static async Task GoThree(string url, int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default) 
        {
            if (url.Length > 0)
            {
                HelpMethod.StatusLog("Выселяем жителей >3 должности...", BotID, Form, Resources.man_minus);

                // Заходим в гостиницу
                string result = await HelpMethod.Get(url, client, cancellationToken);

                // Раскрываем список жителей в гостинице, если это нужно
                if (result.Contains("Раскрыть список"))
                {
                    // Парсим ссылку, для раскрытие списка
                    string expandResidentsLink = new Regex("href=\"(.*?expandResidentsLink.*?)\"").Match(result).Value;

                    // Проверяем то что ссылка не пустая и раскрываем список
                    if (expandResidentsLink.Length > 0)
                    {
                        result = await HelpMethod.Get(expandResidentsLink, client, cancellationToken);
                    }
                }

                // Навсякий случай проверим
                if (result.Contains("Свернуть список"))
                {
                    Dictionary<string, string> hum = new Dictionary<string, string>();
                    foreach (object item in new Regex("<li>.*?</li>", RegexOptions.Singleline).Matches(result))
                    {
                        string li = item.ToString();
                        
                        // Если пустая ячейка, пропускаем
                        if (li.Contains("Свободное место"))
                        {
                            continue;
                        }

                        // Парсим должность жителя
                        string dolg = new Regex(@"<span class=""\D\D"">(\D*?)</span>").Match(li).Groups[1].Value;
                        // Парсим ссылку на жителя
                        string human_url = new Regex("href=\"(.*?/human/[0-9]*.*?)\">").Match(li).Groups[1].Value;
                            //debug: key = {1} Value = {0}
                        hum.Add(human_url,dolg);
                    } //конец цикла
                    string pred = "";
                    int i = 0;
                    foreach (var pair in hum.OrderBy(pair => pair.Value))
                    {
                        
                        if (pair.Value == pred) { i++; } else { i = 0; }
                        if (i >= 3) 
                        {
                            // Переходим на страницу жителя
                            result = await HelpMethod.Get(pair.Key, client);

                            // Парсим ссылку на выселенения
                            string evictLink = new Regex("href=\"(.*?)\">Выселить").Match(result).Groups[1].Value.Replace("&amp;", "&");

                            // Если ссылка на выселенения не пустая
                            if (evictLink.Length > 0)
                            {
                                // Выселяем жителя
                                await HelpMethod.Get($"{evictLink}", client);

                            }
                        }
                        pred = pair.Value;
                    }
                }//если развернутый список
                result = await HelpMethod.Get(url, client);
                if (result.Contains("Свернуть список"))
                {
                    // Парсим ссылку, для раскрытие списка
                    string expandResidentsLink = new Regex("\"(.*?collapseResidentsLink.*?)\"").Match(result).Value;

                    // Проверяем то что ссылка не пустая и сворачиваем список
                    if (expandResidentsLink.Length > 0)
                    {
                        await HelpMethod.Get(expandResidentsLink, client, cancellationToken);
                    }
                }
            } //если не пустая ссылка
        }
        //позвать посетителей
        public static async Task GetLift(int BotID, MainFormAll Form, HttpClient client, int ot, System.Threading.CancellationToken cancellationToken = default)
        {
            //развезём имеющихся
            await Lift(BotID, Form, client, ot, cancellationToken);
            HelpMethod.StatusLog("Зовем посетителей", BotID, Form, Resources.thinking);
            string result = await HelpMethod.Get("/lift", client, cancellationToken);
            string url = new Regex("href=\"(.*?activateLiftLink.*?)\">").Match(result).Groups[1].Value;
            //если правильно распарсили, зовем за 2 бакса
            if (url.Length > 0) { await HelpMethod.Get(url, client, cancellationToken); HelpMethod.Log("Позвали в лифт ", BotID, Form); }

        }
        //открыть 10-ю дверь
        public static async Task Open10 (int BotID, MainFormAll Form, HttpClient client, string MinKolKey)
        {
            
            HelpMethod.StatusLog("Открываем 10 дверь", BotID, Form, Resources.thinking);
            //переходим в лаб
            string result = await HelpMethod.Get("/doors", client);
            //глянем остаток ключей
            string ost = new Regex("Осталось ключей:.*<span>(.*?)</span>").Match(result).Groups[1].Value;
            //если не осталось ключей, выходим
            if (ost.Equals("0")) { HelpMethod.StatusLog("Нет ключей!", BotID, Form, Resources.thinking); return; }
            
            if (HelpMethod.ToInt(ost) <= HelpMethod.ToInt(MinKolKey)) { HelpMethod.StatusLog($"Осталось мало ключей ({ost}<{MinKolKey})", BotID, Form, Resources.thinking); return; }
            
            //смотрим в какой мы комнате
            string room = new Regex("Комната: <b class=\"amount\">(.*?)</b>").Match(result).Groups[1].Value;
            //если не в 10-й комнате, топаем доходить до 10-й двери
            if (!room.Equals("10")) { await OpenDoors(BotID, Form, client,MinKolKey); }
            //открываем первую дверь
            string url = new Regex("<a href=\"(.*?doorLink1.*?)\">").Match(result).Groups[1].Value;
            result = await HelpMethod.Get($"{url}", client);
            result = HelpMethod.GetNagrada10door(result);
            HelpMethod.Log("Открыли 10 дверь. Награда:\n"+result, BotID, Form);
        }
        public static async Task PayBudg(int BotID, MainFormAll Form, HttpClient client,string sum)
        {
            
            HelpMethod.StatusLog("Пополняем бюджет", BotID, Form, Resources.thinking);
            string result = await HelpMethod.Get("/wicket/bookmarkable/ru.overmobile.towers.wicket.pages.guild.GuildPage", client);
            string url = new Regex("<a class=\"link\" href=\"(.*?)\"><img width=\"16\" height=\"16\" src=\"/images/icons/bank.png").Match(result).Groups[1].Value;
            result= await HelpMethod.Get(url, client);
            url = new Regex("<form action=\"(.*?)\"").Match(result).Groups[1].Value;
            if (url.Length > 0)
            {
                string id = new Regex("id=\"(id.*?)\" value=\"Пополнить бюджет\"").Match(result).Groups[1].Value;
                // Генерируем POST запрос
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    {"id", id },
                    {"money", sum },
                    { ":submit", "Пополнить бюджет" }
                };
                await HelpMethod.Post(url, parameters, client);
                HelpMethod.StatusLog("", BotID, Form);
                HelpMethod.Log("Пополнили бюджет на " + sum, BotID, Form);
            }
            else { HelpMethod.Log("Не удалось пополнить бюджет на " + sum, BotID, Form); }
        }
        /// <summary>
        /// Метод, который проходит лабиринт
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        /// <returns></returns>
        public static async Task OpenDoors(int BotID, MainFormAll Form, HttpClient client, string MinKolKey)
        {
            
            
            HelpMethod.StatusLog("Зайдем в лабиринт", BotID, Form, Resources.thinking);
            
            // Переходим в лабиринт
            string result = await HelpMethod.Get("/doors", client);
            // определяем в какой мы комнате
            string room = new Regex("Комната: <b class=\"amount\">(.*?)</b>").Match(result).Groups[1].Value;
            // определяем сколько у нас ключей
            string ost = new Regex("Осталось ключей:.*<span>(.*?)</span>").Match(result).Groups[1].Value;
            
            if (HelpMethod.ToInt(ost) <= HelpMethod.ToInt(MinKolKey)) { return; }
               
            // Проверяем необходимость открывания
            if (room.Equals("10")|| ost.Equals("0")) {  return; }
            
                HelpMethod.StatusLog("Проходим лабиринт", BotID, Form);
                Random rnd = new Random();
                int door;
                int kolKey = 0;
                int baks = 0;
                int coin = 0;
                string other = "";
                int miniPriz = 0;
            string url;
                do
                {
                
                kolKey++;
                    //случайно определяем в какую дверь входить
                    door = rnd.Next(1, 4);

                // Парсим ссылку для случайной двери
                // href = "./doors?0-1.-doorLink1&action=1783530840401" href="./doors?0-1.-doorLink2&action=1783530840401"
                 url = new Regex("<a href=\"(.{30,60}doorLink" + door + ".*?)\">").Match(result).Groups[1].Value.Replace("&amp;", "&");
                if (url.Length == 0) { HelpMethod.Log($"Пустая ссылка {url} {door} на лаб! ", BotID, Form); return; }
                    // определяем в какой мы комнате
                    room = new Regex("Комната: <b class=\"amount\">(.*?)</b>").Match(result).Groups[1].Value;
                    // обновляем статус
                    HelpMethod.StatusLog("Открываю " + door + " дверь в " + room + " комнате", BotID, Form);
                    // Проверяем успешность парсинга
                    if (url.Length > 0)
                    {
                        // Переходим по ссылке 
                        result = await HelpMethod.Get($"{url}", client);
                    await CheckInv(result, BotID, client, Form);
                    result =result.Replace("&#039;", "");
                        if (result.Contains("Мини-приз")||result.Contains("Вы нашли")) { 
                            Match priz = new Regex(@"<span class=""amount""><img src=""/images/icons/mn_(.+?).png"".*?<span>(\d+)</span>").Match(result);
                            if (priz.Groups[1].Value.Equals("iron")) { coin += HelpMethod.ToInt(priz.Groups[2].Value); if (result.Contains("Вы нашли")) { HelpMethod.Log($"Найдено: {HelpMethod.StringNumberFormat(priz.Groups[2].Value,true)} монет", BotID, Form); } else { miniPriz = HelpMethod.ToInt(priz.Groups[2].Value); } }
                            if (priz.Groups[1].Value.Equals("gold")) { baks += HelpMethod.ToInt(priz.Groups[2].Value); HelpMethod.Log($"Найдено: {priz.Groups[2].Value}$", BotID, Form); }
                            priz = new Regex(@"<a class=""buff tdn"" href="".+?"">(.+?)<img.+?>(.+?)</a>").Match(result);
                            if (priz.Success) { other += priz.Groups[1].Value + " " + priz.Groups[2].Value+" "; HelpMethod.Log($"Найдено: {priz.Groups[1].Value + " " + priz.Groups[2].Value}", BotID, Form); }
                    }
                        if (result.Contains("Начать сначала"))
                        {

                            HelpMethod.StatusLog(" Тупик в " + room + " комнате :(", BotID, Form);
                            result = await HelpMethod.Get("/doors", client);
                        //HelpMethod.Log($"Тупик в комнате:{room}. Осталось ключей: {ost}. Мини-приз: {HelpMethod.StringNumberFormat(miniPriz.ToString(),true)} монет", BotID, Form);
                    }
                        ost = new Regex("Осталось ключей:.*<span>(.*?)</span>").Match(result).Groups[1].Value;
                        room = new Regex("Комната: <b class=\"amount\">(.*?)</b>").Match(result).Groups[1].Value;

                    if (HelpMethod.ToInt(ost) <= HelpMethod.ToInt(MinKolKey)) { break; }

                    //не открываем 10-ю дверь
                    if (room.Equals("10")) { break; }
                    // using (StreamWriter sw = System.IO.File.AppendText(AppDomain.CurrentDomain.BaseDirectory + "statDoors.txt"))
                    //  {
                    //      sw.WriteLine($"{$"{DateTime.Now:dd.MM.yyyy,HH:mm:ss},"}" + alg);
                    // }
                    //  break; }
                    if (kolKey==100) { HelpMethod.Log("Потратили " + kolKey + " ключей и не дошли, прерываем", BotID, Form); break; }
                    if (!MainFormAll.Start[$"{BotID}"]) { return; }
                }
                } while (!ost.Equals("0")&& url.Length > 0); //пока есть хоть один ключ

                HelpMethod.Log("Потратили " + kolKey + " ключей", BotID, Form);
                HelpMethod.Log("Дошли до " + room + " двери", BotID, Form);
                HelpMethod.Log("Осталось " + ost + " ключей", BotID, Form);
                HelpMethod.Log($"Получено монет: {HelpMethod.StringNumberFormat(coin.ToString(),true)}", BotID, Form);
                if (baks > 0) { HelpMethod.Log($"Получено баксов: {baks}", BotID, Form); }
                if (other.Length > 0) { HelpMethod.Log($"Получено бонусов: {other}", BotID, Form); }
        }

        /// <summary>
        /// Метод, который нанимает в бирже труда жителей со знаком (+).
        /// </summary>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        /// <returns></returns>
        public static async Task AddDirector(int BotID, MainFormAll Form, HttpClient client)
        {
            // Переходим в биржу труда
            string result = await HelpMethod.Get("/vendor/humans", client);

            // Количество нанятых жителей
            int vendor_humans = 0;

            // Парсим цифру бесплатного количество обновлений
            string update_count = new Regex("осталось ([0-9]) раз").Match(result).Groups[1].Value;

            // Если есть бесплатные попытки обновлений
            if (update_count.Length > 0)
            {
                HelpMethod.StatusLog("Ищем жителей на бирже труда...", BotID, Form);

                // Запускаем цикл
                for (int i = 1; i <= HelpMethod.ToInt(update_count); i++)
                {
                    // Проходимся по жителям
                    foreach (object item in new Regex("<li>.*?</li>", RegexOptions.Singleline).Matches(result))
                    {
                        
                        string li_humans = item.ToString();

                        // Если у жителя есть знак (+)
                        if (li_humans.Contains("(+)") && li_humans.Contains("director"))
                        {
                            string tek_dir = await HelpMethod.Get("/directors", client);
                            List<string> arr = new List<string>();
                            int d = 0;
                            foreach (object itemdir in new Regex("<div class=\"inbl btn p2\">.*?</div>", RegexOptions.Singleline).Matches(tek_dir))
                            {
                                if (itemdir.ToString().Contains("human/director/")) { if(!arr.Contains(new Regex("human/director/(.*?).png").Match(itemdir.ToString()).Groups[1].Value)) d++; arr.Add(new Regex("human/director/(.*?).png").Match(itemdir.ToString()).Groups[1].Value);  }
                            }
                            string tek_hum = "";
                            bool cont = true;
                            if (d > 1)
                            {
                                cont = false;
                                tek_hum = new Regex("human/director/(.*?).png").Match(result).Groups[1].Value;
                                if (arr.Contains(tek_hum)) cont = true;
                            }
                            if (cont)
                            {
                                // Парсим ссылку, чтобы нанять жителя
                                string url = new Regex(@"\?(.*?)""><span>Нанять").Match(li_humans).Groups[1].Value;

                            // Нанимаем жителя
                            result = await HelpMethod.Get(url, client);

                            // Подтверждение
                            if (result.Contains("Подтверждение"))
                            {
                                // Парсим ссылку на подтверждение
                                url = new Regex("href=\"(.*?)\">Да").Match(result).Groups[1].Value;

                                // Переходим
                                result = await HelpMethod.Get($"/{url}", client);

                                if (result.Contains("Новый житель"))
                                {
                                    // Прибавляем количество нанятных жителей
                                    vendor_humans++;

                                    // Переходим обратно в биржу труда
                                    result = await HelpMethod.Get("/vendor/humans", client);
                                }
                            }
                        }
                        }
                    }

                    // Парсим ссылку кнопки "Обновить"
                    string update_url = new Regex("href=\"(.*?)\">Обновить").Match(result).Groups[1].Value;

                    // Переходим
                    result = await HelpMethod.Get($"/{update_url}", client);
                }

                if (vendor_humans > 0)
                {
                    HelpMethod.Log($"Наняли жителей на бирже труда: {vendor_humans}", BotID, Form);
                }
            }
        }

        public static async Task VendorsHumans(int BotID, MainFormAll Form, HttpClient client)
        {
            // Переходим в биржу труда
            string result = await HelpMethod.Get("/vendor/humans", client);

            // Количество нанятых жителей
            int vendor_humans = 0;

            // Парсим цифру бесплатного количество обновлений
            string update_count = new Regex("осталось ([0-9]) раз").Match(result).Groups[1].Value;

            // Если есть бесплатные попытки обновлений
            if (update_count.Length > 0)
            {
                HelpMethod.StatusLog("Ищем жителей на бирже труда...", BotID, Form);

                // Запускаем цикл
                for (int i = 1; i <= HelpMethod.ToInt(update_count); i++)
                {
                    // Проходимся по жителям
                    foreach (object item in new Regex("<li>.*?</li>", RegexOptions.Singleline).Matches(result))
                    {

                        string li_humans = item.ToString();

                        // Если у жителя есть знак (+)
                        if (li_humans.Contains("(+)"))
                        {
                            // Парсим ссылку, чтобы нанять жителя
                            string url = new Regex(@"\?(.*?)""><span>Нанять").Match(li_humans).Groups[1].Value;

                            // Нанимаем жителя
                            result = await HelpMethod.Get(url, client);

                            // Подтверждение
                            if (result.Contains("Подтверждение"))
                            {
                                // Парсим ссылку на подтверждение
                                url = new Regex("href=\"(.*?)\">Да").Match(result).Groups[1].Value;

                                // Переходим
                                result = await HelpMethod.Get($"/{url}", client);

                                if (result.Contains("Новый житель"))
                                {
                                    // Прибавляем количество нанятных жителей
                                    vendor_humans++;

                                    // Переходим обратно в биржу труда
                                    result = await HelpMethod.Get("/vendor/humans", client);
                                }
                            }
                        }
                    }

                    // Парсим ссылку кнопки "Обновить"
                    string update_url = new Regex("href=\"(.*?)\">Обновить").Match(result).Groups[1].Value;

                    // Переходим
                    result = await HelpMethod.Get($"/{update_url}", client);
                }

                if (vendor_humans > 0)
                {
                    HelpMethod.Log($"Наняли жителей на бирже труда: {vendor_humans}", BotID, Form);
                }
            }
        }
        public static async Task Build(int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default) {
            string url = "/home";
            
            string result = await HelpMethod.Get(url,client, cancellationToken);
            HelpMethod.StatusLog("Строим этаж!", BotID, Form);
            url = new Regex("<a class=\"tdu\" href=\"(.*?buyNewFloorPanel.*?)\">").Match(result).Groups[1].Value;
            if (url.Length > 0)
            {
                result = await HelpMethod.Get(url, client, cancellationToken);
            } else { HelpMethod.Log("Нет денег на этаж!", BotID, Form); return; }
            url = new Regex("<a class=\"btng cnfrm\" href=\"(.*?confirmLink.*?)\">Да").Match(result).Groups[1].Value;
            if (url.Length > 0)
            {
                result = await HelpMethod.Get(url, client, cancellationToken);
            }

            url = new Regex("<a href=\"(.*?floorLink.*?)\" class=").Match(result).Groups[1].Value;
            if (url.Length > 0)
            {
                await HelpMethod.Get(url, client, cancellationToken);
                HelpMethod.Log("Построили этаж!",BotID,Form);
            }
       }

        public static async Task CancelZad(int BotID, MainFormAll Form, HttpClient client,string url)
        {
            //string url = "/city/coll";
            string result = await HelpMethod.Get(url, client);
            HelpMethod.StatusLog("Отменяем задание по ссылке "+url, BotID, Form);
            url = new Regex("<a class=\"small minor nshd\" href=\"(.*?cancelLink.*?)\">").Match(result).Groups[1].Value;
            if (url.Length > 0)
            {
                result = await HelpMethod.Get(url, client);
            }
            else { HelpMethod.StatusLog("Нет ссылки на отмену", BotID, Form); return; }
            url = new Regex("<a class=\"btng cnfrm\" href=\"(.*?confirmLink.*?)\">Да").Match(result).Groups[1].Value;
            if (url.Length > 0)
            {
                await HelpMethod.Get(url, client);
            }
            else { HelpMethod.StatusLog("Нет ссылки на подтверждение", BotID, Form); return; }
            HelpMethod.Log("Отменили задание", BotID, Form);
        }
        public static async Task GetSpecial (int BotID, MainFormAll Form, HttpClient client)
        {

            string result = await HelpMethod.Get("/humans", client);
            string url = new Regex("href=\"(.*?filterUpgradeAvailable.*?)\">").Match(result).Groups[1].Value;
            int kol = 0;
            if (url.Length>0)
            {
                result = await HelpMethod.Get($"{url}", client);
                int i = 0;
                do
                {
                    
                    i++;
                    if (i > 300) { break; }
                    result.Replace("&#036;","");
                    string baks = new Regex(@"<img src=""/images/icons/mn_gold\.png"" width=""16"" height=""16"" alt=""\$""/><span>([0-9]+)</span>").Match(result).Groups[1].Value;
                    if (HelpMethod.ToInt(baks) < 10) { break; }
                    url = new Regex("<a class=\"tdu\" href=\"(.*?upgradeLinkPanel.*?)\"><span>Обучить за").Match(result).Groups[1].Value;
                    if (url.Length > 0)
                    {
                        result = await HelpMethod.Get($"{url}", client);
                        kol++;
                    }
                    if (!MainFormAll.Start[$"{BotID}"]) { return; }
                } while (result.Contains("Обучить за"));
            }
           // else { HelpMethod.Log("Все специалисты улучшены",BotID,Form); }
            if (kol > 0) { HelpMethod.Log($"Улучшили {kol} специалистов", BotID, Form); }
        }
        
            /// <summary>
            /// Метод, который собирает награды в осеннем марафоне.
            /// </summary>
            /// <param name="BotID">Идентификатор бота (вкладки).</param>
            /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
            /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
            /// <returns></returns>
            public static async Task AutumnMarathon(int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            // Переходим на страницу марафона
            string result = await HelpMethod.Get("/tasks", client);
            if (result.Contains("Получить награду"))
            {
                await HelpMethod.ClickGreenButton(result, "Получить награду!", client,"marafon");
                HelpMethod.StatusLog("Получаем награды... Марафон", BotID, Form, Resources.leaf_l);
                HelpMethod.Log("Получили награду за марафон" ,BotID,Form);
            }
        }
        public static async Task AwayCity(int BotID, MainFormAll Form, IniFiles settings, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Выходим с города...", BotID, Form);

            Dictionary<string, string> account_settings = settings.GetSett($"USER_{BotID}");

            HttpClient client = HelpMethod.HttpManager(account_settings["LOGIN"]);
           if (!string.IsNullOrEmpty(account_settings["LOGIN"]) && HelpMethod._httpClients.TryGetValue(account_settings["LOGIN"], out var existingClient))
            { 
                client.BaseAddress = new Uri("https://" + account_settings["SERVER"]);
            }
            string url = "/home";
            string result = await HelpMethod.Get(url, client);
            if (result.Contains("Начни играть!"))
            {
                HelpMethod.StatusLog("Авторизация", BotID, Form);
                await Authorization(account_settings["LOGIN"], account_settings["PASSWORD"], client, account_settings["SERVER"]);
            }
            await SetNoVis(client);
            HelpMethod.StatusLog("Выходим с города...", BotID, Form);
            url = "/wicket/bookmarkable/ru.overmobile.towers.wicket.pages.guild.GuildPage";
            result = await HelpMethod.Get(url, client);
            url = new Regex("<a class=\"minor nshd tdn\" href=\"(.*?)\">Покинуть город").Match(result).Groups[1].Value;
            result = await HelpMethod.Get(url, client);
            url = new Regex("<a class=\"btng cnfrm\" href=\"(.*?)\">Да").Match(result).Groups[1].Value;
            await HelpMethod.Get(url, client);
            HelpMethod.StatusLog("", BotID, Form);
        }

            public static async Task Away(String Command, int BotID, MainFormAll Form, HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            HelpMethod.StatusLog("Выселяем жителей...", BotID, Form, Resources.man_minus);

            string url = "/humans";
            int kol = 0;
            int kol_tek = 0;
            string color = "";

            if (Command.StartsWith("Выселить")) { kol = HelpMethod.ToInt(Command.Replace("Выселить ","")); }
            if (Command.Contains("Зелёных")|| Command.Contains("Голубых") || Command.Contains("Жёлтых") || Command.Contains("Фиолетовых") || Command.Contains("Оранжевых")) { color=Command.Replace("Всех кроме ",""); }
            if (Command.Equals("Всех")) { kol = 130; }
            
            if (kol > 0)
            {
                string result = await HelpMethod.Get(url, client);
                int i = 0;
                do
                {
                    i++;
                    if (i > 300) { break; }
                    url = new Regex("<a href=\"(.*?human.*?)\" class=\"white\">").Match(result).Groups[1].Value;
                    result = await HelpMethod.Get(url, client);

                    url = new Regex("<a class=\"btnr\" href=\"(.*?)\">Выселить</a>").Match(result).Groups[1].Value;
                    result = await HelpMethod.Get(url, client);
                    await CheckInv(result, BotID, client, Form);
                    kol_tek++;
                    HelpMethod.StatusLog("Выселили " + kol_tek.ToString(), BotID, Form, Resources.man_minus);
                    if (kol_tek == kol) { break; }
                    if (!MainFormAll.Start[$"{BotID}"]) { return; }
                } while (url.Length > 0 && kol > kol_tek);
            }

            if (!color.Equals("")) 
            {
                string result = await HelpMethod.Get("/home", client);
                url = new Regex("<a class=\"flhdr\" href=\"(.{1,20}/floor/[0-9]{1,20})\">.{1,10}<span class=\"\">1. Гостиница</span>", RegexOptions.Singleline).Match(result).Groups[1].Value;
                result = await HelpMethod.Get(url, client);
                if (result.Contains("Раскрыть список"))
                {
                    // Парсим ссылку, для раскрытие списка
                    string expandResidentsLink = new Regex("\"(.*?collapseResidentsLink.*?)\"").Match(result).Value;

                    // Проверяем то что ссылка не пустая и сворачиваем список
                    if (expandResidentsLink.Length > 0)
                    {
                        await HelpMethod.Get(expandResidentsLink, client);
                    }
                }
                color = color.Replace("Зелёных", "fd");
                color = color.Replace("Голубых", "sr");
                color = color.Replace("Жёлтых", "rc");
                color = color.Replace("Фиолетовых", "fs");
                color = color.Replace("Оранжевых", "el");
                foreach (Match match in Regex.Matches(result, "<li>(.*?)</li>", RegexOptions.Singleline | RegexOptions.Multiline)) 
                {
                    bool usl= match.Groups[1].Value.Contains("class=\"" + color); 
                    if(Command.Contains("кроме")) { usl = !usl; }
                    if (usl)
                    {

                        url = new Regex("<a href=\"(.*?)\" class=\"white\">").Match(match.Groups[1].Value).Groups[1].Value;
                        if (url.Length > 0)
                        {
                            string resInner = await HelpMethod.Get(url, client, cancellationToken);
                            url = new Regex("<a class=\"btnr\" href=\"(.*?)\">Выселить</a>").Match(resInner).Groups[1].Value;
                            if (url.Length > 0)
                            {
                                await HelpMethod.Get(url, client, cancellationToken);
                                kol_tek++;
                                HelpMethod.StatusLog("Выселили " + kol_tek.ToString(), BotID, Form, Resources.man_minus);
                                if (!MainFormAll.Start[$"{BotID}"]) { return; }
                            }
                        }
                    }
                }
                if (result.Contains("Свернуть список"))
                {
                    // Парсим ссылку, для раскрытие списка
                    string expandResidentsLink = new Regex(@"\?wicket:interface=:[0-9]*?:floorPanel:collapseResidentsLink::ILinkListener::").Match(result).Value;

                    // Проверяем то что ссылка не пустая и сворачиваем список
                    if (expandResidentsLink.Length > 0)
                    {
                        await HelpMethod.Get(expandResidentsLink, client);
                    }
                }

            }
            HelpMethod.StatusLog("", BotID, Form);
            HelpMethod.Log($"Выселили {kol_tek} жителей",BotID,Form);
        }
        public static async Task AddDopMesto(string url, int BotID, MainFormAll Form, HttpClient client)
        {
            HelpMethod.StatusLog("Покупаем доп.места в гостинице", BotID, Form);
            string result = await HelpMethod.Get(url,client);
            url = new Regex("<a class=\"tdu\" href=\"(.*?)\">").Match(result).Groups[1].Value;
            if (result.Contains("<span>0</span>")&&url.Length>0)
            {
                result = await HelpMethod.Get(url, client);
                if (result.Contains("Дополнительное место куплено")) { HelpMethod.Log("Купили доп.место в гостинице", BotID, Form); }
            }

        }
        public static async Task SetNoVis(HttpClient client)
        {
           await SetNoVis(client,false);
        }

        public static async Task SetNoVis(HttpClient client,bool inv)
        {
            string url = "/settings";
            string result = await HelpMethod.Get(url, client);

            if (result.Contains("<span class=\"nick\">показывать</span>") && !inv)
            {
                url = new Regex(@"<a href=""(\?wicket.*?:guildSearchLink::ILinkListener::)\"">Изменить</a>").Match(result).Groups[1].Value;
            }
            if (result.Contains("<span class=\"nick\">не показывать</span>") && inv)
            {
                url = new Regex(@"<a href=""(\?wicket.*?:guildSearchLink::ILinkListener::)\"">Изменить</a>").Match(result).Groups[1].Value;
            }
            await HelpMethod.Get(url, client);
        }

        public static async Task<string> SendMsg(MainFormAll Form, int BotID, HttpClient client, string id, System.Threading.CancellationToken cancellationToken = default)
        {
            
            string url;
            string result;
             
            bool inv = id == "free";
            //Console.WriteLine($"id={id} bool inv={inv.ToString()}");
            HelpMethod.StatusLog("Прячусь от городов...", BotID, Form, Resources.update);
            await SetNoVis(client, inv);

            url = "/home";
            result = await HelpMethod.Get(url, client, cancellationToken);
            if (result.Contains("Вас приглашают в город"))
            {
                if (!inv)
                {
                    HelpMethod.StatusLog("Отклоняем предыдущее приглашение", BotID, Form);
                    url = new Regex("<a class=\"minor\" href=\"(.*?)\">отклонить").Match(result).Groups[1].Value;
                }
                else 
                {
                    HelpMethod.StatusLog("Принимаем текущее приглашение", BotID, Form);
                    url = new Regex("<a class=\"btng cnfrm\" href=\"(.*?)\">принять").Match(result).Groups[1].Value;
                    await HelpMethod.Get(url, client);
                    HelpMethod.Log("Перешли в город", BotID, Form);
                    HelpMethod.StatusLog("", BotID, Form);
                    return "";
                }

                await HelpMethod.Get(url, client);
            }

            if (!inv)
            {
                result = await HelpMethod.Get("/mail/send/id/" + id, client);
                url = new Regex("<form action=\"(.*?)\"").Match(result).Groups[1].Value;
                string hidden_input = new Regex("<input type=\"hidden\" name=\"(.*?)\" id=").Match(result).Groups[1].Value;
                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    {hidden_input, "" },
                    {"text", "Пригласи" },
                    { ":submit", "Отправить" }
                };

                // Отправляем запрос
                HelpMethod.StatusLog("Отправляем сообщение боту", BotID, Form);
                await HelpMethod.Post(url, parameters, client);
            }
            url = "/home";
            int i = 0;
            do
            {
                i++;
                if (i > 10) { break; }
                HelpMethod.StatusLog("Ждем ответа "+i.ToString(), BotID, Form);
                result = await HelpMethod.Get(url, client, cancellationToken);
                await HelpMethod.RandomDelay(1000,2000);
                if (cancellationToken.IsCancellationRequested || !MainFormAll.Start[$"{BotID}"]) { return "" ; }
            }
            while (!result.Contains("принять"));
            if (i <= 10)
            {
                url = new Regex("<a class=\"btng cnfrm\" href=\"(.*?)\">").Match(result).Groups[1].Value;
                await HelpMethod.Get(url, client);
                HelpMethod.Log("Перешли в город", BotID, Form);
                HelpMethod.StatusLog("", BotID, Form);
                return "Успешно перешли в город!";
            }
            HelpMethod.StatusLog("", BotID, Form);
            HelpMethod.Log("Нет приглашения в город", BotID, Form);
            return "Нет мест";

        }
        public static async Task<string> Create(MainFormAll Form, IniFiles settings, int BotID, List<int> arrUser, int sex, System.Threading.CancellationToken cancellationToken = default)
        {
            string Sections = "";
            HttpClient client = HelpMethod.HttpManager();
            client.BaseAddress = new Uri("https://nebo.mobi");
            int i = 0;
            string url = "/start";
            HelpMethod.StatusLog("Создаем нового перса...", BotID, Form, Resources.update);
            string result = await HelpMethod.Get(url, client, cancellationToken);

            do
            {
                HelpMethod.StatusLog("Собираю выручку...", BotID, Form, Resources.update);
                url = new Regex("<a class=\"tdu\" href=\"(.*?floorPanel.*?)\">Собрать выручку!").Match(result).Groups[1].Value;
                await HelpMethod.Get(url, client, cancellationToken);

                url = "/home";
                result = await HelpMethod.Get(url, client, cancellationToken);
                if (!MainFormAll.Start[$"{BotID}"]) { return ""; }
            } while (result.Contains("Собрать выручку"));

            do
            {
                HelpMethod.StatusLog("Закупаю товар...", BotID, Form, Resources.update);
                url = new Regex("<a class=\"tdu\" href=\"(floor.*?)\">Закупить товар").Match(result).Groups[1].Value;
                result = await HelpMethod.Get(url, client, cancellationToken);

                url = new Regex("<a class=\"tdu\" href=\"(.*?)\">").Match(result).Groups[1].Value;
                result = await HelpMethod.Get(url, client, cancellationToken);

                url = "/home";
                result = await HelpMethod.Get(url, client, cancellationToken);
                if (!MainFormAll.Start[$"{BotID}"]) { return""; }
            } while (result.Contains("Закупить товар"));

            do
            {
                HelpMethod.StatusLog("Катаю лифт...", BotID, Form, Resources.update);
                url = "/lift";
                result = await HelpMethod.Get(url, client, cancellationToken);

                url = new Regex("<a class=\"tdu\" href=\"(.*?)\">Поднять лифт").Match(result).Groups[1].Value;
                if (url.Length > 0)
                {
                    result = await HelpMethod.Get(url, client);
                    url = new Regex("<a class=\"tdu\" href=\"(.*?)\"><span>Получить чаевые").Match(result).Groups[1].Value;
                    result = await HelpMethod.Get(url, client);
                }
                url = "/lift";
                result = await HelpMethod.Get(url, client);
                if (!MainFormAll.Start[$"{BotID}"]) { return""; }

            } while (result.Contains("Поднять лифт")||result.Contains("Получить чаевые"));

            do
            {
                HelpMethod.StatusLog("Строю этаж...", BotID, Form, Resources.update);
                url = "/home";
                result = await HelpMethod.Get(url, client);

                url = new Regex("<a class=\"tdu\" href=\"(.*?buyNewFloorPanel.*?)\">").Match(result).Groups[1].Value;
                result = await HelpMethod.Get(url, client);

                url = new Regex("<a class=\"btng cnfrm\" href=\"(.*?confirmLink.*?)\">Да").Match(result).Groups[1].Value;
                result = await HelpMethod.Get(url, client);

                url = new Regex("<a href=\"(.*?floorLink.*?) class=").Match(result).Groups[1].Value;
                result = await HelpMethod.Get(url, client);

                url = "/home";
                result = await HelpMethod.Get(url, client);
                if (!MainFormAll.Start[$"{BotID}"]) { return ""; }
            } while (result.Contains("Построй новый этаж"));


            url = "/save";
            result = await HelpMethod.Get(url, client);
            string hidden_input = new Regex("<input type=\"hidden\" name=\"(.*?)\" id=").Match(result).Groups[1].Value;
            url = new Regex("action=\"(.*?)\" id=").Match(result).Groups[1].Value;
            string sex_nebo = "m";
            if (sex > 1) { sex_nebo = "f"; }
                string Login;
                do
                {
                    Login = await GetLogin(sex);
                    // Генерируем POST запрос
                    Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    {hidden_input, "" },
                    {"login", Login },
                    {"sex", sex_nebo },
                    {"password", DEF_PASS },
                    { ":submit", "Вход" }
                };
                    try
                    {
                    // Отправляем запрос
                    HelpMethod.StatusLog("Сохраняю..." + Login, BotID, Form, Resources.update);
                    result = await HelpMethod.Post(url,parameters,client);
                    if (!MainFormAll.Start[$"{BotID}"]) { return""; }
                }
                    catch (Exception ex) { Logger.Write("ex POST save = " + ex); }
                } while (result.Contains("уже занято"));

            HelpMethod.StatusLog("Прячусь от городов...", BotID, Form, Resources.update);
            await SetNoVis(client);
            i = 0;
            do { i++; } while (arrUser.Exists(x => x == i));
            Sections = "USER_" + i.ToString();
            DefSettings(Sections, settings, Login, DEF_PASS);
            client.Dispose();
            return Sections;
        }


        public static async Task<string> GetLogin(int sex, System.Threading.CancellationToken cancellationToken = default)
        {
            HttpClient client = HelpMethod.HttpManager();
            string url = "https://ciox.ru/nickname-generator";
            await HelpMethod.Get(url, client, cancellationToken);

            Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    {"amount", "1" },
                    {"person", sex.ToString() },
                    {"lang", "1" },
                    { "enter", "" }
                };
            string result = "";
            try
            {
                result = await HelpMethod.Post(url, parameters, client);
            }
            catch(Exception ex) { Logger.Write("ex POST = " + ex); }
           
            return new Regex("input_c result\">(.*?)<").Match(result).Groups[1].Value;
        }

        public static void DefSettings(string sections, IniFiles settings,string Login, string Pass, string Server="nebo.mobi")
        {
            settings.Write(sections, "LOGIN", Login); //логин
            settings.Write(sections, "PASSWORD", Pass); //пароль
            settings.Write(sections, "INTERVAL_FROM", "1"); //Интервал повтора от
            settings.Write(sections, "INTERVAL_DO", "2"); //Интервал повтора до
            settings.Write(sections, "SERVER", Server); //Сервер входа
            settings.Write(sections, "AVATAR", "man_no"); //Аватар
            settings.Write(sections, "COLLECT_COIN", "true"); //Собирать выручку
            settings.Write(sections, "SELL_GOODS", "true"); //Выкладывать товар
            settings.Write(sections, "BUY_GOODS", "true"); //Закупать товар
            settings.Write(sections, "FLOOR_OPEN", "true"); //Открывать этажи
            settings.Write(sections, "LIFT_UP", "true"); //Катать лифт
            settings.Write(sections, "QUESTS", "true"); //Брать награду за перс.задания
            settings.Write(sections, "HOSTEL_EVICT_LESS_9", "true"); //Выселять меньше 9
            settings.Write(sections, "HOSTEL_EVICT_MINUS", "true"); //Выселять с (-)
            settings.Write(sections, "HOSTEL_EVICT_PLUS", "false"); //Выселять с (+)
            settings.Write(sections, "BUSINESS_TOURNAMENT", "true"); //Брать награду за бизнес турнир
            settings.Write(sections, "HUMAN_JOBS", "true"); //Нанимать более опытных
            settings.Write(sections, "BUY_BAKS_FOR_COIN", "false"); //Не используется
            settings.Write(sections, "VENDORS_HUMANS", "false"); //нанимать на бирже
            settings.Write(sections, "CITY_QUESTS", "true"); //Получать награду за гор.задания
            settings.Write(sections, "INDIANA", "true"); //Проходить лабиринт
            settings.Write(sections, "START_INV", "false"); //Начинать инвов
            settings.Write(sections, "HELP_INV", "true"); //Помогать с инвами
            settings.Write(sections, "OPEN_1_SUND", "true"); //Открывать только 1 сундук
            settings.Write(sections, "OPEN_KOLL", "true"); //Брать коллекции
            settings.Write(sections, "OPEN_SUND", "true"); //Брать сундуки
            settings.Write(sections, "PAY_MARK", "true"); //Покупать маркетинг
            settings.Write(sections, "PAY_PIAR", "true"); //Покупать пиар
            settings.Write(sections, "OPEN_INDIANA", "true"); //Проходить 10-ю дверь при активном перс.задании
            settings.Write(sections, "GO_LIFT", "false"); //Звать в лифт при активном гор.задании
            settings.Write(sections, "GO_LIFT_PERS", "false"); //Звать в лифт при активном перс.задании
            settings.Write(sections, "UPGR_SPEC", "true"); //Улучшать специалистов
            settings.Write(sections, "OPEN_INDIANA_50", "false"); //Открывать 10-ю дверь в дни повышенного опыта
            settings.Write(sections, "MIN_KOL_KEY", "210"); //Не тратить ключи. если их меньше
            settings.Write(sections, "MIN_FLOOR", "10"); //Строить этажи до
            settings.Write(sections, "DOP_MESTO", "true"); //Расширять гостиницУ, если нет места
            settings.Write(sections, "GET_OPEN_SUND", "true"); //Брать награду за сундуки
            settings.Write(sections, "READ_MAIL", "true"); //Отмечать помечеными сообщения от игры
            settings.Write(sections, "GET_CITY_QUEST", "false"); //Брать гор.задания
            settings.Write(sections, "OPEN10", "false"); //Открывать 10-ю дверь
            settings.Write(sections, "UPG_FLOOR", "true"); //Улучшать этажи в дни акции
            settings.Write(sections, "GO_THREE", "true"); //Выселять >3 жителей одной профессии
            settings.Write(sections, "PAY_BUDG", "false"); //Взносы в бюджет города
            settings.Write(sections, "SUM_BUDG", "5"); //Сумма взносов
            settings.Write(sections, "LIST_GOR", ""); //Список гор.заданий
            settings.Write(sections, "LIST_VIP", "Получи 10 ключей, Набери опыт, Заработай баксы, Задания коллекций, Городские задания, Звезды с заданий, Получи 20 звезд"); //Список заданий ВИПа для отмены
            settings.Write(sections, "TIP_CITY_QUEST", "И брать и помогать"); //Брать/помогать с гор.заданиями
            settings.Write(sections, "BUILD_FLOOR", "false"); //строить этажи
            settings.Write(sections, "CANCEL_VIP", "true"); //Отменять ВИПа
            settings.Write(sections, "NE_INV", "false"); //Не проходить инвов, только участвовать
            settings.Write(sections, "OPEN_LAVKA", "true"); //Открывать лавку подарков
            settings.Write(sections, "ADD_DIRECTOR", "false"); //Нанимать директоров и админов
            settings.Write(sections, "TIP_GOR_ZAD", "за 2 "); //выполнять гор.задания за ..$
            settings.Write(sections, "GO_LIFT_2", "false"); //Постоянно звать в лифт за 2
            settings.Write(sections, "NO_GO", "false"); //Отключить доп.действия
            settings.Write(sections, "LIST_NO", "Отмечаем прочитаным сообщения от Игра, Крутить барабан, Сдавать/получать задания марафона, Собирать/тратить билеты, Открывать/сдавать сундуки, Улучшать специалистов, Открывать этажи, Нанимать более опытных жителей, Улучшать этажи, Пополнять бюджет, Открывать 10-ю если есть перс задание, Покупать доп.место в гостинице, Сдавать гор.задания"); //Список проверок для отмены
            settings.Write(sections, "LOG", "true"); //Вести лог
            settings.Write(sections, "STAT", "true"); //Вести статистику
        }

        /// <summary>
        /// Метод, который обновляет основную статистику и за сессию.
        /// </summary>
        /// <param name="profile_url">Ссылка на профиль.</param>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="HttpClient">Экземпляр <see cref="HttpClient"/>.</param>
        /// <param name="Form">Экземпляр <see cref="MainFormAll"/>.</param>
        /// <param name="Settings">Экземпляр <see cref="IniFiles"/>.</param>
        public static async Task Statistics(string profile_url, int BotID, MainFormAll Form, HttpClient client)
        {
            
            try
            {
                HelpMethod.StatusLog("Обновляем статистику...", BotID, Form, Resources.update);

                // Переходим на главную
                string result_home = await HelpMethod.Get("/home", client);
                string result_profile = await HelpMethod.Get($"/{profile_url}", client);
                string result_minikey = await HelpMethod.Get("/mini/key", client);
                //               string result_lobby= await HelpMethod.Get("/lobby", client);
                await CheckInv(result_minikey, BotID, client, Form);
                string PQ = await PersQuests(BotID, Form, client,false);
                string FreeGor = "";// await FreeGorZad(BotID, Form, client);
                if (PQ == "") { PQ = "Отсутствуют"; }

                result_profile = result_profile.Replace("&#039;","");
                result_home = result_home.Replace("&#039;", "");
                result_minikey = result_minikey.Replace("&#039;", "");
                // Парсим данный из профиля
                string coin = new Regex(@"<img src=""/images/icons/mn_iron\.png"" width=""16"" height=""16"" alt=""o""/><span>([0-9]+)</span>").Match(result_profile).Groups[1].Value;
                string baks = new Regex(@"<img src=""/images/icons/mn_gold\.png"" width=""16"" height=""16"" alt=""\$""/><span>([0-9]+)</span>").Match(result_profile).Groups[1].Value;
                string floor = new Regex("Этажей: <strong class=\"white\">([0-9]+)</strong>").Match(result_profile).Groups[1].Value;
                string level = new Regex("Уровень: <strong class=\"white\">([0-9]+)</strong>").Match(result_profile).Groups[1].Value;
                string avatar = new Regex("/images/icons/user/(.*?).png").Match(result_home).Groups[1].Value;
                string name = new Regex("<span class=\"user\"><span><span>(.*?)</span>").Match(result_profile).Groups[1].Value;
                string sold = new Regex("<span class=\"amount nwr\">.*?<span>([0-9]+)</span>").Match(result_profile).Groups[1].Value;
                string keys = new Regex(@"<img alt="""" src=""/images/icons/key\.png"" width=""16"" height=""16""/><span>([0-9]+)</span>").Match(result_home).Groups[1].Value;
                Match city = new Regex("<a class=\"white\" href=\".*?city.*?\"><span class=\"amount\">(.*?)</span></a>.*?<span>(.*?)</span>").Match(result_profile);
                Match zad = new Regex("<a class=\"white bl tdn\" href=\"(.*?)\"><span.*?>(.*?)</span>.*?<span><span>(.*?)</span>.*?<span>(.*?)</span>", RegexOptions.Singleline | RegexOptions.Multiline).Match(result_home);
                string PM = new Regex("(<div class=\"nfl cntr small ny\">.*?)<ul>", RegexOptions.Singleline | RegexOptions.Multiline).Match(result_profile).Groups[1].Value;
                Match kolMan= new Regex("<span class=\"rs small\">.*?src=\"(.*?png).*?<span>(.*?)</span>.*?src=\"(.*?png).*?<span>(.*?)</span>.*?</a>", RegexOptions.Singleline | RegexOptions.Multiline).Match(result_home);
                string cup= new Regex("<img.*?src=\"(.*?award.*?png)\" alt=").Match(result_home).Groups[1].Value;
                string proc= new Regex("<div class=\"prg\" style=\"width:([0-9]+)%").Match(result_profile).Groups[1].Value;
                Match miniKey = new Regex(@" my"">.*?<td class=""num""><a><span>(\d+)</span>.+?<span>(\d+)</span>", RegexOptions.Singleline | RegexOptions.Multiline).Match(result_minikey);
                string action = new Regex("/action\">(.*?)</a>").Match(result_home).Groups[1].Value;
                //                Match lobby = new Regex("<div class=\"nfl\">.*?<strong class=\"admin\">.*?<div class=\"white\">(.*?)</div>.*?Прогресс:.*?<span><span>(.*?)</span>.*?<span>(.*?)</span>", RegexOptions.Singleline | RegexOptions.Multiline).Match(result_lobby);

                string mail = new Regex("href=\"(.{0,60}/mail)\"").Match(result_home).Groups[1].Value;
                string manager= new Regex("<a class=\"tdn\" href=\"vendor/buff/0/[0-9].*?buff\">Менеджер: (.*?)</span>", RegexOptions.Singleline | RegexOptions.Multiline).Match(result_home).Groups[1].Value;
                manager = manager.Replace("<span ","<");
                PM += manager;
                string txt = "";
                if (mail.Length > 0) { txt = await CheckMail(client, false);  }
                // Получаем ссылку на ToolStrip
                // ToolStrip toolstrip_info_top = FindControl.FindToolStrip("toolstrip_info_top", BotID, Form);
                DisableClickSounds();
                Form.Invoke((MethodInvoker)delegate
                {
                    // Ссылка на вкладку
                    TabPage tabPage = FindControl.FindTabPage("tabPage", BotID, Form);
                    System.Windows.Forms.Button away = FindControl.FindButton("away", BotID, Form);
                    System.Windows.Forms.Button getCity = FindControl.FindButton("getcity", BotID, Form);
                    System.Windows.Forms.Button awayCity = FindControl.FindButton("awaycity", BotID, Form);
                    System.Windows.Forms.Button dop = FindControl.FindButton("dop", BotID, Form);
                    System.Windows.Forms.Button button = FindControl.FindButton("refresh", BotID, Form);
                System.Windows.Forms.Button msgBut = FindControl.FindButton("refresh", BotID, Form);
                away.BringToFront();
                    dop.BringToFront();
                    
                    button.Location = new Point(476, 42);
                    button.Size = new Size(50, 20);
                    button.Font = new Font("Times New Roman", 6F, FontStyle.Regular);

                    // Обновляем текст вкладки и картинку
                    tabPage.Text = name;// $"{FindControl.FindTextBox("textbox_login", BotID, Form).Text}";
                    //tabPage.ToolTipText = $"{BotID} Для удаления вкладки дважды щелкните по ней";
                    tabPage.ImageIndex = Form.imageList1.Images.IndexOfKey($"{avatar.Replace("-", "_")}");
                    if (txt.Length > 0)
                    {
                        /*PictureBox picMsg = new PictureBox
                        {
                            Image = Resources.letters,
                            Location = new Point(362, 11),
                            Name = "picMsg",
                            Size = new Size(19, 19),
                            TabIndex = 10,
                            TabStop = false
                        };
                        picMsg.Click += (s, e) =>
                        {
                            MsgForm msgForm = new MsgForm(BotID, client);
                            msgForm.ShowDialog();
                        };*/
                        //tabPage.Controls.AddRange(new Control[]
                        //    {picMsg});
                        tabPage.ImageIndex = Form.imageList1.Images.IndexOfKey("letters"); tabPage.ToolTipText = txt;
                        /*tabPage.MouseWheel += (s, e) =>
                        {
                            Logger.Write(tabPage.Text);
                            MsgForm msgForm = new MsgForm(BotID, client);
                            msgForm.ShowDialog();

                        };*/
                    } 
                    string page=page1;
                    string server = "http://nebo.mobi";
                    page=page.Replace("%LVL_PRC%",proc);
                    page=page.Replace("%SERVER%", server);
                    page = page.Replace("%NAME%", name);
                    
                    if (cup.Length > 0)
                    {
                        page = page.Replace("%CUP%", $"<img width=\"16\" height=\"16\" src=\"{server}{cup}\" border=\"0\">");
                    }
                    else { page = page.Replace("%CUP%", ""); }
                    if (city.Success)
                    {
                        page = page.Replace("%CITY%", @"<span class=""white""><span class=""amount"">" + city.Groups[1].Value + @"</span></span>, <span class=""white""><span>" + city.Groups[2].Value + "</span></span>");
                        getCity.SendToBack();
                        awayCity.BringToFront();
                    }
                    else 
                    {
                        page = page.Replace("%CITY%", @"<span class=""white""><span class=""amount"">Не в городе</span></span>");
                        if (HelpMethod.ToInt(floor) > 9) getCity.BringToFront();
                        awayCity.SendToBack();

                    }
                    if (keys.Length == 0) { keys = "0"; }
                    page = page.Replace("%LEVEL%", level);
                    page = page.Replace("%FLOOR%", floor);
                    page = page.Replace("%COIN%", HelpMethod.StringNumberFormat(coin,false));
                    page = page.Replace("%BAKS%", baks);
                    page = page.Replace("%KEY%", keys);
                    

                    PM = PM.Replace("<a","<span");
                    PM = PM.Replace("</a", "</span");
                    PM = PM.Replace("gift", "hr");
                    PM = PM.Replace("/images", server+"/images");
                    action=action.Replace("/images", server + "/images");

                    page = page.Replace("%VYR%", PM);
                    page = page.Replace("%ACTION%", "<br/><b>"+action+"</b>");
                    page = page.Replace("%PK%","<b>Мини-турнир.</b> Место: "+miniKey.Groups[1].Value+" Собрано ключей: "+miniKey.Groups[2].Value);
                    page=page.Replace("%PQ%","<b>Перс.задания:</b> "+PQ);
                    page = page.Replace("%KOL_MAN%", $"<img src={server}" + kolMan.Groups[1].Value+ " height=\"16\" width=\"16\"><strong class=\"white\">" + kolMan.Groups[2].Value+ $"</strong> <img src={server}" + kolMan.Groups[3].Value+ " height=\"16\" width=\"16\"><strong class=\"white\">" + kolMan.Groups[4].Value+ "</strong>");
                    string TQ = "";
                    foreach (Match coll in Regex.Matches(result_home, "<a class=\"white bl tdn\" href=\"(.*?)\"><span.*?>(.*?)</span>.*?<span><span>(.*?)</span>.*?<span>(.*?)</span>", RegexOptions.Singleline | RegexOptions.Multiline))
                    {
                        string tekQuest = "";
                        if (coll.Groups[1].Value.Contains("city")) { tekQuest = "города"; }
                        if (coll.Groups[1].Value.Contains("coll")) { tekQuest = "коллекции"; }
                        if (coll.Groups[1].Value.Contains("box")) { tekQuest = "сундуков"; }
                        if (coll.Groups[1].Value.Contains("lobby")) { tekQuest = "VIPa"; }
                        if (coll.Groups[1].Value.Length > 0)
                        {

                            TQ += $"<b>Текущее задание {tekQuest}:</b> {coll.Groups[2].Value} (выполнено {coll.Groups[3].Value} из {coll.Groups[4].Value})<br/>";

                        }
                    }
                    if (FreeGor.Length > 0)
                    {
                        string tmp = "<b>Свободные задания города:</b> ";
                        foreach (Match ZG in Regex.Matches(FreeGor, "(.+?), (.+?), (.+?)\n"))
                        { tmp += ZG.Groups[1].Value + " (" + ZG.Groups[3] + "), "; }
                        tmp = tmp.Substring(0, tmp.Length - 2);
                        TQ += tmp;
                    }
                        page = page.Replace("%TQ%", "" + TQ);
                    try
                    {
                        WebBrowser info = FindControl.FindWebBrouser("Info", BotID, Form);
                        info.IsWebBrowserContextMenuEnabled = false;
                        if (!info.IsBusy){info.DocumentText = page;}

                    }
                    catch (Exception ex)
                    {
                        Logger.Write("stat "+ex);
                        HelpMethod.Log("Ошибка вывода статистики "+ex, BotID, Form);
                        return;
                    }

                });

                HelpMethod.StatusLog("Обновили статистику", BotID, Form, Resources.update);
            } catch(Exception ex) { Logger.Write("stat end " + ex); HelpMethod.Log("Ошибка сбора статистики " + ex, BotID, Form); return; }
        }

        public static async Task<string> CheckMail(HttpClient client, bool read, bool all = false, System.Threading.CancellationToken cancellationToken = default)
        {
            //HelpMethod.StatusLog("Проверяем почту", BotID, Form);
            string res = await HelpMethod.Get("/mail", client, cancellationToken);
            string url = new Regex(@"<a class=""ptd"" href=""(.+?modeNew.+?)""><img").Match(res).Groups[1].Value;
            string txt = "";
            if (!all)
                res = await HelpMethod.Get("/mail" + url, client, cancellationToken);
            if (read)
            {
                foreach (Match match in Regex.Matches(res, @"<span class=""admin"">Игра</span>.+?href=""(.{0,60}mail/read/id/\d+)"">", RegexOptions.Multiline | RegexOptions.Singleline))
                {
                    await HelpMethod.Get(match.Groups[1].Value, client, cancellationToken);
                }
            }
           
                string newall = "tdn link";
                if (all) newall = ".*?";
                /*TextWriter tw2 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "msg_.html");
                tw2.Write(res);
                tw2.Close();
                //debug: newall*/
                foreach (Match match in Regex.Matches(res, @"<a href="".{10,60}tower/id/\d+""><span>(.+?)</span></a>.+?<a class=""tdn link"" href="".{10,60}mail/read/id/\d+""><span>(.+?)</span>", RegexOptions.Multiline | RegexOptions.Singleline))
                {
                    txt += match.Groups[1].Value + ": " + match.Groups[2].Value + "\n";
                }
            
            return txt;
        }

        public static async Task<string> ReadMail(HttpClient client, System.Threading.CancellationToken cancellationToken = default)
        {
            //HelpMethod.StatusLog("Проверяем почту", BotID, Form);
            string res = await HelpMethod.Get("/mail", client, cancellationToken);
            string txt = "";

            /*TextWriter tw2 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "msg_.html");
            tw2.Write(res);
            tw2.Close();*/
            res = new Regex("<div class=\"m5\">(.*?)<div class=\"pgn\">", RegexOptions.Multiline | RegexOptions.Singleline).Match(res).Groups[1].Value;
            /*TextWriter tw2 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "msg_.html");
            tw2.Write(res);
            tw2.Close();*/
            foreach (Match match in Regex.Matches(res, @"<div>\n<img.*?letter(.*?)\.png.*?<span class=""(user|admin).*?[<span>]?(.+?)</span></a>.+?<a class="".*?"" href=""(mail/read/id/\d+)""><span>(.+?)</span></a>", RegexOptions.Multiline | RegexOptions.Singleline))
                {
                    txt += match.Groups[1].Value + "###" + Regex.Replace(match.Groups[3].Value.Substring(2), @"<(.|\n)*?>", "") + "###" + match.Groups[4].Value + "###" + match.Groups[5].Value + "#\n";
                }
            
            return txt;
        }
        public static async Task<string> FreeGorZad(int BotID, MainFormAll Form, HttpClient client)
        {
            HelpMethod.StatusLog("Смотрим свободные гор.задания", BotID, Form);
            string res = await HelpMethod.Get("city/quests",client);
            string txt = "";
            foreach (Match match in Regex.Matches(res, @"<strong class=""sale"">(.{1,30})</strong>.{10,800}<a class=.{1,30} href=""(.+?)"">(.{1,30}?)</a>", RegexOptions.Multiline | RegexOptions.Singleline))
            {
                txt += match.Groups[1].Value + ", " + match.Groups[2].Value + ", " + match.Groups[3].Value + "\n";
            }
                return txt;
        }
        public static async Task GetCityQuest(int BotID, MainFormAll Form, HttpClient client, string settingsQuest, string tipCityQuest, string tipGorZad,int ot)
        {
            HelpMethod.StatusLog("Проверяем наличие гор.заданий", BotID, Form);
            string freeQuest = await FreeGorZad(BotID, Form, client);
            string[] quest = settingsQuest.Split(',');
            foreach (var words in quest)
            {
                string word=words.Trim();
                string url = "";// new Regex($"{word}, (.*?), (.*?)\n").Match(freeQuest).Groups[1].Value;
                switch (tipCityQuest)
                {
                    case "Брать задания города": url = new Regex($"{word}, (.*?), (Выполнить)\n").Match(freeQuest).Groups[1].Value; break;
                    case "Помогать с заданиями": url = new Regex($"{word}, (.*?), (помочь)\n").Match(freeQuest).Groups[1].Value; break;
                    case "И брать и помогать": url = new Regex($"{word}, (.*?), (.*?)\n").Match(freeQuest).Groups[1].Value; break;
                }
                if (url != "")
                {
                    bool change = tipGorZad == "за 75";
                    bool slow = tipGorZad == "за 2 ";
                    int kol;

                    bool check = await CheckCityQuest(client, word);
                    if (!check) { HelpMethod.StatusLog("Невозможно выполнить гор.задание " + word, BotID, Form); continue; }
                    await HelpMethod.Get(url, client); HelpMethod.Log("Взяли гор.задание: " + word, BotID, Form);
                    if (word.Equals("Все выше и выше!")) { await Build(BotID, Form, client); await CityQuests(BotID, Form, client, true); return; }
                    if (word.Equals("Индиана Джонс")) { await Open10(BotID, Form, client, "1"); await CityQuests(BotID, Form, client, true); return; }
                   
                    if (word.Equals("Легкие деньги")) {
                        string nach = await GetNoBaks(client);
                        string kon = "0";
                        while(HelpMethod.ToInt(kon)- HelpMethod.ToInt(nach)<30) {await GoLift15(BotID, Form, client, ot); kon = await GetNoBaks(client); }
                        await CityQuests(BotID, Form, client, true);return;}
                    if (word.Equals("Оптовые закупки") && !slow)
                    {
                        if (await CheckManager(client)) //если есть менеджер
                        {
                            kol = 6;
                            //выполняем развоз за 15(75), 6 раз (153 бакса если за 75 6-й раз, иначе 90 баксов)
                            await GoLift15(BotID, Form, client,ot, kol, change);
                        }
                    }
                    if (word.Equals("Товар на витрине") && !slow)
                    {
                        if (await CheckManager(client)) //если есть менеджер
                        {
                            kol = 10;
                            //выполняем развоз за 15(75), 10 раз (450 если за 75 и 150 если за 15)
                            await GoLift15(BotID, Form, client,ot, kol, change);
                        }
                    }
                    if (word.Equals("Инкассатор") && !slow)
                    {
                        if (await CheckManager(client)) //если есть менеджер
                        {
                            kol = 20;
                            if (change) kol = 14;
                            //выполняем развоз за 15(75), 20(13) раз (750 если за 75 и 300 если за 15)
                            await GoLift15(BotID, Form, client,ot, kol, change);
                        }
                    }
                    if (word.Equals("VIP-перевозчик") && !slow)
                    {
                        kol = 5;
                            //выполняем развоз за 15 5 раз (75 баксов)
                            await GoLift15(BotID, Form, client,ot, kol);
                    }
                }
            }
        }
        public static async Task<bool> CheckManager(HttpClient client)
        {
            string res = await HelpMethod.Get("", client);
            if (res.Contains("/buff/8")) { return true; } else { return false; }
        }

        public static async Task<string> GetNoBaks(HttpClient client)
        {
            string res = await HelpMethod.Get("/lift", client);
            Match freebaks = new Regex(@"Сегодня получено чаевых.*?<span>(.*?)</span>.*?\sиз\s.*?<span>(.*?)</span>", RegexOptions.Multiline | RegexOptions.Singleline).Match(res);

            return freebaks.Groups[1].Value;
        }

        public static async Task<string> GetHumanSund(int BotID, MainFormAll Form, HttpClient client)
        {
            string txt = "";
            string res = await HelpMethod.Get("/city/box/quests", client);

            return txt;
        }

            public static async Task<bool> CheckCityQuest(HttpClient client, string quest)
        {
            string res = await HelpMethod.Get("/home", client);
            string baks = GetFreeBaks(res);
            string kol = GetFreeHuman(res);
            string floor = GetFloor(res);
            switch (quest)
            {
                case "Все выше и выше!":
                    string url = new Regex("(Построить за)").Match(res).Groups[1].Value;
                    if (url.Equals("")) { return false; }
                    break;
                case "Индиана Джонс":
                    url = new Regex("Осталось ключей:.*<span>(.*?)</span>").Match(res).Groups[1].Value;
                    if (HelpMethod.ToInt(url)<201) { return false; }
                    break;
                case "Легкие деньги":
                    res = await HelpMethod.Get("/lift", client); 
                    Match freebaks = new Regex(@"Сегодня получено чаевых.*?<span>(.*?)</span>.*?\sиз\s.*?<span>(.*?)</span>", RegexOptions.Multiline | RegexOptions.Singleline).Match(res);
                    if (HelpMethod.ToInt(freebaks.Groups[2].Value) - HelpMethod.ToInt(freebaks.Groups[1].Value) < 30) { return false; }
                    break;
                case "Новые жители":
                    if (HelpMethod.ToInt(baks) < 100) { return false; }
                    if (HelpMethod.ToInt(kol) < 50) { return false; }
                    break;
                case "Давайдосвидания!":
                    if (HelpMethod.ToInt(kol) < 50) { return false; }
                    if (HelpMethod.ToInt(baks) < 100) { return false; }
                    break;
                case "Перевозчик":
                    if (HelpMethod.ToInt(baks) < 50) { return false; }
                    break;
                case "VIP-перевозчик":
                    if (HelpMethod.ToInt(baks) < 100) { return false; }
                    break;
                case "Оптовые закупки":
                    if (HelpMethod.ToInt(baks) < 153) { return false; }
                    if ((HelpMethod.ToInt(baks)/HelpMethod.ToInt(floor)) < 3) { return false; }
                    break;
                case "Товар на витрине":
                    if (HelpMethod.ToInt(baks) < 155) { return false; }
                    if ((HelpMethod.ToInt(baks) / HelpMethod.ToInt(floor)) < 4) { return false; }
                    break;
                case "Инкассатор":
                    if (HelpMethod.ToInt(baks) < 310) { return false; }
                    if ((HelpMethod.ToInt(baks) / HelpMethod.ToInt(floor)) < 6) { return false; }
                    break;

            }
            return true;
        }

        public static string GetFreeHuman(string res)
        {
            return new Regex("1. Гостиница.*?<span>(.*?)</span>.*?<span>(.*?)</span>", RegexOptions.Multiline | RegexOptions.Singleline).Match(res).Groups[2].Value;
        }
        public static string GetFreeBaks(string res)
        {
            res = res.Replace("&#039;", "");
            return new Regex(@"<img src=""/images/icons/mn_gold\.png"" width=""16"" height=""16"" alt=""\$""/><span>([0-9]+)</span>").Match(res).Groups[1].Value;
        }
        public static string GetFloor(string res)
        {
            res = res.Replace("&#039;", "");
            //string level= new Regex(@"<img src=""/images/icons/mn_star\.png"" alt=""у"" border=""0""/><span>([0-9]+)</span>").Match(res).Groups[1].Value;
            string floor= new Regex(@"div class=""tower"">.*?<span class="""">(\d+)\.\s", RegexOptions.Multiline | RegexOptions.Singleline).Match(res).Groups[1].Value;
            if (HelpMethod.ToInt(floor) == 0) { floor = "1"; }
            return floor;
        }


        /// <summary>
        /// Запускает задачу ожидания.
        /// </summary>
        /// <param name="BotID">Индентификатор бота (вкладки).</param>
        /// <param name="Button">Ссылка на экземпляр класса <see cref="Button"/>.</param>
        /// <param name="Interval">Интервал ожидания, в секундах.</param>
        /// <param name="Form">Ссылка на экземпляр класса <see cref="MainFormAll"/>.</param>
        public static async Task Sleep(int BotID, System.Windows.Forms.Button Button, MainFormAll Form, int Interval = 60)
        {
            
            // Инициализируем таймер ожидания
            DateTime taskStop = DateTime.Now.AddSeconds(Interval);

            // Запускам цикл ожидания
            while (true)
            {
                 // Получаем текущие время
                DateTime now = DateTime.Now;
 
                if (now.CompareTo(taskStop)>0 || Button.Text.Contains(MainFormAll.BUTTON_TEXT_START))
                {
                    break;
                }

                // Обновляем лог
                HelpMethod.StatusLog($"Повтор через {taskStop.Subtract(now):mm} мин : {taskStop.Subtract(now):ss} сек", BotID, Form, Resources.hd_nebo);

                // Задержка
                await Task.Delay(1000);
            }
        }

    }
}
