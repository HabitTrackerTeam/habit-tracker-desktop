using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HabitTracker.ViewModels;

namespace HabitTracker.Views.Tabs
{
    public partial class MeasurementsTabView : UserControl
    {
        public MeasurementsTabView()
        {
            InitializeComponent();
        }

        private async void SaveMeasurement_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MeasurementsViewModel vm)
            {
                await vm.SaveMeasurementAsync();
                vm.IsModalOpen = false; // Close modal on save
            }
        }

        /// <summary>
        /// Blocks non-numeric input. Allows digits, one decimal separator (. or ,).
        /// </summary>
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            string currentText = textBox?.Text ?? "";
            string newChar = e.Text;

            // Allow digits always
            if (Regex.IsMatch(newChar, @"^\d$"))
            {
                e.Handled = false;
                return;
            }

            // Allow a single decimal separator (. or ,)
            if (newChar == "." || newChar == ",")
            {
                if (currentText.Contains('.') || currentText.Contains(','))
                {
                    e.Handled = true; // Already has a separator
                }
                else
                {
                    e.Handled = false;
                }
                return;
            }

            // Block everything else
            e.Handled = true;
        }

        /// <summary>
        /// Blocks pasting non-numeric text.
        /// </summary>
        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                // Normalize commas to dots for validation
                string normalized = pastedText.Replace(',', '.');
                if (!double.TryParse(normalized, System.Globalization.NumberStyles.AllowDecimalPoint,
                    System.Globalization.CultureInfo.InvariantCulture, out _))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        /// <summary>
        /// Blocks space key in numeric fields.
        /// </summary>
        private void NumericTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}