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
    public class PanelEx : Panel
    {
        string path = "";
        string name = "";
        PictureBox box = new PictureBox();

        public PanelEx()
        {
            AllowDrop = true;
            box.Parent = this;
            box.Dock = DockStyle.Fill;
            box.Click += Box_Click;
            box.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(box);
        }

        public void LoadPath(string appPath, string appName)
        {
            path = appPath;
            name = appName;

            char[] separator = new char[] { '\\' };
            string[] arr = path.Split(separator);

            box.SizeMode = PictureBoxSizeMode.StretchImage;

            var curPath = System.IO.Directory.GetCurrentDirectory();
            if (arr[1] == "tool")
            {
                path = curPath + appPath;
            }
            if (File.Exists(path))
            {
                Icon appIcon = Icon.ExtractAssociatedIcon(path);
                box.Image = appIcon.ToBitmap();
            }
            else if (Directory.Exists(appPath))
            {
                box.Image = GetFolderImage();
            }
        }

        public void SetPath(string appPath, string appName)
        {
            path = appPath;            
            name = appName;
            char[] separator = new char[] { '\\' };
            string[] arr = path.Split(separator);

            box.SizeMode = PictureBoxSizeMode.StretchImage;

            var curPath = System.Windows.Forms.Application.StartupPath;
            if (arr[1] == "tool")
            {
                path = curPath + appPath;
                Console.WriteLine(path);
            }
            if (File.Exists(path))
            {
                Icon appIcon = Icon.ExtractAssociatedIcon(path);
                box.Image = appIcon.ToBitmap();
            }
            else if (Directory.Exists(appPath))
            {
                box.Image = GetFolderImage();
            }
        }

        private void Box_Click(object sender, EventArgs e)
        {
            char[] separator = new char[] { '\\' };
            string[] arr = path.Split(separator);

            try
            {
                // 実行前確認
                DialogResult result = MessageBox.Show(name + "を実行します。",
                                        "実行確認",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Exclamation,
                                        MessageBoxDefaultButton.Button2);
                // 選択したボタンを判定
                if (result == DialogResult.Yes)
                {
                    // はいを選択
                    Icon appIcon = Icon.ExtractAssociatedIcon(path);
                    System.Diagnostics.Process.Start(path);
                }
            }
            catch (Exception ex)
            {
                // 異常時（基本的に発生しない想定だが）
                // 例えば、パスにファイルが存在しないとか。
                MessageBox.Show("アプリケーションを実行できません。");

                // できればログに出力したいが、いったんはそのまま表示する（わら）
                MessageBox.Show(ex.Message);
            }
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                path = files[0];

                box.SizeMode = PictureBoxSizeMode.StretchImage;
                if (File.Exists(path))
                {
                    Icon appIcon = Icon.ExtractAssociatedIcon(path);
                    box.Image = appIcon.ToBitmap();
                }
                else if (Directory.Exists(path))
                {
                    box.Image = GetFolderImage();
                }
            }
        }

        Image GetFolderImage()
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hSuccess = SHGetFileInfo("", 0, ref shinfo, (uint)System.Runtime.InteropServices.Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);
            if (hSuccess != IntPtr.Zero)
            {
                Icon appIcon = Icon.FromHandle(shinfo.hIcon);
                return appIcon.ToBitmap();
            }
            return null;
        }

        // SHGetFileInfo関数
        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        // SHGetFileInfo関数で使用する構造体
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        // SHGetFileInfo関数で使用するフラグ
        private const uint SHGFI_ICON = 0x100; // アイコン・リソースの取得
        private const uint SHGFI_LARGEICON = 0x0; // 大きいアイコン
        private const uint SHGFI_SMALLICON = 0x1; // 小さいアイコン
    }
}
