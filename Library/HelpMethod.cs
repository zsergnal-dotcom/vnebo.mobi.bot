using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace vnebo.mobi.bot.Libs
{
    internal class HelpMethod
    {
        #region DLL IMPORT
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        #endregion

        /// <summary>
        /// Инициализированная переменная класса <see cref="Random"/>.
        /// </summary>
        public static readonly Random getRandomNumber = new Random();

        public static HttpClient HttpManager(string login, HttpClientHandler hand)
        {
           
            HttpClient client = new HttpClient(hand)
            {

            };
            //client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/7.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; .NET4.0C; .NET4.0E; Tablet PC 2.0)");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64)");
            client.DefaultRequestHeaders.Connection.ParseAdd("Keep-Alive");
            
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("ru-RU");
            string fileCooc = Path.GetTempPath() + login + ".dat";
            if (File.Exists(@fileCooc))
            {
                // LOAD
                try
                {
                    FileStream inStr = new FileStream(@fileCooc, FileMode.Open);                
                    BinaryFormatter bf = new BinaryFormatter();
                    try
                    {
                         hand.CookieContainer = bf.Deserialize(inStr) as CookieContainer;
                        
                    } catch(Exception ex) { Console.WriteLine("ex des="+ex); }
                    inStr.Close();
                }
                catch (Exception ex) { Console.WriteLine("err open cooc" + ex); }
            }
            
            return client;

        }

        public static HttpClient HttpManager()
        {
            string login = "tmp";
            string fileCooc = Path.GetTempPath() + login + ".dat";
            try
            {
                File.Delete(fileCooc);
            }
            catch (Exception) { }
            HttpClientHandler hand = new HttpClientHandler()
            {

                AllowAutoRedirect = true,
                UseCookies = true

            };
            
            return HttpManager(login, hand);

        }
        public static HttpClient HttpManager(string login)
        {
            
            string fileCooc = Path.GetTempPath() + login + ".dat";
            try
            {
                File.Delete(fileCooc);
            }
            catch (Exception) { }
            HttpClientHandler hand = new HttpClientHandler()
            {

                AllowAutoRedirect = true,
                UseCookies = true

            };

            return HttpManager(login, hand);

        }
        public static void SaveCooc(string login, HttpClientHandler hand)
        {
            string fileCooc = Path.GetTempPath() + login + ".dat";
            // SAVE client Cookies
            FileStream stream = new FileStream(@fileCooc, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, hand.CookieContainer);
            stream.Close();

        }
        internal static string GetEndURI(Uri requestedUri,HttpClient client)
        {
            try
            {
                
                    client.Timeout = new TimeSpan(0, 0, 5);
                    using (var response = client.SendAsync(new HttpRequestMessage(HttpMethod.Head, requestedUri)))
                    {
                        var result = response.Result;
                        return result.RequestMessage.RequestUri.ToString();
                    }
               
            }
            catch (Exception)
            {
                // Something went wrong
                return "";
            }
        }
        public static async Task<string> Get(string url, HttpClient client)
        {
            //Console.WriteLine("token=" + MainForm.source.Token.IsCancellationRequested);
            //if (MainForm.source.Token.IsCancellationRequested) { return ""; }
            
            string result;
            int kol = 0;
            if (url.StartsWith("wicket")) { url = "?" + url; }
            url = HttpUtility.UrlDecode(url);
            try
            {
               
                    await RandomDelay(35, 100);
               
               
                do
                {
                    //var tmp_res= await client.GetAsync(url).Result;
                    //res.ResponseUri.AbsoluteUri; 
                    using (var tmp_res = client.GetAsync( new Uri(client.BaseAddress,url)))
                    {
                        var res = await tmp_res.Result.Content.ReadAsStringAsync();
                        //result =  res.Content.ReadAsStringAsync().ToString();

                        string baseUrl = tmp_res.Result.RequestMessage.RequestUri.ToString();
                        if (!string.IsNullOrEmpty(baseUrl))
                        {
                            baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf("/", StringComparison.Ordinal));
                            baseUrl = HttpUtility.UrlDecode(baseUrl);
                            //result = result.Replace("../../", client.BaseAddress.ToString());
                            res = res.Replace("\"./", "\"" + baseUrl + "/");
                        }


                        result =  res+ "<div id=url name="+url+"></div>";
                    }
                    
                    if (result.Contains("Слишком быстро")) { kol++; await RandomDelay(100*kol, 400*kol);
                        Console.WriteLine("ОСН быстро " + url + " kol = " + kol);
                    }
                }
                while (result.Contains("Слишком быстро"));

                
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("error get, url = "+url+" request ex = "+ex.InnerException.Message);
                await RandomDelay(100, 200);
                return Get(url, client).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("exep get, url = " + url + " ex = " + ex.Message+" "+ex.StackTrace);
                await RandomDelay(100, 200);
                return "exep get, url = " + url + " ex = " + ex.Message + " " + ex.StackTrace+" "+ex.InnerException.Message+" "+ex.ToString();// Get(url,client).ToString();
            }
            return result;
        }
        public static async Task<string> Get2(string url, HttpClient client, int ot)
        {
            //Console.WriteLine("token=" + MainForm.source.Token.IsCancellationRequested);
            //if (MainForm.source.Token.IsCancellationRequested) { return ""; }

            string result;
            int kol = 0;
            if (url.StartsWith("wicket")) { url = "?" + url; }
            url = url.Replace("&amp;", "&");
            try
            {

                await RandomDelay(ot, ot+10);

                do
                {
                    result = await client.GetAsync(url).Result.Content.ReadAsStringAsync();
                    if (result.Contains("Слишком быстро")) { kol++; await RandomDelay(ot+kol*10, ot + kol * 10+10);
                        result = await client.GetAsync(url).Result.Content.ReadAsStringAsync();
                        Console.WriteLine("слишком быстро " + url + " kol = " + kol);
                    }
                    
                }
                while (result.Contains("Слишком быстро"));

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("error get, url = " + url + " request ex = " + ex.InnerException.Message);
                await RandomDelay(100, 200);
                return Get2(url, client,ot).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("exep get, url = " + url + " ex = " + ex.Message + " " + ex.StackTrace);
                await RandomDelay(100, 200);
                return "exep get, url = " + url + " ex = " + ex.Message + " " + ex.StackTrace + " " + ex.InnerException.Message + " " + ex.ToString();// Get(url,client).ToString();
            }
            return result + "kol="+kol.ToString();
        }

        // POST
        public static async Task<string> Post(string url, Dictionary<string, string> data, HttpClient client)
        {
            string result;
            HttpContent content = new FormUrlEncodedContent(data);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            // result= await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result.ToString;
            result = await response.Content.ReadAsStringAsync();
            return result;
        }
    
  

        /// <summary>
        /// Создаёт задачу которая, будет выполнена после случайной задержки.
        /// </summary>
        /// <param name="Minimum">Минимальное число задержки.</param>
        /// <param name="Maximum">Максимальное число задержки.</param>
        /// <returns>Задача, представляющая случайную временную задержку.</returns>
        public static async Task RandomDelay(int Minimum, int Maximum)
        {
            await Task.Delay(getRandomNumber.Next(Minimum, Maximum + 1));
        }

   
        /// <summary>
        /// Устанавливает placeholder для текстовых полей.
        /// </summary>
        /// <param name="TextBox">Ссылка на экземпляр класса <see cref="TextBox"/>.</param>
        /// <param name="PlaceholderText">Текст placeholder.</param>
        public static void SetPlaceholder(TextBox TextBox, string PlaceholderText)
        {
            SendMessage(TextBox.Handle, 0x1500 + 1, 0, PlaceholderText);
        }

        /// <summary>
        /// Устанавливает размер последней вкладки <see cref="TabControl"/> минимального размера.
        /// </summary>
        /// <param name="TabControl">Ссылка на экземпляр <see cref="TabControl"/>.</param>
        public static void TabControlSmallWidth(TabControl TabControl)
        {
            TabControl.HandleCreated += (s, e) =>
            {
                _ = SendMessage(TabControl.Handle, 0x1300 + 49, IntPtr.Zero, (IntPtr)10);
            };
        }

        /// <summary>
        /// Добавляет приложение в автозагрузку Windows.
        /// </summary>
        /// <param name="Flag">True - Добавляет, False - Убирает</param>
        public static void AutoRun(bool Flag)
        {
            // Полный путь к файлу
            string fileFullPath = Application.ExecutablePath;
            string nameDir = new DirectoryInfo(Application.StartupPath).Name;
            // Получаем информацию об файле
            FileInfo fileInfo = new FileInfo(fileFullPath);
            // Получаем имя файла
            string fileName = fileInfo.Name.Replace(".exe", "");
            // Открываем ветку реестра
            RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");

            try
            {
                if (Flag)
                {
                    registryKey.SetValue(nameDir+"_"+fileName, fileFullPath);
                }
                else
                {
                    registryKey.DeleteValue(fileName);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Произошла ошибка, автозапуск невозможен.");
            }

            // Закрываем ветку реестра
            registryKey.Close();
        }
        public static async Task<string> ClickGreenButton(string res, string name, HttpClient client, string url="")
        {
            string ret = "";
            DateTime now = DateTime.UtcNow.AddHours(3);
            if (url.Length > 0)
            {
                string[] parts = url.Split('/'); // или s.Split() - роль разделителей будут играть любые пробельные символы
                url=url.Replace(parts.Last(), "");
            }
            
            foreach (Match match in Regex.Matches(res, "<a class=\".{0,40}btng.{0,40}\" href=\"(.*?)\">(.*?)</a>"))
            {

                if (match.Groups[1].Value.Length > 0&& (name.Contains(match.Groups[2].Value)|| match.Groups[2].Value.Contains(name)))
                {
                    if (res.Contains("Сегодня выполнено заданий: 7 из 7")&&now.DayOfWeek==DayOfWeek.Sunday) { break; }
                    res = await Get(match.Groups[1].Value, client);
                    if (res.Contains("Переговоры успешны"))
                    {
                        return "Ваша награда за инвесторов:\n" + GetNagrada(res);
                    }

                    if (res.Contains("Задание выполнено!")|| match.Groups[2].Value== "Получить награду!")
                    {
                        return "Ваша награда:\n" + GetNagrada(res);
                    }

                    /*if (help != "hepl_inv"&&name== "Получить награду")
                    {
                        TextWriter tw2 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "green_but_nagrada_"+help+ ".html");
                        tw2.Write(match.Groups[1].Value+" = "+ match.Groups[2].Value + res);
                        tw2.Close();
                    }*/

                    ret = match.Groups[2].Value;
                } 
            }
            return ret;
          
        }

        public static string GetNagrada(string result)
        {

            string blok = new Regex("Награда.+?<div class=\"amount\">(.+?)</div>", RegexOptions.Singleline | RegexOptions.Multiline).Match(result).Groups[1].Value;
            blok = blok.Replace("&#039;", "");

            blok = Regex.Replace(blok, "<img src=\"/images/icons/mn_gold.png\".*?>", "Баксов:");
            blok = Regex.Replace(blok, "<img src=\"/images/icons/st_sold.png\".*?>", "Монет:");
            blok = Regex.Replace(blok, "<img src=\"/images/icons/key.png\".*?>", "Ключей:");
            blok = Regex.Replace(blok, "<img src=\"/images/icons/star_f.png\".*?>", "Звёзд:");
            blok = Regex.Replace(blok, "<img src=\"/images/icons/star.png\".*?>", "Опыта:");

            blok = Regex.Replace(blok, @"<(.|\n)*?>", " ");
            blok = Regex.Replace(blok, @"\r?\n", " ");
            blok = Regex.Replace(blok, @"\s+", " ");
            blok = Regex.Replace(blok, @"(\d+)", "$1\n");
            return blok;
        }

        public static string GetNagrada10door(string result)
        {

            string blok = new Regex("Награда.+?<span class=\"amount\">(.+?)<div", RegexOptions.Singleline | RegexOptions.Multiline).Match(result).Groups[1].Value;
            blok = blok.Replace("&#039;", "");

            blok = blok.Replace("<img src=\"/images/icons/mn_iron.png\" width=\"16\" height=\"16\" alt=\"o\"/>", "Монет:");
            blok = blok.Replace("<img src=\"/images/icons/star.png\" alt=\"e\" width=\"16\" height=\"16\">", "Опыта:");

            blok = Regex.Replace(blok, @"<(.|\n)*?>", " ");
            blok = Regex.Replace(blok, @"\r?\n", " ");
            blok = Regex.Replace(blok, @"\s+", " ");
            blok = Regex.Replace(blok, @"(\d+)", "$1\n");
            return blok;
        }
        /// <summary>
        /// Форматирует цифровую строку в красивый вид.
        /// </summary>
        /// <param name="Number">Число строкой.</param>
        /// <param name="Format_type">Укорачивает цифровую строку, true - 1.11k, false - 100,000</param>
        /// <returns></returns>
        public static string StringNumberFormat(string Number, bool Format_type = true)
        {
            if (Number.Length > 0)
            {
                // Если нужно укорачивать строку
                if (Format_type)
                {
                    // Создаём временные переменные
                    string number_text;
                    double number_double = Convert.ToDouble(Number);

                    // Если число меньше 1 000, то просто возвращаем.
                    if (number_double < 1000)
                    {
                        number_text = Number.ToString();
                    }
                    // Если число меньше 1 000 000 (тысячи), то возвращаем в конце букву "k"
                    else if (number_double < 1000000d)
                    {
                        number_text = (number_double / 1000d).ToString("#.##k");
                    }
                    // Если число меньше 1 000 000 000 (миллионы), то возвращаем в конце букву "m"
                    else if (number_double < 1000000000d)
                    {
                        number_text = (number_double / 1000000d).ToString("#.##m");
                    }
                    // Если число меньше 1 000 000 000 000 (миллиарды), то возвращаем в конце букву "g"
                    else if (number_double < 1000000000000d)
                    {
                        number_text = (number_double / 1000000000d).ToString("#.##g");
                    }
                    // Если число меньше 1 000 000 000 000 000 (триллионы), то возвращаем в конце букву "t"
                    else if (number_double < 1000000000000000d)
                    {
                        number_text = (number_double / 1000000000000d).ToString("#.##t");
                    }
                    else
                    {
                        number_text = (number_double / 1000000000000000d).ToString("#.##p");
                    }

                    return number_text.Trim();
                }

                return Convert.ToDouble(Number).ToString("#,##0", new CultureInfo("en-US")).Replace(" ", "");
            }

            return "0";
        }

        /// <summary>
        /// Метод, который конвертирует строку в логическое выражение.
        /// </summary>
        /// <param name="boolean">Строка логического типа.</param>
        /// <returns>Если строка ровна <see cref="true"/> (регистр неважен), вернется <see cref="true"/>, в остальных случаев вернется <see cref="false"/>.</returns>
        public static bool ToBoolean(Dictionary<String,String> account_settings, string boolean)
        {

            if (account_settings.ContainsKey(boolean)) { return account_settings[boolean].ToLower().Equals("true"); }
            else
            {
                return false;
            }
        }
   
        public static int ToInt(string str)
        {
            try
            {
                if (Int32.TryParse(str, out int intStr))
                {
                    return intStr;
                }
                else { return 0; }
            }catch(Exception ex) { Console.WriteLine("err to int "+ex); return 0; }
        }

        /// <summary>
        /// Отправляет строку в <see cref="RichTextBox"/> с поддержкой цвета и скрытие времени.
        /// </summary>
        /// <param name="Text">Строка.</param>
        /// <param name="BotID">Идентификатор бота (вкладки).</param>
        /// <param name="Form">Ссылка на <see cref="Auth"/>.</param>
        /// <param name="Color">Цвет текста.</param>
        /// <param name="ShowTime">True - Показывать время, False - Не показывать время.</param>
        public static void Log(string Text, int BotID, MainFormAll Form, Color Color = new Color(), bool ShowTime = true)
        {
            Form.Invoke((MethodInvoker)delegate
            {
                RichTextBox logs = FindControl.FindRichTextBox("richtextbox_log", BotID, Form);
                logs.SelectionColor = Color;
                logs.Text= $" {(Text.Length > 0 ? "--" : "")} {Text} {Environment.NewLine}"+logs.Text;
                logs.SelectionColor = SystemColors.ControlDarkDark;
                logs.Text = $" {(ShowTime ? $"[ { DateTime.Now:dd.MM.yyyy HH:mm:ss} ]" : "")}" + logs.Text;
                /*TextBox login = FindControl.FindTextBox("textbox_login", BotID, Form);
                string read_file = "";
                string lofFile = Path.GetTempPath() + $"{login.Text}.log";
                if (File.Exists(lofFile))
                {
                    read_file = File.ReadAllText(lofFile);
                }
                TextWriter tw = new StreamWriter(lofFile);
                tw.Write($" {(ShowTime ? $"[ { DateTime.Now:dd.MM.yyyy HH:mm:ss} ]" : "")}" + $" {(Text.Length > 0 ? "--" : "")} {Text}"+ Environment.NewLine + read_file);
                tw.Close();*/
                //logs.ScrollToCaret();
            });
        }

        /// <summary>
        /// Отправляет строку в <see cref="ToolStrip"/> с поддержкой <see cref="Image"/>.
        /// </summary>
        /// <param name="Text">Строка.</param>
        /// <param name="BotID">Индентификатор бота (вкладки).</param>
        /// <param name="Form1">Ссылка на <see cref="Auth"/>.</param>
        /// <param name="Image">Картинка.</param>
        public static void StatusLog(string Text, int BotID, MainFormAll Form1, Image Image = null)
        {
            //
            //
            // ПЕРЕДЕЛАТЬ ДАННЫЙ МЕТОД!!!!
            // ПЕРЕДЕЛАТЬ ДАННЫЙ МЕТОД!!!!
            //
            //
            Form1.Invoke((MethodInvoker)delegate
            {
                FindControl.FindToolStrip("toolstrip_info_top", BotID, Form1).Items[0].Text = "User_"+BotID.ToString()+" "+ Text;
                FindControl.FindToolStrip("toolstrip_info_top", BotID, Form1).Items[0].Image = Image;
            });
        }
    }
}
