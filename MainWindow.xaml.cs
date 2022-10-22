using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using Newtonsoft.Json;

//This namespace is used for DataTable
using System.Data;

//This namespace is used for Regular Expression
using System.Text.RegularExpressions;

//This namespace is used for SQL Classes
using System.Data.SqlClient;

//This namespace is used for ConfigurationManager and ConfigurationManager is used to fetch connection string from App.config file.
using System.Configuration;
using System.Threading.Tasks;
using System.Net.Http;

//The class names declared in one namespace should not conflict with the same class names declared in another.
namespace currencyconverter
{
    public partial class MainWindow : Window
    {


        root val = new root();

        public class root
        {
            public rate rates { get; set; }
            public long timestamp;
            public string license;
        }

        public class rate
        {
            public double INR { get; set; }

            public double JPY { get; set; }

            public double USD { get; set; }

            public double EUR { get; set; }
            public double CAD { get; set; }
            public double ISK { get; set; }
            public double PHP { get; set; }
            public double DKK { get; set; }
            public double CZK { get; set; }
            
        }

        //Create object for SqlConnection
        SqlConnection con = new SqlConnection();

        //Create an object for SqlCommand
        SqlCommand cmd = new SqlCommand();

        //Create object for SqlDataAdapter
        SqlDataAdapter da = new SqlDataAdapter();

        //Declare CurrencyId with int data type and assign value as 0.
        private int CurrencyId = 0;

        //Declare FromAmount with double data type and assign value 0.
        private double FromAmount = 0;

        //Declare ToAmount with double data type and assign value 0.
        private double ToAmount = 0;

        public MainWindow()
        {
            //We drag controls to the form in Visual Studio. Behind the scenes, Visual Studio adds code to the InitializeComponent method
            InitializeComponent();

            //ClearControls method to clear all controls value
            ClearControls();

            //BindCurrency is used for bind currrency name with value in Combobox
            BindCurrency();

            //GetData method is used to bind DataGrid
            GetData();
        }

        public async void getvalue()
        {
            val = await GetData<root>("https://openexchangerates.org/api/latest.json?app_id=b19dd67cffd1415fbf4470f3052f3232");
            BindCurrency();
        }

