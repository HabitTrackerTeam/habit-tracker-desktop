using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HabitTracker.ViewModels
{
    public class MeasurementItemViewModel : ViewModelBase
    {
        public string BodyPartId { get; set; }
        public string BodyPartName { get; set; }

        private string _value = "";
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ParsedValue));
            }
        }

        /// <summary>
        /// Returns the parsed double value (supports '.' and ','), or null if invalid.
        /// </summary>
        public double? ParsedValue
        {
            get
            {
                if (MeasurementsViewModel.TryParseDouble(Value, out double result))
                    return result;
                return null;
            }
        }
    }
}