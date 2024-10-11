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
            BothRadioButton.IsChecked = true;
            BothRadioButton2.IsChecked = true;
        }

        //Eingabe
        private void Button_set_data(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(@"SERVER=localhost;DATABASE=car_db;UID=root;PASSWORD=******;"))
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
                        CreateResultTab(reader);
                    }
                    else
                    {
                        MessageBox.Show("Keine Ergebnisse gefunden.");
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
                    bool isEQ = ElectricRadioButton.IsChecked == true;
                    bool both = BothRadioButton.IsChecked == true;
                    string price = PriceTextBox.Text;

                    // Basis-SQL-Abfrage
                    string searchQuery = $"SELECT * FROM {brand}";

                    // Bedingung für Modell hinzufügen, wenn angegeben
                    if (!string.IsNullOrEmpty(model))
                    {
                        searchQuery += " WHERE model=@model";
                    }

                    // Bedingung für Fahrzeugtyp hinzufügen
                    if (!both)
                    {
                        if (!string.IsNullOrEmpty(model))
                        {
                            searchQuery += " AND isEQ=@isEQ";
                        }
                        else
                        {
                            searchQuery += " WHERE isEQ=@isEQ";
                        }
                    }

                    // Bedingung für Preis hinzufügen
                    if (!string.IsNullOrEmpty(price))
                    {
                        if (!string.IsNullOrEmpty(model) || !both)
                        {
                            searchQuery += " AND price<=@price";
                        }
                        else
                        {
                            searchQuery += " WHERE price<=@price";
                        }
                    }

                    MySqlCommand searchCmd = new MySqlCommand(searchQuery, con);

                    // Parameter für Modell hinzufügen, wenn angegeben
                    if (!string.IsNullOrEmpty(model))
                    {
                        searchCmd.Parameters.AddWithValue("@model", model);
                    }

                    // Parameter für Fahrzeugtyp hinzufügen, wenn nicht beide ausgewählt sind
                    if (!both)
                    {
                        searchCmd.Parameters.AddWithValue("@isEQ", isEQ);
                    }

                    // Parameter für Preis hinzufügen
                    if (!string.IsNullOrEmpty(price))
                    {
                        searchCmd.Parameters.AddWithValue("@price", price);
                    }

                    MySqlDataReader reader = searchCmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        CreateResultTab(reader);
                    }
                    else
                    {
                        MessageBox.Show("Keine Ergebnisse gefunden.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}");
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

        private void SwitchToAddTab(object sender, MouseButtonEventArgs e)
        {
            MainTabControl.SelectedItem = AddTab;
        }

        private void CreateResultTab(MySqlDataReader reader)
        {
            TabItem resultTab = new TabItem();
            resultTab.Header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock { Text = "Ergebnis", FontWeight = FontWeights.Bold },
                    new Button { Content = "X", Width = 20, Height = 20, Margin = new Thickness(5, 0, 0, 0), Command = new RelayCommand(o => CloseTab(resultTab)) }
                }
            };
            resultTab.Background = Brushes.LightBlue;
            resultTab.Foreground = Brushes.Black;
            resultTab.Content = new DataGrid
            {
                Background = Brushes.LightGray,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                IsReadOnly = true,
                AutoGenerateColumns = true,
                ItemsSource = GetDataTableFromReader(reader).DefaultView
            };
            MainTabControl.Items.Insert(MainTabControl.Items.Count - 1, resultTab);
            MainTabControl.SelectedItem = resultTab;
        }

        private System.Data.DataTable GetDataTableFromReader(MySqlDataReader reader)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        private void CloseTab(TabItem tabItem)
        {
            MainTabControl.Items.Remove(tabItem);
        }

        private void Button_create_vehicle(object sender, RoutedEventArgs e)
        {
            string vin = NewVinTextBox.Text;
            string brand = (NewBrandComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string model = NewModelTextBox.Text;
            string year = NewYearTextBox.Text;
            string price = NewPriceTextBox.Text;

            // Validierung der VIN
            if (vin.Length != 17)
            {
                NewVinTextBox.BorderBrush = Brushes.Red;
                VinErrorLabel.Content = "VIN muss genau 17 Zeichen lang sein.";
                VinErrorLabel.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                NewVinTextBox.BorderBrush = Brushes.Green;
                VinErrorLabel.Visibility = Visibility.Collapsed;
            }

            // Validierung des Produktionsjahres
            if (!int.TryParse(year, out int productionYear) || productionYear < 1886 || productionYear > DateTime.Now.Year)
            {
                NewYearTextBox.BorderBrush = Brushes.Red;
                VinErrorLabel.Content = "Bitte geben Sie ein gültiges Produktionsjahr ein.";
                VinErrorLabel.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                NewYearTextBox.BorderBrush = Brushes.Green;
                VinErrorLabel.Visibility = Visibility.Collapsed;
            }

            // Validierung des Preises
            if (!decimal.TryParse(price, out decimal vehiclePrice) || vehiclePrice < 0)
            {
                NewPriceTextBox.BorderBrush = Brushes.Red;
                VinErrorLabel.Content = "Bitte geben Sie einen gültigen Preis ein.";
                VinErrorLabel.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                NewPriceTextBox.BorderBrush = Brushes.Green;
                VinErrorLabel.Visibility = Visibility.Collapsed;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(@"SERVER=localhost;DATABASE=projectx;UID=root;PASSWORD=IchBinBroke1!;"))
                {
                    con.Open();
                    // Überprüfen auf Duplikate
                    string checkQuery = $"SELECT COUNT(*) FROM {brand} WHERE vin=@vin";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@vin", vin);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                    {
                        NewVinTextBox.BorderBrush = Brushes.Red;
                        VinErrorLabel.Content = "Diese VIN existiert bereits in der Datenbank.";
                        VinErrorLabel.Visibility = Visibility.Visible;
                        return;
                    }

                    // Einfügen des neuen Fahrzeugs
                    bool isEQ = ElectricRadioButton2.IsChecked == true;
                    string insertQuery = $"INSERT INTO {brand} (vin, model, prod_year, isEQ, price) VALUES (@vin, @model, @year, @isEQ, @price)";
                    MySqlCommand insertCmd = new MySqlCommand(insertQuery, con);
                    insertCmd.Parameters.AddWithValue("@vin", vin);
                    insertCmd.Parameters.AddWithValue("@model", model);
                    insertCmd.Parameters.AddWithValue("@year", productionYear);
                    insertCmd.Parameters.AddWithValue("@isEQ", isEQ);
                    insertCmd.Parameters.AddWithValue("@price", vehiclePrice);
                    insertCmd.ExecuteNonQuery();
                    MessageBox.Show("Fahrzeug erfolgreich eingepflegt!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}");
            }
        }


        // Radiobuttons deaktivieren, wenn "Alle Antriebsarten" ausgewählt ist
        private void BothRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            CombustionRadioButton.IsEnabled = false;
            ElectricRadioButton.IsEnabled = false;
        }

        private void BothRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CombustionRadioButton.IsEnabled = true;
            ElectricRadioButton.IsEnabled = true;
        }

        // Checkbox deaktivieren, wenn entweder Verbrenner oder Elektro ausgewählt ist
        private void CombustionRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            BothRadioButton.IsChecked = false;
            BothRadioButton.IsEnabled = false;
        }

        private void ElectricRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            BothRadioButton.IsChecked = false;
            BothRadioButton.IsEnabled = false;
        }

        private void CombustionRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            BothRadioButton.IsEnabled = true;
        }

        private void ElectricRadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            BothRadioButton.IsEnabled = true;
        }
    }
}

// Definition der RelayCommand-Klasse
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
}
