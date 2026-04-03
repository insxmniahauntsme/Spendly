using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MediatR;
using SkiaSharp;
using Spendly.Application.Handlers.Analytics;
using Spendly.Application.Handlers.Analytics.Requests;
using Spendly.Helpers;
using Spendly.Infrastructure.Queries;

namespace Spendly.ViewModels.Analytics;

public partial class AnalyticsViewModel : ObservableObject
{
	private static readonly string[] _accountEmojis =
	[
		"💳", "🏦", "💰", "📈", "🧾", "💵", "💼"
	];
	
	private readonly CategoryQueries _categoryQueries;
	private readonly IMediator _mediator;

	// TOP SECTION DATA
	// Overspent
	[ObservableProperty] private bool _hasOverspent;
	[ObservableProperty] private string _overspentTitle = "";
	[ObservableProperty] private string _overspentIcon = "";
	[ObservableProperty] private string _overspentCurrentText = "";
	[ObservableProperty] private string _overspentLimitText = "";
	[ObservableProperty] private double _overspentProgress;

	// Underused
	[ObservableProperty] private bool _hasUnderused;
	[ObservableProperty] private string _underusedTitle = "";
	[ObservableProperty] private string _underusedIcon = "";
	[ObservableProperty] private string _underusedCurrentText = "";
	[ObservableProperty] private string _underusedLimitText = "";
	[ObservableProperty] private double _underusedProgress;

	// Risk zone
	[ObservableProperty] private bool _hasRiskItems;
	[ObservableProperty] private ObservableCollection<RiskZoneItemVm> _riskItems = [];

	// Forecast
	[ObservableProperty] private bool _hasForecast;
	[ObservableProperty] private string _forecastAmountText = "₴ -";

	[ObservableProperty] private ObservableCollection<CategoryChipItem> _categoryChips = [];
	[ObservableProperty] private Guid? _selectedCategoryId;
	[ObservableProperty] private ISeries[] _trendSeries = [];
	[ObservableProperty] private Axis[] _trendXAxes = [];
	[ObservableProperty] private Axis[] _trendYAxes = [];
	[ObservableProperty] private ObservableCollection<AccountCardItem> _topAccounts = [];
	[ObservableProperty] private ObservableCollection<AccountBreakdownItem> _accountBreakdownItems = [];

	public AnalyticsViewModel(CategoryQueries categoryQueries, IMediator mediator)
	{
		_categoryQueries = categoryQueries;
		_mediator = mediator;
		_ = LoadData();
	}

	[RelayCommand]
	private async Task SelectCategory(CategoryChipItem? item)
	{
		if (item is null)
			return;

		if (SelectedCategoryId == item.Id)
			return;

		SelectedCategoryId = item.Id;

		foreach (var chip in CategoryChips)
			chip.IsSelected = chip.Id == item.Id;

		await BuildTrendSection();
	}

	public async Task LoadData()
	{
		var request = new GetAnalyticsDataRequest();
		var data = await _mediator.Send(request);

		await BuildCategoryChips();
		BuildTopSection(data.TopSectionData);
		await BuildTrendSection();
		BuildAccountsSection(data.AccountsSectionData);
	}

	private async Task BuildCategoryChips()
	{
		var items = await _categoryQueries.GetCategoriesAsync();

		CategoryChips.Clear();

		foreach (var item in items)
		{
			CategoryChips.Add(
				new CategoryChipItem(
					item.Id,
					item.Name,
					CategoryIconProvider.GetIcon(item.Name)));
		}
		
		var first = CategoryChips.FirstOrDefault();
		if (first is null)
			return;

		first.IsSelected = true;
		SelectedCategoryId = first.Id;
	}

