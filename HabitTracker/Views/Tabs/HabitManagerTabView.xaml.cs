using System.Windows;
using System.Windows.Controls;
using HabitTracker.ViewModels;
using System.Linq;
using HabitTracker.Models;
using System.Windows.Media;
using System.Windows.Input;
using HabitTracker.Services;
using Supabase.Postgrest;

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
                    _dashboardVM.NewHabitDaysOfWeek = 0; // default no days selected

                await _dashboardVM.CreateHabitAsync();

                // Reset UI controls after success
                ResetModalControls();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Wystąpił błąd podczas zapisywania nawyku: {ex.Message}");
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
                // If it's transparent/gray (has no explicit local background), we make it green
                var bgBrush = btn.Background as System.Windows.Media.SolidColorBrush;
                bool isSelected = bgBrush != null && bgBrush.Color == System.Windows.Media.Color.FromArgb(255, 22, 101, 52); 

                if (!isSelected)
                {
                    btn.ClearValue(Control.BackgroundProperty);
                    btn.ClearValue(Control.ForegroundProperty);
                    btn.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 22, 101, 52)); // #166534
                    btn.Foreground = System.Windows.Media.Brushes.White;
                    btn.FontWeight = FontWeights.Bold;
                }
                else
                {
                    btn.ClearValue(Control.BackgroundProperty);
                    btn.ClearValue(Control.ForegroundProperty);
                    btn.SetResourceReference(Control.BackgroundProperty, "InputBgBrush");
                    btn.SetResourceReference(Control.ForegroundProperty, "TextMutedBrush");
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
                    btn.ClearValue(Control.BackgroundProperty);
                    btn.SetResourceReference(Control.BackgroundProperty, "CardBgBrush");
                    btn.BorderThickness = new Thickness(0);
                    btn.ClearValue(Control.ForegroundProperty);
                    btn.SetResourceReference(Control.ForegroundProperty, "AccentGreenBrush");
                    btn.FontWeight = FontWeights.Bold;
                }
                else
                {
                    btn.ClearValue(Control.BackgroundProperty);
                    btn.Background = System.Windows.Media.Brushes.Transparent;
                    btn.BorderThickness = new Thickness(0);
                    btn.ClearValue(Control.ForegroundProperty);
                    btn.SetResourceReference(Control.ForegroundProperty, "TextMutedBrush");
                    btn.FontWeight = FontWeights.Normal;
                }
            }
        }        private void SelectIconButton_Click(object sender, RoutedEventArgs e)
        {
            if (IconPickerPopup != null)
            {
                InitializeEmojiGrid();
                IconPickerPopup.IsOpen = true;
            }
        }

        private bool _isEmojiGridInitialized = false;
        private void InitializeEmojiGrid()
        {
            if (_isEmojiGridInitialized || EmojiWrapPanel == null) return;

            string[] emojis = new string[]
            {
                "❤️", "💪", "🏃", "🧘", "💧", "🚴", "🚶", "🥗", "🍎", "🥦", "🥛", "😴", "🦷",
                "🧠", "📖", "✏️", "🎯", "💼", "💻", "📚", "✍️", "🎓", "⏰", "📅", "📝",
                "🧹", "🪴", "🎨", "🎵", "🎸", "🎮", "📸", "🍳", "☕", "🍵", "🐱", "🐶", "🚗", "🔧",
                "🌱", "⭐", "🏆", "💰", "📈", "🔑", "🛡️", "🔋", "🎒", "🌍", "✈️", "☀️", "🌙", "🍀"
            };

            EmojiWrapPanel.Children.Clear();
            foreach (var emoji in emojis)
            {
                var btn = new Button
                {
                    Content = emoji,
                    Style = (Style)FindResource("EmojiButtonStyle")
                };


                btn.Click += (s, ev) =>
                {
                    if (_dashboardVM != null)
                    {
                        _dashboardVM.NewHabitIcon = emoji;
                    }
                    if (SelectedIconPreview != null)
                    {
                        SelectedIconPreview.Text = emoji;
                    }
                    if (IconPickerPopup != null)
                    {
                        IconPickerPopup.IsOpen = false;
                    }
                };

                EmojiWrapPanel.Children.Add(btn);
            }

            _isEmojiGridInitialized = true;
        }

        // ========== DRAG & DROP SORTING ==========
        private void DragHandle_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement dragHandle && dragHandle.DataContext is Habits draggedHabit)
            {
                var parentRow = FindParent<Border>(dragHandle);
                if (parentRow != null)
                {
                    var scaleTransform = new ScaleTransform(1.0, 1.0);
                    parentRow.RenderTransform = scaleTransform;
                    parentRow.RenderTransformOrigin = new Point(0.5, 0.5);
                    
                    var anim = new System.Windows.Media.Animation.DoubleAnimation(0.97, TimeSpan.FromMilliseconds(150));
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                }

                DragDrop.DoDragDrop(dragHandle, draggedHabit, DragDropEffects.Move);

                if (parentRow != null && parentRow.RenderTransform is ScaleTransform st)
                {
                    var anim = new System.Windows.Media.Animation.DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150));
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                }
            }
        }

        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
        }

        private void HabitRow_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Habits)) is Habits draggedHabit && sender is FrameworkElement targetElement && targetElement.DataContext is Habits targetHabit)
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
                
                if (draggedHabit.Id == targetHabit.Id || _dashboardVM == null) return;
                
                var habits = _dashboardVM.Habits;
                int oldIndex = habits.IndexOf(draggedHabit);
                int newIndex = habits.IndexOf(targetHabit);
                
                if (oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex)
                {
                    habits.Move(oldIndex, newIndex);
                }
            }
        }

        private async void HabitRow_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(Habits)) is Habits draggedHabit && sender is FrameworkElement targetElement && targetElement.DataContext is Habits targetHabit)
            {
                if (draggedHabit.Id == targetHabit.Id || _dashboardVM == null) return;

                var habits = _dashboardVM.Habits;
                int oldIndex = habits.IndexOf(draggedHabit);
                int newIndex = habits.IndexOf(targetHabit);

                if (oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex)
                {
                    habits.Move(oldIndex, newIndex);

                    try
                    {
                        for (int i = 0; i < habits.Count; i++)
                        {
                            habits[i].SortOrder = i;
                            await SupabaseService.Client.From<Habits>()
                                .Filter("id", Constants.Operator.Equals, habits[i].Id)
                                .Set(h => h.SortOrder, i)
                                .Update();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error updating sort order: {ex.Message}");
                    }
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
                _dashboardVM.NewHabitIcon = string.IsNullOrEmpty(habit.Icon) ? "❓" : habit.Icon;
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
                    HabitNameBox.SetResourceReference(Control.ForegroundProperty, "TextMainBrush");
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
                // Icon Preview
                if (SelectedIconPreview != null)
                {
                    SelectedIconPreview.Text = string.IsNullOrEmpty(habit.Icon) ? "❓" : habit.Icon;
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
                await _dashboardVM.DeleteHabitAsync(habit);
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
            return mask == 0 ? 0 : mask; // fallback to no days if none selected
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
            var textGreen = System.Windows.Media.Brushes.White;

            foreach (var (btn, bit) in dayButtons)
            {
                if (btn == null) continue;
                if ((mask & bit) != 0)
                {
                    btn.ClearValue(Control.BackgroundProperty);
                    btn.ClearValue(Control.ForegroundProperty);
                    btn.Background = green;
                    btn.Foreground = textGreen;
                    btn.FontWeight = FontWeights.Bold;
                }
                else
                {
                    btn.ClearValue(Control.BackgroundProperty);
                    btn.ClearValue(Control.ForegroundProperty);
                    btn.SetResourceReference(Control.BackgroundProperty, "InputBgBrush");
                    btn.SetResourceReference(Control.ForegroundProperty, "TextMutedBrush");
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
                HabitNameBox.SetResourceReference(Control.ForegroundProperty, "TextMutedBrush");
            }
            if (GoalBox != null) GoalBox.Text = string.Empty;
            if (UnitsCombo != null) UnitsCombo.SelectedIndex = 3; // default "count"
            if (FreqDaily != null) FreqDaily.IsChecked = true;

            // Reset buttons styles to default (Numeric, Medium, IconBtn1)
            var typeButtons = new[] { TypeNumericBtn, TypeCheckboxBtn, TypeTimerBtn };
            ResetButtonGroup(typeButtons, TypeNumericBtn);
            
            var priorityButtons = new[] { PriorityLowBtn, PriorityMediumBtn, PriorityHighBtn };
            ResetButtonGroup(priorityButtons, PriorityMediumBtn);
            
            // Icon Preview Reset
            if (SelectedIconPreview != null)
            {
                SelectedIconPreview.Text = "❓";
            }
            
            // Reset Days Mask to 0 (no days selected)
            UpdateScheduleButtonsFromMask(0);
        }

        // ========== TEXTBOX WATERMARK HANDLERS ==========
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                if (tb.Text.StartsWith("e.g.,"))
                {
                    tb.Text = string.Empty;
                    tb.SetResourceReference(Control.ForegroundProperty, "TextMainBrush");
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

                        
                    tb.SetResourceReference(Control.ForegroundProperty, "TextMutedBrush");
                }
            }
        }
    }
}
