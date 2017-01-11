using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Collections;

namespace lab_work_2_1
{
    public partial class Form1 : Form
    {
        string pathListViewLeft;
        string pathListViewRight;
        ToolStripTextBox toolStripTextBoxLeft;
        ToolStripTextBox toolStripTextBoxRight;

        public Form1()
        {
            InitializeComponent();

            string[] Drives = Environment.GetLogicalDrives();
            
            foreach (string drive in Drives)
            {
                toolStripLeft.Items.Add(new ToolStripButton(drive, null, toolStripButtonDrives_Click) { Image = Properties.Resources.drive_harddisk });
                toolStripLeft.Items.Add(new ToolStripSeparator());
                toolStripRight.Items.Add(new ToolStripButton(drive, null, toolStripButtonDrives_Click) { Image = Properties.Resources.drive_harddisk });
                toolStripRight.Items.Add(new ToolStripSeparator());
            }

            toolStripTextBoxLeft = new ToolStripTextBox("toolStripTextBoxLeft");
            toolStripTextBoxRight = new ToolStripTextBox("toolStripTextBoxRight");
            toolStripTextBoxLeft.KeyDown += toolStripTextBox_KeyDown;
            toolStripTextBoxRight.KeyDown += toolStripTextBox_KeyDown;

            toolStripLeft.Items.Add(toolStripTextBoxLeft);
            toolStripRight.Items.Add(toolStripTextBoxRight);

            toolStripTextBoxLeft.Text = toolStripTextBoxRight.Text = pathListViewLeft = pathListViewRight = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            ViewCatalog(listViewLeft, toolStripTextBoxLeft, ref pathListViewLeft);
            ViewCatalog(listViewRight, toolStripTextBoxRight, ref pathListViewRight);
        }

