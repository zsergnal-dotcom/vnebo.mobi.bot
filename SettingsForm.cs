using System;
using System.Linq;
using System.Windows.Forms;

namespace vnebo.mobi.bot
{

    public partial class SettingsForm : Form
    {
        private static readonly IniFiles settings = new IniFiles();

        public SettingsForm(int BotID)
        {
            
            InitializeComponent();

            // Название секции
            string section_name = $"USER_{BotID}";
            settings.IniParser();

            // Обновляем заголовок формы
            Text = $"Настройки {(settings.KeyExists(section_name, "LOGIN") ? $"( {settings.Read(section_name, "LOGIN")} )" : "")}";

            // Записываем в Tag имя ключа
            checkBox01.Tag = "COLLECT_COIN";
            checkBox02.Tag = "SELL_GOODS";
            checkBox03.Tag = "BUY_GOODS";
            checkBox04.Tag = "LIFT_UP";
            checkBox05.Tag = "FLOOR_OPEN";
            checkBox06.Tag = "QUESTS";
            checkBox08.Tag = "HUMAN_JOBS";
            checkBox09.Tag = "HOSTEL_EVICT_LESS_9";
            checkBox10.Tag = "HOSTEL_EVICT_MINUS";
            checkBox11.Tag = "HOSTEL_EVICT_PLUS";
            checkBox12.Tag = "BUY_BAKS_FOR_COIN";
            checkBox13.Tag = "VENDORS_HUMANS";
            checkBox14.Tag = "CITY_QUESTS";
            checkBox15.Tag = "INDIANA";
            checkBox1.Tag = "START_INV";
            checkBox3.Tag = "HELP_INV";
            checkBoxColl1Sund.Tag = "OPEN_1_SUND";
            checkBoxOpenKoll.Tag = "OPEN_KOLL";
            checkBoxOpenSund.Tag = "OPEN_SUND";
            checkPayMark.Tag = "PAY_MARK";
            checkPayPiar.Tag = "PAY_PIAR";
            checkOpenIndiana.Tag = "OPEN_INDIANA";
            checkGoLiift.Tag = "GO_LIFT";
            checkBoxGoLiftPers.Tag = "GO_LIFT_PERS";
            checkBoxSpec.Tag = "UPGR_SPEC";
            checkBoxOPEN_INDIANA_50.Tag = "OPEN_INDIANA_50";
            MinKolKey.Tag = "MIN_KOL_KEY";
            MinFloor.Tag = "MIN_FLOOR";
            DopMesto.Tag = "DOP_MESTO";
            GetSundOpen.Tag = "GET_OPEN_SUND";
            readMail.Tag = "READ_MAIL";
            GetCityQuest.Tag = "GET_CITY_QUEST";
            open10.Tag = "OPEN10";
            button_upg_floor.Tag = "UPG_FLOOR";
            GoThree.Tag = "GO_THREE";
            payBudg.Tag = "PAY_BUDG";
            sumBudg.Tag = "SUM_BUDG";
            listGor.Tag = "LIST_GOR";
            openLavka.Tag = "OPEN_LAVKA";
            NeInv.Tag = "NE_INV";
            CancelVIP.Tag = "CANCEL_VIP";
            listVIP.Tag = "LIST_VIP";
            CitiQuestCombo.Tag = "TIP_CITY_QUEST";
            buildFloor.Tag = "BUILD_FLOOR";
            butReset.Tag = section_name;
            tipGorZad.Tag = "TIP_GOR_ZAD";
            tipPersQuest.Tag = "TIP_PERS_QUEST";
            addDirector.Tag = "ADD_DIRECTOR";
            goLift2.Tag = "GO_LIFT_2";
            noGo.Tag = "NO_GO";
            listNo.Tag = "LIST_NO";
            checkBox_Log.Tag="LOG";
            checkBox_Stat.Tag = "STAT";
            lift_ot.Tag = "LIFT_OT";
  

            switch (tipGorZad.Text)
            {
                case "за 2 ": priceZad.Text = ""; priceZad.Visible = false; break;
                case "за 15 ": priceZad.Text = "Инкассатор обойдется в 303$\nТовар на витрине обойдется в 153$\nОптовые закупки обойдутся в 93$\nVIP-перевозчик обойдется в 75$"; priceZad.Visible = true; break;
                case "за 75": priceZad.Text = "Инкассатор обойдется в 753$\nТовар на витрине обойдется в 453$\nОптовые закупки обойдутся в 153$\nVIP-перевозчик обойдется в 75$\n";
                    priceZad.Visible = true; break;
            }

            // Прохоидмся по GroupBox

            foreach (TabPage tabPage in dop.TabPages)
            {
                try
                {
                    foreach (CheckBox checkBox in tabPage.Controls.OfType<CheckBox>())
                    {
                        //запишем недостающие настройки в файл
                        if (!settings.KeyExists(section_name, checkBox.Tag.ToString())) { settings.Write(section_name, checkBox.Tag.ToString(), checkBox.Checked.ToString().ToLower()); }
                        // Ставим обработчик событий
                        checkBox.CheckedChanged += (s, e) =>
                        {
                            settings.Write(section_name, (s as CheckBox).Tag.ToString(), (s as CheckBox).Checked.ToString().ToLower());
                        };

                        // Загружаем настройку для этого CheckBox
                        checkBox.Checked = settings.Read(section_name, checkBox.Tag.ToString()) == "true";
                    }
                }catch(Exception ex)
                { Console.WriteLine("ex = "+ex); }

                try
                {
                    foreach (TextBox txt in tabPage.Controls.OfType<TextBox>())
                    {
                        if ((txt.Tag.ToString() == "LIFT_OT") && txt.Text == "")
                        {
                            txt.Text = "100"; 
                        }
                        if (txt.Text == "") { txt.Text = "0"; }
                        
                        //запишем недостающие настройки в файл
                        if (!settings.KeyExists(section_name, txt.Tag.ToString())) { settings.Write(section_name, txt.Tag.ToString(), txt.Text); }
                        // Ставим обработчик событий
                        txt.TextChanged += (s, e) =>
                        {
                            settings.Write(section_name, (s as TextBox).Tag.ToString(), (s as TextBox).Text);
                        };

                        // Загружаем настройку для этого CheckBox
                        txt.Text = settings.Read(section_name, txt.Tag.ToString());
                    }
                }
                catch (Exception ex)
                { Console.WriteLine("ex = " + ex); }

                try
                {
                    foreach (ComboBox txt in tabPage.Controls.OfType<ComboBox>())
                    {
                        if (txt.Text == "") { txt.Text = txt.Items[txt.Items.Count-1].ToString(); }
                        //запишем недостающие настройки в файл
                        if (!settings.KeyExists(section_name, txt.Tag.ToString())) { settings.Write(section_name, txt.Tag.ToString(), txt.Text); }
                        // Ставим обработчик событий
                        txt.SelectedIndexChanged += (s, e) =>
                        {
                            settings.Write(section_name, (s as ComboBox).Tag.ToString(), (s as ComboBox).Text);
                            if(txt.Tag.ToString() == "TIP_GOR_ZAD") 
                            {
                                switch(txt.Text)
                                {
                                    case "за 2 ": priceZad.Text = ""; priceZad.Visible = false; break;
                                    case "за 15 ": priceZad.Text = "Инкассатор обойдется в 303$\nТовар на витрине обойдется в 153$\nОптовые закупки в 93$"; priceZad.Visible = true; break;
                                    case "за 75": priceZad.Text = "Инкассатор обойдется в 753$\nТовар на витрине обойдется в 453$\nОптовые закупки в 153$"; priceZad.Visible = true; break;
                                }
                                
                            }
                        };

                        // Загружаем настройку для этого CheckBox
                        txt.Text = settings.Read(section_name, txt.Tag.ToString());
                    }
                }
                catch (Exception ex)
                { Console.WriteLine("ex = " + ex); }

                try
            {
                foreach (ListBox txt in tabPage.Controls.OfType<ListBox>())
                {
                        string temp="";

                        // Загружаем настройку для этого ListBox
                        temp = settings.Read(section_name, txt.Tag.ToString());
            
                        if (temp!=null&&temp!="")
                        {
                           
                            for (int i = 0; i < txt.Items.Count;)
                            {
                                txt.SetSelected(i, temp.Contains(txt.Items[i].ToString()));
                                i++;
                            }
                            temp = "";
                        }
                        else {
                            temp = "";
                            
                            for (int i = 0; i < txt.Items.Count;)
                            { temp += txt.Items[i].ToString() + ", "; txt.SetSelected(i, true); i++; }
                            temp = temp.Substring(0, temp.Length - 2);
                        }
                        
                        //запишем недостающие настройки в файл
                        if (!settings.KeyExists(section_name, txt.Tag.ToString())) { settings.Write(section_name, txt.Tag.ToString(), temp); }
                    // Ставим обработчик событий
                    txt.SelectedIndexChanged += (s, e) =>
                    {
                        temp = "";
                        foreach (var item in txt.SelectedItems)
                        {
                            temp += item + ", ";
                        }
                        if (temp.Length > 2) { temp = temp.Substring(0, temp.Length - 2); }
                        settings.Write(section_name, (s as ListBox).Tag.ToString(), temp);
                    };

                    
                        
                    }
            }
            catch (Exception ex)
            { Console.WriteLine("ex list = " + ex); }
                
        }
        
        listGor.MouseDown += (s, e) => {
                string txt = "";
                foreach (object item in listGor.SelectedItems)
                { txt += item.ToString() + ", "; }
                if (txt.Length > 2) Text = txt.Substring(0, txt.Length - 2);
                settings.Write(section_name, listGor.Tag.ToString(), txt);
            };
            // Подсказки
            toolTip1.SetToolTip(checkBox01, "Бот будет собирать выручку с этажей.");
            toolTip1.SetToolTip(checkBox02, "Бот будет выкладывать доставленный товар на этажах.");
            toolTip1.SetToolTip(checkBox03, "Бот будет закупать товар на этажах.\nПриоритет закупки товара: 3 - 2 - 1.");
            toolTip1.SetToolTip(checkBox04, "Бот будет поднимать посетителей в лифте.");
            toolTip1.SetToolTip(checkBox05, "Бот будет открывать построенные этажи.");
            toolTip1.SetToolTip(checkBox06, "Бот будет забирать выполненные ежедневные задания.");
            toolTip1.SetToolTip(checkBox08, "Бот будет нанимать более опытных жителей на работу.");
            toolTip1.SetToolTip(checkBox09, "Бот будет выселять жителей ниже 9 уровня.");
            toolTip1.SetToolTip(checkBox10, "Бот будет выселять жителей со знаком (-).");
            toolTip1.SetToolTip(checkBox11, "Бот будет выселять жителей со знаком (+).");
            toolTip1.SetToolTip(checkBox12, "Бот будет выкупать ежедневные баксы за монеты.");
            toolTip1.SetToolTip(checkBox13, "Бот будет нанимать жителей на бирже труда, пока есть бесплатные попытки.");
            toolTip1.SetToolTip(checkBox14, "Бот будет забирать награды за гор.задания.");
            toolTip1.SetToolTip(checkBox15, "Бот будет проходить лабиринт.");
            toolTip1.SetToolTip(checkBox1,  "Бот будет начинать инвов, если они открылись");
            toolTip1.SetToolTip(checkBox3,  "Бот будет помогать проходить инвов");
            toolTip1.SetToolTip(checkBoxColl1Sund, "Бот будет получать награду только за один сундук, но не будет следить, сдал ли сам пользователь сундук или нет");
            toolTip1.SetToolTip(checkBoxOpenKoll, "Бот будет брать задания коллекций, а так же получать награду за них");
            toolTip1.SetToolTip(button_upg_floor, "Бот будет улучшать этажи в дни акции -50% на улучшение");
            toolTip1.SetToolTip(payBudg, "Бот будет вносить в бюджет города сумму");
        }

