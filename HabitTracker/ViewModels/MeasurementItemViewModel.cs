using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HabitTracker.ViewModels
{
    public class MeasurementItemViewModel : ViewModelBase
    {
        public string BodyPartId { get; set; }
        public string BodyPartName { get; set; }

        private double? _value;
        public double? Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
    }
}