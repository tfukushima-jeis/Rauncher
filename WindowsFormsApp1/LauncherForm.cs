using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
//using tabExControl;

namespace WindowsFormsApp1
{
    public partial class LauncherForm : Form
    {
        TabEx tabEx = new TabEx();
        public List<PanelEx> panels = new List<PanelEx>();
        ContextMenuStrip appContextMenuStrip; //アプリケーションのコンテキストメニュー
        Control contextMenuSourceControl = null;
        Control[] label = null;
        Control[] textBox = null;
        ContextMenuStrip tabContextMenuStrip = new ContextMenuStrip();
        ToolStripMenuItem menuItem = new ToolStripMenuItem();
        TabPage clickedTabPage = null;
        bool delTabFlg = false;

        int tabMax = 8; //タブの最大登録数
        int appMax = 30; //アプリケーションの最大登録数
        int appNameMax = 16; //アプリケーション名の最大文字数
        string newTabName = "New Tab"; //新しいタブの名前
        

        public LauncherForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(0, 0, 0);

            //タブコントロールの設定
            tabEx.Location = new Point(0, 0);
            tabEx.Size = new Size(606, 606);
            tabEx.DragDrop += new DragEventHandler(AppDragDrop);
            tabEx.MouseDoubleClick += new MouseEventHandler(tabControlEx_MouseDoubleClick);
            tabEx.Selecting += new TabControlCancelEventHandler(TabControl_Selecting);
            tabEx.MouseDown += new MouseEventHandler(tabControlEx_MouseDown);
            //tabEx.MouseUp += new MouseEventHandler(tabControlEx_MouseUp);
            tabEx.HandleCreated += tabEx_HandleCreated;

            //タブコントロール用コンテキストメニューの設定
            menuItem.Click += new EventHandler(TabMenuItem_Click);
            tabContextMenuStrip.Items.Add(menuItem);
            tabContextMenuStrip.Opening += new CancelEventHandler(TabContextMenu_Opening);
            tabContextMenuStrip.Closing += new ToolStripDropDownClosingEventHandler(TabContextMenu_Closing);
            tabEx.ContextMenuStrip = tabContextMenuStrip;

            //アプリケーション用コンテキストメニュー設定
            ToolStripMenuItem menuItem1, menuItem2, menuItem3;
            menuItem1 = new ToolStripMenuItem();
            menuItem1.Text = "削除";
            menuItem2 = new ToolStripMenuItem();
            menuItem2.Text = "名前の変更";
            menuItem3 = new ToolStripMenuItem();
            menuItem3.Text = "アイコンの変更";
            appContextMenuStrip = new ContextMenuStrip();
            appContextMenuStrip.Items.Add(menuItem1);
            appContextMenuStrip.Items.Add(menuItem2);
            //contextMenuStrip.Items.Add(menuItem3);
            appContextMenuStrip.Opening += new CancelEventHandler(AppContextMenu_Opening);
            menuItem1.Click += new EventHandler(AppMenuItem1_Click);
            menuItem2.Click += new EventHandler(AppMenuItem2_Click);
            //menuItem3.Click += new EventHandler(AppMenuItem3_Click);

            //タブ情報の読み込みと表示
            Launcher_Load();
        }