	private void BuildTopSection(AnalyticsTopSectionData data)
	{
		var overspentData = data.Overspent;
		var underusedData = data.Underused;
		var riskItems = data.RiskItems;

		HasOverspent = overspentData.HasData;

		if (HasOverspent)
		{
			OverspentTitle = overspentData.CategoryName;
			OverspentIcon = CategoryIconProvider.GetIcon(overspentData.CategoryName);
			OverspentCurrentText = Money(overspentData.Current);
			OverspentLimitText = Money(overspentData.Limit);
			OverspentProgress = overspentData.Progress;
		}

		HasUnderused = underusedData.HasData;

		if (HasUnderused)
		{
			UnderusedTitle = underusedData.CategoryName;
			UnderusedIcon = CategoryIconProvider.GetIcon(underusedData.CategoryName);
			UnderusedCurrentText = Money(underusedData.Current);
			UnderusedLimitText = Money(underusedData.Limit);
			UnderusedProgress = underusedData.Progress;
		}

		RiskItems.Clear();

		foreach (var item in riskItems)
		{
			RiskItems.Add(new RiskZoneItemVm
			{
				CategoryId = item.CategoryId,
				CategoryName = item.CategoryName,
				IconSource = CategoryIconProvider.GetIcon(item.CategoryName),
				ProgressText = $"{item.Progress:P0}",
			});
		}

		HasRiskItems = riskItems.Count != 0;

		HasForecast = data.HasForecast;
		ForecastAmountText = HasForecast
			? Money(data.ForecastAmount)
			: "₴ -";
	}

	private async Task BuildTrendSection()
	{
		var request = new GetAnalyticsTrendDataRequest(SelectedCategoryId!.Value);
		
		var data = await _mediator.Send(request);
		
		var points = data.Points
			.OrderBy(x => x.Month)
			.ToList();

		var values = points
			.Select(x => (double)x.Amount)
			.ToArray();

		var labels = points
			.Select(x =>
			{
				var month = x.Month
					.ToDateTime(TimeOnly.MinValue)
					.ToString("MMM", System.Globalization.CultureInfo.InvariantCulture)
					.ToUpper();

				return $"{Money(x.Amount)}{Environment.NewLine}{month}";
			})
			.ToArray();

		TrendSeries =
		[
			new LineSeries<double>
			{
				Values = values,
				GeometrySize = 0,
				LineSmoothness = 0,
				Stroke = new SolidColorPaint(new SKColor(0x6C, 0x63, 0xFF, 255), 2),
				Fill = new LinearGradientPaint(
					[
						new SKColor(0x6C, 0x63, 0xFF, 20),
						new SKColor(0x6C, 0x63, 0xFF, 6),
						new SKColor(0x6C, 0x63, 0xFF, 0)
					],
					new SKPoint(0.5f, 0f),
					new SKPoint(0.5f, 1f))
			}
		];

		TrendXAxes =
		[
			new Axis
			{
				Labels = labels,
				LabelsPaint = new SolidColorPaint(new SKColor(0x88, 0x88, 0xAA, 0xFF)),
				SeparatorsPaint = null,
				TicksPaint = null,
				TextSize = 11,
				Padding = new LiveChartsCore.Drawing.Padding(0, 18, 0, 0),
				MinStep = 1,
				ForceStepToMin = true
			}
		];

		TrendYAxes =
		[
			new Axis
			{
				IsVisible = false,
				MinLimit = 0,
				SeparatorsPaint = null,
				TicksPaint = null,
				LabelsPaint = null,
				Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0)
			}
		];
	}

	private void BuildAccountsSection(AnalyticsAccountsSectionData data)
	{
		TopAccounts.Clear();
		AccountBreakdownItems.Clear();

		foreach (var item in data.Items.Take(2))
		{
			TopAccounts.Add(new AccountCardItem
			{
				Rank = item.Rank,
				AccountName = item.Name,
				BadgeText = item.Rank == 1 ? "TOP SPENDING ACCOUNT" : "SECOND BY SPENDING",
				AmountText = Money(item.Amount),
				ShareText = $"{item.Share:P0} of all expenses",
				IconText = GetAccountEmoji(item.Name),
				AccentBrush = GetAccountAccentBrush(item.Rank),
				BadgeBackground = GetAccountBadgeBackground(item.Rank),
				IconBackground = GetAccountIconBackground(item.Rank)
			});
		}

		foreach (var item in data.Items)
		{
			AccountBreakdownItems.Add(new AccountBreakdownItem
			{
				Rank = item.Rank,
				Name = item.Name,
				Percent = (int)Math.Round(item.Share * 100),
				PercentText = $"{item.Share:P0}",
				Progress = item.Share,
				IconText = GetAccountEmoji(item.Name),
				FillBrush = GetBreakdownFillBrush(item.Rank),
				IconBackground = GetAccountIconBackground(item.Rank)
			});
		}
	}

	private static string Money(decimal value)
		=> $"₴ {value:N0}";
	
	private static string GetAccountEmoji(string accountName)
	{
		if (string.IsNullOrWhiteSpace(accountName))
			return "💳";

		var index = Math.Abs(accountName.GetHashCode()) % _accountEmojis.Length;
		return _accountEmojis[index];
	}

	private static Brush GetAccountAccentBrush(int rank) => rank switch
	{
		1 => (Brush)System.Windows.Application.Current.FindResource("Brush.Danger")!,
		2 => (Brush)System.Windows.Application.Current.FindResource("Brush.Primary")!,
		_ => (Brush)System.Windows.Application.Current.FindResource("Brush.Warning")!
	};

	private static Brush GetAccountBadgeBackground(int rank) => rank switch
	{
		1 => (Brush)System.Windows.Application.Current.FindResource("Brush.DangerOverlay")!,
		2 => (Brush)System.Windows.Application.Current.FindResource("Brush.PrimaryOverlay")!,
		_ => (Brush)System.Windows.Application.Current.FindResource("Brush.WarningOverlay")!
	};

	private static Brush GetBreakdownFillBrush(int rank) => rank switch
	{
		1 => (Brush)System.Windows.Application.Current.FindResource("Brush.Danger")!,
		2 => (Brush)System.Windows.Application.Current.FindResource("Brush.Primary")!,
		3 => (Brush)System.Windows.Application.Current.FindResource("Brush.Warning")!,
		_ => (Brush)System.Windows.Application.Current.FindResource("Brush.Success")!
	};

	private static Brush GetAccountIconBackground(int rank) => rank switch
	{
		1 => (Brush)System.Windows.Application.Current.FindResource("Brush.DangerOverlay")!,
		2 => (Brush)System.Windows.Application.Current.FindResource("Brush.PrimaryOverlay")!,
		3 => (Brush)System.Windows.Application.Current.FindResource("Brush.WarningOverlay")!,
		_ => (Brush)System.Windows.Application.Current.FindResource("Brush.SuccessOverlay")!
	};

	private static Brush Brush(string hex)
		=> new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);

	private static Brush BrushFromResource(string key)
		=> (Brush)System.Windows.Application.Current.FindResource(key)!;
}

