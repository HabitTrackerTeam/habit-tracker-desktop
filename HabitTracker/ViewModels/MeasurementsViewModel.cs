using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HabitTracker.Models;
using HabitTracker.Services;
using HabitTracker.Commands;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Globalization;
using System.Collections.Generic;

namespace HabitTracker.ViewModels
{
    public class MeasurementsViewModel : ViewModelBase
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ObservableCollection<BodyParts> BodyParts { get; set; } = new();
        public ObservableCollection<CircumferenceLogs> MeasurementLogs { get; set; } = new();

        public ObservableCollection<BodyParts> AvailableBodyParts { get; set; } = new();
        public ObservableCollection<MeasurementItemViewModel> CurrentSessionMeasurements { get; set; } = new();

        public ObservableCollection<BodyMetricItem> BodyMetrics { get; set; } = new();
        private BodyMetricItem _selectedBodyMetric;
        public BodyMetricItem SelectedBodyMetric
        {
            get => _selectedBodyMetric;
            set
            {
                _selectedBodyMetric = value;
                OnPropertyChanged();

                if (_selectedBodyMetric != null)
                {
                    UpdateChartDataAsync(_selectedBodyMetric.BodyPartId);
                }
            }
        }

        private double _currentWeight;
        public double CurrentWeight { get => _currentWeight; set { _currentWeight = value; OnPropertyChanged(); } }
        private double _weightDelta;
        public double WeightDelta { get => _weightDelta; set { _weightDelta = value; OnPropertyChanged(); } }
        private double _currentHeight;
        public double CurrentHeight { get => _currentHeight; set { _currentHeight = value; OnPropertyChanged(); } }
        private double _bmiValue;
        public double BmiValue { get => _bmiValue; set { _bmiValue = value; OnPropertyChanged(); } }
        private string _bmiStatus = "";
        public string BmiStatus { get => _bmiStatus; set { _bmiStatus = value; OnPropertyChanged(); } }

        private double? _newWeightValue;
        public double? NewWeightValue { get => _newWeightValue; set { _newWeightValue = value; OnPropertyChanged(); } }

        private double? _newHeightValue;
        public double? NewHeightValue { get => _newHeightValue; set { _newHeightValue = value; OnPropertyChanged(); } }

        private bool _isModalOpen;
        public bool IsModalOpen
        {
            get => _isModalOpen;
            set { _isModalOpen = value; OnPropertyChanged(); }
        }
        public ICommand OpenLogModalCommand { get; }
        public ICommand CloseLogModalCommand { get; }

        public ISeries[] ChartSeries { get; set; } = new ISeries[0];
        public Axis[] XAxes { get; set; } = new Axis[0];
        public Axis[] YAxes { get; set; } = new Axis[0];

