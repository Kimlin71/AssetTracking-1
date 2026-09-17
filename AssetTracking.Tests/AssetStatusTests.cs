using AssetTracking.Domain;
using Xunit;

namespace AssetTracking.Tests;

// file = visible only inside this file; sealed = cannot be subclassed further.
// Overriding Today lets each test set a precise reference date so results never depend on the real clock.
file sealed class TestAsset : Asset
{
    private readonly DateOnly _today;
    public override string AssetType => "Test";
    protected override DateOnly Today => _today;

    public TestAsset(DateOnly purchaseDate, DateOnly today)
        : base("Test", "Model", purchaseDate, 100m, Office.USA)
    {
        _today = today;
    }
}

public class AssetStatusTests
{
    // [Fact] marks a method as a single test case that xUnit will discover and run automatically.
    // _nextId is static on Asset; IDs increment across tests but no test asserts Id values, so order doesn't matter.

    [Fact]
    public void Status_IsNone_When7MonthsRemaining()
    {
        var today        = new DateOnly(2026, 9, 1);
        var purchaseDate = today.AddYears(-3).AddMonths(7);
        var asset        = new TestAsset(purchaseDate, today);

        Assert.Equal(AssetStatus.None, asset.Status);
    }

    [Fact]
    public void Status_IsNone_WhenExactly6MonthsRemaining()
    {
        // Boundary: 6 months remaining is still None (Red threshold is < 6).
        var today        = new DateOnly(2026, 9, 1);
        var purchaseDate = today.AddYears(-3).AddMonths(6);
        var asset        = new TestAsset(purchaseDate, today);

        Assert.Equal(AssetStatus.None, asset.Status);
    }

    [Fact]
    public void Status_IsRed_When5MonthsRemaining()
    {
        var today        = new DateOnly(2026, 9, 1);
        var purchaseDate = today.AddYears(-3).AddMonths(5);
        var asset        = new TestAsset(purchaseDate, today);

        Assert.Equal(AssetStatus.Red, asset.Status);
    }

    [Fact]
    public void Status_IsRed_When3MonthsRemaining()
    {
        // Boundary: 3 months remaining is still Red (Yellow threshold is < 3).
        var today        = new DateOnly(2026, 9, 1);
        var purchaseDate = today.AddYears(-3).AddMonths(3);
        var asset        = new TestAsset(purchaseDate, today);

        Assert.Equal(AssetStatus.Red, asset.Status);
    }

    [Fact]
    public void Status_IsYellow_When2MonthsRemaining()
    {
        var today        = new DateOnly(2026, 9, 1);
        var purchaseDate = today.AddYears(-3).AddMonths(2);
        var asset        = new TestAsset(purchaseDate, today);

        Assert.Equal(AssetStatus.Yellow, asset.Status);
    }

    [Fact]
    public void Status_IsYellow_When1MonthRemaining()
    {
        var today        = new DateOnly(2026, 9, 1);
        var purchaseDate = today.AddYears(-3).AddMonths(1);
        var asset        = new TestAsset(purchaseDate, today);

        Assert.Equal(AssetStatus.Yellow, asset.Status);
    }

    [Fact]
    public void Status_IsExpired_WhenTodayEqualsEndOfLife()
    {
        // Boundary: the day end-of-life is reached is already Expired.
        var today        = new DateOnly(2026, 9, 1);
        var purchaseDate = today.AddYears(-3);  // EndOfLife == today
        var asset        = new TestAsset(purchaseDate, today);

        Assert.Equal(AssetStatus.Expired, asset.Status);
    }

    [Fact]
    public void Status_IsExpired_When1MonthPastEndOfLife()
    {
        var today        = new DateOnly(2026, 9, 1);
        var purchaseDate = today.AddYears(-3).AddMonths(-1);
        var asset        = new TestAsset(purchaseDate, today);

        Assert.Equal(AssetStatus.Expired, asset.Status);
    }

    [Fact]
    public void Status_IsYellow_WhenSameMonthAsEndOfLifeButDayBefore()
    {                                                                              
        // months formula rounds to 0 but today < EndOfLife, so the asset is not yet Expired.
        var endOfLife    = new DateOnly(2026, 9, 15);
        var purchaseDate = endOfLife.AddYears(-3);           // EndOfLife == 2026-09-15
        var today        = new DateOnly(2026, 9, 14);        // one day before EOL
        var asset        = new TestAsset(purchaseDate, today);

        Assert.Equal(AssetStatus.Yellow, asset.Status);
    }

    [Fact]
    public void EndOfLife_IsExactly3YearsAfterPurchase()
    {
        var purchase = new DateOnly(2023, 4, 15);
        var asset    = new TestAsset(purchase, new DateOnly(2026, 9, 1));

        Assert.Equal(new DateOnly(2026, 4, 15), asset.EndOfLife);
    }
}
