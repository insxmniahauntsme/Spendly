namespace Spendly.Application.Models.Dashboard;

public sealed record CategorySlice(
    string CategoryName,
    decimal Amount
);