        public void Launcher_Load()
        {
            //Console.WriteLine("Load_Start");
            //config.xmlが存在しない場合新規作成
            string filePath = @"config.xml";
            if (!File.Exists(filePath))
            {
                CreateConfigXML();
            }

            //toolフォルダーが存在しない場合新規作成
            string folderPath = @"tool";
            if (!Directory.Exists(folderPath))
            {
                CreateToolFolder();
            }

            try
            {
                //tabタグをid順にソート
                XDocument doc = XDocument.Load(@"config.xml");
                var baseElement = doc.XPathSelectElement("tabs");
                var sortedElements = baseElement.Elements()
                    .OrderBy(e => (int)e.Attribute("id"))
                    .ToList();
                baseElement.ReplaceAll(sortedElements);

                //appタグをid順にソート
                var baseElements = doc.XPathSelectElements("tabs/tab");
                foreach (XElement el in baseElements)
                {
                    var sortedElement = el.Elements()
                        .OrderBy(e => (int)e.Attribute("id"))
                        .ToList();
                    el.ReplaceNodes(sortedElement);
                }
                doc.Save(@"config.xml");

                //タブ情報ファイルの読み込み
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(@"config.xml");
                var tabs = xmlDoc.SelectNodes("tabs/tab"); //tabタグ情報

                //tabタグのidをソート順に再割り振り
                for (var i = 0; i < tabs.Count; i++)
                {
                    tabs[i].Attributes[0].Value = i.ToString();
                    var apps = xmlDoc.SelectNodes("tabs/tab[@id='" + i + "']/app"); //appタグ情報

                    //appタグのidをソート順に再割り振り
                    for (var j = 0; j < apps.Count; j++)
                    {
                        apps[j].Attributes[0].Value = j.ToString();
                    }
                }
                xmlDoc.Save(@"config.xml");

                //tabタグの数分タブコントロールにタブページ追加
                for (var i = 0; i < tabs.Count; i++)
                {
                    var tabname = tabs[i].SelectSingleNode("tabname").InnerText; //tabタグの名前
                    var apps = xmlDoc.SelectNodes("tabs/tab[@id='" + i + "']/app"); //appタグ情報

                    //タブページの設定
                    TabPage tabPage = new TabPage();
                    tabPage.Name = i.ToString();
                    tabPage.Text = tabname;

                    //タブコントロールにタブページ追加
                    tabEx.TabPages.Add(tabPage);

                    //appタグの数分タブページにアプリケーション表示用パネルとラベル追加
                    for (var j = 0; j < apps.Count; j++)
                    {
                        var appPath = apps[j].SelectSingleNode("path").InnerText; //アプリケーションのパス
                        var appName = apps[j].SelectSingleNode("name").InnerText; //アプリケーションの名前

                        //アプリケーション表示用パネルの設定
                        PanelEx panelEx = new PanelEx();
                        panelEx.Location = new Point(25 + (j % 6) * 100, 25 + (j / 6) * 100);
                        panelEx.Size = new Size(50, 50);
                        panelEx.Name = i.ToString() + "_p_" + j.ToString();
                        panelEx.LoadPath(appPath);
                        panelEx.ContextMenuStrip = appContextMenuStrip;

                        //アプリケーションのラベル設定
                        Label label = new Label();
                        label.Name = i.ToString() + "_l_" + j.ToString();
                        if (appName.Length > appNameMax)
                        {
                            label.Text = appName.Substring(0, appNameMax);
                        }
                        else
                        {
                            label.Text = appName;
                        }
                        label.TextAlign = ContentAlignment.MiddleCenter;
                        label.AutoSize = false;
                        label.Size = new Size(85, 25);
                        label.Location = new Point(8 + (j % 6) * 100, 78 + (j / 6) * 100);

                        //アプリケーションのテキストボックス設定
                        TextBox textBox = new TextBox();
                        textBox.Location = new Point(8 + (j % 6) * 100, 78 + (j / 6) * 100);
                        textBox.Size = new Size(85, 50);
                        textBox.Name = i.ToString() + "_t_" + j.ToString();
                        if (appName.Length > appNameMax)
                        {
                            textBox.Text = appName.Substring(0, appNameMax);
                        }
                        else
                        {
                            textBox.Text = appName;
                        }
                        textBox.Visible = false;
                        textBox.KeyPress += new KeyPressEventHandler(textBox_KeyPress);
                        //textBox.Leave += Leave_TextBox;

                        tabPage.Controls.Add(label);
                        tabPage.Controls.Add(textBox);
                        tabPage.Controls.Add(panelEx);

                        if (j > appMax)
                        {
                            break;
                        }
                    }
                    if (i > tabMax - 1)
                    {
                        break;
                    }
                }
                Controls.Add(tabEx);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                //FileNotFoundExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルが見つかりませんでした。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルが見つかりませんでした。");
            }
            catch (System.IO.IOException ex)
            {
                //IOExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルがロックされている可能性があります。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルがロックされている可能性があります。");
            }
            catch (System.UnauthorizedAccessException ex)
            {
                //UnauthorizedAccessExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルのアクセス許可がありません。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルのアクセス許可がありません。");
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;
        private void tabEx_HandleCreated(object sender, EventArgs e)
        {
            SendMessage(this.tabEx.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr)16);
        }

        //config.xmlの新規作成
        public void CreateConfigXML()
        {
            XmlWriter writer = XmlWriter.Create(@"config.xml");

            writer.WriteStartElement("tabs");

            writer.WriteStartElement("tab");
            writer.WriteAttributeString("id", "0");
            writer.WriteElementString("tabname", newTabName);
            writer.WriteEndElement();

            writer.WriteStartElement("tab");
            writer.WriteAttributeString("id", "1");
            writer.WriteElementString("tabname", "+");
            writer.WriteEndElement();

            writer.WriteEndElement();

            writer.Close();
        }

        //toolフォルダの新規作成
        public void CreateToolFolder()
        {
            string folderPath = @"tool";
            DirectoryInfo di = new DirectoryInfo(folderPath);
            di.Create();
        }

        //タブコントロールクリック時にクリック座標がタブ部分を含んでいるか判定
        private void tabControlEx_MouseDown(object sender, MouseEventArgs e)
        {
            Console.WriteLine("tabControlEx_MouseDown");
            for (int i = 0; i < tabEx.TabCount; i++)
            {
                if (tabEx.GetTabRect(i).Contains(e.X, e.Y))
                {
                    clickedTabPage = (TabPage)tabEx.GetControl(i);
                    Console.WriteLine(clickedTabPage.Name);
                }
            }
        }

        //タブコントロールクリック時に右ボタンが離れた場合のみコンテキストメニュー表示
        /*private void tabControlEx_MouseUp(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("tabControlEx_MouseUp");
            if (clickedTabPage != null)
            {
                if (e.Button == MouseButtons.Right)
                {
                    //tabContextMenuStrip.Show((TabControl)sender, e.Location);
                }
            }
        }*/

        //タブコントロールのタブ部分ダブルクリック時に名前の編集
        private void tabControlEx_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("tabControlEx_MouseDoubleClick");
            TabEx tabEx = sender as TabEx;
            tabEx.EditTabText(tabEx.SelectedTab.Name);
        }

