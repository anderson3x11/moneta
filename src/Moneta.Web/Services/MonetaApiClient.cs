using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moneta.Application.Contracts;

namespace Moneta.Web.Services;

/// <summary>
/// Typed client over the Moneta Web API. The bearer token is read from the
/// circuit-scoped <see cref="TokenStore"/> and attached to every request.
/// </summary>
public sealed class MonetaApiClient(HttpClient http, TokenStore tokens)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public sealed record FileResult(byte[] Content, string FileName, string ContentType);

    public async Task<bool> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password), Json, ct);
        if (!response.IsSuccessStatusCode)
            return false;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(Json, ct);
        if (auth is null)
            return false;

        tokens.Set(auth.Token, auth.Email);
        return true;
    }

    public Task<IReadOnlyList<ClientResponse>> GetClientsAsync(CancellationToken ct = default) =>
        GetListAsync<ClientResponse>("api/clients", ct);

    public Task<ClientResponse?> CreateClientAsync(CreateClientRequest request, CancellationToken ct = default) =>
        PostAsync<CreateClientRequest, ClientResponse>("api/clients", request, ct);

    public Task<IReadOnlyList<InvoiceSummaryResponse>> GetInvoicesAsync(CancellationToken ct = default) =>
        GetListAsync<InvoiceSummaryResponse>("api/invoices", ct);

    public Task<InvoiceResponse?> GetInvoiceAsync(Guid id, CancellationToken ct = default) =>
        GetAsync<InvoiceResponse>($"api/invoices/{id}", ct);

    public Task<InvoiceResponse?> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken ct = default) =>
        PostAsync<CreateInvoiceRequest, InvoiceResponse>("api/invoices", request, ct);

    public Task<InvoiceResponse?> IssueInvoiceAsync(Guid id, CancellationToken ct = default) =>
        PostAsync<InvoiceResponse>($"api/invoices/{id}/issue", ct);

    public Task<InvoiceResponse?> PayInvoiceAsync(Guid id, CancellationToken ct = default) =>
        PostAsync<InvoiceResponse>($"api/invoices/{id}/pay", ct);

    public Task<InvoiceResponse?> CancelInvoiceAsync(Guid id, CancellationToken ct = default) =>
        PostAsync<InvoiceResponse>($"api/invoices/{id}/cancel", ct);

    public async Task<FileResult?> DownloadFacturXAsync(Guid id, CancellationToken ct = default)
    {
        using var request = Authorized(HttpMethod.Get, $"api/invoices/{id}/facturx");
        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? $"{id}.pdf";
        return new FileResult(bytes, fileName, "application/pdf");
    }

    private async Task<IReadOnlyList<T>> GetListAsync<T>(string url, CancellationToken ct)
    {
        var result = await GetAsync<List<T>>(url, ct);
        return result ?? [];
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        using var request = Authorized(HttpMethod.Get, url);
        using var response = await http.SendAsync(request, ct);
        return await ReadAsync<T>(response, ct);
    }

    private async Task<TOut?> PostAsync<TIn, TOut>(string url, TIn body, CancellationToken ct)
    {
        using var request = Authorized(HttpMethod.Post, url);
        request.Content = JsonContent.Create(body, options: Json);
        using var response = await http.SendAsync(request, ct);
        return await ReadAsync<TOut>(response, ct);
    }

    private async Task<TOut?> PostAsync<TOut>(string url, CancellationToken ct)
    {
        using var request = Authorized(HttpMethod.Post, url);
        using var response = await http.SendAsync(request, ct);
        return await ReadAsync<TOut>(response, ct);
    }

    private static async Task<T?> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.NoContent)
            return default;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(Json, ct);
    }

    private HttpRequestMessage Authorized(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);
        if (tokens.IsAuthenticated)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Token);
        return request;
    }
}
