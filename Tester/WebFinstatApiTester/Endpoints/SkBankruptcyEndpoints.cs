using System.Globalization;
using FinstatApi;
using WebFinstatApiTester.Infrastructure;

namespace WebFinstatApiTester.Endpoints;

public static class SkBankruptcyEndpoints
{
    public sealed record PersonBody(string Name, string Surname, string DateOfBirth);
    public sealed record CompanyBody(string Ico, string Name);

    public static IEndpointRouteBuilder MapSkBankruptcyEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/sk/bankruptcy");

        group.MapPost("/persons", async (PersonBody req, FinstatCredentials creds) =>
        {
            if (!DateTime.TryParseExact(req.DateOfBirth, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var dob))
                throw new BadHttpRequestException("DateOfBirth must be ISO date (yyyy-MM-dd).");

            var client = ApiClientFactory.SkBankruptcyRestructuring(creds);
            var items  = (BankruptcyRestructuring[])
                         await client.RequestPersonBankruptcyProceedings(req.Name, req.Surname, dob, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(items, creds.Json));
        });

        group.MapPost("/company", async (CompanyBody req, FinstatCredentials creds) =>
        {
            var client = ApiClientFactory.SkBankruptcyRestructuring(creds);
            var items  = (BankruptcyRestructuring[])
                         await client.RequestCompanyBankruptcyRestructuring(req.Ico, req.Name, creds.Json);
            return Results.Ok(FinstatEnvelopeBuilder.Build(items, creds.Json));
        });

        return routes;
    }
}
