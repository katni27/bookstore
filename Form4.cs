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
    public partial class Form4 : Form
    {
        Form1 authorizationForm;

        string connectionString = @"Data Source=MSSQL2017;Initial Catalog=katichev-22;Integrated Security=false;User ID=katichev-22;Password=ymwxcs";

        DataSet dataSet = new DataSet();
        DataTable invoice = new DataTable();
        SqlConnection connection = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        BindingSource bindingSourceBooks = new BindingSource();
        BindingSource bindingSourcePublisher = new BindingSource();
        BindingSource bindingSourceAuthor = new BindingSource();
        BindingSource bindingSourceGenre = new BindingSource();
        BindingSource bindingSourceInvoice = new BindingSource();
        private int costInvoice;
        private int quantityInvoice;
        private int publisherIdInvoice;

        public Form4()
        {
            InitializeComponent();
        }

        public Form4(Form1 form)
        {
            this.InitializeComponent();
            authorizationForm = form;
        }

        private void CreateColumnsInvoice()
        {
            invoice.Columns.Add("ID", typeof(int));
            invoice.Columns.Add("Название", typeof(String));
            invoice.Columns.Add("Автор", typeof(String));
            invoice.Columns.Add("Издательство", typeof(String));
            invoice.Columns.Add("Цена", typeof(int));
            invoice.Columns.Add("Количество", typeof(int));
        }

        private void SetToDataGridView(BindingSource bs, DataTable tbl, DataGridView dgv)
        {
            bs.DataSource = tbl;
            dgv.DataSource = bs;
            dgv.ReadOnly = true;
        }

        private void SetToComboBox(DataTable tbl, ComboBox cb, string valueMember, string displayMember)
        {
            cb.DataSource = tbl;
            cb.ValueMember = valueMember;
            cb.DisplayMember = displayMember;
        }

        private void UpdateInvoiceInformation(int cost, int quantity)
        {
            if (bindingSourceInvoice.Count == 0)
            {
                label12.Text = "Издательство: ";
            }
            costInvoice += cost;
            quantityInvoice += quantity;
            label6.Text = $"Сумма: {costInvoice}";
            label7.Text = $"Количество товаров: {quantityInvoice}";
        }

        private int CreateInvoice(int publiherID)
        {
            int invoiceID = 0;
            try
            {
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("CreateInvoice", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                var param1 = new SqlParameter();
                param1.ParameterName = "@Publisher_ID";
                param1.Value = publiherID;
                param1.SqlDbType = SqlDbType.Int;
                cmd.Parameters.Add(param1);

                var param2 = new SqlParameter();
                param2.ParameterName = "@ID";
                param2.SqlDbType = SqlDbType.Int;
                param2.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(param2);

                connection.Open();
                cmd.ExecuteNonQuery();
                invoiceID = (int)cmd.Parameters["@ID"].Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return invoiceID;
        }

        private void UpdateTables()
        {
            try
            {
                dataSet.Clear();

                var connection = new SqlConnection(connectionString);
                var adapter = new SqlDataAdapter("[dbo].[SelectForManager]", connection);

                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.TableMappings.Add("Table", "Books");
                adapter.TableMappings.Add("Table1", "Author");
                adapter.TableMappings.Add("Table2", "Genre");
                adapter.TableMappings.Add("Table3", "Publisher");
                adapter.Fill(dataSet);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {

            dataSet.Tables.Add("Books");
            dataSet.Tables.Add("Author");
            dataSet.Tables.Add("Genre");
            dataSet.Tables.Add("Publisher");

            costInvoice = 0;
            quantityInvoice = 0;

            CreateColumnsInvoice();

            try
            {
                UpdateTables();

                SetToDataGridView(bindingSourceBooks, dataSet.Tables["Books"], dataGridView1);
                SetToDataGridView(bindingSourceAuthor, dataSet.Tables["Author"], dataGridView4);
                SetToDataGridView(bindingSourceGenre, dataSet.Tables["Genre"], dataGridView3);
                SetToDataGridView(bindingSourcePublisher, dataSet.Tables["Publisher"], dataGridView7);
                SetToDataGridView(bindingSourceInvoice, invoice, dataGridView2);

                SetToComboBox(dataSet.Tables["Author"], comboBox2, "ID", "ФИО");
                SetToComboBox(dataSet.Tables["Genre"], comboBox1, "ID", "Жанр");
                SetToComboBox(dataSet.Tables["Publisher"], comboBox3, "ID", "Издательство");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            var row = (DataRowView)bindingSourceBooks.Current;

            int genreID = Convert.ToInt32(comboBox1.SelectedValue);
            int authorID = Convert.ToInt32(comboBox2.SelectedValue);
            int publisherID = Convert.ToInt32(comboBox3.SelectedValue);
            try
            {
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("[dbo].[AddBook]", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@BookName", SqlDbType.VarChar, 50).Value = textBox3.Text;
                cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = 0;
                cmd.Parameters.Add("@Price", SqlDbType.Int).Value = Convert.ToInt32(textBox1.Text);
                cmd.Parameters.Add("@ISBN", SqlDbType.VarChar, 17).Value = textBox2.Text;
                cmd.Parameters.Add("@Publisher_ID", SqlDbType.Int).Value = publisherID;
                cmd.Parameters.Add("@Genre_ID", SqlDbType.Int).Value = genreID;
                cmd.Parameters.Add("@Author_ID", SqlDbType.Int).Value = authorID;

                connection.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Книга добавлена!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = (DataRowView)bindingSourceBooks.Current;

            label1.Text = $"Книга №{row["ID"]}";
            label14.Text = $"Книга №{row["ID"]}";
            label16.Text = $"Книга №{row["ID"]}";
            label19.Text = $"Издательство: {row["Издательство"]}";
            textBox4.Text = "1";

            textBox3.Text = row["Название"].ToString();
            textBox1.Text = row["Цена"].ToString();
            textBox2.Text = row["ISBN"].ToString();

            comboBox2.SelectedIndex = comboBox2.FindStringExact(row["Автор"].ToString());
            comboBox1.SelectedIndex = comboBox1.FindStringExact(row["Жанр"].ToString());
            comboBox3.SelectedIndex = comboBox3.FindStringExact(row["Издательство"].ToString());
        }

        private void button7_Click(object sender, EventArgs e)
        {
            UpdateTables();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var row = (DataRowView)bindingSourceBooks.Current;

            int bookID = Convert.ToInt32(row["ID"]);
            int genreID = Convert.ToInt32(comboBox1.SelectedValue);
            int authorID = Convert.ToInt32(comboBox2.SelectedValue);
            int publisherID = Convert.ToInt32(comboBox3.SelectedValue);
            try
            {
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("[dbo].[UpdateBook]", connection);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Book_ID", SqlDbType.Int).Value = bookID;
                cmd.Parameters.Add("@BookName", SqlDbType.VarChar, 50).Value = textBox3.Text;
                cmd.Parameters.Add("@Price", SqlDbType.Int).Value = Convert.ToInt32(textBox1.Text);
                cmd.Parameters.Add("@ISBN", SqlDbType.VarChar, 17).Value = textBox2.Text;
                cmd.Parameters.Add("@Publisher_ID", SqlDbType.Int).Value = publisherID;
                cmd.Parameters.Add("@Genre_ID", SqlDbType.Int).Value = genreID;
                cmd.Parameters.Add("@Author_ID", SqlDbType.Int).Value = authorID;

                connection.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Информация о книге обновлена!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var row = (DataRowView)bindingSourceBooks.Current;

            try
            {
                connection = new SqlConnection(connectionString);
                string queryString = $"UPDATE Book SET Quantity = 0 WHERE Book_ID = {row["ID"]}";
                cmd = new SqlCommand(queryString, connection);

                connection.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Книга изъята из продажи!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var rowBooks = (DataRowView)bindingSourceBooks.Current;
            var rowInvoice = (DataRowView)bindingSourceInvoice.Current;

            if (string.IsNullOrEmpty(textBox5.Text))
            {
                MessageBox.Show("Введите цену!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (bindingSourceInvoice.Count != 0 && !rowBooks["Издательство"].Equals(rowInvoice["Издательство"]))
            {
                MessageBox.Show("В накладной книги другого издательства!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var newRow = invoice.NewRow();

                int quantity = Convert.ToInt32(textBox4.Text);
                int price = Convert.ToInt32(textBox5.Text);

                Object[] record = { rowBooks["ID"], rowBooks["Название"], rowBooks["Автор"],
                                    rowBooks["Издательство"], price, quantity};
                newRow.ItemArray = record;

                invoice.Rows.Add(newRow);
                UpdateInvoiceInformation(price * quantity, quantity);
                publisherIdInvoice = Convert.ToInt32(comboBox3.SelectedValue);
                label12.Text = $"Издательство: {rowBooks["Издательство"]}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (bindingSourceInvoice.Count != 0)
            {
                var row = (DataRowView)bindingSourceInvoice.Current;
                int quantity = Convert.ToInt32(row["Количество"]);
                int price = Convert.ToInt32(row["Цена"]);
                bindingSourceInvoice.RemoveCurrent();
                UpdateInvoiceInformation(-(price * quantity), -quantity);
            }
            else
            {
                MessageBox.Show("Товары отсутствуют в накладной!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (DataRowView row in bindingSourceInvoice.List)
            {
                row.Delete();
            }
            UpdateInvoiceInformation(-costInvoice, -quantityInvoice);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var row = (DataRowView)bindingSourceInvoice.Current;
            int invoiceID = CreateInvoice(publisherIdInvoice);

            try
            {
                connection = new SqlConnection(connectionString);
                cmd = new SqlCommand("[dbo].[AddItemToInvoice]", connection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@Invoice_ID", SqlDbType.Int);
                cmd.Parameters.Add("@Book_ID", SqlDbType.Int);
                cmd.Parameters.Add("@Quantity", SqlDbType.Int);
                cmd.Parameters.Add("@Price", SqlDbType.Int);
                cmd.Parameters["@Invoice_ID"].Value = invoiceID;

                connection.Open();

                foreach (DataRowView rowInvoice in bindingSourceInvoice.List)
                {
                    cmd.Parameters["@Book_ID"].Value = rowInvoice["ID"];
                    cmd.Parameters["@Quantity"].Value = rowInvoice["Количество"];
                    cmd.Parameters["@Price"].Value = rowInvoice["Цена"];
                    cmd.ExecuteNonQuery();
                    rowInvoice.Delete();
                }

                UpdateInvoiceInformation(-costInvoice, -quantityInvoice);

                MessageBox.Show("Накладная создана!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void dataGridView7_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = (DataRowView)bindingSourcePublisher.Current;

            label23.Text = $"Издательство №{row["ID"]}";

            textBox8.Text = row["Издательство"].ToString();
            textBox10.Text = row["Адрес"].ToString();
            textBox9.Text = row["Телефон"].ToString();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            var row = (DataRowView)bindingSourcePublisher.Current;

            try
            {
                connection = new SqlConnection(connectionString);
                string queryString = $"UPDATE Publisher SET PublisherName = '{textBox8.Text}', " +
                                     $"Address = '{textBox10.Text}', Phone = '{textBox9.Text}' " +
                                     $"WHERE Publisher_ID = {row["ID"]}";
                cmd = new SqlCommand(queryString, connection);

                connection.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Информация об издательстве обновлена!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            try
            {
                connection = new SqlConnection(connectionString);
                string queryString = $"INSERT INTO Publisher VALUES('{textBox8.Text}','{textBox10.Text}','{textBox9.Text}')";
                cmd = new SqlCommand(queryString, connection);

                connection.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Издательство добавлено!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            var row = (DataRowView)bindingSourceAuthor.Current;

            try
            {
                connection = new SqlConnection(connectionString);
                string queryString = $"UPDATE Author SET FIO = '{textBox7.Text}' " +
                                     $"WHERE Author_ID = {row["ID"]}";
                cmd = new SqlCommand(queryString, connection);

                connection.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Информация об авторе обновлена!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void dataGridView4_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = (DataRowView)bindingSourceAuthor.Current;

            label15.Text = $"Автор №{row["ID"]}";

            textBox7.Text = row["ФИО"].ToString();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                connection = new SqlConnection(connectionString);
                string queryString = $"INSERT INTO Author VALUES('{textBox7.Text}')";
                cmd = new SqlCommand(queryString, connection);

                connection.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Автор добавлен!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = (DataRowView)bindingSourceGenre.Current;

            label18.Text = $"Жанр №{row["ID"]}";

            textBox6.Text = row["Жанр"].ToString();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var row = (DataRowView)bindingSourceGenre.Current;

            try
            {
                connection = new SqlConnection(connectionString);
                string queryString = $"UPDATE Genre SET GenreName = '{textBox6.Text}' " +
                                     $"WHERE Genre_ID = {row["ID"]}";
                cmd = new SqlCommand(queryString, connection);

                connection.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Информация о жанре обновлена!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                connection = new SqlConnection(connectionString);
                string queryString = $"INSERT INTO Genre VALUES('{textBox6.Text}')";
                cmd = new SqlCommand(queryString, connection);

                connection.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Жанр добавлен!", "Успех!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
                UpdateTables();
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            DataTable sales = new DataTable();
            DataTable purchase = new DataTable();

            connection = new SqlConnection(connectionString);
            cmd = new SqlCommand("SELECT * FROM [dbo].[BookSales](@d1,@d2)", connection);
            DateTime begin = dateTimePicker1.Value;
            DateTime end = dateTimePicker2.Value;

            cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = begin;
            cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = end;

            var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(sales);

            dataGridView6.DataSource = sales;
            dataGridView6.ReadOnly = true;

            cmd = new SqlCommand("SELECT * FROM [dbo].[BookPurchase](@d1,@d2)", connection);
            cmd.Parameters.Add("@d1", SqlDbType.DateTime).Value = begin;
            cmd.Parameters.Add("@d2", SqlDbType.DateTime).Value = end;

            adapter = new SqlDataAdapter(cmd);
            adapter.Fill(purchase);

            dataGridView5.DataSource = purchase;
            dataGridView5.ReadOnly = true;
        }
    }
}