        //タブコントロールの末尾のタブ（＋ボタン）押下時に新しいタブ追加
        private void TabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {

            Console.WriteLine("AddNewTab_Start");
            try
            {
                if (!delTabFlg)
                {
                    if (e.TabPage != null)
                    {
                        Console.WriteLine(e.TabPage.Name);
                        Console.WriteLine((tabEx.TabCount - 1).ToString());
                        if (e.TabPageIndex == tabEx.TabCount - 1)
                        {
                            e.Cancel = true;
                            if (tabEx.TabCount - 1 < tabMax)
                            {
                                //Console.WriteLine("AddNewTab_Start");
                                TabPage newTabPage = new TabPage();
                                newTabPage.Name = (tabEx.TabCount - 1).ToString();
                                newTabPage.Text = newTabName;

                                tabEx.TabPages.Insert(tabEx.TabCount - 1, newTabPage);
                                tabEx.SelectedIndex = tabEx.TabCount - 1;

                                //タブ情報ファイルの読み込み
                                var xmlDoc = new XmlDocument();
                                xmlDoc.Load(@"config.xml");
                                var tabNodes = xmlDoc.SelectNodes("tabs/tab"); //tabタグ情報

                                //タブ情報ファイルの更新
                                XmlNode plustabNode = xmlDoc.SelectSingleNode("tabs/tab[@id='" + (tabEx.TabCount - 2).ToString() + "']/tabname");
                                plustabNode.InnerText = newTabName;

                                XmlNode rootNode = xmlDoc.SelectSingleNode("tabs");
                                XmlNode newTab = xmlDoc.CreateNode(XmlNodeType.Element, "tab", null);
                                ((XmlElement)newTab).SetAttribute("id", (tabEx.TabCount - 1).ToString());
                                XmlElement tabnameElement = xmlDoc.CreateElement("tabname");
                                XmlText tabnameText = xmlDoc.CreateTextNode("+");
                                tabnameElement.AppendChild(tabnameText); ;
                                newTab.AppendChild(tabnameElement);
                                rootNode.AppendChild(newTab);
                                xmlDoc.Save(@"config.xml");
                            }
                            else
                            {
                                MessageBox.Show("タブをこれ以上登録できません。");
                            }
                        }
                    }
                }
                delTabFlg = false;
            }
            catch (System.IO.FileNotFoundException ex)
            {
                //FileNotFoundExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルが見つかりませんでした。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルが見つかりませんでした。");
            }
            catch (System.IO.IOException ex)
            {
                //IOExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルがロックされている可能性があります。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルがロックされている可能性があります。");
            }
            catch (System.UnauthorizedAccessException ex)
            {
                //UnauthorizedAccessExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルのアクセス許可がありません。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルのアクセス許可がありません。");
            }
        }

