using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vnebo.mobi.bot.Libs;

namespace vnebo.mobi.bot
{
    public partial class MsgForm : Form
    {
        public MsgForm(int BotID, HttpClient client)
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                string txt = await BotEngine.ReadMail(client);
                Invoke((MethodInvoker)delegate
                {
                    foreach (string str in txt.Split(new string[] { "#\n" }, StringSplitOptions.None))
                    {
                        listBox1.Items.Add(str);
                        Console.WriteLine($"txt={str}");
                    }
                });
            });
            
        }
    }
}
