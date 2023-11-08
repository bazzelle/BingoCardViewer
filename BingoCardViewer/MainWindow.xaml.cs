using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32; // Add this for OpenFileDialog

namespace BingoCardViewer
{
    public partial class MainWindow : Window
    {
        private const string BingoCardFilePath = @"C:\temp\bingo\bingo_card.json";
        private const string CalledNumbersFilePath = @"C:\temp\bingo\Called Numbers.txt";
        private List<BingoNumber> bingoNumbers = new List<BingoNumber>();
        private DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            //LoadBingoCard();
            SetupTimer();
        }

        private void LoadBingoCard()
        {
            var bingoCardData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(BingoCardFilePath));

            // Clear the existing numbers
            bingoNumbers.Clear();

            // Add the BINGO headers
            foreach (char c in "BINGO")
            {
                bingoNumbers.Add(new BingoNumber { Number = c.ToString(), Background = Brushes.LightGray });
            }

            // Add the numbers
            foreach (var columnName in bingoCardData.Keys)
            {
                foreach (string value in bingoCardData[columnName])
                {
                    bingoNumbers.Add(new BingoNumber { Number = value, Background = Brushes.White });
                }
            }

            BingoItemsControl.ItemsSource = bingoNumbers;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var bingoNumber = button.DataContext as BingoNumber;

            // Check if the button is a header by verifying its content
            if ("BINGO".Contains(bingoNumber.Number))
            {
                // It's a header, do nothing
                return;
            }

            // It's a number, toggle the background color
            button.Background = button.Background == Brushes.Red ? Brushes.White : Brushes.Red;
        }

        private void SetupTimer()
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCalledNumbers();
        }

        private void UpdateCalledNumbers()
        {
            try
            {
                string[] calledNumbers = File.ReadAllLines(CalledNumbersFilePath);
                CalledNumbersTextBlock.Text = string.Join(" ", calledNumbers);
            }
            catch (IOException ex)
            {
                // Handle file read exceptions here if necessary
                MessageBox.Show("Error reading called numbers: " + ex.Message);
            }
        }

        private void LoadNewCardButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Read the new bingo card file
                    var bingoCardData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(openFileDialog.FileName));

                    // Clear the existing numbers
                    bingoNumbers.Clear();

                    // Add the BINGO headers
                    foreach (char c in "BINGO")
                    {
                        bingoNumbers.Add(new BingoNumber { Number = c.ToString(), Background = Brushes.LightGray });
                    }

                    // Add the numbers from the new file
                    foreach (var columnName in bingoCardData.Keys)
                    {
                        foreach (string value in bingoCardData[columnName])
                        {
                            bingoNumbers.Add(new BingoNumber { Number = value, Background = Brushes.White });
                        }
                    }

                    BingoItemsControl.ItemsSource = null; // Reset ItemsSource
                    BingoItemsControl.ItemsSource = bingoNumbers; // Set new ItemsSource
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading new card: " + ex.Message);
                }
            }
        }
    }

    public class BingoNumber
    {
        public string Number { get; set; }
        public Brush Background { get; set; }
    }
}