        private void ViewCatalog(ListView listViewRef, ToolStripTextBox textBoxRef, ref string path)
        {
            textBoxRef.Text = path;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                listViewRef.Items.Clear();

                if (path.Length > 3)
                {
                    ListViewItem itemRes = new ListViewItem(new[] { "...", "", "", "" }, 2);
                    listViewRef.Items.Add(itemRes);
                }

                foreach (var item in dir.GetDirectories())
                {
                    ListViewItem itemRes = new ListViewItem(new[] { item.Name, "Папка", "-", item.LastWriteTime.ToString() }, 0);
                    listViewRef.Items.Add(itemRes);
                }

                foreach (var item in dir.GetFiles())
                {
                    ListViewItem itemRes = new ListViewItem(new[] { item.Name, Path.GetExtension(item.FullName), item.Length.ToString(), item.LastWriteTime.ToString() }, 1);
                    listViewRef.Items.Add(itemRes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripButtonDrives_Click(object sender, EventArgs e)
        {
            ToolStripButton tmp = (ToolStripButton)sender;

            if (tmp.Owner.Name == "toolStripLeft")
            {
                pathListViewLeft = tmp.Text;
                ViewCatalog(listViewLeft, toolStripTextBoxLeft, ref pathListViewLeft);
            }
            else if (tmp.Owner.Name == "toolStripRight")
            {
                pathListViewRight = tmp.Text;
                ViewCatalog(listViewRight, toolStripTextBoxRight, ref pathListViewRight);
            }
        }

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            ListView itemSel = (ListView)sender;

            if (itemSel.Name == "listViewLeft")
            {
                if (File.Exists(Path.Combine(pathListViewLeft, itemSel.SelectedItems[0].Text)))
                    Process.Start(Path.Combine(pathListViewLeft, itemSel.SelectedItems[0].Text));
                else if (itemSel.SelectedItems[0].Text == "...")
                {
                    DirectoryInfo dir = new DirectoryInfo(pathListViewLeft);
                    pathListViewLeft = dir.Parent.FullName;
                }
                else if (Directory.Exists(Path.Combine(pathListViewLeft, itemSel.SelectedItems[0].Text)))
                    pathListViewLeft = Path.Combine(pathListViewLeft, itemSel.SelectedItems[0].Text);

                ViewCatalog(listViewLeft, toolStripTextBoxLeft, ref pathListViewLeft);
            }
            else if (itemSel.Name == "listViewRight")
            {
                if (File.Exists(Path.Combine(pathListViewRight, itemSel.SelectedItems[0].Text)))
                    Process.Start(Path.Combine(pathListViewRight, itemSel.SelectedItems[0].Text));
                else if (itemSel.SelectedItems[0].Text == "...")
                {
                    DirectoryInfo dir = new DirectoryInfo(pathListViewRight);
                    pathListViewRight = dir.Parent.FullName;
                }
                else if (Directory.Exists(Path.Combine(pathListViewRight, itemSel.SelectedItems[0].Text)))
                    pathListViewRight = Path.Combine(pathListViewRight, itemSel.SelectedItems[0].Text);

                ViewCatalog(listViewRight, toolStripTextBoxRight, ref pathListViewRight);
            }
        }

        private void WindowResize(object sender, EventArgs e)
        {
            toolStripTextBoxLeft.Size = new Size(toolStripLeft.Width - toolStripTextBoxLeft.Control.Location.X - 2, 25);
            toolStripTextBoxRight.Size = new Size(toolStripRight.Width - toolStripTextBoxLeft.Control.Location.X - 2, 25);
        }

        private void listView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)) == true)
                e.Effect = DragDropEffects.Move;
        }

        private void listView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(((ListView)sender).SelectedItems, DragDropEffects.Move);
        }

        private void listView_DragDrop(object sender, DragEventArgs e)
        {
            var type = typeof(ListView.SelectedListViewItemCollection);

            if (e.Data.GetDataPresent(type))
            {
                ListView.SelectedListViewItemCollection items = (ListView.SelectedListViewItemCollection)e.Data.GetData(type);

                foreach (ListViewItem item in items)
                {
                    string path = "", wherePath = "";

                    if(item.ListView.Name == "listViewLeft")
                    {
                        path = pathListViewLeft;
                        wherePath = pathListViewRight;
                    }
                    else if (item.ListView.Name == "listViewRight")
                    {
                        path = pathListViewRight;
                        wherePath = pathListViewLeft;
                    }

                    path = Path.Combine(path, item.Text);
                    wherePath = Path.Combine(wherePath, item.Text);

                    if (File.Exists(path))
                        File.Move(path, wherePath);
                    else if (Directory.Exists(path))
                        Directory.Move(path, wherePath);

                    ViewCatalog(listViewLeft, toolStripTextBoxLeft, ref pathListViewLeft);
                    ViewCatalog(listViewRight, toolStripTextBoxRight, ref pathListViewRight);
                }
            }
        }

        private void переименоватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listViewLeft.SelectedItems.Count != 0)
                listViewLeft.SelectedItems[0].BeginEdit();
            else if (listViewRight.SelectedItems.Count != 0)
                listViewRight.SelectedItems[0].BeginEdit();
        }

        private void listView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                if (e.Label.Length > 0)
                {
                    ListViewItem item = ((ListView)sender).SelectedItems[0];
                    string path = "", toPath = "";

                    if (item.ListView.Name == "listViewLeft")
                        path = toPath = pathListViewLeft;
                    else if (item.ListView.Name == "listViewRight")
                        path = toPath = pathListViewRight;

                    path = Path.Combine(path, item.Text);
                    toPath = Path.Combine(toPath, e.Label);

                    try
                    {
                        if (File.Exists(path))
                            File.Move(path, toPath);
                        else if (Directory.Exists(path))
                            Directory.Move(path, toPath);
                    }
                    catch
                    {
                        e.CancelEdit = true;
                    }
                }
                else
                {
                    e.CancelEdit = true;
                }
            }

            listViewLeft.SelectedItems.Clear();
            listViewRight.SelectedItems.Clear();
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView listView = null;
            string path = "", tmp;

            if (listViewLeft.SelectedItems.Count != 0)
            {
                path = pathListViewLeft;
                listView = listViewLeft;
            }
            else if (listViewRight.SelectedItems.Count != 0)
            {
                path = pathListViewRight;
                listView = listViewRight;
            }

            try
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    tmp = Path.Combine(path, item.Text);

                    if (File.Exists(tmp))
                        File.Delete(tmp);
                    else if (Directory.Exists(tmp))
                        Directory.Delete(tmp);

                    item.Remove();
                }
            }
            catch { }
        }

        private void обновитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewCatalog(listViewLeft, toolStripTextBoxLeft, ref pathListViewLeft);
            ViewCatalog(listViewRight, toolStripTextBoxRight, ref pathListViewRight);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listViewLeft.SelectedItems.Count != 0 || listViewRight.SelectedItems.Count != 0)
            {
                переименоватьToolStripMenuItem.Enabled = true;
                удалитьToolStripMenuItem.Enabled = true;
            }
            else
            {
                переименоватьToolStripMenuItem.Enabled = false;
                удалитьToolStripMenuItem.Enabled = false;
            }
        }

        private void listView_Leave(object sender, EventArgs e)
        {
            ((ListView)sender).SelectedItems.Clear();
            
            toolStripButton1.Enabled = false;
            toolStripButton2.Enabled = false;
            toolStripButton6.Enabled = false;
        }

        private void папкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string pathCatalog = "";

            if (listViewLeft.Focused)
                pathCatalog = pathListViewLeft;
            else if (listViewRight.Focused)
                pathCatalog = pathListViewRight;

            for (int i = 0; true; i++)
            {
                string path = pathCatalog + "\\Новая Папка " + (i + 1);

                if (!(Directory.Exists(path)))
                {
                    Directory.CreateDirectory(path);
                    ViewCatalog(listViewLeft, toolStripTextBoxLeft, ref pathListViewLeft);
                    ViewCatalog(listViewRight, toolStripTextBoxRight, ref pathListViewRight);
                    break;
                }
            }
        }

        private void файлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string pathCatalog = "";

            if (listViewLeft.Focused)
                pathCatalog = pathListViewLeft;
            else if (listViewRight.Focused)
                pathCatalog = pathListViewRight;

            for (int i = 0; true; i++)
            {
                string path = pathCatalog + "\\Новый Файл " + (i + 1) + ".txt";

                if (!(File.Exists(path)))
                {
                    File.Create(path).Close();
                    ViewCatalog(listViewLeft, toolStripTextBoxLeft, ref pathListViewLeft);
                    ViewCatalog(listViewRight, toolStripTextBoxRight, ref pathListViewRight);
                    break;
                }
            }
        }

        private void создатьToolStripMenuItem1_MouseEnter(object sender, EventArgs e)
        {
            if (listViewLeft.Focused || listViewRight.Focused)
            {
                папкуToolStripMenuItem1.Enabled = true;
                файлtxtToolStripMenuItem.Enabled = true;
            }
            else
            {
                папкуToolStripMenuItem1.Enabled = false;
                файлtxtToolStripMenuItem.Enabled = false;
            }
        }

        private void действияToolStripMenuItem_MouseEnter(object sender, EventArgs e)
        {
            if (listViewLeft.Focused || listViewRight.Focused)
                выделитьВсеToolStripMenuItem.Enabled = true;
            else
                выделитьВсеToolStripMenuItem.Enabled = false;

            if (listViewLeft.SelectedItems.Count != 0 || listViewRight.SelectedItems.Count != 0)
            {
                переименоватьToolStripMenuItem1.Enabled = true;
                удалитьToolStripMenuItem1.Enabled = true;
            }
            else
            {
                переименоватьToolStripMenuItem1.Enabled = false;
                удалитьToolStripMenuItem1.Enabled = false;
            }
        }

        private void выделитьВсеToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listViewLeft.Focused)
            {
                for (int i = 0; i < listViewLeft.Items.Count; i++)
                {
                    if (listViewLeft.Items[i].Text == "...")
                        continue;

                    listViewLeft.Items[i].Selected = true;
                }
            }
            else if (listViewRight.Focused)
            {
                for (int i = 0; i < listViewRight.Items.Count; i++)
                {
                    if (listViewRight.Items[i].Text == "...")
                        continue;

                    listViewRight.Items[i].Selected = true;
                }
            }
        }

        private void listView_Enter(object sender, EventArgs e)
        {
            toolStripButton1.Enabled = true;
            toolStripButton2.Enabled = true;
            toolStripButton6.Enabled = true;
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewLeft.SelectedItems.Count != 0 || listViewRight.SelectedItems.Count != 0)
            {
                toolStripButton4.Enabled = true;
                toolStripButton5.Enabled = true;
            }
            else
            {
                toolStripButton4.Enabled = false;
                toolStripButton5.Enabled = false;
            }
        }

        private void toolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                if(((ToolStripTextBox)sender).Name == "toolStripTextBoxLeft")
                {
                    pathListViewLeft = toolStripTextBoxLeft.Text;
                    ViewCatalog(listViewLeft, toolStripTextBoxLeft, ref pathListViewLeft);
                }
                else if(((ToolStripTextBox)sender).Name == "toolStripTextBoxRight")
                {
                    pathListViewRight = toolStripTextBoxRight.Text;
                    ViewCatalog(listViewRight, toolStripTextBoxRight, ref pathListViewRight);
                }
            }
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ((ListView)sender).ListViewItemSorter = new ListViewItemComparer(e.Column);
        }
    }

    class ListViewItemComparer : IComparer
    {
        private int col = 0;

        public ListViewItemComparer(int column)
        {
            col = column;
        }

        public int Compare(object obj1, object obj2)
        {
            if (col == 3)
            {
                if (((ListViewItem)obj1).SubItems[col].Text == "")
                    return 0;

                return DateTime.Compare(Convert.ToDateTime(((ListViewItem)obj1).SubItems[col].Text), Convert.ToDateTime(((ListViewItem)obj2).SubItems[col].Text));
            }
            else
                return String.Compare(((ListViewItem)obj1).SubItems[col].Text, ((ListViewItem)obj2).SubItems[col].Text);
        }
    }

}
