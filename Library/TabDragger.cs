using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace vnebo.mobi.bot.Libs
{
    internal class TabDragger
    {
        public TabDragger(TabControl tabControl)
            : base()
        {
            this.tabControl = tabControl;
            tabControl.MouseDown += new MouseEventHandler(tabControl_MouseDown);
            tabControl.MouseMove += new MouseEventHandler(tabControl_MouseMove);
        }

        public TabDragger(TabControl tabControl, TabDragBehavior behavior)
            : this(tabControl)
        {
            this.dragBehavior = behavior;
        }

        private TabControl tabControl;
        private TabPage dragTab = null;
        private TabDragBehavior dragBehavior = TabDragBehavior.TabDragArrange;

        private TabDragBehavior DragBehavior
        {
            get
            {
                //if (!tabControl.Multiline)
                    return dragBehavior;
                //return TabDragBehavior.None;
            }
        }

        private void tabControl_MouseDown(object sender, MouseEventArgs e)
        {
            dragTab = TabUnderMouse();
        }

        private void tabControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (DragBehavior == TabDragBehavior.None)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (dragTab != null)
                {
                    if (tabControl.TabPages.Contains(dragTab))
                    {
                        if (PointInTabStrip(e.Location))
                        {
                            TabPage hotTab = TabUnderMouse();
                            if (hotTab != dragTab && hotTab != null)
                            {
                                int id1 = tabControl.TabPages.IndexOf(dragTab);
                                int id2 = tabControl.TabPages.IndexOf(hotTab);
                                if (id1 > id2)
                                {
                                    for (int id = id2; id <= id1; id++)
                                    {
                                        tabControl.Cursor = Cursors.PanWest;
                                        SwapTabPages(id1, id);
                                    }
                                }
                                else
                                {
                                    for (int id = id2; id > id1; id--)
                                    {
                                        tabControl.Cursor = Cursors.PanEast;
                                        SwapTabPages(id1, id);
                                    }
                                }
                                tabControl.Cursor = Cursors.Default;
                                tabControl.SelectedTab = dragTab;
                            }
                        }

                    }
                }
            }
        }

 

        #region Private Methods

        private TabPage TabUnderMouse()
        {
            NativeMethods.TCHITTESTINFO HTI = new NativeMethods.TCHITTESTINFO(tabControl.PointToClient(Cursor.Position));
            int tabID = NativeMethods.SendMessage(tabControl.Handle, NativeMethods.TCM_HITTEST, IntPtr.Zero, ref HTI);
            return tabID == -1 ? null : tabControl.TabPages[tabID];
        }

        private bool PointInTabStrip(Point point)
        {
            Rectangle tabBounds = Rectangle.Empty;
            Rectangle displayRC = tabControl.DisplayRectangle; ;

            switch (tabControl.Alignment)
            {
                case TabAlignment.Bottom:
                    tabBounds.Location = new Point(0, displayRC.Bottom);
                    tabBounds.Size = new Size(tabControl.Width, tabControl.Height - displayRC.Height);
                    break;

                case TabAlignment.Left:
                    tabBounds.Size = new Size(displayRC.Left, tabControl.Height);
                    break;

                case TabAlignment.Right:
                    tabBounds.Location = new Point(displayRC.Right, 0);
                    tabBounds.Size = new Size(tabControl.Width - displayRC.Width, tabControl.Height);
                    break;

                default:
                    tabBounds.Size = new Size(tabControl.Width, displayRC.Top);
                    break;
            }
            tabBounds.Inflate(-3, -3);
            return tabBounds.Contains(point);
        }

        private void SwapTabPages(int index1, int index2)
        {
            int count = tabControl.TabPages.Count;
            if (((index1 | index2) != -1)&& ((index1 | index2) != count))
            {
                TabPage tab1 = tabControl.TabPages[index1];
                TabPage tab2 = tabControl.TabPages[index2];
                tabControl.TabPages[index1] = tab2;
                tabControl.TabPages[index2] = tab1;
                SwapSections(tab1.Tag.ToString(), tab2.Tag.ToString());
            }
        }

        public void SwapSections(string id1, string id2)
        {
            if (id1 == id2) return;
            string Path = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "settings.ini").FullName.ToString();
            string read_file = File.ReadAllText(Path);
            string section1 = $"[USER_{id1}]";
            string section2 = $"[USER_{id2}]";
            string tmp = "TMP";
            read_file = read_file.Replace(section1, tmp);
            read_file = read_file.Replace(section2, section1);
            read_file = read_file.Replace(tmp, section2);
            //Console.WriteLine($"repl {section1}=>{tmp}/n {section2}=>{section1} /n {tmp}=>{section2}");
            try
            {
                TextWriter tw = new StreamWriter(Path);
                tw.Write(read_file);
                tw.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ex swap sec = " + ex);
                throw ex;
            }
            
        }
        #endregion

    }
}
