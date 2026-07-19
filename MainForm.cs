using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using vnebo.mobi.bot.Libs;
using vnebo.mobi.bot.Properties;

namespace vnebo.mobi.bot
{
    public partial class MainFormAll : Form
    {

        /// <summary>
        /// Количество вкладкок (а так же идентификатор вкладки и компонентов).
        /// </summary>
        private int AccountCount = 0;

        private readonly List<int> arrUser = new List<int>();
        private readonly Dictionary<string, string> arrBots = new Dictionary<string, string>();
        /// <summary>
        /// Количество аккаунтов, нужна для подсчета сколько аккаунтов на данный момент.
        /// </summary>
        private int Account = 0;

        /// <summary>
        /// Максимальное число доступное для создание аккаунтов.
        /// </summary>
        private readonly int maxAccount = 50;
        //private int 
        /// <summary>
        /// Версия приложения.
        /// </summary>
        private readonly string v = "v2.1.5";
        private readonly string d = "19.07.2026";
        /// <summary>
        /// Информация о сайте обновления/файла бота
        /// </summary>

        public static readonly string domen = "http://31.41.63.106/";
        //public static readonly string name_file = "vnebo.mobi";
        /// <summary>
        /// Текст кнопки "ЗАПУСТИТЬ БОТА".
        /// </summary>
        public static string BUTTON_TEXT_START = "ЗАПУСТИТЬ БОТА";

        /// <summary>
        /// Текст кнопки "ОСТАНОВИТЬ БОТА".
        /// </summary>
        public static string BUTTON_TEXT_STOP = "ОСТАНОВИТЬ БОТА";

        /// <summary>
        /// Стандартный аватар.
        /// </summary>
        private static readonly string AVATAR_DEFAULT = "man_no";

        private static readonly string[] AwayMenuItems =
        {
            "Выселить 15", "Выселить 30", "Выселить 45", "Выселить 60",
            "Всех", "Зелёных", "Голубых", "Жёлтых", "Фиолетовых", "Оранжевых",
            "Всех кроме Зелёных", "Всех кроме Голубых", "Всех кроме Жёлтых",
            "Всех кроме Фиолетовых", "Всех кроме Оранжевых"
        };

        private static readonly (string Text, string Tag)[] DopMenuItems =
        {
            ("Открыть 10-ю дверь", "open10"),
            ("Отменить коллекцию", "cancelColl"),
            ("Отменить VIPa", "cancelVIP"),
            ("Позвать в лифт", "lift"),
            ("Позвать в лифт за 15", "lift15"),
            ("Построить этаж", "floor"),
            ("Показывать на странице Без города", "vis"),
            ("Не показывать на странице Без города", "novis")
        };
        /// <summary>
        /// Глобальный класс настроек, запись, чтение и т.д
        /// </summary>
        private static readonly IniFiles settings = new IniFiles();
        public static readonly Dictionary<string, bool> Start = new Dictionary<string, bool>();
        // Cancellation token sources per bot to allow cancelling tasks
        private readonly Dictionary<string, System.Threading.CancellationTokenSource> ctsPerBot = new Dictionary<string, System.Threading.CancellationTokenSource>();

        //public static CancellationTokenSource source= new CancellationTokenSource();

        //public CancellationToken token = source.Token;*/
        
        public MainFormAll()
        {
            InitializeComponent();
            //TabDragger DragTabs = new TabDragger(tabControl1, TabDragBehavior.TabDragArrange);
            // Загружаем из ресурсов аватары в ImageList
            foreach (DictionaryEntry entry in Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true))
            {
                string key = (string)entry.Key;

                if (key.Contains("player"))
                {
                    imageList1.Images.Add(key, (System.Drawing.Image)entry.Value);
                }

                if (key == "man_no")
                {
                    imageList1.Images.Add(key, (System.Drawing.Image)entry.Value);
                }
                if (key == "letters")
                {
                    imageList1.Images.Add(key, (System.Drawing.Image)entry.Value);
                }
            }

            // Изменяем максимальный размер формы
            //MaximumSize = new Size(Width, Height);
            //panel1.Size = new Size(1114,556);
            // Хак на уменьшения размера последней вкладки
            HelpMethod.TabControlSmallWidth(tabControl1);
            //tabControl1.Size = new Size(1114, 517);
            string nameDir = new DirectoryInfo(Application.StartupPath).Name;
            // Заголовок окна
            Text = $"Бот для мобильной браузерной игры \"Небоскребы\" в папке {nameDir}";
            MinimumSize = new Size(930,470);
            // Заголовок в меню "О программе"
            toolStripMenuItem10.Text = $"{v} ({d})";
            toolStripMenuItemUpd.Text = "Обновить программу";
            toolStripMenuItemUpd.Click += (s, e) =>
            {
                CheckUpd();
            };
            // Заголовок иконки в трее
          //  notifyIcon1.Text = Text;

        }


        private void CreateTemplate(TabPage tabPage)
        {
            ToolStrip toolstrip_info_top = new ToolStrip
            {
                AutoSize = true,
                BackColor = Color.White,
                Dock = DockStyle.None,
                Font = new Font("Segoe UI", 9F),
                GripStyle = ToolStripGripStyle.Hidden,
                Location = new Point(6, 3),
                RenderMode = ToolStripRenderMode.System,
                Size = new Size(612, 25),
                Name = $"toolstrip_info_top{AccountCount}"//,
                //Anchor = (AnchorStyles.Top |  AnchorStyles.Left)
            };

            ToolStripLabel toolstriplabel_status_log = new ToolStripLabel
            {
                Size = new Size(115, 22),
                Text = "",
                Name = $"toolstriplabel_status_log{AccountCount}"
            };


            GroupBox groupbox1 = new GroupBox
            {
                Location = new Point(6, 24),
                Size = new Size(211, 166),
                TabStop = false
            };

            GroupBox groupbox2 = new GroupBox
            {
                Location = new Point(7, 74),
                Size = new Size(198, 82),
                Text = "Интервал повторов ( мин )",
                TabStop = false
            };

            GroupBox groupbox3 = new GroupBox
            {
                Location = new Point(8, 196),
                Size = new Size(198, 63),
                Text = "",
                TabStop = false
            };

            System.Windows.Forms.ComboBox comboBox = new System.Windows.Forms.ComboBox
            {
                Location = new Point(7, 22),
                Size = new Size(185, 21),
                Text = "",
                Tag = $"{AccountCount}",
                Name = $"server{AccountCount}",
                Items = {
                "nebo.mobi","vnebo.mobi","happytower.mobi","odkl.vnebo.mobi","mm.vnebo.mobi","fs.vnebo.mobi"
                }
            };


            System.Windows.Forms.TextBox textbox_login = new System.Windows.Forms.TextBox
            {
                Location = new Point(7, 17),
                Size = new Size(198, 22),
                TabStop = true,
                MaxLength = 30,
                Name = $"textbox_login{AccountCount}",
                Tag = AccountCount
            };

            System.Windows.Forms.TextBox textbox_password = new System.Windows.Forms.TextBox
            {
                Location = new Point(7, 46),
                Size = new Size(198, 22),
                PasswordChar = '*',
                TabStop = true,
                Name = $"textbox_password{AccountCount}",
                Tag = AccountCount
            };

            System.Windows.Forms.Label label1 = new System.Windows.Forms.Label
            {
                AutoSize = true,
                Location = new Point(8, 25),
                Size = new Size(21, 13),
                Text = "ОТ",
                TextAlign = ContentAlignment.MiddleLeft
            };

            System.Windows.Forms.Label label2 = new System.Windows.Forms.Label
            {
                AutoSize = true,
                Location = new Point(9, 54),
                Size = new Size(23, 13),
                Text = "ДО",
                TextAlign = ContentAlignment.MiddleLeft
            };

            NumericUpDown numericupdown_interval_from = new NumericUpDown
            {
                Location = new Point(38, 22),
                Maximum = new decimal(new int[] { 1000, 0, 0, 0 }),
                Minimum = new decimal(new int[] { 1, 0, 0, 0 }),
                Size = new Size(154, 22),
                Value = new decimal(new int[] { 1, 0, 0, 0 }),
                TabStop = true,
                Name = $"numericupdown_interval_from{AccountCount}",
                Tag = AccountCount
            };

            NumericUpDown numericupdown_interval_do = new NumericUpDown
            {
                Location = new Point(38, 51),
                Maximum = new decimal(new int[] { 1000, 0, 0, 0 }),
                Minimum = new decimal(new int[] { 1, 0, 0, 0 }),
                Size = new Size(154, 22),
                Value = new decimal(new int[] { 2, 0, 0, 0 }),
                TabStop = true,
                Name = $"numericupdown_interval_do{AccountCount}",
                Tag = AccountCount
            };

            System.Windows.Forms.Button button_start = new System.Windows.Forms.Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                //Anchor = AnchorStyles.Left ,
                Location = new Point(6, 260),
                Size = new Size(211, 34),
                Text = BUTTON_TEXT_START,
                UseVisualStyleBackColor = true,
                TabStop = true,
                Anchor = (AnchorStyles.Left | AnchorStyles.Bottom),
                Name = $"button_start{AccountCount}",
                Tag = AccountCount
            };

