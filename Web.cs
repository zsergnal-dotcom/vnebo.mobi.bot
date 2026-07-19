using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using vnebo.mobi.bot.Libs;

namespace vnebo.mobi.bot
{
    public partial class Web : Form
    {
        
        public static HttpClientHandler hand = new HttpClientHandler()
        {
            AllowAutoRedirect = true,
            UseCookies = true
        };
        public static HttpClient client = HelpMethod.HttpManager("tmp", hand);
        public Web(Dictionary<string, string> account_settings, string Auth = "")
        {

            InitializeComponent();
              //HttpClient client = HelpMethod.HttpManager(account_settings["LOGIN"], hand);
        client = HelpMethod.HttpManager(account_settings["LOGIN"], hand);
            client.BaseAddress = new Uri("https://" + account_settings["SERVER"]);
            
            Task.Run(async () =>
            {
                string AuthorizationResult = "";

                if (Auth=="")
                {
                    AuthorizationResult = await HelpMethod.Get("/home", client);
                    if (!AuthorizationResult.Contains("Мой профиль"))
                    {

                        AuthorizationResult = await BotEngine.Authorization(account_settings["LOGIN"], account_settings["PASSWORD"], client, account_settings["SERVER"]);
                    }

                    if (!AuthorizationResult.Contains("Мой профиль"))
                    {
                        return;
                    }
                    string url = new System.Text.RegularExpressions.Regex("href=\"(.*?collapseTowerLink.*?)\"><img").Match(AuthorizationResult).Groups[1].Value;
                    if (url.Length > 0)
                    {
                        AuthorizationResult = await HelpMethod.Get(url, client);
                    }
                    AuthorizationResult = AuthorizationResult.Replace("=\"/", "=\"" + client.BaseAddress.ToString() + "/");
                    Invoke((MethodInvoker)delegate
                    {
                        WebB.DocumentText = UpdResult(AuthorizationResult);
                    });
                }
                else { 
                
                    Invoke((MethodInvoker)delegate
                    {
                        WebB.DocumentText = Auth;
                    });

                }
               
            });   
            
        }
        void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {

            string url = e.Url.ToString();
            if (url == "about:blank") return;

            url = url.Replace("about:blank", client.BaseAddress.ToString());
            url = url.Replace("about:", client.BaseAddress.ToString());
            
            
            e.Cancel = true;
            WebBrowser WebB = (WebBrowser)sender;
            Task.Run(async () =>
            {
                string result = UpdResult(await HelpMethod.Get(url, client));
                //"<div id=url name="+url+"></div>"
                string baseUrl =  new Regex("<div id=url name=(.*?)></div>").Match(result).Groups[1].Value;
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf("/", StringComparison.Ordinal));
                    baseUrl = HttpUtility.UrlDecode(baseUrl);
                    //debug tek str
                    //result = result.Replace("../../", client.BaseAddress.ToString());
                    result = result.Replace("\"./", "\"" + baseUrl + "/");
                }
                Invoke((MethodInvoker)delegate
                {
                    WebB.DocumentText = result;
                });
            });
            

        }
 
        private string UpdResult(string result)
        {
            result = result.Replace("=\"/", "=\"" + client.BaseAddress.ToString() + "/");
            return result;
        }

        private void Web_FormClosing(object sender, FormClosingEventArgs e)
        {
            hand = new HttpClientHandler()
            {
                AllowAutoRedirect = true,
                UseCookies = true
            };
          client = HelpMethod.HttpManager("tmp", hand);
    }
    }
}

