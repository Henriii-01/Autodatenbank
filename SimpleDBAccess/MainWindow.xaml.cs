using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleDBAccess
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        //Eingabe
        private void Button_set_data(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(@"SERVER=localhost;DATABASE=hallo;UID=root;PASSWORD=IchBinBroke1!;"))
                {
                    con.Open();
                    string insertQuery = "INSERT INTO YourTableName (Column1, Column2, Column3) VALUES (@value1, @value2, @value3)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, con);
                    insertCmd.Parameters.AddWithValue("@value1", "Value1");
                    insertCmd.Parameters.AddWithValue("@value2", "Value2");
                    insertCmd.Parameters.AddWithValue("@value3", "Value3");
                    insertCmd.ExecuteNonQuery();
                    MessageBox.Show("Data inserted successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        //Abfrage für VIN
        private void Button_get_data(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(@"SERVER=localhost;DATABASE=projectx;UID=root;PASSWORD=IchBinBroke1!;"))
                {
                    con.Open();

                    string searchValue = SearchValueTextBox.Text;
                    if (string.IsNullOrEmpty(searchValue))
                    {
                        MessageBox.Show("Bitte geben Sie eine VIN ein.");
                        return;
                    }

                    string searchQuery = "SELECT * FROM Mercedes WHERE vin=@value UNION SELECT * FROM BMW WHERE vin=@value UNION SELECT * FROM Porsche WHERE vin=@value";
                    MySqlCommand searchCmd = new MySqlCommand(searchQuery, con);
                    searchCmd.Parameters.AddWithValue("@value", searchValue);

                    MySqlDataReader reader = searchCmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        AusgabeFeld.Visibility = Visibility.Visible;
                        AusgabeFeld.Clear();
                        while (reader.Read())
                        {
                            AusgabeFeld.AppendText($"{reader[0]} {reader[1]} {reader[2]}\n");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Keine Ergebnisse gefunden.");
                        AusgabeFeld.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}");
            }
        }

        //Abfrage für Marke, Modell und isEQ
        private void Button_search_data(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(@"SERVER=localhost;DATABASE=projectx;UID=root;PASSWORD=IchBinBroke1!;"))
                {
                    con.Open();

                    string brand = (BrandComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    string model = ModelTextBox.Text;
                    bool isEQ = IsEQCheckBox.IsChecked == true;

                    string searchQuery = $"SELECT * FROM {brand} WHERE model=@model AND isEQ=@isEQ";
                    MySqlCommand searchCmd = new MySqlCommand(searchQuery, con);
                    searchCmd.Parameters.AddWithValue("@model", model);
                    searchCmd.Parameters.AddWithValue("@isEQ", isEQ);

                    MySqlDataReader reader = searchCmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        ResultFeld.Visibility = Visibility.Visible;
                        ResultFeld.Clear();
                        while (reader.Read())
                        {
                            ResultFeld.AppendText($"{reader[0]} {reader[1]} {reader[2]}\n");
                        }
                    }
                    else
                    {
                        MessageBox.Show("No results found.");
                        ResultFeld.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private MySqlCommand BuildSearchQuery(MySqlConnection con)
        {
            string searchValue = SearchValueTextBox.Text;

            if (string.IsNullOrEmpty(searchValue))
            {
                MessageBox.Show("Bitte geben Sie eine VIN ein.");
                return null;
            }

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM Mercedes WHERE vin=@value UNION SELECT * FROM BMW WHERE vin=@value UNION SELECT * FROM Porsche WHERE vin=@value";
            cmd.Parameters.AddWithValue("@value", searchValue);

            return cmd;
        }
    }
}