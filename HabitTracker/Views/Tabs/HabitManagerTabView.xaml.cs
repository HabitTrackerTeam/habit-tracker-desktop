using System.Windows;
using System.Windows.Controls;
using HabitTracker.ViewModels;
using System.Linq;
using HabitTracker.Models;

namespace HabitTracker.Views.Tabs
{
    public partial class HabitManagerTabView : UserControl
    {
        private DashboardViewModel _dashboardVM => DataContext as DashboardViewModel;

        public HabitManagerTabView()
        {
            InitializeComponent();
            this.Loaded += HabitManagerTabView_Loaded;
        }

        private async void HabitManagerTabView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null)
            {
                await _dashboardVM.LoadHabitsAsync();
            }
        }

        // ========== ADD HABIT MODAL ==========
        private void AddHabit_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null)
            {
                _dashboardVM.ResetAddHabitForm();
                ResetModalControls();
                _dashboardVM.ModalTitle = "Add New Habit";
                _dashboardVM.ModalButtonText = "Plant Habit";
                _dashboardVM.IsAddHabitModalOpen = true;
            }
        }

        private void CloseAddHabitModal_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null)
                _dashboardVM.IsAddHabitModalOpen = false;
        }

        private void CancelAddHabit_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null)
                _dashboardVM.IsAddHabitModalOpen = false;
        }

        private async void PlantHabit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dashboardVM == null) return;

                // Collect data from UI controls into ViewModel
                var habitName = HabitNameBox?.Text?.Trim();
                if (habitName != null && habitName.StartsWith("e.g.,")) habitName = string.Empty;
                _dashboardVM.NewHabitName = habitName;

                // Goal
                if (double.TryParse(GoalBox?.Text, out double goal))
                    _dashboardVM.NewHabitGoal = goal;
                else
                    _dashboardVM.NewHabitGoal = 1;

                // Units
                if (UnitsCombo?.SelectedItem is System.Windows.Controls.ComboBoxItem unitItem)
                    _dashboardVM.NewHabitUnit = unitItem.Content?.ToString() ?? "count";



                // Frequency from RadioButtons
                if (FreqDaily?.IsChecked == true) _dashboardVM.NewHabitFrequency = "Daily";
                else if (FreqWeekly?.IsChecked == true) _dashboardVM.NewHabitFrequency = "Weekly";
                else if (FreqMonthly?.IsChecked == true) _dashboardVM.NewHabitFrequency = "Monthly";
                else if (FreqSpecific?.IsChecked == true) _dashboardVM.NewHabitFrequency = "Specific";

                // Days of week bitmask (from schedule buttons)
                if (_dashboardVM.NewHabitFrequency == "Specific")
                    _dashboardVM.NewHabitDaysOfWeek = CollectDaysOfWeekBitmask();
                else
                    _dashboardVM.NewHabitDaysOfWeek = 127; // all days

                await _dashboardVM.CreateHabitAsync();

                // Reset UI controls after success
                ResetModalControls();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas zapisywania nawyku: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var typeButtons = new Button?[] {
                    this.FindName("TypeNumericBtn") as Button,
                    this.FindName("TypeCheckboxBtn") as Button,
                    this.FindName("TypeTimerBtn") as Button
                };
                ResetButtonGroup(typeButtons, btn);

                // Set type on ViewModel
                if (_dashboardVM != null)
                {
                    if (btn == TypeNumericBtn) _dashboardVM.NewHabitType = "Numeric";
                    else if (btn == TypeCheckboxBtn) _dashboardVM.NewHabitType = "Checkbox";
                    else if (btn == TypeTimerBtn) _dashboardVM.NewHabitType = "Timer";
                }
            }
        }

        private void PriorityButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var priorityButtons = new Button?[] {
                    this.FindName("PriorityLowBtn") as Button,
                    this.FindName("PriorityMediumBtn") as Button,
                    this.FindName("PriorityHighBtn") as Button
                };
                ResetButtonGroup(priorityButtons, btn);

                // Set priority on ViewModel
                if (_dashboardVM != null)
                {
                    if (btn == PriorityLowBtn) _dashboardVM.NewHabitPriority = 3;
                    else if (btn == PriorityMediumBtn) _dashboardVM.NewHabitPriority = 2;
                    else if (btn == PriorityHighBtn) _dashboardVM.NewHabitPriority = 1;
                }
            }
        }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                var bgBrush = btn.Background as System.Windows.Media.SolidColorBrush;
                // If it's transparent/gray, we make it green
                bool isSelected = bgBrush != null && bgBrush.Color == System.Windows.Media.Color.FromArgb(255, 226, 232, 240); 

                if (isSelected)
                {
                    btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 22, 101, 52)); // #166534
                    btn.Foreground = System.Windows.Media.Brushes.White;
                    btn.FontWeight = FontWeights.Bold;
                }
                else
                {
                    btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 226, 232, 240)); // #E2E8F0
                    btn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 116, 139)); // #64748B
                    btn.FontWeight = FontWeights.Normal;
                }
            }
        }

        private void FreqRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (SchedulePanel == null) return;
            SchedulePanel.Visibility = (sender == FreqSpecific)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }



        private void ResetButtonGroup(Button?[] buttons, Button selectedBtn)
        {

            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                
                if (btn == selectedBtn)
                {
                    btn.Background = System.Windows.Media.Brushes.White;
                    btn.BorderThickness = new Thickness(0);
                    btn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 22, 101, 52)); // #166534
                    btn.FontWeight = FontWeights.Bold;
                }
                else
                {
                    btn.Background = System.Windows.Media.Brushes.Transparent;
                    btn.BorderThickness = new Thickness(0);
                    btn.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 116, 139)); // #64748B
                    btn.FontWeight = FontWeights.Normal;
                }
            }
        }


        private void HabitIconButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedBtn && clickedBtn.Parent is WrapPanel panel)
            {
                var greenBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 22, 101, 52)); // #166534
                var grayBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 241, 245, 249)); // #F1F5F9

                foreach (var child in panel.Children)
                {
                    if (child is Button btn)
                    {
                        if (btn == clickedBtn)
                        {
                            btn.Background = greenBackground;
                            btn.Foreground = System.Windows.Media.Brushes.White;
                        }
                        else
                        {
                            btn.Background = grayBackground;
                            btn.Foreground = System.Windows.Media.Brushes.Black;
                        }
                    }
                }

                if (_dashboardVM != null && clickedBtn.Content != null)
                {
                    _dashboardVM.NewHabitIcon = clickedBtn.Content.ToString();
                }
            }
        }

        private void EditHabit_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null && sender is System.Windows.Controls.Button btn && btn.DataContext is Habits habit)
            {
                _dashboardVM.EditingHabit = habit;
                
                // Populate VM values
                _dashboardVM.NewHabitName = habit.Name ?? string.Empty;
                _dashboardVM.NewHabitPriority = habit.Priority;
                _dashboardVM.NewHabitFrequency = habit.Period ?? "Daily";
                _dashboardVM.NewHabitDaysOfWeek = habit.DaysOfWeek;
                _dashboardVM.NewHabitIcon = habit.Icon ?? "❤️";
                _dashboardVM.NewHabitGoal = habit.TargetFrequency;
                _dashboardVM.NewHabitUnit = habit.Unit ?? habit.DefaultUnit ?? "count";
                _dashboardVM.ModalTitle = "Edit Habit";
                _dashboardVM.ModalButtonText = "Save Changes";
                
                var type = _dashboardVM.HabitTypes?.FirstOrDefault(t => t.Id == habit.HabitTypeId);
                if (type != null) 
                {
                    _dashboardVM.NewHabitType = type.DisplayType;
                }
                
                // Update UI control states
                if (HabitNameBox != null)
                {
                    HabitNameBox.Text = habit.Name;
                    HabitNameBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 15, 23, 42)); // Active color
                }
                if (GoalBox != null)
                {
                    GoalBox.Text = habit.TargetFrequency.ToString();
                }
                if (UnitsCombo != null)
                {
                    var targetUnit = habit.Unit ?? habit.DefaultUnit ?? "count";
                    foreach (System.Windows.Controls.ComboBoxItem item in UnitsCombo.Items)
                    {
                        if (string.Equals(item.Content?.ToString(), targetUnit, StringComparison.OrdinalIgnoreCase))
                        {
                            UnitsCombo.SelectedItem = item;
                            break;
                        }
                    }
                }
                
                // Habit Type Buttons
                Button? selectedTypeBtn = null;
                if (type?.DisplayType == "Numeric") selectedTypeBtn = TypeNumericBtn;
                else if (type?.DisplayType == "Checkbox") selectedTypeBtn = TypeCheckboxBtn;
                else if (type?.DisplayType == "Timer") selectedTypeBtn = TypeTimerBtn;
                
                if (selectedTypeBtn != null)
                {
                    var typeButtons = new[] { TypeNumericBtn, TypeCheckboxBtn, TypeTimerBtn };
                    ResetButtonGroup(typeButtons, selectedTypeBtn);
                }
                
                // Priority Buttons
                Button? selectedPriorityBtn = null;
                if (habit.Priority == 1) selectedPriorityBtn = PriorityHighBtn;
                else if (habit.Priority == 2) selectedPriorityBtn = PriorityMediumBtn;
                else if (habit.Priority == 3) selectedPriorityBtn = PriorityLowBtn;
                
                if (selectedPriorityBtn != null)
                {
                    var priorityButtons = new[] { PriorityLowBtn, PriorityMediumBtn, PriorityHighBtn };
                    ResetButtonGroup(priorityButtons, selectedPriorityBtn);
                }
                
                // Icon Buttons
                var iconButtons = new[] { IconBtn1, IconBtn2, IconBtn3, IconBtn4, IconBtn5, IconBtn6, IconBtn7, IconBtn8, IconBtn9, IconBtn10 };
                var greenBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 22, 101, 52)); // #166534
                var grayBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 241, 245, 249)); // #F1F5F9

                foreach (var iconBtn in iconButtons)
                {
                    if (iconBtn == null) continue;
                    if (iconBtn.Content?.ToString() == habit.Icon)
                    {
                        iconBtn.Background = greenBackground;
                        iconBtn.Foreground = System.Windows.Media.Brushes.White;
                    }
                    else
                    {
                        iconBtn.Background = grayBackground;
                        iconBtn.Foreground = System.Windows.Media.Brushes.Black;
                    }
                }
                
                // Frequency RadioButtons
                if (habit.Period == "Daily") FreqDaily.IsChecked = true;
                else if (habit.Period == "Weekly") FreqWeekly.IsChecked = true;
                else if (habit.Period == "Monthly") FreqMonthly.IsChecked = true;
                else if (habit.Period == "Specific") FreqSpecific.IsChecked = true;
                
                // Schedule Buttons (Days mask)
                UpdateScheduleButtonsFromMask(habit.DaysOfWeek);
                
                _dashboardVM.IsAddHabitModalOpen = true;
            }
        }

        private async void DeleteHabit_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null && sender is System.Windows.Controls.Button btn && btn.DataContext is Habits habit)
            {
                var res = System.Windows.MessageBox.Show($"Are you sure you want to deactivate '{habit.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    await _dashboardVM.DeleteHabitAsync(habit);
                }
            }
        }

        // Helper: Collect days of week bitmask from schedule buttons
        private int CollectDaysOfWeekBitmask()
        {
            int mask = 0;
            var dayButtons = new[] {
                (DayMonday, 64),    // bit 6
                (DayTuesday, 32),   // bit 5
                (DayWednesday, 16), // bit 4
                (DayThursday, 8),   // bit 3
                (DayFriday, 4),     // bit 2
                (DaySaturday, 2),   // bit 1
                (DaySunday, 1)      // bit 0
            };

            foreach (var (btn, bit) in dayButtons)
            {
                if (btn == null) continue;
                if (btn.Background is System.Windows.Media.SolidColorBrush brush)
                {
                    // Green (#166534) means selected
                    if (brush.Color.R == 22 && brush.Color.G == 101 && brush.Color.B == 52)
                        mask |= bit;
                }
            }
            return mask == 0 ? 127 : mask; // fallback to all days if none selected
        }

        private void UpdateScheduleButtonsFromMask(int mask)
        {
            var dayButtons = new[] {
                (DayMonday, 64),    // bit 6
                (DayTuesday, 32),   // bit 5
                (DayWednesday, 16), // bit 4
                (DayThursday, 8),   // bit 3
                (DayFriday, 4),     // bit 2
                (DaySaturday, 2),   // bit 1
                (DaySunday, 1)      // bit 0
            };

            var green = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 22, 101, 52)); // #166534
            var gray = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 226, 232, 240)); // #E2E8F0
            var textGreen = System.Windows.Media.Brushes.White;
            var textGray = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 116, 139)); // #64748B

            foreach (var (btn, bit) in dayButtons)
            {
                if (btn == null) continue;
                if ((mask & bit) != 0)
                {
                    btn.Background = green;
                    btn.Foreground = textGreen;
                    btn.FontWeight = FontWeights.Bold;
                }
                else
                {
                    btn.Background = gray;
                    btn.Foreground = textGray;
                    btn.FontWeight = FontWeights.Normal;
                }
            }
        }

        // Helper: Reset modal UI controls after successful creation
        private void ResetModalControls()
        {
            if (HabitNameBox != null)
            {
                HabitNameBox.Text = "e.g., Daily Meditation";
                HabitNameBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 148, 163, 184)); // #94A3B8
            }
            if (GoalBox != null) GoalBox.Text = string.Empty;
            if (UnitsCombo != null) UnitsCombo.SelectedIndex = 3; // default "count"
            if (FreqDaily != null) FreqDaily.IsChecked = true;

            // Reset buttons styles to default (Numeric, Medium, IconBtn1)
            var typeButtons = new[] { TypeNumericBtn, TypeCheckboxBtn, TypeTimerBtn };
            ResetButtonGroup(typeButtons, TypeNumericBtn);
            
            var priorityButtons = new[] { PriorityLowBtn, PriorityMediumBtn, PriorityHighBtn };
            ResetButtonGroup(priorityButtons, PriorityMediumBtn);
            
            // Icon WrapPanel Buttons
            var iconButtons = new[] { IconBtn1, IconBtn2, IconBtn3, IconBtn4, IconBtn5, IconBtn6, IconBtn7, IconBtn8, IconBtn9, IconBtn10 };
            var greenBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 22, 101, 52)); // #166534
            var grayBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 241, 245, 249)); // #F1F5F9
            foreach(var btn in iconButtons)
            {
                if (btn == null) continue;
                if (btn == IconBtn1)
                {
                    btn.Background = greenBackground;
                    btn.Foreground = System.Windows.Media.Brushes.White;
                }
                else
                {
                    btn.Background = grayBackground;
                    btn.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
            
            // Reset Days Mask to 127
            UpdateScheduleButtonsFromMask(127);
        }

        // ========== TEXTBOX WATERMARK HANDLERS ==========
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                if (tb.Text.StartsWith("e.g.,"))
                {
                    tb.Text = string.Empty;
                    tb.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 15, 23, 42)); // #0F172A
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    if (tb.Name == "HabitNameBox")
                        tb.Text = "e.g., Daily Meditation";

                        
                    tb.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 148, 163, 184)); // #94A3B8
                }
            }
        }
    }
}
