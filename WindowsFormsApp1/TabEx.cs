using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace WindowsFormsApp1
{
    public class TabEx : TabControl
    {
        Form placeHolder = null;
        TextBox textbox = null;
        string tabName = null;
        int tabNameMax = 12; //タブ名の最大文字数

        public TabEx()
        {            
            AllowDrop = true;
            SelectedIndex = 0;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        public void EditTabText(string name)
        {
            tabName = name;
            placeHolder = new Form();
            placeHolder.SuspendLayout();

            textbox = new TextBox();
            textbox.BackColor = SystemColors.InactiveCaption;
            textbox.BorderStyle = BorderStyle.None;
            textbox.TextAlign = HorizontalAlignment.Center;
            textbox.KeyPress += Textbox_KeyPress;

            placeHolder.AutoScaleMode = AutoScaleMode.Font;
            placeHolder.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            placeHolder.Controls.Add(textbox);
            placeHolder.FormBorderStyle = FormBorderStyle.None;
            placeHolder.TopMost = true;
            placeHolder.Load += PlaceHolder_Load;
            placeHolder.Deactivate += PlaceHolder_Deactivate;

            placeHolder.ResumeLayout(false);
            placeHolder.PerformLayout();

            placeHolder.Show();
        }

        private void PlaceHolder_Load(object sender, EventArgs e)
        {
            Rectangle tabRect = this.GetTabRect(this.SelectedIndex);
            tabRect.Location = this.FindForm().PointToScreen(tabRect.Location);
            placeHolder.DesktopBounds = tabRect;
            textbox.Bounds = new Rectangle(0, (tabRect.Height - textbox.Height) / 2, tabRect.Width, tabRect.Height);
            textbox.Text = this.SelectedTab.Text;
        }

        private void PlaceHolder_Deactivate(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textbox.Text))
                {
                    if (textbox.Text.Length > tabNameMax)
                    {
                        this.SelectedTab.Text = textbox.Text.Substring(0, tabNameMax);
                    }
                    else
                    {
                        this.SelectedTab.Text = textbox.Text;
                    }

                    //タブ情報ファイルの読み込み
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(@"config.xml");
                    var tabNodes = xmlDoc.SelectNodes("tabs/tab"); //tabタグ情報

                    for (var i = 0; i < tabNodes.Count; i++)
                    {
                        var tabid = ((XmlElement)tabNodes[i]).GetAttribute("id");

                        if (tabid == tabName)
                        {
                            XmlNode tabNameNode = xmlDoc.SelectSingleNode("tabs/tab[@id='" + i + "']/tabname");
                            if (textbox.Text.Length > tabNameMax)
                            {
                                tabNameNode.InnerText = textbox.Text.Substring(0, tabNameMax);
                            }
                            else
                            {
                                tabNameNode.InnerText = textbox.Text;
                            }
                        }
                    }
                    xmlDoc.Save(@"config.xml");
                }
                placeHolder.Close();
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

        private void Textbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch ((Keys)e.KeyChar)
            {
                case Keys.Escape:
                    textbox.Text = string.Empty;
                    goto case Keys.Enter;

                case Keys.Enter:
                    e.Handled = true;
                    placeHolder.Hide();
                    return;
            }
        }
    }
}