        private void MinKolKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;

            if (!Char.IsDigit(number) && e.KeyChar != Convert.ToChar(8))
            {
                e.Handled = true;
            }
        }

        


        /*private void MoveSelectedItemUp(this ListBox listBox)
        {
            _MoveSelectedItem(listBox, -1);
        }

        private void MoveSelectedItemDown(this ListBox listBox)
        {
            _MoveSelectedItem(listBox, 1);
        }*/

        private void MoveSelectedItem(ListBox listBox, int direction)
        {
            // Checking selected item
            //Console.WriteLine($"listBox.SelectedItem={listBox.SelectedItem} listBox.SelectedIndex={listBox.SelectedIndex}");
            if (listBox.SelectedItem == null || listBox.SelectedIndex < 0)
                return; // No selected item - nothing to do

            // Calculate new index using move direction
            int newIndex = listBox.SelectedIndex + direction;
            //Console.WriteLine($"newIndex={newIndex} ");
            // Checking bounds of the range
            if (newIndex < 0 || newIndex >= listBox.Items.Count)
                return; // Index out of range - nothing to do

            object selected = listBox.SelectedItem;

           

            // Removing removable element
            listBox.Items.Remove(selected);
            // Insert it in new position
            listBox.Items.Insert(newIndex, selected);
            // Restore selection
            listBox.SetSelected(newIndex, true);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            MoveSelectedItem(this.listGor, -1);
        }

