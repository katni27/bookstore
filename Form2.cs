using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace BookStore
{
    public partial class Form2 : Form
    {
        Form1 authorizationForm;

        string connectionString = @"Data Source=MSSQL2017;Initial Catalog=katichev-22;Integrated Security=false;User ID=katichev-22;Password=ymwxcs";

        DataSet dataSet = new DataSet();
        BindingSource bindingSource = new BindingSource();

        public Form2()
        {
            InitializeComponent();
        }

        public Form2(Form1 form)
        {
            this.InitializeComponent();
            authorizationForm = form;
        }

        private void DataGridSet(BindingSource bs, DataTable tbl, DataGridView dgv, string[] columnNames)
        {
            bs.DataSource = tbl;
            dgv.DataSource = bs;
            dgv.ReadOnly = true;
            dgv.Columns[0].Visible = false;
            for (int i = 0; i < columnNames.Length; ++i) 
            {
                dgv.Columns[i + 1].HeaderText = columnNames[i];
            }
        }

        private void ComboBoxSet(DataTable tbl, ComboBox cb, string valueMember, string displayMember)
        {
            var newRow = tbl.NewRow();
            newRow[1] = "Все";
            tbl.Rows.InsertAt(newRow, 0);
            cb.DataSource = tbl;
            cb.ValueMember = valueMember;
            cb.DisplayMember = displayMember;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            var connection = new SqlConnection(connectionString);
            dataSet.Clear();
            dataSet.Tables.Add("Books");
            dataSet.Tables.Add("Author");
            dataSet.Tables.Add("Genre");
            var adapter = new SqlDataAdapter("[dbo].[SelectForShopper]", connection);
            adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
            adapter.TableMappings.Add("Table", "Books");
            adapter.TableMappings.Add("Table1", "Author");
            adapter.TableMappings.Add("Table2", "Genre");
            try
            {
                adapter.Fill(dataSet);
                DataGridSet(bindingSource, dataSet.Tables["Books"], dataGridView1, 
                            new string[] { "Название", "Автор", "Жанр", "Цена", "Количество"});
                ComboBoxSet(dataSet.Tables["Author"], comboBox2, "Author_ID", "FIO");
                ComboBoxSet(dataSet.Tables["Genre"], comboBox1, "Genre_ID", "GenreName");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            //oldForm.Show();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != 0)
            {
                bindingSource.Filter = $"GenreName like '{comboBox1.SelectedText}%'";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }
    }
}