        //タブコントロール用コンテキストメニュー表示時
        private void TabContextMenu_Opening(object sender, CancelEventArgs e)
        {
            Console.WriteLine("TabContextMenu_Open");
            ContextMenuStrip menu = sender as ContextMenuStrip;
            contextMenuSourceControl = menu.SourceControl;
            //clickedTabPage = (TabEx)contextMenuSourceControl

            //タブが押下された場合
            if (clickedTabPage != null)
            {
                Console.WriteLine(clickedTabPage.Name);
                Console.WriteLine((tabEx.TabCount - 1).ToString());
                //+ボタン以外が押されたとき
                if (!(clickedTabPage.Name == (tabEx.TabCount - 1).ToString()))
                {
                    menuItem.Text = "削除";
                    menu.Items.Clear();
                    menu.Items.Add(menuItem);
                    delTabFlg = true;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = true;
            }            
        }

        //タブコントロール用コンテキストメニューが閉じるとき
        private void TabContextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            Console.WriteLine("TabContextMenu_Close");
            //clickedTabPage = null;
        }        

        //アプリケーション用コンテキストメニュー表示時、対象アプリケーション情報の退避
        private void AppContextMenu_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip menu = sender as ContextMenuStrip;
            contextMenuSourceControl = menu.SourceControl;

            char[] separator = new char[] { '_' };
            string[] arr = contextMenuSourceControl.Name.Split(separator);

            var lName = arr[0] + "_l_" + arr[2];
            var tName = arr[0] + "_t_" + arr[2];

