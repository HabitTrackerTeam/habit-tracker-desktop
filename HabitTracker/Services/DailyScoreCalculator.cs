using System;
using System.Collections.Generic;
using System.Linq;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    /// <summary>
    /// Pure, stateless calculator for the Daily Score (0-100%).
    /// 
    /// This is the SINGLE SOURCE OF TRUTH for day scoring used by both
    /// the Home dashboard (real-time) and the Calendar view (historical).
    /// 
    /// Business rules implemented:
    ///   1. Frequency filtering — daily habits always count; weekly-on-specific-days
    ///      count only when the evaluated date matches; flexible habits are EXCLUDED.
    ///   2. Scoring — checkbox: 0.0 or 1.0; numeric/timer: proportional
    ///      (actual / target), capped at 1.0.
    ///   3. Weighting — the final score is a weighted average where each habit's
    ///      score is multiplied by its priority value (3 = High = ×3, 2 = Medium = ×2,
    ///      1 = Low = ×1).
    /// </summary>
    public static class DailyScoreCalculator
    {
        // ──────────────────────────────────────────────
        //  Public result types
        // ──────────────────────────────────────────────

        /// <summary>
        /// Result of scoring a single day.
        /// </summary>
        public sealed class DailyScoreResult
        {
            /// <summary>Final day score rounded to 0-100.</summary>
            public int Percentage { get; init; }

            /// <summary>Number of habits that were planned (and scored) for this day.</summary>
            public int PlannedCount { get; init; }

            /// <summary>Per-habit breakdown (useful for the Calendar detail panel).</summary>
            public IReadOnlyList<HabitScoreDetail> Details { get; init; } = Array.Empty<HabitScoreDetail>();
        }

        /// <summary>
        /// Scoring detail for a single habit on a given day.
        /// </summary>
        public sealed class HabitScoreDetail
        {
            public string HabitId { get; init; } = "";
            public string HabitName { get; init; } = "";

            /// <summary>The resolved type name: "checkbox", "numeric", or "timer".</summary>
            public string TypeName { get; init; } = "checkbox";

            /// <summary>Raw score 0.0–1.0 (proportional, capped).</summary>
            public double RawScore { get; init; }

            /// <summary>Priority weight that was applied.</summary>
            public double Weight { get; init; }

            /// <summary>Whether the habit is considered fully completed (score == 1.0).</summary>
            public bool IsCompleted { get; init; }

            /// <summary>Human-readable status text for the UI: "DONE", "MISSED", or "5/8 H" etc.</summary>
            public string StatusText { get; init; } = "MISSED";
        }

        // ──────────────────────────────────────────────
        //  Main entry point
        // ──────────────────────────────────────────────

        /// <summary>
        /// Calculates the weighted daily score for <paramref name="date"/>.
        /// </summary>
        /// <param name="date">The calendar date being evaluated.</param>
        /// <param name="allHabits">All active (non-archived) habits of the user.</param>
        /// <param name="dayLogs">Habit log entries whose LogDate falls on <paramref name="date"/>.</param>
        /// <param name="habitTypeMap">Lookup from HabitType.Id → HabitTypes model.</param>
        /// <returns>A <see cref="DailyScoreResult"/> with the percentage and per-habit details.</returns>
        public static DailyScoreResult CalculateDailyScore(
            DateTime date,
            IEnumerable<Habits> allHabits,
            IEnumerable<HabitLogs> dayLogs,
            Dictionary<string, HabitTypes> habitTypeMap)
        {
            if (allHabits == null) throw new ArgumentNullException(nameof(allHabits));

            var logsByHabitId = (dayLogs ?? Enumerable.Empty<HabitLogs>())
                .GroupBy(l => l.HabitId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(l => l.UpdatedTime).First());

            var details = new List<HabitScoreDetail>();
            double totalWeight = 0.0;
            double weightedScore = 0.0;

            foreach (var habit in allHabits)
            {
                // ── Rule 1: Frequency filtering ──────────────────────

                // Skip habits created after the evaluated date
                if (habit.CreatedDate.Date > date.Date) continue;

                // Flexible habits ("3× per week") are EXCLUDED from daily scoring
                if (habit.IsFlexible) continue;

                // Weekly and Monthly habits are EXCLUDED from daily scoring
                if (string.Equals(habit.Period, "weekly", StringComparison.OrdinalIgnoreCase) || 
                    string.Equals(habit.Period, "monthly", StringComparison.OrdinalIgnoreCase)) continue;

                // Check day-of-week schedule (mask == 0 means "every day")
                if (!IsScheduledForDay(habit.DaysOfWeek, date.DayOfWeek)) continue;

                // ── Resolve habit type ───────────────────────────────
                habitTypeMap.TryGetValue(habit.HabitTypeId ?? "", out var habitType);
                var typeName = habitType?.Type?.ToLower() ?? "checkbox";

                // ── Lookup log entry ─────────────────────────────────
                logsByHabitId.TryGetValue(habit.Id, out var log);

                // ── Rule 2: Scoring ──────────────────────────────────
                double rawScore = ScoreHabit(log, habit, typeName);

                // ── Rule 3: Weighting ────────────────────────────────
                double weight = GetPriorityWeight(habit.Priority);
                totalWeight += weight;
                weightedScore += rawScore * weight;

                // ── Build status text for UI ─────────────────────────
                bool isCompleted = rawScore >= 1.0;
                string statusText = BuildStatusText(log, habit, habitType, typeName, isCompleted);

                details.Add(new HabitScoreDetail
                {
                    HabitId = habit.Id,
                    HabitName = habit.Name,
                    TypeName = typeName,
                    RawScore = rawScore,
                    Weight = weight,
                    IsCompleted = isCompleted,
                    StatusText = statusText
                });
            }

            int percentage = details.Count > 0 && totalWeight > 0
                ? (int)Math.Round(weightedScore / totalWeight * 100.0)
                : 0;

            return new DailyScoreResult
            {
                Percentage = percentage,
                PlannedCount = details.Count,
                Details = details
            };
        }

        // ──────────────────────────────────────────────
        //  Overload for Home dashboard (in-memory state)
        // ──────────────────────────────────────────────

        /// <summary>
        /// Convenience overload that synthesizes HabitLogs from the in-memory
        /// Habits state (CurrentProgress, IsCompleted) so the Home dashboard
        /// can call the same algorithm without constructing log objects manually.
        /// </summary>
        public static DailyScoreResult CalculateFromLiveState(
            IEnumerable<Habits> activeHabits,
            Dictionary<string, HabitTypes> habitTypeMap)
        {
            var today = DateTime.Today;

            // Build synthetic logs from in-memory UI state
            var syntheticLogs = new List<HabitLogs>();
            foreach (var h in activeHabits)
            {
                syntheticLogs.Add(new HabitLogs
                {
                    HabitId = h.Id,
                    LogDate = today,
                    IsCompleted = h.IsCompleted,
                    NumericValue = h.CurrentProgress
                });
            }

            return CalculateDailyScore(today, activeHabits, syntheticLogs, habitTypeMap);
        }

        // ──────────────────────────────────────────────
        //  Private helpers
        // ──────────────────────────────────────────────

        /// <summary>
        /// Scores a single habit on a 0.0–1.0 scale.
        /// Checkbox → binary (0 or 1).
        /// Numeric/Timer → proportional (actual / target), capped at 1.0.
        /// </summary>
        public static double ScoreHabit(HabitLogs? log, Habits habit, string typeName)
        {
            if (log == null) return 0.0;

            if (typeName == "checkbox")
                return log.IsCompleted ? 1.0 : 0.0;

            // Numeric or Timer
            if (habit.TargetFrequency <= 0)
                return log.NumericValue > 0 ? 1.0 : 0.0;

            return Math.Min(log.NumericValue / (double)habit.TargetFrequency, 1.0);
        }

        /// <summary>
        /// Returns the weight multiplier for a habit's priority.
        /// Database convention: Priority 1 = High, 2 = Medium, 3 = Low.
        /// Mapped to weights:   1 → ×3,  2 → ×2,  3 → ×1.
        /// </summary>
        private static double GetPriorityWeight(int priority)
        {
            return priority switch
            {
                1 => 3.0, // High   → ×3
                2 => 2.0, // Medium → ×2
                _ => 1.0  // Low    → ×1
            };
        }

        /// <summary>
        /// Builds a human-readable status string for the calendar detail panel.
        /// </summary>
        private static string BuildStatusText(
            HabitLogs? log, Habits habit, HabitTypes? habitType,
            string typeName, bool isCompleted)
        {
            if (log == null) return "MISSED";

            if (typeName == "checkbox")
                return log.IsCompleted ? "DONE" : "MISSED";

            // Numeric / Timer
            string unit = (habit.Unit ?? habitType?.DefaultUnit ?? "").ToUpper();

            if (log.NumericValue <= 0)
                return "MISSED";

            // Show "value/target UNIT" for partial, or "value UNIT" for completed
            if (isCompleted)
                return $"{log.NumericValue}/{habit.TargetFrequency} {unit}".Trim();
            else
                return $"{log.NumericValue}/{habit.TargetFrequency} {unit}".Trim();
        }

        /// <summary>
        /// Checks if a habit is scheduled on a given day of the week
        /// using the DaysOfWeek bitmask.
        /// Bitmask: bit 6=Mon, 5=Tue, 4=Wed, 3=Thu, 2=Fri, 1=Sat, 0=Sun.
        /// A mask of 0 is treated as "every day" (daily habit with unset mask).
        /// </summary>
        public static bool IsScheduledForDay(int daysOfWeekMask, DayOfWeek dayOfWeek)
        {
            if (daysOfWeekMask == 0) return true;

            int bit = dayOfWeek switch
            {
                DayOfWeek.Monday    => 64,
                DayOfWeek.Tuesday   => 32,
                DayOfWeek.Wednesday => 16,
                DayOfWeek.Thursday  => 8,
                DayOfWeek.Friday    => 4,
                DayOfWeek.Saturday  => 2,
                DayOfWeek.Sunday    => 1,
                _ => 0
            };

            return (daysOfWeekMask & bit) != 0;
        }

        public static void GetDayColors(int percentage, out string badgeColor, out string dotColor)
        {
            // Detect current theme by checking a known dark-mode brush value
            bool isDark = false;
            try
            {
                var appBgBrush = System.Windows.Application.Current.Resources["AppBgBrush"] as System.Windows.Media.SolidColorBrush;
                if (appBgBrush != null)
                {
                    // Dark mode AppBgBrush is dark (R+G+B < 300)
                    var c = appBgBrush.Color;
                    isDark = (c.R + c.G + c.B) < 300;
                }
            }
            catch { }

            if (percentage < 0)
            {
                badgeColor = isDark ? "#2A2A2A" : "#E5E7EB";
                dotColor = isDark ? "#A0A0A0" : "#6B7280"; // Gray text for future days
                return;
            }

            dotColor = (percentage >= 25 && percentage <= 70) ? "#374151" : "#FFFFFF"; // Dark text on yellow/orange, white on red/green

            // Snap percentage to nearest 5%
            percentage = (percentage / 5) * 5;

            // Keyframes for smooth gradient (SLIGHTLY MISTY VIBRANT)
            // Red -> Orange -> Yellow -> Light Green -> Dark Green
            (int p, byte r, byte g, byte b)[] lightFrames = {
                (0,   226,  92,  92), // Misty Red
                (25,  235, 132,  70), // Misty Orange
                (50,  224, 186,  67), // Misty Yellow
                (75,   82, 191, 122), // Misty Light Green
                (100,  47, 143,  81)  // Misty Dark Green
            };

            (int p, byte r, byte g, byte b)[] darkFrames = {
                (0,   189,  64,  64), // Dark Misty Red
                (25,  196, 103,  49), // Dark Misty Orange
                (50,  186, 148,  45), // Dark Misty Yellow
                (75,   46, 148,  83), // Dark Misty Light Green
                (100,  34, 107,  59)  // Dark Misty Dark Green
            };

            var frames = isDark ? darkFrames : lightFrames;

            for (int i = 0; i < frames.Length - 1; i++)
            {
                if (percentage >= frames[i].p && percentage <= frames[i+1].p)
                {
                    float t = (float)(percentage - frames[i].p) / (frames[i+1].p - frames[i].p);
                    byte r = (byte)(frames[i].r + t * (frames[i+1].r - frames[i].r));
                    byte g = (byte)(frames[i].g + t * (frames[i+1].g - frames[i].g));
                    byte b = (byte)(frames[i].b + t * (frames[i+1].b - frames[i].b));
                    badgeColor = $"#{r:X2}{g:X2}{b:X2}";
                    return;
                }
            }

            badgeColor = $"#{frames[^1].r:X2}{frames[^1].g:X2}{frames[^1].b:X2}";
        }
    }
}
