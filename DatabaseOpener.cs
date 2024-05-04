using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;

namespace DatabaseOpenerKraus
{
    public partial class DatabaseOpener : Form
    {
        MySqlConnection dbConnect;
        public DatabaseOpener()
        {
            InitializeComponent();
            btnConnect.BackColor = Color.Green;
            cmbAscDesc.Items.Add("DESC");
            cmbAscDesc.Items.Add("ASC");
        }

        public void conStatus()
        {
            if (btnConnect.Text == "Connect")
            {
                dbConnect.Open();
                lblConnection.Text = "Open";
                lblConnection.BackColor = Color.Green;
                btnConnect.BackColor = Color.Red;
                btnConnect.Text = "Disconnect";
                lblDebug.Text = "Database succesfully connected!";
                txtServer.Enabled = false;
                txtUser.Enabled = false;
                txtPwd.Enabled = false;
                txtDatabase.Enabled = false;
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\windows\Media\Windows Hardware Insert.wav");
                player.Play();
            }
            else if (btnConnect.Text == "Disconnect")
            {
                chkColumns.Items.Clear();
                cmbTable.Items.Clear();
                lstViewData.Columns.Clear();
                dbConnect.Close();
                lblConnection.Text = "Close";
                lblConnection.BackColor = Color.Red;
                btnConnect.BackColor = Color.Green;
                btnConnect.Text = "Connect";
                lblDebug.Text = "Database succesfully disconnected!";
                txtServer.Enabled = true;
                txtUser.Enabled = true;
                txtPwd.Enabled = true;
                txtDatabase.Enabled = true;
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"c:\Windows\Media\Windows Hardware Remove.wav");
                player.Play();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string connectStr = "SERVER=" + txtServer.Text + "; UID=" + txtUser.Text + "; PASSWORD=" + txtPwd.Text + "; DATABASE=" + txtDatabase.Text;
            try
            {
                dbConnect = new MySqlConnection(connectStr);
                if (dbConnect.State == ConnectionState.Closed)
                {
                    conStatus();
                }
                else if (dbConnect.State == ConnectionState.Open)
                {
                    conStatus();
                }
                string cmdSQL = "SHOW TABLES FROM " + txtDatabase.Text;
                MySqlCommand requestTables = new MySqlCommand(cmdSQL, dbConnect);
                MySqlDataReader tableDataSQL = requestTables.ExecuteReader();
                while (tableDataSQL.Read())
                {
                    cmbTable.Items.Add(tableDataSQL[0].ToString());
                }
                tableDataSQL.Close();
                cmbTable.SelectedIndex = 0;
            }
            catch (Exception err)
            {
                lblDebug.Text = err.Message;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string cmdSQL = "SELECT " + selArray() + " FROM " + cmbTable.SelectedItem + " " + txtOrder.Text;
                lblDebug.Text = cmdSQL;
                string spojRet = "SERVER=" + txtServer.Text + "; UID=" + txtUser.Text + "; PASSWORD=" + txtPwd.Text + "; DATABASE=" + txtDatabase.Text;
                dbConnect = new MySqlConnection(spojRet);
                if (dbConnect.State == ConnectionState.Closed)
                {
                    dbConnect.Open();
                    MySqlCommand requestRows = new MySqlCommand(cmdSQL, dbConnect);
                    MySqlDataReader dataSQL = requestRows.ExecuteReader();

                    try
                    {
                        while (dataSQL.Read())
                        {
                            ListViewItem record = new ListViewItem(dataSQL.GetValue(0).ToString());
                            for (int i = 1; i < dataSQL.FieldCount; i++)
                            {
                                record.SubItems.Add(dataSQL.GetValue(i).ToString());
                            }
                            lstViewData.Items.Add(record);
                        }
                        dataSQL.Close();

                    }
                    catch (Exception)
                    {

                        MessageBox.Show("Zvolte prosím sloupce tabulky!", "String", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Zvolte prosím sloupce tabulky!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private string selArray()
        {
            lstViewData.Columns.Clear();
            lstViewData.Items.Clear();
            string selection = "";
            foreach (int item in chkColumns.CheckedIndices)
            {
                selection += chkColumns.Items[item] + ", ";
                lstViewData.Columns.Add(chkColumns.Items[item].ToString());
            }
            return selection.Substring(0, selection.Length - 2);
        }

        private void btnAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chkColumns.Items.Count; i++)
            {
                chkColumns.SetItemChecked(i, true);
            }
        }

        private void btnNothing_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chkColumns.Items.Count; i++)
            {
                chkColumns.SetItemChecked(i, false);
            }
        }

        private void btnInvert_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < chkColumns.Items.Count; i++)
            {
                chkColumns.SetItemChecked(i, !chkColumns.GetItemChecked(i));
            }
        }

        private void cmbTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkColumns.Items.Clear();
            lstViewData.Clear();
            string cmdSQL = "SHOW COLUMNS FROM " + cmbTable.SelectedItem;
            MySqlCommand requestColumns = new MySqlCommand(cmdSQL, dbConnect);
            MySqlDataReader dataSQL = requestColumns.ExecuteReader();
            while (dataSQL.Read())
            {
                chkColumns.Items.Add(dataSQL[0].ToString(), true);//.GetValue(0).ToString()

            }
            dataSQL.Close();
        }

        private void chkColumns_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            cmbOrderBy.Items.Clear();
            cmbWhere.Items.Clear();
            List<string> checkedItems = new List<string>();
            foreach (var item in chkColumns.CheckedItems)
                checkedItems.Add(item.ToString());

            if (e.NewValue == CheckState.Checked)
                checkedItems.Add(chkColumns.Items[e.Index].ToString());
            else
                checkedItems.Remove(chkColumns.Items[e.Index].ToString());

            foreach (string item in checkedItems)
            {
                cmbOrderBy.Items.Add(item);
                cmbWhere.Items.Add(item);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cmbWhere.SelectedIndex > -1)
            {
                if (cmbOrderBy.SelectedIndex > -1)
                {
                    txtOrder.Text = "WHERE " + cmbWhere.SelectedItem + "=" + txtWhere.Text + " ORDER BY " + cmbOrderBy.SelectedItem + " " + cmbAscDesc.SelectedItem;
                }
                else
                {
                    txtOrder.Text = "WHERE " + cmbWhere.SelectedItem + "=" + txtWhere.Text;
                }
            }
            else
            {
                txtOrder.Text += " ORDER BY " + cmbOrderBy.SelectedItem + " " + cmbAscDesc.SelectedItem;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtOrder.Text = "";
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            Edit edit = new Edit();
            edit.Show();
        }

        private void oproduktuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About aboutApp = new About();
            aboutApp.Show();
        }
    }
}