            label = Controls.Find(lName, true);
            textBox = Controls.Find(tName, true);
        }

        //タブコントロールのコンテキストメニューアイテム（削除）
        public void TabMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("DeleteTab_Start");
            Control source = contextMenuSourceControl;
            bool deleteflg = false;

            //ダイアログの表示
            DialogResult result = MessageBox.Show("「" + clickedTabPage.Text + "」タブを削除しますか？",
                "確認",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            try
            {
                if (result == DialogResult.OK)
                {
                    //タブ情報ファイルの読み込み
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(@"config.xml");
                    var tabNodes = xmlDoc.SelectNodes("tabs/tab");

                    Console.WriteLine(tabNodes.Count);
                    for (var i = 0; i < tabNodes.Count; i++)
                    {
                        var tabid = ((XmlElement)tabNodes[i]).GetAttribute("id");
                        XmlNode tabNode = xmlDoc.SelectSingleNode("tabs/tab[@id='" + i + "']");

                        if (tabid == clickedTabPage.Name)
                        {
                            /*if (tabEx.TabCount < 2)
                            {

                            }*/
                            tabNode.ParentNode.RemoveChild(tabNode);
                            deleteflg = true;
                        }
                        Console.WriteLine(tabEx.TabCount);
                        if (tabEx.TabCount < 3)
                        {
                            Console.WriteLine(tabEx.TabCount - 1);
                            TabPage newTabPage = new TabPage();
                            newTabPage.Name = (tabEx.TabCount - 1).ToString();
                            newTabPage.Text = newTabName;

                            tabEx.TabPages.Insert(tabEx.TabCount - 1, newTabPage);
                            tabEx.SelectedIndex = tabEx.TabCount - 1;

                            //タブ情報ファイルの更新
                            XmlNode plustabNode = xmlDoc.SelectSingleNode("tabs/tab[@id='" + (tabEx.TabCount - 2).ToString() + "']/tabname");
                            plustabNode.InnerText = newTabName;

                            XmlNode rootNode = xmlDoc.SelectSingleNode("tabs");
                            XmlNode newTab = xmlDoc.CreateNode(XmlNodeType.Element, "tab", null);
                            ((XmlElement)newTab).SetAttribute("id", (tabEx.TabCount - 2).ToString());
                            XmlElement tabnameElement = xmlDoc.CreateElement("tabname");
                            XmlText tabnameText = xmlDoc.CreateTextNode("+");
                            tabnameElement.AppendChild(tabnameText); ;
                            newTab.AppendChild(tabnameElement);
                            rootNode.AppendChild(newTab);
                        }

                        if (deleteflg)
                        {
                            ((XmlElement)tabNode).SetAttribute("id", (i - 1).ToString());
                        }
                    }
                    xmlDoc.Save(@"config.xml");

                    if (deleteflg)
                    {
                        //tabEx.Controls.Remove(tabEx.Selecting);
                        tabEx.Controls.Clear();
                        Launcher_Load();
                    }
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                //FileNotFoundExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルが見つかりませんでした。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルが見つかりませんでした。");
            }
            catch (System.IO.IOException ex)
            {
                //IOExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルがロックされている可能性があります。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルがロックされている可能性があります。");
            }
            catch (System.UnauthorizedAccessException ex)
            {
                //UnauthorizedAccessExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルのアクセス許可がありません。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルのアクセス許可がありません。");
            }
        }

        //アプリケーションのコンテキストメニューアイテム１（削除）
        public void AppMenuItem1_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("DeleteApp_Start");
            Control source = contextMenuSourceControl;//menu.SourceControl;
            bool deleteflg = false;

            //ダイアログの表示
            DialogResult result = MessageBox.Show("「" + ((Label)label[0]).Text + "」を削除しますか？",
                "確認",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            try
            {
                if (result == DialogResult.OK)
                {
                    //タブ情報ファイルの読み込み
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(@"config.xml");
                    var tabNodes = xmlDoc.SelectNodes("tabs/tab");

                    for (var i = 0; i < tabNodes.Count; i++)
                    {
                        var tabid = ((XmlElement)tabNodes[i]).GetAttribute("id");

                        if (tabid == source.Parent.Name)
                        {
                            var appNodes = xmlDoc.SelectNodes("tabs/tab[@id='" + i + "']/app");
                            var appId = appNodes.Count;

                            for (var j = 0; j < appNodes.Count; j++)
                            {
                                XmlNode appNode = xmlDoc.SelectSingleNode("tabs/tab[@id='" + i + "']/app[@id='" + j + "']");
                                if (source.Name == i.ToString() + "_p_" + j.ToString())
                                {
                                    appNode.ParentNode.RemoveChild(appNode);
                                    deleteflg = true;
                                }

                                if (deleteflg)
                                {
                                    ((XmlElement)appNode).SetAttribute("id", (j - 1).ToString());
                                }
                            }
                        }

                    }
                    xmlDoc.Save(@"config.xml");

                    if (deleteflg)
                    {
                        tabEx.Controls.Clear();
                        Launcher_Load();
                    }
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                //FileNotFoundExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルが見つかりませんでした。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルが見つかりませんでした。");
            }
            catch (System.IO.IOException ex)
            {
                //IOExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルがロックされている可能性があります。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルがロックされている可能性があります。");
            }
            catch (System.UnauthorizedAccessException ex)
            {
                //UnauthorizedAccessExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルのアクセス許可がありません。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルのアクセス許可がありません。");
            }
        }

        //アプリケーションのコンテキストメニューアイテム２（名前の変更）
        public void AppMenuItem2_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("ChangeAppName_Start");

            ((Label)label[0]).Visible = false;
            ((TextBox)textBox[0]).Visible = true;
            ((TextBox)textBox[0]).Focus();
        }

        /*private void Leave_TextBox(object sender, EventArgs e)
        {
            
            Control source = contextMenuSourceControl;
            if (!string.IsNullOrEmpty(((TextBox)textBox[0]).Text))
            {
                Console.WriteLine(((TextBox)textBox[0]).Text);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(@"test.xml");
                var tabNodes = xmlDoc.SelectNodes("tabs/tab");

                for (var i = 0; i < tabNodes.Count; i++)
                {
                    var tabid = ((XmlElement)tabNodes[i]).GetAttribute("id");

                    if (tabid == source.Parent.Name)
                    {
                        var appNodes = xmlDoc.SelectNodes("tabs/tab[@id='" + i + "']/app");
                        var appId = appNodes.Count;

                        for (var j = 0; j < appNodes.Count; j++)
                        {
                            XmlNode appNode = xmlDoc.SelectSingleNode("tabs/tab[@id='" + i + "']/app[@id='" + j + "']/name");

                            if (source.Name == i.ToString() + "_p_" + j.ToString())
                            {
                                appNode.InnerText = ((TextBox)textBox[0]).Text;
                            }
                        }
                    }
                }
                xmlDoc.Save(@"test.xml");

                ((TextBox)textBox[0]).Visible = false;
                ((Label)label[0]).Text = ((TextBox)textBox[0]).Text;
                ((Label)label[0]).Visible = true;
            }
        }*/

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                Control source = contextMenuSourceControl;
                if (e.KeyChar == (Char)Keys.Enter)
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(@"config.xml");
                    var tabNodes = xmlDoc.SelectNodes("tabs/tab");

                    for (var i = 0; i < tabNodes.Count; i++)
                    {
                        var tabid = ((XmlElement)tabNodes[i]).GetAttribute("id");

                        if (tabid == source.Parent.Name)
                        {
                            var appNodes = xmlDoc.SelectNodes("tabs/tab[@id='" + i + "']/app");
                            var appId = appNodes.Count;

                            for (var j = 0; j < appNodes.Count; j++)
                            {
                                XmlNode appNode = xmlDoc.SelectSingleNode("tabs/tab[@id='" + i + "']/app[@id='" + j + "']/name");

                                if (source.Name == i.ToString() + "_p_" + j.ToString())
                                {
                                    appNode.InnerText = ((TextBox)textBox[0]).Text;
                                }
                            }
                        }
                    }
                    xmlDoc.Save(@"config.xml");

                    ((TextBox)textBox[0]).Visible = false;
                    ((Label)label[0]).Text = ((TextBox)textBox[0]).Text;
                    ((Label)label[0]).Visible = true;
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                //FileNotFoundExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルが見つかりませんでした。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルが見つかりませんでした。");
            }
            catch (System.IO.IOException ex)
            {
                //IOExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルがロックされている可能性があります。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルがロックされている可能性があります。");
            }
            catch (System.UnauthorizedAccessException ex)
            {
                //UnauthorizedAccessExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルのアクセス許可がありません。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルのアクセス許可がありません。");
            }
        }

        //アプリケーションのドラッグアンドドロップ
        public void AppDragDrop(object sender, DragEventArgs e)
        {
            Console.WriteLine("AppDragDrop_Start");
            try
            {
                TabEx tabEx = sender as TabEx;

                //ファイルのドラッグアンドドロップ時
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    //ファイルの絶対パスと名前の取得
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    var appPath = files[0];
                    char[] separator = new char[] { '\\' };
                    string[] arr = appPath.Split(separator);

                    string appName;
                    if (arr[arr.Length - 1].Length > appNameMax)
                    {
                        appName = arr[arr.Length - 1].Substring(0, appNameMax);
                    }
                    else
                    {
                        appName = arr[arr.Length - 1];
                    }

                    //タブ情報ファイルの読み込み
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(@"config.xml");
                    var tabNodes = xmlDoc.SelectNodes("tabs/tab");

                    //タブ情報ファイルの更新とアプリケーション表示用パネルの設置
                    for (var i = 0; i < tabNodes.Count; i++)
                    {
                        var tabId = ((XmlElement)tabNodes[i]).GetAttribute("id"); //tabタグのid

                        //現在選択中のタブとtabタグのidが一致した場合
                        if (tabEx.SelectedTab.Name == tabId)
                        {
                            var appNodes = xmlDoc.SelectNodes("tabs/tab[@id='" + i + "']/app"); //appタグ情報
                            var appId = appNodes.Count;

                            //登録されているアプリケーションが最大値未満の場合
                            if (appId < appMax)
                            {
                                //タブ情報ファイルの更新
                                XmlNode rootNode = xmlDoc.SelectSingleNode("tabs/tab[@id='" + i + "']");
                                XmlNode newAppNode = xmlDoc.CreateNode(XmlNodeType.Element, "app", null);
                                ((XmlElement)newAppNode).SetAttribute("id", appId.ToString());
                                XmlElement pathElement = xmlDoc.CreateElement("path");
                                XmlElement nameElement = xmlDoc.CreateElement("name");
                                XmlText pathText = xmlDoc.CreateTextNode(appPath);
                                //相対パスの登録
                                var absPath = System.IO.Directory.GetCurrentDirectory();
                                if (appPath.Contains(absPath))
                                {
                                    pathText = xmlDoc.CreateTextNode("\\tool\\" + arr[arr.Length - 1]);
                                }
                                XmlText nameText = xmlDoc.CreateTextNode(appName);
                                pathElement.AppendChild(pathText);
                                nameElement.AppendChild(nameText);
                                newAppNode.AppendChild(pathElement);
                                newAppNode.AppendChild(nameElement);
                                rootNode.AppendChild(newAppNode);
                                xmlDoc.Save(@"config.xml");

                                //アプリケーション表示用パネルの設置
                                PanelEx panelEx = new PanelEx();
                                panelEx.Location = new Point(25 + (appId % 6) * 100, 25 + (appId / 6) * 100);
                                panelEx.Size = new Size(50, 50);
                                panelEx.Name = i.ToString() + "_p_" + appId.ToString();
                                panelEx.SetPath(appPath);
                                panelEx.ContextMenuStrip = appContextMenuStrip;

                                //アプリケーションのラベル設定
                                Label label = new Label();
                                label.Location = new Point(8 + (appId % 6) * 100, 78 + (appId / 6) * 100);
                                label.Size = new Size(85, 25);
                                label.Name = i.ToString() + "_l_" + appId.ToString();
                                if (appName.Length > appNameMax)
                                {
                                    label.Text = appName.Substring(0, appNameMax);
                                }
                                else
                                {
                                    label.Text = appName;
                                }
                                label.TextAlign = ContentAlignment.MiddleCenter;
                                label.AutoSize = false;

                                //アプリケーションのテキストボックス設定
                                TextBox textBox = new TextBox();
                                textBox.Location = new Point(8 + (appId % 6) * 100, 78 + (appId / 6) * 100);
                                textBox.Size = new Size(85, 50);
                                textBox.Name = i.ToString() + "_t_" + appId.ToString();
                                if (appName.Length > appNameMax)
                                {
                                    textBox.Text = appName.Substring(0, appNameMax);
                                }
                                else
                                {
                                    textBox.Text = appName;
                                }
                                textBox.Visible = false;
                                textBox.KeyPress += new KeyPressEventHandler(textBox_KeyPress);

                                tabEx.SelectedTab.Controls.Add(panelEx);
                                tabEx.SelectedTab.Controls.Add(label);
                                tabEx.SelectedTab.Controls.Add(textBox);
                            }
                            else
                            {
                                MessageBox.Show("アプリケーションをこれ以上登録できません。");
                            }
                        }
                    }
                }
            }
            catch (System.IO.FileNotFoundException ex)
            {
                //FileNotFoundExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルが見つかりませんでした。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルが見つかりませんでした。");
            }
            catch (System.IO.IOException ex)
            {
                //IOExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルがロックされている可能性があります。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルがロックされている可能性があります。");
            }
            catch (System.UnauthorizedAccessException ex)
            {
                //UnauthorizedAccessExceptionをキャッチした時
                System.Console.WriteLine("設定ファイルのアクセス許可がありません。");
                System.Console.WriteLine(ex.Message);
                MessageBox.Show("設定ファイルのアクセス許可がありません。");
            }
        }
    }
}