        public static async Task<root> GetData<T>(string url)
        {
            var myroot = new root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(1);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if(response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var ResponceString = await response.Content.ReadAsStringAsync();
                        var ResponceObject = JsonConvert.DeserializeObject<root>(ResponceString);

                        MessageBox.Show("timestamp: " + ResponceObject.timestamp, "information", MessageBoxButton.OK,MessageBoxImage.Information);

                        return ResponceObject;
                    }
                    return myroot;
                }
            }
            catch
            {
                return myroot;
            }
        }

        public void mycon()
        {
            //Database connection string
            String Conn = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            con = new SqlConnection(Conn);

            //Open the connection
            con.Open();
        }

        //Bind the currency name to From currency and To currency Combobox.
        private void BindCurrency()
        {
            mycon();

            //Create Object for DataTable
            DataTable dt = new DataTable();

            //Write SQL Query for Get Data from Database Table.
            cmd = new SqlCommand("select Id, CurrencyName from Currency_Master", con);

            //CommandType Define Which type of Command we Use for Write a Query
            cmd.CommandType = CommandType.Text;

            //It accepts a parameter that contains the command text of the object's SelectCommand property.
            da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            //Create a DataRow object
            DataRow newRow = dt.NewRow();

            //Assign a value to Id column
            newRow["Id"] = 0;

            //Assign value to CurrencyName column
            newRow["CurrencyName"] = "--SELECT--";

            //Insert a new row in dt with a data at 0 position
            dt.Rows.InsertAt(newRow, 0);

            //dt is not null and rows count greater than 0
            if (dt != null && dt.Rows.Count > 0)
            {
                //Assign data table data to From currency Combobox using item source property.
                cmbFromCurrency.ItemsSource = dt.DefaultView;

                //Assign data table data to To currency Combobox using item source property.
                cmbToCurrency.ItemsSource = dt.DefaultView;
            }
            con.Close();

            //To display the underlying datasource for cmbFromCurrency
            cmbFromCurrency.DisplayMemberPath = "CurrencyName";


            //To use as the actual value for the items
            cmbFromCurrency.SelectedValuePath = "Id";

            //Show default item in Combobox
            cmbFromCurrency.SelectedValue = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedValue = 0;

        }

        #region Extra Events
        //Method is used to clear all the input which user entered
        private void ClearControls()
        {
            try
            {
                txtCurrency.Text = string.Empty;
                if (cmbFromCurrency.Items.Count > 0)
                    cmbFromCurrency.SelectedIndex = 0;
                if (cmbToCurrency.Items.Count > 0)
                    cmbToCurrency.SelectedIndex = 0;
                lblCurrency.Content = "";
                txtCurrency.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Allow only integer in the Textbox
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            //Regular expression to add Regex. Add library using System.Text.RegularExpressions;
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion

        #region Currency Converter Tab Button Click Event

        // Assign the click event to convert button
        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Declare ConvertedValue variable with double data type to store converted currency value
                double ConvertedValue;

                //Check amount textbox is Null or Blank
                if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
                {
                    //If amount Textbox is Null or Blank then show dialog box
                    MessageBox.Show("Please enter currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    //Set focus to amount textbox
                    txtCurrency.Focus();
                    return;
                }
                //If From currency selected value is null or default text as --SELECT--
                else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
                {
                    //Open Dialog box
                    MessageBox.Show("Please select from currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    cmbFromCurrency.Focus();
                    return;
                }
                else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
                {
                    MessageBox.Show("Please select to currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    cmbToCurrency.Focus();
                    return;
                }

                if (cmbFromCurrency.SelectedValue == cmbToCurrency.SelectedValue)   //Check if From and To Combobox Selected Same Value
                {
                    //Amount textbox value is set in ConvertedValue. The double.parse is used to change Datatype from String To Double. 
                    //Textbox text has string, and ConvertedValue is double.
                    ConvertedValue = double.Parse(txtCurrency.Text);

                    //Show the label converted currency name and converted currency amount. The ToString("N3") is used for Placing 000 after the dot(.)
                    lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
                }
                else
                {
                    if (FromAmount != null && FromAmount != 0 && ToAmount != null && ToAmount != 0)
                    {
                        //Calculation for currency converter is From currency value Multiplied(*) with amount textbox value and then that total is divided(/) with To currency value.
                        ConvertedValue = FromAmount * double.Parse(txtCurrency.Text) / ToAmount;


                        //Show the label converted currency name and converted currency amount.
                        lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Assign the clear button click event
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            //ClearControls method used to clear all the control values which user entered
            ClearControls();
        }
        #endregion

        #region Currency Master Button Click Event
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter currency name", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrencyName.Focus();
                    return;
                }
                else
                {   //Edit time and set that record Id in CurrencyId variable.
                    //Code to Update. If CurrencyId greater than zero than it is go for update.
                    if (CurrencyId > 0)
                    {
                        //lwde message bhejega yeh sql ko
                        if (MessageBox.Show("Are you sure you want to update ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            mycon();
                            DataTable dt = new DataTable();

                            
                            cmd = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, CurrencyName = @CurrencyName WHERE Id = @Id", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Id", CurrencyId);
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data updated successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                   //bak bc kaam kar
                    else
                    {
                        if (MessageBox.Show("Are you sure you want to save ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            mycon();
                           
                            cmd = new SqlCommand("INSERT INTO Currency_Master(Amount, CurrencyName) VALUES(@Amount, @CurrencyName)", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data saved successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    ClearMaster();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Assign the cancel button click event
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMaster();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        //Bind data to the DataGrid view.
        public void GetData()
        {

            //Method is used for connect with database and open database connection
            mycon();

            //Create Datatable object
            DataTable dt = new DataTable();

            //Write SQL query to get the data from database table. Query written in double quotes and after comma provide connection.
            cmd = new SqlCommand("SELECT * FROM Currency_Master", con);

            //CommandType define which type of command will execute like Text, StoredProcedure, TableDirect.
            cmd.CommandType = CommandType.Text;

            //It is accept a parameter that contains the command text of the object's SelectCommand property.
            da = new SqlDataAdapter(cmd);

            //The DataAdapter serves as a bridge between a DataSet and a data source for retrieving and saving data. 
            //The fill operation then adds the rows to destination DataTable objects in the DataSet
            da.Fill(dt);

            //dt is not null and rows count greater than 0
            if (dt != null && dt.Rows.Count > 0)
                //Assign DataTable data to dgvCurrency using item source property.
                dgvCurrency.ItemsSource = dt.DefaultView;
            else
                dgvCurrency.ItemsSource = null;

            //Database connection close
            con.Close();
        }

        //Method is used to clear all the input which user entered in currency master tab
        private void ClearMaster()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                CurrencyId = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //DataGrid selected cell changed event
        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                //Create object for DataGrid
                DataGrid grd = (DataGrid)sender;

                //Create an object for DataRowView
                DataRowView row_selected = grd.CurrentItem as DataRowView;

                //If row_selected is not null
                if (row_selected != null)
                {
                    //dgvCurrency items count greater than zero
                    if (dgvCurrency.Items.Count > 0)
                    {
                        if (grd.SelectedCells.Count > 0)
                        {
                            //Get selected row id column value and set it to the CurrencyId variable
                            CurrencyId = Int32.Parse(row_selected["Id"].ToString());

                            //DisplayIndex is equal to zero in the Edited cell
                            if (grd.SelectedCells[0].Column.DisplayIndex == 0)
                            {
                                //Get selected row amount column value and set to amount textbox
                                txtAmount.Text = row_selected["Amount"].ToString();

                                //Get selected row CurrencyName column value and set it to CurrencyName textbox
                                txtCurrencyName.Text = row_selected["CurrencyName"].ToString();
                                btnSave.Content = "Update";     //Change save button text Save to Update
                            }

                            //DisplayIndex is equal to one in the deleted cell
                            if (grd.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                //Show confirmation dialog box
                                if (MessageBox.Show("Are you sure you want to delete ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    mycon();
                                    DataTable dt = new DataTable();

                                    //Execute delete query to delete record from table using Id
                                    cmd = new SqlCommand("DELETE FROM Currency_Master WHERE Id = @Id", con);
                                    cmd.CommandType = CommandType.Text;

                                    //CurrencyId set in @Id parameter and send it in delete statement
                                    cmd.Parameters.AddWithValue("@Id", CurrencyId);
                                    cmd.ExecuteNonQuery();
                                    con.Close();

                                    MessageBox.Show("Data deleted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearMaster();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Selection Changed Events

        //From currency Combobox selection changed event to get the amount of currency on selection change of currency name
        private void cmbFromCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //If cmbFromCurrency selected value is not equal to null and not equal to zero
                if (cmbFromCurrency.SelectedValue != null && int.Parse(cmbFromCurrency.SelectedValue.ToString()) != 0 && cmbFromCurrency.SelectedIndex != 0)
                {
                    //cmbFromCurrency selectedvalue set in CurrencyFromId variable
                    int CurrencyFromId = int.Parse(cmbFromCurrency.SelectedValue.ToString());

                    mycon();
                    DataTable dt = new DataTable();

                    //Select query to get amount from database using id
                    cmd = new SqlCommand("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyFromId", con);
                    cmd.CommandType = CommandType.Text;

                    if (CurrencyFromId != null && CurrencyFromId != 0)
                        //CurrencyFromId set in @CurrencyFromId parameter and send parameter in our query
                        cmd.Parameters.AddWithValue("@CurrencyFromId", CurrencyFromId);

                    da = new SqlDataAdapter(cmd);

                    //Set the data that the query returns in the data table
                    da.Fill(dt);

                    if (dt != null && dt.Rows.Count > 0)
                        //Get amount column value from datatable and set amount value in From amount variable which is declared globally
                        FromAmount = double.Parse(dt.Rows[0]["Amount"].ToString());

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        //To currency Combobox selection changed event to get the amount of currency on selection change of currency name
        private void cmbToCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //If cmbToCurrency selectedvalue is not equal to null and not equal to zero
                if (cmbToCurrency.SelectedValue != null && int.Parse(cmbToCurrency.SelectedValue.ToString()) != 0 && cmbToCurrency.SelectedIndex != 0)
                {
                    //cmbToCurrency selectedvalue is set to CurrencyToId variable
                    int CurrencyToId = int.Parse(cmbToCurrency.SelectedValue.ToString());

                    mycon();

                    DataTable dt = new DataTable();
                    //Select query for get Amount from database using id
                    cmd = new SqlCommand("SELECT Amount FROM Currency_Master WHERE Id = @CurrencyToId", con);
                    cmd.CommandType = CommandType.Text;

                    if (CurrencyToId != null && CurrencyToId != 0)
                        //CurrencyToId set in @CurrencyToId parameter and send parameter in our query
                        cmd.Parameters.AddWithValue("@CurrencyToId", CurrencyToId);

                    da = new SqlDataAdapter(cmd);

                    //Set the data that the query returns in the data table
                    da.Fill(dt);

                    if (dt != null && dt.Rows.Count > 0)
                        //Get amount column value from datatable and set amount value in ToAmount variable which is declared globally
                        ToAmount = double.Parse(dt.Rows[0]["Amount"].ToString());
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Preview Key Down Events
        //cmbFromCurrency preview key down event
        private void cmbFromCurrency_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //If the user press Tab or Enter key then cmbFromCurrency_SelectionChanged event is executed
            if (e.Key == Key.Tab || e.SystemKey == Key.Enter)
            {
                cmbFromCurrency_SelectionChanged(sender, null);
            }
        }

        //cmbToCurrency preview key down event
        private void cmbToCurrency_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //If the user press Tab or Enter key then cmbToCurrency_SelectionChanged event is executed
            if (e.Key == Key.Tab || e.SystemKey == Key.Enter)
            {
                cmbToCurrency_SelectionChanged(sender, null);
            }
        }
        #endregion
    }
}