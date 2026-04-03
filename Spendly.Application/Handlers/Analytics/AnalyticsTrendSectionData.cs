namespace Spendly.Application.Handlers.Analytics;

public record AnalyticsTrendSectionData(
	List<AnalyticsTrendCategoryItem> Categories,
	Guid? SelectedCategoryId,
	List<AnalyticsTrendPointItem> Points);