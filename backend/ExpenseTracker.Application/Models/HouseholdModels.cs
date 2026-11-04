namespace ExpenseTracker.Application.Models;

/// <summary>This month's budget position for one category.</summary>
public sealed record BudgetRowDto(string Category, decimal? Limit, decimal Spent, int Percent, bool Over);

/// <summary>A savings goal and what it needs each month.</summary>
public sealed record SavingsGoalDto(int Id, string Name, decimal Target, string DueDate, decimal Saved, decimal MonthlyNeeded, int Percent);

/// <summary>A recurring bill and its status this month.</summary>
public sealed record RecurringBillDto(int Id, string Name, decimal Amount, int DayOfMonth, string Category, string DueDate, string Status);

/// <summary>Everything on the planning page.</summary>
public sealed record PlanningDto(string Month, IReadOnlyList<string> Categories, IReadOnlyList<BudgetRowDto> Budgets, IReadOnlyList<SavingsGoalDto> Goals, IReadOnlyList<RecurringBillDto> Bills, int RemindersDue, IReadOnlyList<string> CustomCategories);

/// <summary>Sets a budget; no limit removes it.</summary>
public sealed record AddCategoryRequest(string? Name);

/// <summary>Sets or clears a budget.</summary>
public sealed record SetBudgetRequest(string Category, decimal? Limit);

/// <summary>Creates a savings goal.</summary>
public sealed record AddGoalRequest(string Name, decimal Target, string DueDate);

/// <summary>Adds to or takes from a savings goal.</summary>
public sealed record GoalSavingRequest(decimal Amount);

/// <summary>Creates a recurring bill.</summary>
public sealed record AddRecurringBillRequest(string Name, decimal Amount, int DayOfMonth, string Category);

/// <summary>Pays a recurring bill; the amount may differ from the usual one.</summary>
public sealed record PayBillRequest(decimal? Amount);

/// <summary>One purchase in a price history.</summary>
public sealed record PricePointDto(string Date, string Shop, decimal Price, int Percent);

/// <summary>A shop's price for a product on its latest purchase.</summary>
public sealed record ShopPriceDto(string Shop, decimal Price, string Date);

/// <summary>A product's price over the household's own bills.</summary>
public sealed record PriceHistoryDto(string Name, string? NameSi, string? Unit, IReadOnlyList<PricePointDto> Points, decimal ChangePercent, IReadOnlyList<ShopPriceDto> Shops);

/// <summary>A pantry line.</summary>
public sealed record PantryItemDto(int Id, string Name, decimal Qty, string? Unit, bool IsOut);

/// <summary>A shopping list line.</summary>
public sealed record ShoppingItemDto(int Id, string Name, bool Done, decimal? Qty, string? Unit);

/// <summary>The pantry and shopping list.</summary>
public sealed record PantryDto(IReadOnlyList<PantryItemDto> Pantry, IReadOnlyList<ShoppingItemDto> Shopping);

/// <summary>Adds a pantry item.</summary>
public sealed record ToBuyLastMonthDto(string Month, decimal Total);

/// <summary>An item suggested for the shopping list.</summary>
public sealed record ToBuySuggestionDto(string Name, string? NameSi, decimal Qty, string? Unit);

/// <summary>Adds a pantry item by hand.</summary>
public sealed record AddPantryRequest(string Name, decimal Qty, string? Unit);

/// <summary>Changes a pantry quantity.</summary>
public sealed record AdjustPantryRequest(decimal Change);

/// <summary>Adds a shopping list line.</summary>
public sealed record AddShoppingRequest(string Name);

/// <summary>Ticks or unticks a shopping list line.</summary>
public sealed record SetDoneRequest(bool Done);

/// <summary>Sets how much of a shopping list line to buy.</summary>
public sealed record SetShoppingAmountRequest(decimal? Qty, string? Unit);

/// <summary>Pasted bank messages.</summary>
public sealed record SmsPreviewRequest(string? Text);

/// <summary>A payment found in a message, for the user to check.</summary>
public sealed record SmsDraftDto(decimal Amount, string Merchant, string? Date, string Category, string Reference, bool AlreadyImported);

/// <summary>A checked payment to save.</summary>
public sealed record SmsImportItem(decimal Amount, string Merchant, string? Date, string Category, string Reference);

/// <summary>The checked payments.</summary>
public sealed record SmsImportRequest(IReadOnlyList<SmsImportItem>? Items);

/// <summary>A household member, as sent over the API.</summary>
public sealed record MemberDto(int Id, string Name, bool IsOwner);

/// <summary>Who is signed in, and whether the first account still has to be created.</summary>
public sealed record SessionDto(bool NeedsSetup, MemberDto? User);

/// <summary>A name and password.</summary>
public sealed record CredentialsRequest(string? Name, string? Password);