            System.Windows.Forms.Button button_web = new System.Windows.Forms.Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold),
                //Anchor = AnchorStyles.Left ,
                Location = new Point(285, 2),
                Size = new Size(200, 30),
                Text = "Зайти на страницу перса",
                UseVisualStyleBackColor = true,
                TabStop = true,
                //Anchor = (AnchorStyles.Left | AnchorStyles.Bottom),
                Name = $"button_web{AccountCount}",
                Tag = AccountCount
            };

            System.Windows.Forms.Button button_show_settings = new System.Windows.Forms.Button
            {
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                Location = new Point(6, 300),
                Size = new Size(211, 27),
                Text = "ОТКРЫТЬ НАСТРОЙКИ",
                UseVisualStyleBackColor = true,
                Anchor = (AnchorStyles.Left | AnchorStyles.Bottom),
                TabStop = true,
                Tag = AccountCount,
                Name = $"button_show_settings{AccountCount}"
            };

            System.Windows.Forms.Button button_away = new System.Windows.Forms.Button
            {

                Font = new Font("Times New Roman", 6F, FontStyle.Regular),
                Location = new Point(235, 127),
                Size = new Size(48, 19),
                Text = "Выселить",
                Tag = $"{AccountCount}",
                UseVisualStyleBackColor = true,
                TabStop = true,
                Name = $"away{AccountCount}"
            };
            System.Windows.Forms.Button button_getcity = new System.Windows.Forms.Button
            {
                //FlatStyle = System.Windows.Forms.FlatStyle.Popup,
                Font = new Font("Times New Roman", 6F, FontStyle.Regular),
                Location = new Point(430, 74),
                Text = "Попроситься в город",
                Tag = AccountCount,
                Size = new Size(95, 19),
                UseVisualStyleBackColor = true,
                TabStop = true,
                Name = $"getcity{AccountCount}"
                //this.getCity.Click += new System.EventHandler(this.getCity_Click);
            };

            System.Windows.Forms.Button button_getmail = new System.Windows.Forms.Button
            {
                
                Font = new Font("Times New Roman", 6F, FontStyle.Regular),
                Location = new Point(430, 130),
                Text = "Прочитать сообщения",
                Tag = AccountCount,
                Size = new Size(80, 20),
                UseVisualStyleBackColor = true,
                TabStop = true,
                Name = $"mail{AccountCount}"
                
                //this.getCity.Click += new System.EventHandler(this.getCity_Click);
            };

            System.Windows.Forms.Button button_awaycity = new System.Windows.Forms.Button
            {
                //FlatStyle = System.Windows.Forms.FlatStyle.Popup,
                Font = new Font("Times New Roman", 6F, FontStyle.Regular),
                Location = new Point(235, 74),
                Text = "Покинуть",
                Tag = AccountCount,
                Size = new Size(48, 19),
                UseVisualStyleBackColor = true,
                TabStop = true,
                Name = $"awaycity{AccountCount}"
                //this.getCity.Click += new System.EventHandler(this.getCity_Click);
            };

            System.Windows.Forms.Button button_dop = new System.Windows.Forms.Button
            {
                //FlatStyle = System.Windows.Forms.FlatStyle.Popup,
                Font = new Font("Times New Roman", 6F, FontStyle.Regular),
                Location = new Point(235, 42),
                Text = "Доп.действия",
                Tag = AccountCount,
                Size = new Size(48, 19),
                UseVisualStyleBackColor = true,
                TabStop = true,
                Name = $"dop{AccountCount}"
                //this.getCity.Click += new System.EventHandler(this.getCity_Click);
            };

            RichTextBox richtextbox_log = new RichTextBox
            {

                BackColor = SystemColors.ControlLightLight,
                Multiline = true,
                ScrollBars = RichTextBoxScrollBars.Both,
                WordWrap = false,
                Location = new Point(540, 32),
                ReadOnly = true,
                
               Size = new Size(460, 300),
                //Size Size.Width = new Size.Width - 10,
            TabStop = false,
                Name = $"richtextbox_log{AccountCount}",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            WebBrowser Info = new WebBrowser
            {
                BackColor = ColorTranslator.FromHtml("#036"),
                Location = new Point(233, 32),
                Size = new Size(305, 300),
                TabStop = false,
                
                Name = $"Info{AccountCount}",
                DocumentText= "<html><head><link rel=\"stylesheet\" type=\"text/css\" href=\"https://nebo.mobi/images/style.css\"></head><body><div align=center><h3><font color=Yellow>Для отображения статистики запустите бота или нажмите на кнопку \"Обновить\"</font></h3></div></body></html>",
                Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom)
            };
            System.Windows.Forms.Button refresh = new System.Windows.Forms.Button
            {
                Font = new Font("Times New Roman", 20F, FontStyle.Bold),
                Location = new Point(300, 133),
                Size = new Size(145, 40),
                Name= $"refresh{AccountCount}",
                Text="Обновить",
                Tag=AccountCount
            };

 
            refresh.Click += (s, e) =>
            {
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)s;
                int BotID = HelpMethod.ToInt(button.Tag.ToString());

                RunAuthorizedAction(BotID, async (client, accountSettings, authResult) =>
                {
                    string profile_url = new Regex("href=\".*?(tower/id/[0-9]*.?)\"><span>").Match(authResult).Groups[1].Value;
                    await BotEngine.Statistics(profile_url, BotID, this, client);
                }, logMailRuHintOnFailure: true);
            };
            button_web.Click += (s, e) =>
            {
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)s;
                int BotID = HelpMethod.ToInt(button.Tag.ToString());
                
                Dictionary<string, string> account_settings = settings.GetSett($"USER_{BotID}");
                Web WebForm = new Web(account_settings);
                    WebForm.ShowDialog();
            };

            // Добавляем элементы в верхний ToolStrip
            toolstrip_info_top.Items.AddRange(new ToolStripItem[]
            {
                toolstriplabel_status_log

            });


            // Добавляем элементы на GroupBox
            groupbox1.Controls.AddRange(new Control[]
            {
                textbox_login,
                textbox_password,
                groupbox2
            });

            // Добавляем элементы на GroupBox
            groupbox2.Controls.AddRange(new Control[]
            {
                label1,
                label2,
                numericupdown_interval_from,
                numericupdown_interval_do,
             });

            groupbox3.Controls.AddRange(new Control[]
                { comboBox});
            //groupbox4.Controls.AddRange(new Control[]
             //   { richtextbox_log});
            // Добавляем элементы на вкладку
            tabPage.Controls.AddRange(new Control[]
            {
                toolstrip_info_top,
                //toolstrip_info_bottom,
                groupbox1,
                groupbox3,
                button_web,
                button_start,
                button_show_settings,
                richtextbox_log,
                Info,
                button_away,
                button_getcity,
                button_awaycity,
                button_dop,
                button_getmail,
                refresh
            }) ;

            // Устанавливаем Placeholder
            HelpMethod.SetPlaceholder(textbox_login, "Ваш логин");
            HelpMethod.SetPlaceholder(textbox_password, "Ваш пароль");

            // Обработчики событий
            textbox_login.KeyUp += (s, e) =>
            {
                System.Windows.Forms.TextBox login = (System.Windows.Forms.TextBox)s;
                int botID = (int) login.Tag;
                settings.Write($"USER_{botID}", "LOGIN", (s as System.Windows.Forms.TextBox).Text);
                tabControl1.TabPages[tabControl1.SelectedIndex].Text = (s as System.Windows.Forms.TextBox).Text.Length > 0 ? $"{(s as System.Windows.Forms.TextBox).Text}" : "Новый персонаж";
            };

            textbox_password.KeyUp += (s, e) =>
            {
                System.Windows.Forms.TextBox pass = (System.Windows.Forms.TextBox)s;
                int botID = (int)pass.Tag;

                settings.Write($"USER_{botID}", "PASSWORD", (s as System.Windows.Forms.TextBox).Text);
                
            };

            numericupdown_interval_from.ValueChanged += (s, e) =>
            {
                NumericUpDown numericUpDown = (NumericUpDown)s; int botID = (int)numericUpDown.Tag;

                // Максимальное значения ОТ = ДО
                numericUpDown.Maximum = FindControl.FindNumericUpDown("numericupdown_interval_do", botID, this).Value;

                // Записываем значения в файл сохранений
                settings.Write($"USER_{botID}", "INTERVAL_FROM", numericUpDown.Value.ToString());
               
            };

            numericupdown_interval_do.ValueChanged += (s, e) =>
            {
                NumericUpDown numericUpDown = (NumericUpDown)s; int botID = (int)numericUpDown.Tag;

                // Максимальное значение ОТ = ДО
                FindControl.FindNumericUpDown("numericupdown_interval_from", botID, this).Maximum = numericUpDown.Value;
                // Записываем значения в файл сохранений
                settings.Write($"USER_{botID}", "INTERVAL_DO", numericUpDown.Value.ToString());
                
            };

            button_start.Click += (s, e) =>
            {
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)s;

                // Если текст кнопки равен "ОСТАНОВИТЬ БОТА"
                // То меняет на "ЗАПУСТИТЬ БОТА" и выходим из метода
                if (button.Text == BUTTON_TEXT_STOP)
                {
                    button.Text = BUTTON_TEXT_START;
                    string botID = button.Tag.ToString();
                    Start[botID] = false;
                    // Остановим задачу: если существует CTS в словаре, отменим
                    if (ctsPerBot.ContainsKey(botID))
                    {
                        try { ctsPerBot[botID].Cancel(); } catch { }
                        try { ctsPerBot[botID].Dispose(); } catch { }
                        ctsPerBot.Remove(botID);
                    }
                    return;
                }

                // ЗАПУСКАЕМ БОТА
                BOT_START(HelpMethod.ToInt(button.Tag.ToString()));

            };
            ContextMenuStrip contextMenuStrip1 = new ContextMenuStrip();
            button_away.Click += (s, e) =>
              {
                  System.Windows.Forms.Button button = (System.Windows.Forms.Button)s;
                  
                  contextMenuStrip1.Items.Clear();
                  foreach (string item in AwayMenuItems)
                  {
                      contextMenuStrip1.Items.Add(item);
                  }

                  contextMenuStrip1.Tag = button.Tag;
                  contextMenuStrip1.Show(button, new Point(0, button.Height));
                  
              };

            contextMenuStrip1.ItemClicked += (s, e) =>
              {
                  ContextMenuStrip menuAway = (ContextMenuStrip)s;
                  int BotID = HelpMethod.ToInt(menuAway.Tag.ToString());

                  RunAuthorizedAction(BotID, async (client, accountSettings, authResult) =>
                  {
                      await BotEngine.Away(e.ClickedItem.Text, BotID, this, client);
                  }, setStartFlag: true);
              };
            ContextMenuStrip contextMenuStrip2 = new ContextMenuStrip();

            button_getcity.Click += (s, e) =>
            {
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)s;
                contextMenuStrip2.Items.Clear();
                int i = 0;
                foreach (string key in arrBots.Keys) // перебираем все ключи
                {

                    contextMenuStrip2.Items.Add(arrBots[key]);
                    contextMenuStrip2.Items[i].Tag = key;
                    i++;
                }
                contextMenuStrip2.Tag = button.Tag;
                contextMenuStrip2.Show(button, new Point(0, button.Height));
            };
            contextMenuStrip2.ItemClicked += (s, e) =>
            {
                ContextMenuStrip menuGetCity = (ContextMenuStrip)s;
                int BotID = HelpMethod.ToInt(menuGetCity.Tag.ToString());

                RunAuthorizedAction(BotID, async (client, accountSettings, authResult) =>
                {
                    await BotEngine.SendMsg(this, BotID, client, e.ClickedItem.Tag.ToString());
                }, disposeClientOnAuthFailure: true);
            };
            ContextMenuStrip contextMenuStrip3 = new ContextMenuStrip();
            button_dop.Click += (s, e) =>
            {
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)s;
                contextMenuStrip3.Items.Clear();
                foreach ((string text, string tag) in DopMenuItems)
                {
                    ToolStripItem item = contextMenuStrip3.Items.Add(text);
                    item.Tag = tag;
                }

                contextMenuStrip3.Tag = button.Tag;
                contextMenuStrip3.Show(button, new Point(0, button.Height));
            };
            contextMenuStrip3.ItemClicked += (s, e) =>
            {
                ContextMenuStrip menuDop = (ContextMenuStrip)s;
                int BotID = HelpMethod.ToInt(menuDop.Tag.ToString());

                RunAuthorizedAction(BotID, async (client, accountSettings, authResult) =>
                {
                    switch (e.ClickedItem.Tag)
                    {
                        case "open10": await BotEngine.Open10(BotID, this, client, "1"); break;
                        case "cancelColl": await BotEngine.CancelZad(BotID, this, client, "/city/coll"); break;
                        case "cancelVIP": await BotEngine.CancelZad(BotID, this, client, "/lobby"); break;
                        case "lift": await BotEngine.GetLift(BotID, this, client, 100, ctsPerBot.ContainsKey(BotID.ToString()) ? ctsPerBot[BotID.ToString()].Token : default); break;
                        case "lift15": await BotEngine.GoLift15(BotID, this, client, 100); break;
                        case "floor": await BotEngine.Build(BotID, this, client); break;
                        case "vis": await BotEngine.SetNoVis(client, true); break;
                        case "novis": await BotEngine.SetNoVis(client); break;
                    }
                }, setStartFlag: true, disposeClientOnAuthFailure: true);
            };

            button_awaycity.Click += (s, e) =>
            {
                System.Windows.Forms.Button button = (System.Windows.Forms.Button)s;
                DialogResult result = MessageBox.Show("Вы действительно хотите выйти с города?", "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    int BotID = HelpMethod.ToInt(button.Tag.ToString());

                    RunAuthorizedAction(BotID, async (client, accountSettings, authResult) =>
                    {
                        await BotEngine.AwayCity(BotID, this, settings);
                    }, disposeClientOnAuthFailure: true);
                }
            };

            button_show_settings.Click += (s, e) =>
            {
                try
                {
                    System.Windows.Forms.Button button = (System.Windows.Forms.Button)s;
                    SettingsForm settingForm = new SettingsForm((int)button.Tag);
                    settingForm.ShowDialog();
                    settings.IniParser();
                } catch(Exception ex) { Logger.Write("err open Sett "+ex); }
                //int botID = (int)tabPage.Tag;
                //settings.EnumSection($"USER_{botID}");
             };

            button_getmail.SendToBack();
            button_getmail.Click += (s, e) =>
            {
                try
                {
                    int BotID = HelpMethod.ToInt(button_getmail.Tag.ToString());
                    Dictionary<string, string> accountSettings = GetBotSettings(BotID);
                    HttpClient client = CreateBotHttpClient(accountSettings, out HttpClientHandler handler);
                    MsgForm msgForm = new MsgForm(BotID, client);
                    msgForm.ShowDialog();
                }
                catch (Exception ex) { Logger.Write("err open Sett " + ex); }
            };

            comboBox.SelectionChangeCommitted += (s, e) =>
            {
                System.Windows.Forms.ComboBox senderComboBox = (System.Windows.Forms.ComboBox) s;

                // Change the length of the text box depending on what the user has 
                // selected and committed using the SelectionLength property.
                if (senderComboBox.SelectionLength > 0)
                {

                    senderComboBox.Text = senderComboBox.SelectedItem.ToString();
                }

                int botID = (int)tabPage.Tag;
                 settings.Write($"USER_{botID}", "SERVER", senderComboBox.Text);
             };
            refresh.BringToFront();
            // Подсказки
            toolTip1.SetToolTip(button_start, "Пока бот выполняет работу, остановить его невозможно.");
            toolTip1.SetToolTip(numericupdown_interval_from, "Тут можно выбрать интервал повторов.\r\nНапример: Бот выберет рандомное число от 10 до 20 минут до следующего старта.");
            toolTip1.SetToolTip(numericupdown_interval_do, "Тут можно выбрать интервал повторов.\r\nНапример: Бот выберет рандомное число от 10 до 20 минут до следующего старта.");
            //toolTip1.SetToolTip(comboBox, "Тут можно выбрать сервер, на котором зарегистрирован перс.\r\n Для одноклассников выбрать odkl.vnebo.mobi.");
        }


        private void CheckStop(int BotID, System.Windows.Forms.Button Button, NumericUpDown Interval_From, NumericUpDown Interval_Do, System.Windows.Forms.ComboBox server)
        {
            if (Button.Text != BUTTON_TEXT_START)
            {
                BOT_START(BotID);
            }
            else
            {
                var botKey = BotID.ToString();
                if (ctsPerBot.ContainsKey(botKey))
                {
                    try { ctsPerBot[botKey].Cancel(); } catch { }
                    try { ctsPerBot[botKey].Dispose(); } catch { }
                    ctsPerBot.Remove(botKey);
                }
                
                Invoke((MethodInvoker)delegate
                {
                    Button.Text = BUTTON_TEXT_START;
                    Interval_From.Enabled = true;
                    Interval_Do.Enabled = true;
                    server.Enabled = true;
                });

                HelpMethod.StatusLog("", BotID, this);
            }
        }

 
        private void BOT_START(int BotID)
        {
            // Создаём локальный CancellationTokenSource для этой задачи
            var cts = new System.Threading.CancellationTokenSource();
            var token = cts.Token;
            // Сохраняем CTS, чтобы можно было отменить задачу извне (при нажатии стоп)
            var botKey = BotID.ToString();
            if (ctsPerBot.ContainsKey(botKey))
            {
                try { ctsPerBot[botKey].Cancel(); } catch { }
                try { ctsPerBot[botKey].Dispose(); } catch { }
                ctsPerBot[botKey] = cts;
            }
            else
            {
                ctsPerBot.Add(botKey, cts);
            }
            try
            {
                // Получаем ссылки на компоненты
                System.Windows.Forms.Button button_start = FindControl.FindButton("button_start", BotID, this);
                NumericUpDown interval_from = FindControl.FindNumericUpDown("numericupdown_interval_from", BotID, this);
                NumericUpDown interval_do = FindControl.FindNumericUpDown("numericupdown_interval_do", BotID, this);
                System.Windows.Forms.ComboBox server = FindControl.FindComboBox("server", BotID, this);
                TabPage tekPage = FindControl.FindTabPage("tabPage",BotID,this);
                System.Windows.Forms.CheckBox goLift = FindControl.FindCheckBox("goLift",BotID,this);
                DateTime now = DateTime.UtcNow.AddHours(3); //Всегда используем московское время
                // Отключаем компоненты
                Invoke((MethodInvoker)delegate
                {
                    button_start.Text = BUTTON_TEXT_STOP;
                    //button_start.Enabled = false;
                    interval_from.Enabled = false;
                    interval_do.Enabled = false;
                    server.Enabled = false;
                });
                // Получаем настройки
                Dictionary<string, string> account_settings = GetBotSettings(BotID);
                
                Start[BotID.ToString()] = true;
                // Проверяем логин и пароль на пустоту
                if (account_settings["LOGIN"].Length > 0 & account_settings["PASSWORD"].Length > 0 & account_settings["SERVER"].Length > 0)
                {
                    HttpClient client = CreateBotHttpClient(account_settings, out HttpClientHandler hand);
                    ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    // Запускаем основной поток
                    Task t = Task.Run(async () =>
                    {

                        string result = await EnsureAuthorizedAsync(BotID, client, account_settings, logAuthFailure: false);
                        int ot = 0;
                        if (account_settings.ContainsKey("LIFT_OT")) { ot = HelpMethod.ToInt(account_settings["LIFT_OT"]); }
                        if (ot == 0) { ot = 50; account_settings["LIFT_OT"] = ot.ToString();settings.Write($"USER_{BotID}", "LIFT_OT", ot.ToString()); }
                        
                        // Если успешно авторизовались
                        if (result.Contains("Мой профиль") && !result.Equals("error"))
                        {
                            string str_no = "";
                            if (HelpMethod.ToBoolean(account_settings, "GO_LIFT_2")&& HelpMethod.ToBoolean(account_settings, "NO_GO"))
                            {
                                str_no = account_settings["LIST_NO"];
                            }
                            
                            string url;
                            // Разворачиваем этажи
                            //string url = new Regex("href=\"(.*?expandTowerLink.*?)\">").Match(result).Groups[1].Value;
                           // if (url.Length > 0)
                           // {
                           //     await HelpMethod.Get(url, client, token);
                           // }

                            // Если включена опция "начинать инвов"
                            if (HelpMethod.ToBoolean(account_settings, "START_INV") && Start[$"{BotID}"] && !str_no.Contains("Начинать инвов"))
                            {
                                await BotEngine.StartInv(BotID, this, client, token);
                            }

                            // Если включена опция "помогать проходить инвов"
                            if (HelpMethod.ToBoolean(account_settings, "HELP_INV") && Start[$"{BotID}"] && !str_no.Contains("Помогать с инвами"))
                            {
                                await BotEngine.HelpInv(BotID, this, client, !HelpMethod.ToBoolean(account_settings, "NE_INV"), token);
                            }
                            //if (goLift.Checked) 
                            // Отмечаем прочитаным сообщения от Игра
                            if (HelpMethod.ToBoolean(account_settings, "READ_MAIL") && Start[$"{BotID}"] && !str_no.Contains("Отмечаем прочитаным сообщения от Игра"))
                            {
                                string mail = new Regex("a href=\"(.{0,60}/mail)\"><img").Match(result).Groups[1].Value;
                                string txt = "";
                                if (mail.Length > 0) { txt = await BotEngine.CheckMail(client, true, all: false, cancellationToken: token); }
                            }
                           // await BotEngine.CollectFootball(BotID, this, client, token);

                            if ( !str_no.Contains("Крутить барабан"))
                            {
                                if (result.Contains("href=\"baraban\""))
                                {
                                    await BotEngine.CollectBaraban(BotID, this, client, token);
                                }
                            }
                            if (result.Contains("Стрелы любви</a>"))
                            {
                                await BotEngine.Check_val(BotID, this, client, "gift/smile");
                            }
                            // Парсим ссылку на профиль
                            string profile_url = new Regex("href=\".*?(tower/id/[0-9]*.?)\"><span>").Match(result).Groups[1].Value;

                            // Заносим в переменную ссылку на гостиницу<a class="flhdr" href="./floor/68037603">
                            string hostel_url = new Regex("<a class=\"flhdr\" href=\"(.{1,50}/floor/[0-9]{1,20})\">.{1,10}<span class=\"\">1. Гостиница</span>", RegexOptions.Singleline).Match(result).Groups[1].Value;
     
                            // Если есть марафон
                            if (result.Contains("марафон") && Start[$"{BotID}"] && !str_no.Contains("Сдавать/получать задания марафона"))
                            {
                                await BotEngine.AutumnMarathon(BotID, this, client);
                            }



                            // Если есть лавка собираем и тратим билеты
                            if (result.Contains("\"ny") || result.Contains("\"fabric") && Start[$"{BotID}"] && !str_no.Contains("Собирать/тратить билеты"))
                            {
                                string url_a= new Regex("<a class=\"tdn\" href=\"(.*?)\">\r?\n<div class=\"nfl cntr ny\">").Match(result).Groups[1].Value;
                                await BotEngine.OpenNY(BotID, url_a, this, client);

                                // Если в настройках выбрали тратить билеты
                                if (HelpMethod.ToBoolean(account_settings, "OPEN_LAVKA") && Start[$"{BotID}"])
                                {
                                string tmp_h=await HelpMethod.Get(url_a, client, token);
                                    url_a= new Regex("<a class=\"flhdr cntr\" href=\"(.*?)\">").Match(tmp_h).Groups[1].Value;
                                   // Console.WriteLine($"url_a={url_a}");
                                    /*TextWriter tw2 = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "lavka.html");
                                    tw2.Write(tmp_h);
                                    tw2.Close();*/
                                    
                                    await BotEngine.OpenLavka(BotID, url_a, this, client);
                                }
                            }


                            /*    result = await HelpMethod.Get(profile_url, client);

                                if (!account_settings.ContainsKey("MIN_FLOOR")) { account_settings["MIN_FLOOR"] = "10"; settings.Write("USER_" + BotID.ToString(), "MIN_FLOOR", "10"); }*/

                            /* if (HelpMethod.ToInt(account_settings["MIN_FLOOR"]) - HelpMethod.ToInt(new Regex("Этажей: <strong class=\"white\">([0-9]+)</strong>").Match(result).Groups[1].Value)>0)
                             {
                                 await BotEngine.Build(BotID, this, client); 
                              }
                            */
                            /* CancellationTokenSource s_cts = new CancellationTokenSource();
                             try
                             {

                                 s_cts.CancelAfter(5);*/
                            if (!account_settings.ContainsKey("STAT")) { account_settings["STAT"] = "true"; settings.Write("USER_" + BotID.ToString(), "STAT", "true"); }
                            if (!account_settings.ContainsKey("LOG")) { account_settings["LOG"] = "true"; settings.Write("USER_" + BotID.ToString(), "LOG", "true"); }
                            // Обновляем статистику
                            if (!str_no.Contains("Обновлять статистику")&& HelpMethod.ToBoolean(account_settings, "STAT"))
                            {
                                await BotEngine.Statistics(profile_url, BotID, this, client);
                            }

                            if (!HelpMethod.ToBoolean(account_settings, "LOG")) {
                                RichTextBox logs = FindControl.FindRichTextBox("richtextbox_log", BotID, this);
                                Invoke((MethodInvoker)delegate
                                {
                                    logs.Text = ""; });
                                }
                           /* }
                            catch (OperationCanceledException)
                            {
                                Logger.Write("Tasks cancelled: timed out.");
                                HelpMethod.Log("Ошибка вывода статистики: timed out. ", BotID, this);
                            }
                            finally
                            {
                                s_cts.Dispose();
                            }*/
                            //Проверяем коллекцию и берем или сдаем ее
                            if (HelpMethod.ToBoolean(account_settings, "OPEN_KOLL") && Start[$"{BotID}"] && !str_no.Contains("Брать/сдавать коллекции"))
                            {
                                await BotEngine.GetCollection(BotID, this, client, token);
                            }

                            // Покупаем Пиар
                            if (HelpMethod.ToBoolean(account_settings, "PAY_PIAR") && Start[$"{BotID}"] && !str_no.Contains("Покупать маркетинг/пиар"))
                            {
                                await BotEngine.BuyPiarMark(BotID, this, client, "piar");
                            }

                            // Покупаем Маркетинг
                            if (HelpMethod.ToBoolean(account_settings, "PAY_MARK") && Start[$"{BotID}"] && !str_no.Contains("Покупать маркетинг/пиар"))
                            {
                                await BotEngine.BuyPiarMark(BotID, this, client, "mark");
                            }

                            // Берем задания города
                            if (HelpMethod.ToBoolean(account_settings, "GET_CITY_QUEST") && now.DayOfWeek != DayOfWeek.Sunday && Start[$"{BotID}"] && !str_no.Contains("Брать гор.задания"))
                            {
                                string cityQuest = "";
                                if (!settings.KeyExists($"USER_{BotID}", "TIP_CITY_QUEST")) account_settings["TIP_CITY_QUEST"] = "И брать и помогать";
                                if (!settings.KeyExists($"USER_{BotID}", "TIP_GOR_ZAD")) account_settings["TIP_GOR_ZAD"] = "за 2";
                                if (settings.KeyExists($"USER_{BotID}", "LIST_GOR"))
                                {
                                    cityQuest = account_settings["LIST_GOR"];
                                    if (cityQuest.Length > 0) { await BotEngine.GetCityQuest(BotID, this, client, cityQuest, account_settings["TIP_CITY_QUEST"], account_settings["TIP_GOR_ZAD"],ot); }
                                }
                            }

                            //проверяем сундуки если есть настройка открывать сундуки, получать награду за один сундук и текущий день недели суббота, воскресенье или понедельник
                            if ((HelpMethod.ToBoolean(account_settings, "OPEN_SUND") || HelpMethod.ToBoolean(account_settings, "OPEN_1_SUND")) && (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday || now.DayOfWeek == DayOfWeek.Friday) && Start[$"{BotID}"] && !str_no.Contains("Открывать/сдавать сундуки"))
                            {
                                // Определяем номер тек. недели с начала года
                                GregorianCalendar cal = new GregorianCalendar();
                                int weekNumber = cal.GetWeekOfYear(now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
                                bool nagrada = true; //забирать ли награду
                                if (HelpMethod.ToBoolean(account_settings, "OPEN_1_SUND") && Start[$"{BotID}"]) //Открывать только 1 сундук
                                {
                                    if (settings.KeyExists($"USER_{BotID}", "WEEK_SUND") && Start[$"{BotID}"])
                                    {
                                        if (weekNumber.Equals(HelpMethod.ToInt(account_settings["WEEK_SUND"])) || !HelpMethod.ToBoolean(account_settings, "GET_OPEN_SUND"))
                                        {
                                            nagrada = false;
                                        }

                                    }
                                }
                                if (!HelpMethod.ToBoolean(account_settings, "GET_OPEN_SUND"))
                                {
                                    nagrada = false;
                                }

                                bool poluchili = await BotEngine.GetSund(BotID, this, nagrada, client);
                                if (poluchili) { settings.Write($"USER_{BotID}", "WEEK_SUND", weekNumber.ToString()); }
                            }

                            //Улучшать жителей до специалистов
                            if (HelpMethod.ToBoolean(account_settings, "UPGR_SPEC") && Start[$"{BotID}"] && !str_no.Contains("Улучшать специалистов"))
                            {
                                await BotEngine.GetSpecial(BotID, this, client);
                            }

                            // Если есть построенные этажи и включена опция "Открывать построенные этажи"
                            if (HelpMethod.ToBoolean(account_settings, "FLOOR_OPEN") && Start[$"{BotID}"] && !str_no.Contains("Открывать этажи"))
                            {
                                // Открываем этажи
                                await BotEngine.FloorOpen(BotID, this, client);
                            }

                            // Если включена опция "Нанимать более опытных жителей."
                            if (HelpMethod.ToBoolean(account_settings, "HUMAN_JOBS") && Start[$"{BotID}"] && !str_no.Contains("Нанимать более опытных жителей"))
                            {
                                // Нанимаем более опытных
                                await BotEngine.HumanJobs(hostel_url, BotID, this, client);
                            }

                            // Делаем несколько прогонов сбора выручки, выкладки товара, закупки и развозки лифта
                            if (!str_no.Contains("Выручка/выкладка/закупка"))
                            {
                                for (int i = 1; i <= 2; i++)
                                {
                                    // Если включена опция "Собирать выручку"
                                    if (HelpMethod.ToBoolean(account_settings, "COLLECT_COIN") && Start[$"{BotID}"])
                                    {
                                        await BotEngine.CollectCoins(BotID, this, client, token); // Собираем выручку
                                    }

                                    // Если включена опция "Выкладывать товар"
                                    if (HelpMethod.ToBoolean(account_settings, "SELL_GOODS") && Start[$"{BotID}"])
                                    {
                                        await BotEngine.SellGoods(BotID, this, client);// Выкладываем товар
                                    }

                                    // Если включена опция "Закупать товар"
                                    if (HelpMethod.ToBoolean(account_settings, "BUY_GOODS") && Start[$"{BotID}"])
                                    {
                                        await BotEngine.BuyGoods(BotID, this, client, token); // Закупаем товар 
                                    }

                                    // Если включена опция "Доставлять посетителей"
                                    if (HelpMethod.ToBoolean(account_settings, "LIFT_UP") && Start[$"{BotID}"] )
                                    {
                                        await BotEngine.Lift(BotID, this, client,ot, token);// Лифт
                                    }
                                }
                            }
                            //Улучшаем этажи только когда есть акция
                            if (HelpMethod.ToBoolean(account_settings, "UPG_FLOOR") && result.Contains("Акция: Этажи") && Start[$"{BotID}"] && !str_no.Contains("Улучшать этажи")) { await BotEngine.UgrFloor(BotID, this, client); }
                            
                            // Если включена одна из опций [Выселять жителей ниже 9 уровня, Выселять со знаком (-), Выселять со знаком (+)]
                            if ((HelpMethod.ToBoolean(account_settings, "HOSTEL_EVICT_LESS_9") || HelpMethod.ToBoolean(account_settings, "HOSTEL_EVICT_MINUS") || HelpMethod.ToBoolean(account_settings, "HOSTEL_EVICT_PLUS")) && Start[$"{BotID}"] && !str_no.Contains("Выселять жителей<9"))
                            {
                                await BotEngine.HostelEvict(hostel_url, BotID, this, client, HelpMethod.ToBoolean(account_settings, "HOSTEL_EVICT_LESS_9"), HelpMethod.ToBoolean(account_settings, "HOSTEL_EVICT_MINUS"), HelpMethod.ToBoolean(account_settings, "HOSTEL_EVICT_PLUS"));
                            }

                            // Выселяем жителей >3 одинаковых специальностей
                            if (HelpMethod.ToBoolean(account_settings, "GO_THREE") && Start[$"{BotID}"] && !str_no.Contains("Выселять >3")) { await BotEngine.GoThree(hostel_url, BotID, this, client); }

                            //звать ли в лифт
                            bool getLift = false;
                            //при каких заданиях зовем в лифт
                            string[] zad = { "Новые жители", "Давайдосвидания", "Оптовые закупки", "Товар на витрине", "Инкассатор", "VIP-перевозчик", "Перевозчик" };

                            //Задаем мин.остаток ключей
                            if (!account_settings.ContainsKey("MIN_KOL_KEY")) { account_settings["MIN_KOL_KEY"] = "210"; settings.Write("USER_" + BotID.ToString(), "MIN_KOL_KEY", "200"); }
                            //Задаем сумму в бюджет
                            if (!account_settings.ContainsKey("SUM_BUDG")) { account_settings["SUM_BUDG"] = "5"; settings.Write("USER_" + BotID.ToString(), "SUM_BUDG", account_settings["SUM_BUDG"]); }
                            
                            //пополняем бюджет вводим случайность, чтобы все одновременно не пошли атаковать файл настроек
                            if (HelpMethod.ToBoolean(account_settings, "PAY_BUDG") && Start[$"{BotID}"] && HelpMethod.getRandomNumber.Next(1,6)== 3 && !str_no.Contains("Пополнять бюджет"))
                            {
                                if (!account_settings.ContainsKey("LAST_PAY"))
                                {
                                    account_settings["LAST_PAY"] = now.ToString("d");
                                    settings.Write("USER_" + BotID.ToString(), "LAST_PAY", now.ToString("d"));
                                }
                                //записываем в настройку дату последнего пополнения бюджета
                                if (account_settings["LAST_PAY"] != now.ToString("d"))
                                {
                                    account_settings["LAST_PAY"] = now.ToString("d");
                                    settings.Write("USER_" + BotID.ToString(), "LAST_PAY", now.ToString("d"));
                                    await BotEngine.PayBudg(BotID, this, client, account_settings["SUM_BUDG"]);
                                }
                            }
                            // Если включена опция "Забирать ежедневные задания"
                            if (HelpMethod.ToBoolean(account_settings, "QUESTS") && Start[$"{BotID}"] && !str_no.Contains("Забирать ежедневные задания"))
                            {
                                //Смотрим активные перс.задания и, если есть выполненные, то получаем награду
                                string tekQuest = await BotEngine.PersQuests(BotID, this, client, true);
                                string res = await HelpMethod.Get("/quests", client, token);

                                //Если есть настройка звать в лифт при перс.заданиях и не воскресенье
                                if (tekQuest.Length > 0 && HelpMethod.ToBoolean(account_settings, "GO_LIFT_PERS") && now.DayOfWeek != DayOfWeek.Sunday)// && !res.Contains("Выполнено 7 заданий"))
                                {
                                    if (!settings.KeyExists($"USER_{BotID}", "TIP_PERS_QUEST")) account_settings["TIP_PERS_QUEST"] = "Звать в лифт, если <7 заданий выполнено";
                                    if ((account_settings["TIP_PERS_QUEST"] == "Звать в лифт, если <7 заданий выполнено" && !res.Contains("Выполнено 7 заданий")) || account_settings["TIP_PERS_QUEST"] != "Звать в лифт, если <7 заданий выполнено")
                                    {
                                        tekQuest = await BotEngine.PersQuests(BotID, this, client, false);
                                        if (Array.Exists(zad, element => tekQuest.Contains(element)) && Start[$"{BotID}"]) { await BotEngine.GetLift(BotID, this, client, ot); HelpMethod.Log($"Есть перс.задания: {tekQuest}, зовем посетителей в лифт.", BotID, this); getLift = true; }
                                    }
                                }
                                //откроем 10-ю дверь если есть перс.задание Индиана и не воскресение
                                if (tekQuest.Contains("Индиана Джонс") && HelpMethod.ToBoolean(account_settings, "OPEN_INDIANA") && now.DayOfWeek != DayOfWeek.Sunday) { await BotEngine.Open10(BotID, this, client, account_settings["MIN_KOL_KEY"]); }

                            }
                            // Получим ВИПку, кубок, еженедельный кубок
                            if (!str_no.Contains("Получать ВИПку, кубок, еженедельный кубок"))
                                {
                                    await BotEngine.Cup(BotID, this, client, "/lobby");
                                    await BotEngine.Cup(BotID, this, client, "cup");
                                    string mypage = await HelpMethod.Get(profile_url, client, token);
                                    if (mypage.Contains("/cup/tournament"))
                                        await BotEngine.Cup(BotID, this, client, "/cup/tournament");

                                }
                            
                            //если есть галочка звать в лифт и до этого не звали, то исполняем прихоть
                            if (HelpMethod.ToBoolean(account_settings, "GO_LIFT_2") && !getLift) { await BotEngine.GetLift(BotID, this, client, ot); }
                            
                            //Отменяем неугодные ВИПы
                            if (HelpMethod.ToBoolean(account_settings, "CANCEL_VIP") && Start[$"{BotID}"] && !str_no.Contains("Отменять ВИПку"))
                            {
                                result= await HelpMethod.Get("/lobby", client, token);
                                string tekVIP= new Regex("<strong class=\"admin\">(.*?)</strong>").Match(result).Groups[1].Value;
                                if (account_settings["LIST_VIP"].Contains(tekVIP))
                                    await BotEngine.CancelZad(BotID, this, client, "/lobby");
                            }

                            //Откроем 10-ю дверь, если настройка тратить ключи 
                            if (HelpMethod.ToBoolean(account_settings, "OPEN10") && Start[$"{BotID}"] && !str_no.Contains("Открывать 10-ю если есть перс задание"))
                            {
                                await BotEngine.Open10(BotID, this, client, account_settings["MIN_KOL_KEY"]);
                            }

                            //Купим доп.место в гостинице
                            if (HelpMethod.ToBoolean(account_settings, "DOP_MESTO") && Start[$"{BotID}"] && !str_no.Contains("Покупать доп.место в гостинице"))
                            {
                                await BotEngine.AddDopMesto(hostel_url, BotID, this, client);
                            }

                            //Зовем в лифт, если есть гор.задание и сдадим, если готово Сдавать гор.задания
                            if (HelpMethod.ToBoolean(account_settings, "CITY_QUESTS") && Start[$"{BotID}"] &&  !str_no.Contains("Сдавать гор.задания"))
                            {
                                // получим список гор.Заданияи и сдадим, если готово
                                string tekQuest = await BotEngine.CityQuests(BotID, this, client, true);

                                //Зовем в лифт, если есть гор.задание
                                if (tekQuest != null && HelpMethod.ToBoolean(account_settings, "GO_LIFT") && !getLift)
                                {
                                    if (Array.Exists(zad, element => element == tekQuest) && Start[$"{BotID}"]) { getLift = true; await BotEngine.GetLift(BotID, this, client, ot); HelpMethod.Log($"Есть гор.задание: {tekQuest}, зовем посетителей в лифт.", BotID, this); }
                                }
                            }

                            //Откроем 10-ю дверь, если сегодня опыт +...%
                            if (HelpMethod.ToBoolean(account_settings, "OPEN_INDIANA_50") && result.Contains("Сегодня опыт +") && Start[$"{BotID}"] && !goLift.Checked)
                            {
                                await BotEngine.Open10(BotID, this, client, account_settings["MIN_KOL_KEY"]);
                            }

                            //Доходим до 10-й двери
                            if (HelpMethod.ToBoolean(account_settings, "INDIANA") && Start[$"{BotID}"] && !str_no.Contains("Открывать 10-ю при опыт+"))
                            {
                                await BotEngine.OpenDoors(BotID, this, client, account_settings["MIN_KOL_KEY"]);
                            }

                            // Если включена опция "Нанимать жителей на бирже труда"
                            if (HelpMethod.ToBoolean(account_settings, "VENDORS_HUMANS") && Start[$"{BotID}"] && !str_no.Contains("Нанимать на бирже"))
                            {
                                await BotEngine.VendorsHumans(BotID, this, client);
                            }

                            // Обновляем статистику
                            if (Start[$"{BotID}"] && !str_no.Contains("Обновлять статистику") && HelpMethod.ToBoolean(account_settings, "STAT"))
                            {
                                await BotEngine.Statistics(profile_url, BotID, this, client);
                            }
                            if (!HelpMethod.ToBoolean(account_settings, "GO_LIFT_2"))
                            {
                                //Сворачиваем этажи
                                url = new Regex("href=\"(.*?collapseTowerLink.*?)\">").Match(result).Groups[1].Value;
                                if (url.Length > 0)
                                {
                                    await HelpMethod.Get(url, client, token);
                                }
                            }
                                //сохраняем куки
                            HelpMethod.SaveCooc(account_settings["LOGIN"], hand);
                            //освобождаем потоки
                            client.Dispose();
                            hand.Dispose();

                            // Получаем рандомный интервал ожидания
                            int interval_sec = HelpMethod.getRandomNumber.Next(
                                (HelpMethod.ToInt(interval_from.Value.ToString()) * 60),
                                (HelpMethod.ToInt(interval_do.Value.ToString()) * 60)
                                + 1);

                            //Если позвали в лифт из-за гор. или перс. задания, то уменьшаем период ожидания до 10 сек.
                            if (getLift) { interval_sec = HelpMethod.getRandomNumber.Next(1, 10); }
                            //Если стоит галка звать в лифт, то уменьшаем период ожидания до 1 сек.
                            if (HelpMethod.ToBoolean(account_settings, "GO_LIFT_2")) { interval_sec = 1; }
                            // Ожидание
                            await BotEngine.Sleep(BotID, button_start, this, interval_sec);

                            // Новая пустая строка в логе
                            HelpMethod.Log("", BotID, this, ShowTime: false);

                            // Проверяем не остановлен ли бот
                            CheckStop(BotID, button_start, interval_from, interval_do, server);
                            }
                            else if (result == "error") //если ошибка авторизации, ждем 1 минуту и вновь попытаемся
                            {
                                // Ожидание
                                await BotEngine.Sleep(BotID, button_start, this, 60);
                            LogMailRuLoginHint(BotID, account_settings);

                            // Проверяем не остановлен ли бот
                            CheckStop(BotID, button_start, interval_from, interval_do, server);
                            }
                            else //если ответ какой-то есть, но нет "Мой профиль" значит неверный логин/пароль
                            {
                                HelpMethod.Log("Неправильный логин или пароль.", BotID, this, Color.Red);

                            //Останавливаем бота
                           /* Invoke((MethodInvoker)delegate
                            {
                                button_start.Enabled = true;
                                button_start.Text = BUTTON_TEXT_START;
                                interval_from.Enabled = true;
                                interval_do.Enabled = true;
                                server.Enabled = true;

                            });*/
                            // Запускаем таймер ожидания
                            // Иногда бот отваливается с этим, причин не знаю, временно так пофиксить можно
                            await BotEngine.Sleep(BotID, button_start, this, 60);

                            // Проверяем не остановлен ли бот
                            CheckStop(BotID, button_start, interval_from, interval_do, server);
                        }
                    });

                    
                }
                        else
                        {
                            HelpMethod.Log("Логин, пароль или сервер не могут быть пустыми.", BotID, this, Color.Red);

                            // Меняем текст кнопки (ЗАПУСТИТЬ БОТА), разблокируем кнопку и интервалы ОТ и ДО
                            Invoke((MethodInvoker)delegate
                            {
                                button_start.Enabled = true;
                                button_start.Text = BUTTON_TEXT_START;
                                interval_from.Enabled = true;
                                interval_do.Enabled = true;
                                server.Enabled = true;

                            });
                        }
                    
            }
            catch(Exception ex) { 
                Logger.Write("ex = "+ex); HelpMethod.Log("Ошибка запуска бота USER_"+BotID.ToString()+" ",BotID,this);
                HelpMethod.Log("Ошибка "+ex, BotID, this);
                //await BotEngine.Sleep(BotID, button_start, this, 60);
                BOT_START(BotID); }
        }

         private void CheckUpd()
        {
            try
            {

                Task.Run(async () =>
                {

                    HttpClient client = HelpMethod.HttpManager();
                    client.BaseAddress = new Uri(domen);
                    string url = domen + "version.txt";
                    string result = await HelpMethod.Get(url, client);

                    if (result.Length > 0)
                    {
                        
                        foreach (Match item in new Regex("v=\"(.*?)\".*?d=\"(.*?)\"", RegexOptions.Singleline | RegexOptions.Multiline).Matches(result))
                        {
                            if (item.Groups[1].Value!=v || item.Groups[2].Value != d)
                            {
                                 await DownloadUpd(client);
                            }
                            
                        }
                    }

                    client.Dispose();
                });

            }
            catch(Exception ex) { MessageBox.Show("Ошибка проверки обновления, попробуйте позже "+ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Information); }
        }
        public static async Task<bool> DownloadUpd(HttpClient client) 
        {
             var response = await client.GetAsync(new Uri(domen+"vnebo.mobi.bot.exe"));
            var response2 = await client.GetAsync(new Uri(domen + "updater.exe"));
            using (var fs = new FileStream(Path.GetTempPath()+ "tmp.vnebo.mobi.bot.exe", FileMode.OpenOrCreate))
            {
                await response.Content.CopyToAsync(fs);
                using (var fs2 = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "updater.exe", FileMode.OpenOrCreate))
                {
                    await response2.Content.CopyToAsync(fs2);
                }
                string fileFullPath = Application.ExecutablePath;
                // Получаем информацию об файле
                string fileInfo = new FileInfo(fileFullPath).Name.ToString();

                Process.Start(AppDomain.CurrentDomain.BaseDirectory + "updater.exe", fileInfo + " "+ Path.GetTempPath() + "tmp.vnebo.mobi.bot.exe");
                return true;
            }
        }
        private TabPage AddPage(int LastIndex = 0, bool Default = true, string section = "")
        {

            if (Account < maxAccount)
            {
                // Увеличиваем количество вкладкок
                AccountCount++;

                // Увеличиваем количество аккаунтов
                Account++;
               
                if (!section.Equals("")) 
                {
                    AccountCount = HelpMethod.ToInt(section.Replace("USER_", ""));
                }
                else
                {
                    int i = 0;
                    do { i++; } while (arrUser.Exists(x=>x==i));
                    AccountCount = i;
                    arrUser.Add(AccountCount);
                }


                // Генерируем новую вкладку
                TabPage tabPage = new TabPage
                {
                    Text = (section.Length > 0 && settings.Read(section, "LOGIN").Length > 0) ? settings.Read(section, "LOGIN") : "Новый персонаж",
                    Name = $"tabPage{AccountCount}",
                    Size= new Size(999, 340),
                    BackColor = Color.White,
                    ToolTipText = "Для удаления профиля, нажмите несколько раз по вкладке. " + AccountCount,
                    Tag = AccountCount,
                    ImageIndex = imageList1.Images.IndexOfKey(AVATAR_DEFAULT),
                    AllowDrop = true
                };

                // Ставим обработчик событий на двойной клик
                tabPage.DoubleClick += TabControl1_DoubleClick;

                // Добавляем шаблон на вкладку
                CreateTemplate(tabPage);

                // Если новая вкладка создается пользователем
                if (Default)
                {
                    // Сохраняем профиль со стандартными настройками
                    BotEngine.DefSettings($"USER_{AccountCount}", settings, "", "");

                    // Добавляем вкладку
                    tabControl1.TabPages.Insert(LastIndex, tabPage);
                }
                else
                {
                    // Добавляем вкладку
                    tabControl1.TabPages.Insert(tabControl1.TabCount - 1, tabPage);

                    // Загружаем логин и пароль
                    FindControl.FindTextBox("textbox_login", AccountCount, this).Text = settings.Read(section, "LOGIN");
                    FindControl.FindTextBox("textbox_password", AccountCount, this).Text = settings.Read(section, "PASSWORD");

                    // Загружаем интервал от и до
                    FindControl.FindNumericUpDown("numericupdown_interval_from", AccountCount, this).Value = Math.Max(HelpMethod.ToInt(settings.Read(section, "INTERVAL_FROM")),1);
                    FindControl.FindNumericUpDown("numericupdown_interval_do", AccountCount, this).Value = Math.Max(HelpMethod.ToInt(settings.Read(section, "INTERVAL_DO")),2);

                    //Server
                    FindControl.FindComboBox("server", AccountCount, this).Text = settings.Read(section, "SERVER");
                    /*string read_file = "";
                    string lofFile = Path.GetTempPath() + settings.Read(section, "LOGIN")+".log";
                    if (File.Exists(lofFile))
                    {
                        read_file = File.ReadAllText(lofFile);
                    }
                    if(read_file.Length>0)
                    FindControl.FindRichTextBox("richtextbox_log", AccountCount, this).Text = read_file;*/

                }

                // Делаем вкладку выбранной
                tabControl1.SelectedTab = tabPage;

                // Возвращаем вкладку, чтобы с ней можно было работать из вне.
                return tabPage;
            }

            return null;
        }

        private void TabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            int lastIndex = tabControl1.TabCount - 1;

            if (e.Button == MouseButtons.Left)
            {
               
                    if (tabControl1.GetTabRect(lastIndex).Contains(e.Location))
                    {
                    DialogResult result = MessageBox.Show("Вы действительно хотите добавить профиль?", "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        if (Account < maxAccount)
                        {
                            _ = AddPage(lastIndex);
                            return;
                        }

                        MessageBox.Show($"Нельзя создать больше {maxAccount} вкладок!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }

        private void TabControl1_DoubleClick(object sender, EventArgs e)
        {
            int selectIndex = tabControl1.SelectedIndex - 1;

            if (tabControl1.GetTabRect(selectIndex + 1).Contains(((MouseEventArgs)e).Location))
            {
                if (((MouseEventArgs)e).Button == MouseButtons.Left)
                {
                    DialogResult result = MessageBox.Show("Вы действительно хотите удалить профиль?", "Внимание!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {

                        // Удаляем бота из сохранения
                        settings.DeleteSection($"USER_{tabControl1.TabPages[tabControl1.SelectedIndex].Tag}");
                        arrUser.Remove((int)tabControl1.TabPages[tabControl1.SelectedIndex].Tag);
                        // Удаляем вкладку
                        tabControl1.TabPages.Remove(tabControl1.SelectedTab);
                        tabControl1.SelectedIndex = selectIndex == -1 ? 0 : selectIndex;
                        //foreach (int i in arrUser) { Console.WriteLine(i); }
                        
                        // Убавляем количество аккаунтов
                        Account--;

                        // Если вкладок 0, то создаем новый пустой профиль
                        if (tabControl1.TabCount == 1)
                        {
                            AddPage();
                        }
                    }
                }
            }
        }

        private void TabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPageIndex == tabControl1.TabCount - 1)
            {
                e.Cancel = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string fileFullPath = Application.ExecutablePath;
            FileInfo fileInfo = new FileInfo(fileFullPath);
            // Получаем имя файла
            string fileName = fileInfo.Name.Replace(".exe", "");
            int kol = 0;
            if (Process.GetProcessesByName(fileName).Length > 0)
            {
                Process[] myProcesses2 = Process.GetProcessesByName(fileName);

                for (int i = 0; i < myProcesses2.Length; i++)
                {
                    if(myProcesses2[i].MainModule.FileName== fileFullPath)
                    {
                        kol++;
                        if (kol > 1)
                        {
                            MessageBox.Show("Приложение уже запущено, проверьте трей возле часов", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            //поиск окна по заголовку
                                Environment.Exit(0); // no-op
                        }
                    }
                }
            }
            settings.IniParser();
            if(!settings.KeyExists("GLOBAL", "AUTO_START")) { settings.Write("GLOBAL", "AUTO_START","false"); }
            //GetBots();
            string read_file;
            // Читаем файл настроек
            try
            {
                read_file = File.Exists(settings.Path) ? File.ReadAllText(settings.Path) : "";
            }
            catch(Exception ex){
                Logger.Write("exep read file ini = "+ex);
                HelpMethod.Log("Ошибка чтения файла настройки бота!  Бот остановлен", 1, this);
                read_file = null;
            }
            // Если файл настроек не пустой
            if (read_file.Length > 0)
            {
                foreach (Match item in new Regex("USER_([0-9]+)").Matches(read_file))
                {
                    arrUser.Add(HelpMethod.ToInt(item.Groups[1].Value));
                }
                //read_file = null;
                arrUser.Sort();
                foreach (int i in arrUser)
                {
                    AddPage(Default: false, section: "USER_" + i.ToString());
                    //Console.WriteLine("section = '" + "USER_" + i.ToString() + "'");
                }


                // Автоматический старт
                if (settings.KeyExists("GLOBAL", "AUTO_START"))
                {
                    // Получаем значение
                    bool auto_start = settings.Read("GLOBAL", "AUTO_START") == "true";

                    // Если включено
                    if (auto_start)
                    {
                        // Меняем настройку
                        toolStripMenuItem5.Checked = auto_start;

                        // Запускаем всех ботов
                        toolStripMenuItem3.PerformClick();

                        // Скрываем приложее в трей
                        WindowState = FormWindowState.Minimized;
                    }
                }
                if (arrUser.Count == 0) AddPage();
                //return;
            }
            else
            {
                // Создаём новый профиль, если в настройках их нет
                AddPage();
            }
        }

        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            // Запускаем всех ботов
            //for (int i = 1; i <= AccountCount; i++)
            foreach(int i in arrUser)
            {
                System.Windows.Forms.Button button = FindControl.FindButton("button_start", i, this);

                if (button.Text == BUTTON_TEXT_START & button.Enabled)
                {
                    BOT_START(i);
                }
            }
        }

        private void ToolStripMenuItem4_Click(object sender, EventArgs e)
        {
            // Останавливаем всех ботов
            // for (int i = 1; i <= AccountCount; i++)
            foreach (int i in arrUser)
            {
                System.Windows.Forms.Button button = FindControl.FindButton("button_start", i, this);

                if (button.Text == BUTTON_TEXT_STOP)
                {
                    button.Text = BUTTON_TEXT_START;
                    Start[$"{i}"] = false;
                }
            }
        }

        private void ToolStripMenuItem5_Click(object sender, EventArgs e)
        {
            // Меняем Checked
            toolStripMenuItem5.Checked = !toolStripMenuItem5.Checked;

            // Сохраняем
            settings.Write("GLOBAL", "AUTO_START", toolStripMenuItem5.Checked.ToString().ToLower());

            // Добавляем в автозагрузку
            HelpMethod.AutoRun(toolStripMenuItem5.Checked);
        }

        private void NotifyIcon1_Click(object sender, EventArgs e)
        {
            // Если была нажата правая кнопка мыши
            if (((MouseEventArgs)e).Button != MouseButtons.None)
            {
                // Показываем форму
                Show();

                // Разворачиваем приложение
                WindowState = FormWindowState.Normal;

                // Показываем значек в панели задач
                ShowInTaskbar = true;

                // Скрываем иконку из трее
                notifyIcon1.Visible = false;
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Если окно было свернуто
            if (WindowState == FormWindowState.Minimized)
            {
                // Прячем из панели задач
                ShowInTaskbar = false;

                // Прячем форму
                Hide();

                // Показываем иконку в трее
                notifyIcon1.Visible = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Отменяем все фоновые задачи
            try
            {
                foreach (var kv in ctsPerBot)
                {
                    try { kv.Value.Cancel(); } catch { }
                    try { kv.Value.Dispose(); } catch { }
                }
                ctsPerBot.Clear();
            }
            catch { }
            //settings.SaveSettings();
            //settings.SortIni();
            Environment.Exit(0);
        }

      
        private async void CreatePers_Click(object sender, EventArgs e)
        {
            int sex;
            if (sender.ToString().Equals("Мальчика")) { sex = 1; } else { sex = 2; }

            try
            {
                //создадим перса
                string sections = await BotEngine.Create(this, settings, HelpMethod.ToInt(this.tabControl1.SelectedTab.Tag.ToString()), arrUser, sex);
                if (sections != "")
                {
                    AddPage(Default: false, section: sections);
                    BOT_START(HelpMethod.ToInt(sections.Replace("USER_","")));
                }
      
            }catch(Exception ex)
            { Logger.WriteError("ex = " + ex); }
        }
      
        private void GetBots()
        {
            Task.Run(async () =>
            {

            HttpClient client = HelpMethod.HttpManager();
            string url = domen+"1q2.php";
            string result = await HelpMethod.Get(url, client);

                if (result.Length > 0)
                {
                    foreach (Match item in new Regex(@"(.+?)###(\d+?)###(\d)###(.+?)\r?\n").Matches(result))
                    {
                        arrBots.Add(item.Groups[2].Value, item.Groups[1].Value+" Город: "+ item.Groups[4].Value);
                    }
                }
             arrBots.Add("22325554", "Болтун Город: Добренькие Мы");
             arrBots.Add("free", "встать на биржу");
            client.Dispose();
            });

        }

        private void UpdateMenu(object sender, EventArgs e)
        {

            toolStripMenuItem1.DropDownItems.Clear();
            
            foreach (int i in arrUser)
            {
                string section = $"USER_{i}";
                ToolStripMenuItem Pers = new ToolStripMenuItem
                {
                    Name = section,
                    //Console.WriteLine($"item name = {section}, accountCount={i}");
                    Image = Resources.play,
                    ToolTipText = BUTTON_TEXT_START
                };
                System.Windows.Forms.Button but_start = FindControl.FindButton("button_start", i, this);
                TabPage tab = FindControl.FindTabPage("tabPage", i, this);
                if (but_start.Text.Equals(BUTTON_TEXT_STOP)) { Pers.Image = Resources.stop; Pers.ToolTipText = BUTTON_TEXT_STOP; }

                Pers.Click += (s, e1) =>
                {
                    //Console.WriteLine($"acc={but_start.Tag} but=" + but_start.Name);
                    if (but_start.Text == BUTTON_TEXT_STOP)
                    {
                        but_start.Text = BUTTON_TEXT_START;
                        Start[$"{but_start.Tag}"] = false;
                        Pers.Image = Resources.stop; Pers.ToolTipText = BUTTON_TEXT_STOP;

                    }
                    else
                    {
                        Pers.Image = Resources.play;
                        Pers.ToolTipText = BUTTON_TEXT_START;
                        BOT_START(HelpMethod.ToInt(but_start.Tag.ToString()));
                    }
                    UpdateMenu(sender,e);
                };
                Pers.Text = tab.Text;

                //this.Pers.Size = new System.Drawing.Size(162, 22);


                toolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] {
                    Pers,
                    toolStripSeparator1,
            toolStripMenuItem3,
            toolStripMenuItem4,
            toolStripSeparator1,
            CreatePers
                });
               
            }
            toolStripMenuItem1.Visible = true;
            toolStripMenuItem1.Enabled = true;
            toolStripMenuItem1.ShowDropDown();
          
        }

        private void очиститьВсеЛогиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (int i in arrUser)
            {
                RichTextBox log = FindControl.FindRichTextBox("richtextbox_log", i, this);
                log.Clear();

               
            }
        }

        private void toolStripMenuItemUpd_Click(object sender, EventArgs e)
        {

            CheckUpd();

        }

        private void группаВТГToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://t.me/+R8eIoREobA9kZmFi");
        }
    }
}