        private BodyParts _selectedBodyPartToAdd;
        public BodyParts SelectedBodyPartToAdd
        {
            get => _selectedBodyPartToAdd;
            set
            {
                _selectedBodyPartToAdd = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddMeasurementCommand { get; }
        public ICommand RemoveMeasurementCommand { get; }

        private DateTime _measurementDate = DateTime.Now;
        public DateTime MeasurementDate
        {
            get => _measurementDate;
            set { _measurementDate = value; OnPropertyChanged(); }
        }

        public ICommand SaveMeasurementCommand { get; }

        public MeasurementsViewModel()
        {
            AddMeasurementCommand = new RelayCommand(ExecuteAddMeasurement, CanExecuteAddMeasurement);
            RemoveMeasurementCommand = new RelayCommand(ExecuteRemoveMeasurement);
            OpenLogModalCommand = new RelayCommand(_ => IsModalOpen = true);
            CloseLogModalCommand = new RelayCommand(_ => IsModalOpen = false);
            SaveMeasurementCommand = new AsyncRelayCommand(SaveMeasurementAsync);

            // Subscribe to theme changes to refresh chart axis colors
            HabitTracker.MainWindow.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(bool isDark)
        {
            // Refresh axis paint colors to match the new theme
            var labelColor = isDark ? new SKColor(160, 160, 160) : SKColors.Gray;
            var separatorColor = isDark ? new SKColor(80, 80, 80) : SKColors.LightGray;

            if (XAxes != null && XAxes.Length > 0)
            {
                XAxes[0].LabelsPaint = new SolidColorPaint(labelColor);
                OnPropertyChanged(nameof(XAxes));
            }
            if (YAxes != null && YAxes.Length > 0)
            {
                YAxes[0].SeparatorsPaint = new SolidColorPaint(separatorColor) { StrokeThickness = 1, PathEffect = new LiveChartsCore.SkiaSharpView.Painting.Effects.DashEffect(new float[] { 3, 3 }) };
                OnPropertyChanged(nameof(YAxes));
            }
        }

        private bool CanExecuteAddMeasurement(object obj)
        {
            return SelectedBodyPartToAdd != null;
        }

        private void ExecuteAddMeasurement(object obj)
        {
            if (SelectedBodyPartToAdd != null)
            {
                var bodyPart = SelectedBodyPartToAdd;

                CurrentSessionMeasurements.Add(new MeasurementItemViewModel
                {
                    BodyPartId = bodyPart.Id,
                    BodyPartName = bodyPart.DisplayName
                });

                AvailableBodyParts.Remove(bodyPart);
                SelectedBodyPartToAdd = AvailableBodyParts.FirstOrDefault();
            }
        }

        private void ExecuteRemoveMeasurement(object obj)
        {
            if (obj is MeasurementItemViewModel item)
            {
                CurrentSessionMeasurements.Remove(item);

                var bodyPart = BodyParts.FirstOrDefault(bp => bp.Id == item.BodyPartId);
                if (bodyPart != null)
                {
                    AvailableBodyParts.Add(bodyPart);
                }

                if (SelectedBodyPartToAdd == null)
                {
                    SelectedBodyPartToAdd = AvailableBodyParts.FirstOrDefault();
                }
            }
        }

        public async Task LoadMeasurementsAsync()
        {
            try
            {
                IsLoading = true;
                var parts = await SupabaseService.Client.From<BodyParts>().Get();
                BodyParts = new ObservableCollection<BodyParts>(parts.Models);
                OnPropertyChanged(nameof(BodyParts));

                AvailableBodyParts.Clear();
                foreach (var part in BodyParts)
                {
                    AvailableBodyParts.Add(part);
                }
                SelectedBodyPartToAdd = AvailableBodyParts.FirstOrDefault();

                CurrentSessionMeasurements.Clear();

                var logs = await SupabaseService.Client.From<CircumferenceLogs>().Get();
                MeasurementLogs = new ObservableCollection<CircumferenceLogs>(logs.Models);
                OnPropertyChanged(nameof(MeasurementLogs));

                // Process logs to populate BodyMetrics
                BodyMetrics.Clear();
                foreach (var part in BodyParts)
                {
                    var partLogs = MeasurementLogs.Where(l => l.BodyPartId == part.Id)
                                                  .OrderByDescending(l => l.Session?.MeasurementDate).ToList();

                    if (partLogs.Any())
                    {
                        var latest = partLogs.First().Value;
                        double previous = partLogs.Count > 1 ? partLogs[1].Value : latest;
                        double delta = latest - previous;

                        BodyMetrics.Add(new BodyMetricItem
                        {
                            BodyPartId = part.Id,
                            PartName = part.DisplayName,
                            LatestValue = latest,
                            Delta = delta,
                            IsPositiveTrend = delta > 0
                        });
                    }
                }

                if (BodyMetrics.Any() && SelectedBodyMetric == null)
                {
                    SelectedBodyMetric = BodyMetrics.First();
                }

                // Temporary weight and height logic
                string currentUserId = SupabaseService.Client.Auth.CurrentUser?.Id;
                var bodyMetricsDb = await SupabaseService.Client.From<HabitTracker.Models.BodyMetrics>()
                    .Where(m => m.UserId == currentUserId)
                    .Get();
                var metricsRecords = bodyMetricsDb.Models.OrderByDescending(m => m.MeasurementDate).ThenByDescending(m => m.Id).ToList();
                var latestBodyMetric = metricsRecords.FirstOrDefault();
                if (latestBodyMetric != null)
                {
                    CurrentWeight = latestBodyMetric.Weight;
                    CurrentHeight = latestBodyMetric.Height;

                    if (metricsRecords.Count > 1)
                    {
                        WeightDelta = CurrentWeight - metricsRecords[1].Weight;
                    }

                    CalculateBmi();

                    double heightDelta = metricsRecords.Count > 1 ? CurrentHeight - metricsRecords[1].Height : 0;

                    BodyMetrics.Insert(0, new BodyMetricItem
                    {
                        BodyPartId = "HEIGHT",
                        PartName = LocalizationService.Instance.Height,
                        LatestValue = CurrentHeight,
                        Delta = heightDelta,
                        IsPositiveTrend = heightDelta > 0,
                        Unit = "cm"
                    });

                    BodyMetrics.Insert(0, new BodyMetricItem
                    {
                        BodyPartId = "WEIGHT",
                        PartName = LocalizationService.Instance.Weight,
                        LatestValue = CurrentWeight,
                        Delta = WeightDelta,
                        IsPositiveTrend = WeightDelta > 0,
                        Unit = "kg"
                    });
                }
                
                if (SelectedBodyMetric == null && BodyMetrics.Any())
                {
                    SelectedBodyMetric = BodyMetrics.First();
                }
            }
            catch (Exception ex)
            {
                HabitTracker.Views.CustomMessageBox.Show(LocalizationService.Instance.MeasLoadError + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculateBmi()
        {
            if (CurrentHeight > 0)
            {
                var heightInMeters = CurrentHeight / 100.0;
                BmiValue = Math.Round(CurrentWeight / (heightInMeters * heightInMeters), 1);

                var loc = LocalizationService.Instance;
                if (BmiValue < 18.5) BmiStatus = loc.BmiUnderweight;
                else if (BmiValue < 25) BmiStatus = loc.BmiNormal;
                else if (BmiValue < 30) BmiStatus = loc.BmiOverweight;
                else BmiStatus = loc.BmiObese;
            }
        }

        private async void UpdateChartDataAsync(string bodyPartId)
        {
            try
            {
                var values = new List<double>();
                var labels = new List<string>();

                if (bodyPartId == "WEIGHT" || bodyPartId == "HEIGHT")
                {
                    var currentUserId = SupabaseService.Client.Auth.CurrentUser?.Id;
                    var logs = await SupabaseService.Client.From<HabitTracker.Models.BodyMetrics>()
                        .Where(m => m.UserId == currentUserId)
                        .Get();

                    var sortedLogs = logs.Models.OrderBy(m => m.MeasurementDate).ToList();

                    foreach (var log in sortedLogs)
                    {
                        values.Add(bodyPartId == "WEIGHT" ? log.Weight : log.Height);
                        labels.Add(log.MeasurementDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    var logs = await SupabaseService.Client.From<CircumferenceLogs>()
                        .Where(l => l.BodyPartId == bodyPartId)
                        .Get();

                    var sortedLogs = logs.Models
                        .Where(l => l.Session != null)
                        .OrderBy(l => l.Session.MeasurementDate)
                        .ToList();

                    foreach (var log in sortedLogs)
                    {
                        values.Add(log.Value);
                        labels.Add(log.Session.MeasurementDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture));
                    }
                }

                ChartSeries = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = values,
                        Name = "Measurement",
                        Fill = new SolidColorPaint(new SKColor(50, 138, 93).WithAlpha(50)), // Green-ish transparent fill
                        Stroke = new SolidColorPaint(new SKColor(50, 138, 93)) { StrokeThickness = 3 },
                        GeometryFill = new SolidColorPaint(new SKColor(255, 255, 255)),
                        GeometryStroke = new SolidColorPaint(new SKColor(50, 138, 93)) { StrokeThickness = 2 }
                    }
                };
                OnPropertyChanged(nameof(ChartSeries));

                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = labels,
                        LabelsPaint = new SolidColorPaint(SKColors.Gray),
                        TextSize = 12
                    }
                };
                OnPropertyChanged(nameof(XAxes));

                YAxes = new Axis[]
                {
                    new Axis
                    {
                        LabelsPaint = new SolidColorPaint(SKColors.Transparent), // Hide Y Axis labels if we want it clean like the UI
                        SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1, PathEffect = new LiveChartsCore.SkiaSharpView.Painting.Effects.DashEffect(new float[] { 3, 3 }) }
                    }
                };
                OnPropertyChanged(nameof(YAxes));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating chart: {ex.Message}");
            }
        }

        public async Task SaveMeasurementAsync()
        {
            if (!CurrentSessionMeasurements.Any() && !NewWeightValue.HasValue && !NewHeightValue.HasValue)
            {
                HabitTracker.Views.CustomMessageBox.Show(LocalizationService.Instance.MeasAddAtLeastOne);
                return;
            }

            var validItems = CurrentSessionMeasurements.Where(i => i.Value.HasValue && i.Value.Value > 0).ToList();
            if (validItems.Count != CurrentSessionMeasurements.Count)
            {
                HabitTracker.Views.CustomMessageBox.Show(LocalizationService.Instance.MeasAllValid);
                return;
            }

            try
            {
                IsLoading = true;

                var userId = SupabaseService.Client.Auth.CurrentUser.Id;

                // 1. BEZPIECZNA DATA: Wymuszamy czas UTC, żeby baza PostgreSQL (typ date) nie przesunęła nam dnia
                var todayUtc = DateTime.SpecifyKind(MeasurementDate.Date, DateTimeKind.Utc);
                var todayDateString = MeasurementDate.ToString("yyyy-MM-dd");

                // 2. ZAPIS WAGI I WZROSTU (Prawdziwy UPSERT)
                if (NewWeightValue.HasValue || NewHeightValue.HasValue)
                {
                    double w = NewWeightValue ?? CurrentWeight;
                    double h = NewHeightValue ?? CurrentHeight;

                    var bmEntry = new HabitTracker.Models.BodyMetrics
                    {
                        Id = Guid.NewGuid().ToString(), // Generate ID in case of insert, Postgres will ignore on conflict if we specify upsert logic correctly
                        UserId = userId,
                        MeasurementDate = todayUtc,
                        Weight = w,
                        Height = h,
                        AdditionalNotes = ""
                    };

                    // Mówimy bazie: "Spróbuj wstawić. Jak znajdziesz konflikt dla tego użytkownika i tej daty, po prostu zaktualizuj wagę/wzrost"
                    var options = new Supabase.Postgrest.QueryOptions { OnConflict = "user_id, measurement_date" };
                    await SupabaseService.Client.From<HabitTracker.Models.BodyMetrics>().Upsert(bmEntry, options);

                    // Wyczyszczenie inputów po udanym zapisie
                    NewWeightValue = null;
                    NewHeightValue = null;
                }

                // Proceed with existing circumferences setup only if there are items
                if (validItems.Any())
                {
                    // 1. Fetch or Create Session using exact date filtering
                    var userSessions = await SupabaseService.Client.From<MeasurementSessions>()
                        .Where(s => s.UserId == userId)
                        .Get();

                    var existingSession = userSessions.Models.FirstOrDefault(s => s.MeasurementDate.ToString("yyyy-MM-dd") == todayDateString);

                    string sessionId;
                    if (existingSession != null)
                    {
                        // Session exists
                        sessionId = existingSession.Id;
                    }
                    else
                    {
                        // Create new session
                        var session = new MeasurementSessions
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = userId,
                            MeasurementDate = todayUtc,
                            AdditionalNotes = ""
                        };
                        var sessionResponse = await SupabaseService.Client.From<MeasurementSessions>().Insert(session);
                        sessionId = sessionResponse.Models.FirstOrDefault()?.Id;
                    }

                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        // 2. Upsert circumference logs
                        foreach (var item in validItems)
                        {
                            // Fetch existing to see if we update or insert (to avoid generating new Id on update which violates unique constraints)
                            var existingLogResponse = await SupabaseService.Client.From<CircumferenceLogs>()
                                .Where(l => l.SessionId == sessionId && l.BodyPartId == item.BodyPartId)
                                .Get();

                            var existingLog = existingLogResponse.Models.FirstOrDefault();

                            if (existingLog != null)
                            {
                                existingLog.Value = item.Value.Value;
                                await SupabaseService.Client.From<CircumferenceLogs>().Update(existingLog);
                            }
                            else
                            {
                                var log = new CircumferenceLogs
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    SessionId = sessionId,
                                    BodyPartId = item.BodyPartId,
                                    Value = item.Value.Value
                                };
                                await SupabaseService.Client.From<CircumferenceLogs>().Insert(log);
                            }
                        }

                        CurrentSessionMeasurements.Clear();

                        AvailableBodyParts.Clear();
                        foreach (var part in BodyParts)
                        {
                            AvailableBodyParts.Add(part);
                        }
                        SelectedBodyPartToAdd = AvailableBodyParts.FirstOrDefault();

                        MeasurementDate = DateTime.Now;
                    }
                } // This closes: if (validItems.Any())

                await LoadMeasurementsAsync();
                HabitTracker.Views.CustomMessageBox.Show(LocalizationService.Instance.MeasSavedOk);
            }
            catch (Exception ex)
            {
                HabitTracker.Views.CustomMessageBox.Show(LocalizationService.Instance.MeasSaveError + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
