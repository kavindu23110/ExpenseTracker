using System.Text.Json;
using ExpenseTracker.Application.Models;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain;
using ExpenseTracker.Domain.Bills;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Api;

/// <summary>The HTTP surface; endpoints only translate to and from application services.</summary>
public static class BillEndpoints
{
    /// <summary>Maps every API route.</summary>
    public static void MapBillEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api").RequireAuthorization();

        api.MapHouseholdEndpoints();

        api.MapGet("/dashboard", ([AsParameters] DashboardQuery q, IDashboardService s, CancellationToken ct) => s.GetAsync(q, ct));

        api.MapGet("/bills/{id:int}", async (int id, IBillFilingService s, CancellationToken ct) =>
            await s.GetAsync(id, ct) is { } bill ? Results.Ok(bill) : Results.NotFound());

        api.MapGet("/bills/{id:int}/file", async (int id, HttpContext http, IBillFilingService s, CancellationToken ct) =>
        {
            var file = await s.GetFileAsync(id, ct);
            if (file is null) return Results.NotFound();
            http.Response.Headers.XContentTypeOptions = "nosniff";
            return Results.File(file.Data, file.ContentType);
        });

        api.MapPost("/bills", async (HttpRequest http, IBillFilingService s, CancellationToken ct) =>
        {
            var form = await http.ReadFormAsync(ct);
            var json = form["bill"].ToString();
            var request = TryParse(json) ?? throw new DomainException("The bill data was not valid.");
            var id = await s.SaveAsync(request, await ReadFileAsync(form.Files["file"], ct), ct);
            return Results.Created($"/api/bills/{id}", new { id });
        });

        api.MapPut("/bills/{id:int}", async (int id, EditBillRequest r, IBillFilingService s, CancellationToken ct) =>
            await s.EditAsync(id, r, ct) ? Results.NoContent() : Results.NotFound());

        api.MapPut("/bills/{id:int}/category", async (int id, ChangeBillCategoryRequest r, IBillFilingService s, CancellationToken ct) =>
            await s.ChangeCategoryAsync(id, r.Category, ct) ? Results.NoContent() : Results.NotFound());

        api.MapPost("/item-mappings", async (ItemMappingRequest request, IItemMappingService s, CancellationToken ct) =>
        {
            await s.DecideAsync(request, ct);
            return Results.NoContent();
        });

        api.MapPost("/scan", async (HttpRequest http, IBillScanService s, CancellationToken ct) =>
        {
            var form = await http.ReadFormAsync(ct);
            var file = await ReadFileAsync(form.Files["file"], ct) ?? throw new DomainException("Choose a file to scan.");
            return Results.Ok(await s.ScanAsync(file, form["lang"].ToString(), ct));
        });
    }

    private static SaveBillRequest? TryParse(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<SaveBillRequest>(json, JsonSerializerOptions.Web);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static async Task<UploadedFile?> ReadFileAsync(IFormFile? file, CancellationToken ct)
    {
        if (file is null) return null;
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        return new UploadedFile(Path.GetFileName(file.FileName), ms.ToArray());
    }
}