public partial class CategoryChipItem : ObservableObject
{
	[ObservableProperty] private Guid _id;
	[ObservableProperty] private string _name;
	[ObservableProperty] private string _icon;
	[ObservableProperty] private bool _isSelected;

	public CategoryChipItem(Guid id, string name, string icon, bool isSelected = false)
	{
		_id = id;
		_name = name;
		_icon = icon;
		_isSelected = isSelected;
	}
}

public partial class AccountCardItem : ObservableObject
{
	[ObservableProperty] private string _iconText = "";
	[ObservableProperty] private string _shareText = "";
	[ObservableProperty] private string _badgeText = "";
	[ObservableProperty] private string _amountText = "";
	[ObservableProperty] private string _accountName = "";
	[ObservableProperty] private int _rank;

	[ObservableProperty] private Brush _accentBrush = Brushes.Transparent;
	[ObservableProperty] private Brush _iconBackground = Brushes.Transparent;
	[ObservableProperty] private Brush _badgeBackground = Brushes.Transparent;
}

public partial class AccountBreakdownItem : ObservableObject
{
	[ObservableProperty] private int _rank;
	[ObservableProperty] private int _percent;
	[ObservableProperty] private double _progress;

	[ObservableProperty] private string _name = "";
	[ObservableProperty] private string _iconText = "";
	[ObservableProperty] private string _percentText = "";

	[ObservableProperty] private Brush _fillBrush = Brushes.Transparent;
	[ObservableProperty] private Brush _iconBackground = Brushes.Transparent;
}

public partial class RiskZoneItemVm : ObservableObject
{
	[ObservableProperty] private Guid _categoryId;
	[ObservableProperty] private string _iconSource = "";
	[ObservableProperty] private string _categoryName = "";
	[ObservableProperty] private string _progressText = "";
}