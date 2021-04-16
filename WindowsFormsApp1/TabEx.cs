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
            if (!string.IsNullOrEmpty(textbox.Text))
            {
                this.SelectedTab.Text = textbox.Text;

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
                        tabNameNode.InnerText = textbox.Text.Substring(0, 12);
                    }
                }
                xmlDoc.Save(@"config.xml");
            }
            placeHolder.Close();
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