        private void Down_Click(object sender, EventArgs e)
        {
            MoveSelectedItem(this.listGor, 1);
        }

        private void SelUnsel(bool set=true)
        {
            for (int i = 0; i < this.listGor.Items.Count; i++)
            {
                this.listGor.SetSelected(i, set);
            }
        }

        private void SelAll_Click(object sender, EventArgs e)
        {
            SelUnsel(true);
        }
        private void UnSelAll_Click(object sender, EventArgs e)
        {
            SelUnsel(false);
        }

        private void ResetSettings(object sender, EventArgs e)
        {
            try
            {
                string section_name = (sender as Button).Tag.ToString();
                foreach (TabPage tabPage in dop.TabPages)
                {
                    foreach (CheckBox checkBox in tabPage.Controls.OfType<CheckBox>())
                    {
                        //запишем недостающие настройки в файл
                        settings.Write(section_name, checkBox.Tag.ToString(), "false");
                        checkBox.Checked = false;
                    }

                    foreach (ListBox txt in tabPage.Controls.OfType<ListBox>())
                    {
                        txt.ClearSelected();
                    }
                }
                }
            catch (Exception ex)
            { Console.WriteLine("reset ex = " + ex); }
        }

        private void lift_ot_KeyPress(object sender, KeyPressEventArgs e)
        {

             char number = e.KeyChar;

            if (!Char.IsDigit(number))
            {
                e.Handled = true;
            }
        }
    }
}
