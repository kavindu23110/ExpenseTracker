using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;

namespace ExpenseTracker.Api;

/// <summary>Routes for budgets, goals, recurring bills, prices, pantry and bank messages.</summary>
public static class HouseholdEndpoints
{
    /// <summary>Maps the routes under a protected group.</summary>
    public static void MapHouseholdEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGet("/planning", (PlanningService s, CancellationToken ct) => s.GetAsync(ct));
        api.MapPost("/categories", async (AddCategoryRequest r, CategoryService s, CancellationToken ct) => { await s.AddAsync(r.Name, ct); return Results.NoContent(); });
        api.MapDelete("/categories/{name}", async (string name, CategoryService s, CancellationToken ct) => Done(await s.RemoveAsync(name, ct)));
        api.MapPut("/budgets", async (SetBudgetRequest r, PlanningService s, CancellationToken ct) => { await s.SetBudgetAsync(r, ct); return Results.NoContent(); });
        api.MapPost("/goals", async (AddGoalRequest r, PlanningService s, CancellationToken ct) => { await s.AddGoalAsync(r, ct); return Results.NoContent(); });
        api.MapPost("/goals/{id:int}/savings", async (int id, GoalSavingRequest r, PlanningService s, CancellationToken ct) => Done(await s.AddSavingAsync(id, r.Amount, ct)));
        api.MapDelete("/goals/{id:int}", async (int id, PlanningService s, CancellationToken ct) => Done(await s.RemoveGoalAsync(id, ct)));
        api.MapPost("/recurring", async (AddRecurringBillRequest r, PlanningService s, CancellationToken ct) => { await s.AddRecurringAsync(r, ct); return Results.NoContent(); });
        api.MapPost("/recurring/{id:int}/pay", async (int id, PayBillRequest r, PlanningService s, CancellationToken ct) => Done(await s.PayAsync(id, r.Amount, ct)));
        api.MapDelete("/recurring/{id:int}", async (int id, PlanningService s, CancellationToken ct) => Done(await s.RemoveRecurringAsync(id, ct)));

        api.MapGet("/prices", (PriceHistoryService s, CancellationToken ct) => s.GetAsync(ct));

        api.MapGet("/pantry", (PantryService s, CancellationToken ct) => s.GetAsync(ct));
        api.MapPost("/pantry", async (AddPantryRequest r, PantryService s, CancellationToken ct) => { await s.AddAsync(r, ct); return Results.NoContent(); });
        api.MapPost("/pantry/{id:int}/adjust", async (int id, AdjustPantryRequest r, PantryService s, CancellationToken ct) => Done(await s.AdjustAsync(id, r.Change, ct)));
        api.MapPost("/pantry/{id:int}/use-up", async (int id, PantryService s, CancellationToken ct) => Done(await s.UseUpAsync(id, ct)));
        api.MapDelete("/pantry/{id:int}", async (int id, PantryService s, CancellationToken ct) => Done(await s.RemoveAsync(id, ct)));
        api.MapGet("/shopping/suggestions", (PantryService s, CancellationToken ct) => s.GetSuggestionsAsync(ct));
        api.MapGet("/shopping/last-month", (PantryService s, CancellationToken ct) => s.GetLastMonthAsync(ct));
        api.MapGet("/shopping/known", (PantryService s, CancellationToken ct) => s.GetKnownItemsAsync(ct));
        api.MapPost("/shopping", async (AddShoppingRequest r, PantryService s, CancellationToken ct) => { await s.AddShoppingAsync(r.Name, ct); return Results.NoContent(); });
        api.MapPut("/shopping/{id:int}", async (int id, SetDoneRequest r, PantryService s, CancellationToken ct) => Done(await s.SetDoneAsync(id, r.Done, ct)));
        api.MapPut("/shopping/{id:int}/amount", async (int id, SetShoppingAmountRequest r, PantryService s, CancellationToken ct) => Done(await s.SetShoppingAmountAsync(id, r.Qty, r.Unit, ct)));
        api.MapDelete("/shopping/{id:int}", async (int id, PantryService s, CancellationToken ct) => Done(await s.RemoveShoppingAsync(id, ct)));

        api.MapPost("/sms/preview", (SmsPreviewRequest r, SmsImportService s, CancellationToken ct) => s.PreviewAsync(r.Text, ct));
        api.MapPost("/sms/import", async (SmsImportRequest r, SmsImportService s, CancellationToken ct) => Results.Ok(new { added = await s.ImportAsync(r.Items, ct) }));
    }

    private static IResult Done(bool found) => found ? Results.NoContent() : Results.NotFound();
}
