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
                _dashboardVM.IsAddHabitModalOpen = true;
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
                if (_dashboardVM != null)
                {
                    await _dashboardVM.CreateHabitAsync();
                    _dashboardVM.IsAddHabitModalOpen = false;
                    await _dashboardVM.LoadHabitsAsync();
                }
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

        private void NewCategory_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null)
                _dashboardVM.IsCreateCategoryModalOpen = true;
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

        // ========== CREATE CATEGORY MODAL HANDLERS ==========
        private void CloseCreateCategoryModal_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null)
                _dashboardVM.IsCreateCategoryModalOpen = false;
        }

        private void CancelCreateCategory_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null)
                _dashboardVM.IsCreateCategoryModalOpen = false;
        }

        private void CreateCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var categoryNameBox = this.FindName("CategoryNameBoxCreateCategory") as System.Windows.Controls.TextBox;
            string categoryName = categoryNameBox?.Text?.Trim();
            
            if (string.IsNullOrEmpty(categoryName))
            {
                System.Windows.MessageBox.Show("Please enter a category name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            System.Windows.MessageBox.Show(
                $"Category '{categoryName}' will be created!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            if (_dashboardVM != null)
                _dashboardVM.IsCreateCategoryModalOpen = false;
        }

        private void CategoryIconButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                UpdateIconSelection(btn);
            }
        }

        private void CategoryColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                UpdateColorSelection(btn);
            }
        }

        private void UpdateIconSelection(System.Windows.Controls.Button selectedBtn)
        {
            var children = (selectedBtn.Parent as Panel)?.Children;
            if (children == null) return;

            var selectedBackground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 50, 138, 93));

            foreach (var child in children)
            {
                if (child is System.Windows.Controls.Button btn)
                {
                    btn.Background = btn == selectedBtn
                        ? selectedBackground
                        : System.Windows.Media.Brushes.White;
                    btn.Foreground = btn == selectedBtn
                        ? System.Windows.Media.Brushes.White
                        : System.Windows.Media.Brushes.Black;
                }
            }
        }

        private void UpdateColorSelection(System.Windows.Controls.Button selectedBtn)
        {
            var parentPanel = selectedBtn.Parent as System.Windows.Controls.StackPanel;
            if (parentPanel == null) return;

            foreach (var child in parentPanel.Children)
            {
                if (child is System.Windows.Controls.Button btn && btn.Width == 40)
                {
                    if (btn == selectedBtn)
                    {
                        btn.BorderThickness = new Thickness(3);
                        btn.BorderBrush = System.Windows.Media.Brushes.Black;
                    }
                    else
                    {
                        btn.BorderThickness = new Thickness(0);
                        btn.BorderBrush = null;
                    }
                }
            }
        }

        private void EditHabit_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null && sender is System.Windows.Controls.Button btn && btn.DataContext is Habits habit)
            {
                _dashboardVM.NewHabitName = habit.Name ?? string.Empty;
                var cat = _dashboardVM.Categories?.FirstOrDefault(c => c.Id == habit.CategoryId);
                if (cat != null) _dashboardVM.SelectedCategory = cat;
                var type = _dashboardVM.HabitTypes?.FirstOrDefault(t => t.Id == habit.HabitTypeId);
                if (type != null) _dashboardVM.SelectedType = type;
                _dashboardVM.IsAddHabitModalOpen = true;
            }
        }

        private void DeleteHabit_Click(object sender, RoutedEventArgs e)
        {
            if (_dashboardVM != null && sender is System.Windows.Controls.Button btn && btn.DataContext is Habits habit)
            {
                var res = System.Windows.MessageBox.Show($"Are you sure you want to deactivate '{habit.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    _dashboardVM.Habits.Remove(habit);
                    // TODO: call backend to mark habit as inactive/persist change
                }
            }
        }
    }
}
