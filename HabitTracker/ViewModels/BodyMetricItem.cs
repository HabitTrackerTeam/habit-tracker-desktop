using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HabitTracker.Models;

namespace HabitTracker.ViewModels
{
    public class BodyMetricItem : ViewModelBase
    {
        private string _partName;
        public string PartName
        {
            get => _partName;
            set { _partName = value; OnPropertyChanged(); }
        }

        private string _bodyPartId;
        public string BodyPartId
        {
            get => _bodyPartId;
            set { _bodyPartId = value; OnPropertyChanged(); }
        }

        private double _latestValue;
        public double LatestValue
        {
            get => _latestValue;
            set { _latestValue = value; OnPropertyChanged(); }
        }

        private double _delta;
        public double Delta
        {
            get => _delta;
            set { _delta = value; OnPropertyChanged(); }
        }

        private bool _isPositiveTrend;
        public bool IsPositiveTrend
        {
            get => _isPositiveTrend;
            set { _isPositiveTrend = value; OnPropertyChanged(); }
        }

        private string _unit = "cm";
        public string Unit
        {
            get => _unit;
            set { _unit = value; OnPropertyChanged(); }
        }
    }
}
