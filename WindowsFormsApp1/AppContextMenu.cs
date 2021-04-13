using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace WindowsFormsApp1
{
    public class AppContextMenuEx : ContextMenuStrip
    {
        ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
        public AppContextMenuEx()
        {
            ToolStripMenuItem menuItem1, menuItem2, menuItem3;
            menuItem1 = new ToolStripMenuItem();
            menuItem1.Text = "削除";
            menuItem2 = new ToolStripMenuItem();
            menuItem2.Text = "名前の変更";
            menuItem3 = new ToolStripMenuItem();
            menuItem3.Text = "アイコンの変更";

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add(menuItem1);
            contextMenuStrip.Items.Add(menuItem2);
            contextMenuStrip.Items.Add(menuItem3);

            this.Controls.Add(contextMenuStrip);
        }        
    }
}
