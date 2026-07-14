using System.Linq;
using System.Windows.Forms;

namespace vnebo.mobi.bot.Libs
{
    internal class FindControl
    {
        public static TextBox FindTextBox(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as TextBox;
            }

            return new TextBox();
        }
        public static WebBrowser FindWebBrouser(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as WebBrowser;
            }

            return new WebBrowser();
        }

        public static Label FindLabel(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as Label;
            }

            return new Label();
        }

        public static NumericUpDown FindNumericUpDown(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as NumericUpDown;
            }

            return new NumericUpDown();
        }

        public static ToolStrip FindToolStrip(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as ToolStrip;
            }

            return new ToolStrip();
        }

        public static Button FindButton(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as Button;
            }

            return new Button();
        }

        public static RichTextBox FindRichTextBox(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as RichTextBox;
            }

            return new RichTextBox();
        }

        public static TabPage FindTabPage(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as TabPage;
            }

            return new TabPage();
        }

        public static CheckBox FindCheckBox(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as CheckBox;
            }

            return new CheckBox();
        }
        public static ComboBox FindComboBox(string name, int id, MainFormAll form)
        {
            Control[] controls = form.Controls.Find(name + id.ToString(), true);

            if (controls.Any())
            {
                return controls.First() as ComboBox;
            }

            return new ComboBox();
        }
    }
}
