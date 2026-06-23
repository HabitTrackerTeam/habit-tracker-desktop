using HabitTracker.Models;
using HabitTracker.Services;

namespace HabitTracker.Tests;

public class DailyScoreCalculatorTests
{
    // ─── Helper factories ────────────────────────────────────

    private static Habits MakeHabit(
        string id = "h1",
        string name = "Test Habit",
        string period = "daily",
        int targetFrequency = 1,
        int priority = 2,
        string habitTypeId = "type-cb",
        int daysOfWeek = 0,
        bool isFlexible = false,
        DateTime? createdDate = null)
    {
        return new Habits
        {
            Id = id,
            Name = name,
            Period = period,
            TargetFrequency = targetFrequency,
            Priority = priority,
            HabitTypeId = habitTypeId,
            DaysOfWeek = daysOfWeek,
            IsFlexible = isFlexible,
            CreatedDate = createdDate ?? new DateTime(2025, 1, 1)
        };
    }

    private static HabitLogs MakeLog(
        string habitId,
        DateTime logDate,
        bool isCompleted = false,
        double numericValue = 0)
    {
        return new HabitLogs
        {
            Id = Guid.NewGuid().ToString(),
            HabitId = habitId,
            LogDate = logDate,
            IsCompleted = isCompleted,
            NumericValue = numericValue,
            UpdatedTime = logDate
        };
    }

    private static Dictionary<string, HabitTypes> MakeTypeMap(params (string id, string type)[] types)
    {
        var map = new Dictionary<string, HabitTypes>();
        foreach (var (id, type) in types)
        {
            map[id] = new HabitTypes { Id = id, Type = type, DefaultUnit = "reps" };
        }
        return map;
    }

    // ─── 1-3: IsScheduledForDay ──────────────────────────────

    [Fact]
    public void IsScheduledForDay_MaskZero_ReturnsTrueForAnyDay()
    {
        // Maska 0 oznacza "codziennie" - powinno zwracać true dla każdego dnia
        Assert.True(DailyScoreCalculator.IsScheduledForDay(0, DayOfWeek.Monday));
        Assert.True(DailyScoreCalculator.IsScheduledForDay(0, DayOfWeek.Friday));
        Assert.True(DailyScoreCalculator.IsScheduledForDay(0, DayOfWeek.Sunday));
    }

    [Theory]
    [InlineData(64, DayOfWeek.Monday, true)]   // bit 6 = Monday
    [InlineData(64, DayOfWeek.Tuesday, false)]
    [InlineData(32, DayOfWeek.Tuesday, true)]   // bit 5 = Tuesday
    [InlineData(4, DayOfWeek.Friday, true)]     // bit 2 = Friday
    [InlineData(4, DayOfWeek.Saturday, false)]
    [InlineData(1, DayOfWeek.Sunday, true)]     // bit 0 = Sunday
    [InlineData(1, DayOfWeek.Monday, false)]
    public void IsScheduledForDay_SingleBit_MatchesCorrectDay(int mask, DayOfWeek day, bool expected)
    {
        Assert.Equal(expected, DailyScoreCalculator.IsScheduledForDay(mask, day));
    }

    [Fact]
    public void IsScheduledForDay_MultipleBits_WorkdaysOnly()
    {
        // Pn-Pt = 64+32+16+8+4 = 124
        int weekdaysMask = 124;

        Assert.True(DailyScoreCalculator.IsScheduledForDay(weekdaysMask, DayOfWeek.Monday));
        Assert.True(DailyScoreCalculator.IsScheduledForDay(weekdaysMask, DayOfWeek.Wednesday));
        Assert.True(DailyScoreCalculator.IsScheduledForDay(weekdaysMask, DayOfWeek.Friday));
        Assert.False(DailyScoreCalculator.IsScheduledForDay(weekdaysMask, DayOfWeek.Saturday));
        Assert.False(DailyScoreCalculator.IsScheduledForDay(weekdaysMask, DayOfWeek.Sunday));
    }

    // ─── 4: CalculateDailyScore — checkbox habit ─────────────

    [Fact]
    public void CalculateDailyScore_SingleCheckboxCompleted_Returns100Percent()
    {
        var date = new DateTime(2025, 6, 10); // Wtorek
        var typeMap = MakeTypeMap(("type-cb", "checkbox"));
        var habits = new[] { MakeHabit(id: "h1", habitTypeId: "type-cb") };
        var logs = new[] { MakeLog("h1", date, isCompleted: true) };

        var result = DailyScoreCalculator.CalculateDailyScore(date, habits, logs, typeMap);

        Assert.Equal(100, result.Percentage);
        Assert.Equal(1, result.PlannedCount);
        Assert.Single(result.Details);
        Assert.True(result.Details[0].IsCompleted);
    }

    // ─── 5: CalculateDailyScore — numeric habit partial ──────

    [Fact]
    public void CalculateDailyScore_NumericHabitPartialProgress_ReturnsProportionalScore()
    {
        var date = new DateTime(2025, 6, 10);
        var typeMap = MakeTypeMap(("type-num", "numeric"));
        var habits = new[] { MakeHabit(id: "h1", habitTypeId: "type-num", targetFrequency: 10) };
        var logs = new[] { MakeLog("h1", date, numericValue: 5) }; // 50% postępu

        var result = DailyScoreCalculator.CalculateDailyScore(date, habits, logs, typeMap);

        Assert.Equal(50, result.Percentage);
        Assert.False(result.Details[0].IsCompleted);
    }

    // ─── 6: CalculateDailyScore — priority weighting ─────────

    [Fact]
    public void CalculateDailyScore_PriorityWeighting_HighPriorityHasMoreImpact()
    {
        var date = new DateTime(2025, 6, 10);
        var typeMap = MakeTypeMap(("type-cb", "checkbox"));

        var habits = new[]
        {
            MakeHabit(id: "high", habitTypeId: "type-cb", priority: 1),  // High (waga 3)
            MakeHabit(id: "low", habitTypeId: "type-cb", priority: 3),   // Low (waga 1)
        };

        // Tylko high-priority nawyk ukończony
        var logs = new[] { MakeLog("high", date, isCompleted: true) };

        var result = DailyScoreCalculator.CalculateDailyScore(date, habits, logs, typeMap);

        // Waga: high=3*1.0=3, low=1*0.0=0, total_weight=4
        // Score = 3/4 * 100 = 75%
        Assert.Equal(75, result.Percentage);
        Assert.Equal(2, result.PlannedCount);
    }

    // ─── 7: Habits model — IsPeriodCompleted & CurrentProgressText ──

    [Fact]
    public void HabitsModel_IsPeriodCompleted_And_CurrentProgressText()
    {
        var habit = MakeHabit(period: "daily", targetFrequency: 8);
        habit.DisplayTypeName = "Numeric";

        // Przed ustawieniem progress
        Assert.False(habit.IsPeriodCompleted);
        Assert.Equal("", habit.CurrentProgressText);

        // Częściowy progress
        habit.CurrentProgress = 5;
        Assert.False(habit.IsPeriodCompleted);
        Assert.Equal("5", habit.CurrentProgressText);

        // Osiągnięty cel
        habit.CurrentProgress = 8;
        Assert.True(habit.IsPeriodCompleted);
        Assert.Equal("8", habit.CurrentProgressText);

        // Powyżej celu — nadal completed
        habit.CurrentProgress = 12;
        Assert.True(habit.IsPeriodCompleted);
    }
}
