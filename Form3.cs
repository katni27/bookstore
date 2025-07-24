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
    public partial class Form3 : Form
    {
        Form1 authorizationForm;

        string connectionString = @"Data Source=MSSQL2017;Initial Catalog=katichev-22;Integrated Security=false;User ID=katichev-22;Password=ymwxcs";

        DataSet dataSet = new DataSet();
        DataTable cart = new DataTable();
        SqlConnection connection = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        BindingSource bindingSourceProducts = new BindingSource();
        BindingSource bindingSourceCart = new BindingSource();
        private int costCart;
        private int quantityCart;

        public Form3()
        {
            InitializeComponent();
        }

        public Form3(Form1 form)
        {
            this.InitializeComponent();
            authorizationForm = form;
        }

        private void SetToDataGridView(BindingSource bs, DataTable tbl, DataGridView dgv)
        {
            bs.DataSource = tbl;
            dgv.DataSource = bs;
            dgv.ReadOnly = true;
        }

        private void UpdateCartInformation(int cost, int quantity)
        {
            this.costCart += cost;
            this.quantityCart += quantity;
            label6.Text = $"Сумма: {this.costCart}";
            label7.Text = $"Количество товаров: {this.quantityCart}";
        }

        private void SetToComboBox(DataTable tbl, ComboBox cb, string valueMember, string displayMember)
        {
            var newRow = tbl.NewRow();
            newRow[1] = "Все";
            tbl.Rows.InsertAt(newRow, 0);
            cb.DataSource = tbl;
            cb.ValueMember = valueMember;
            cb.DisplayMember = displayMember;
        }

        private int CreateCheque()
        {
            int chequeID = 0;
            try
            {
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("CreateCheque", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                var param = new SqlParameter();
                param.ParameterName = "@ID";
                param.SqlDbType = SqlDbType.Int;
                param.Direction = ParameterDirection.Output;

                cmd.Parameters.Add(param);
                connection.Open();
                cmd.ExecuteNonQuery();
                chequeID = (int)cmd.Parameters["@ID"].Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return chequeID;
        }

        private void CreateColumnsCart()
        {
            cart.Columns.Add("ID", typeof(int));
            cart.Columns.Add("Название", typeof(String));
            cart.Columns.Add("Автор", typeof(String));
            cart.Columns.Add("Цена", typeof(int));
            cart.Columns.Add("Количество", typeof(int));
        }

        private void UpdateTables()
        {
            try
            {
                dataSet.Clear();

                var connection = new SqlConnection(connectionString);
                var adapter = new SqlDataAdapter("[dbo].[SelectForShopper]", connection);

                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.TableMappings.Add("Table", "Books");
                adapter.TableMappings.Add("Table1", "Author");
                adapter.TableMappings.Add("Table2", "Genre");
                adapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            dataSet.Tables.Add("Books");
            dataSet.Tables.Add("Author");
            dataSet.Tables.Add("Genre");

            costCart = 0;
            quantityCart = 0;

            CreateColumnsCart();

            try
            {
                UpdateTables();

                SetToDataGridView(bindingSourceProducts, dataSet.Tables["Books"], dataGridView1);
                SetToDataGridView(bindingSourceCart, cart, dataGridView2);

                SetToComboBox(dataSet.Tables["Author"], comboBox2, "Author_ID", "FIO");
                SetToComboBox(dataSet.Tables["Genre"], comboBox1, "Genre_ID", "GenreName");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != 0)
            {
                bindingSourceProducts.Filter = $"Жанр like '{comboBox1.SelectedText}%'";
            }
            else
            {
                bindingSourceProducts.Filter = "";
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != 0)
            {
                bindingSourceProducts.Filter = $"Автор like '{comboBox1.SelectedText}%'";
            }
            else
            {
                bindingSourceProducts.Filter = "";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text))
            {
                bindingSourceProducts.Filter = $"Автор like '{textBox3.Text}%'";
            }
            else
            {
                MessageBox.Show("Введите название!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            textBox3.Text = "";
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = (DataRowView)bindingSourceProducts.Current;

            label1.Text = $"Товар №{row["ID"]}";
            textBox2.Text = "1";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var newRow = cart.NewRow();
                var row = (DataRowView)bindingSourceProducts.Current;

                int quantity = Convert.ToInt32(textBox2.Text);
                int price = Convert.ToInt32(row["Цена"]);

                Object[] record = { row["ID"], row["Название"], row["Автор"],
                                    row["Цена"], quantity};
                newRow.ItemArray = record;
                UpdateCartInformation(quantity * price, quantity);

                cart.Rows.Add(newRow);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (bindingSourceCart.Count != 0)
            {
                var row = (DataRowView)bindingSourceCart.Current;
                int quantity = Convert.ToInt32(row["Количество"]);
                int price = Convert.ToInt32(row["Цена"]);

                UpdateCartInformation(-(quantity * price), -quantity);
                bindingSourceCart.RemoveCurrent();
            }
            else
            {
                MessageBox.Show("Товары отсутствуют!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (DataRowView row in bindingSourceCart.List)
            {
                row.Delete();
            }

            UpdateCartInformation(-this.costCart, -this.quantityCart);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int chequeID = CreateCheque();
            try
            {
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("[dbo].[AddItemToCheque]", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Cheque_ID", SqlDbType.Int);
                cmd.Parameters.Add("@Book_ID", SqlDbType.Int);
                cmd.Parameters.Add("@Quantity", SqlDbType.Int);
                cmd.Parameters["@Cheque_ID"].Value = chequeID; 

                connection.Open();

                foreach (DataRowView row in bindingSourceCart.List)
                {
                    cmd.Parameters["@Book_ID"].Value = row["ID"];
                    cmd.Parameters["@Quantity"].Value = row["Количество"];
                    cmd.ExecuteNonQuery();
                    row.Delete();
                }

                UpdateCartInformation(-this.costCart, -this.quantityCart);

                MessageBox.Show("Чек создан!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                var connection = new SqlConnection(connectionString);
                var adapter = new SqlDataAdapter("[dbo].[SelectForShopper]", connection);

                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.TableMappings.Add("Table", "Books");
                adapter.TableMappings.Add("Table1", "Author");
                adapter.TableMappings.Add("Table2", "Genre");
                adapter.Update(dataSet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
