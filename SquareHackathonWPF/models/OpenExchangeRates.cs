using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Square.Models;

namespace SquareHackathonWPF.Models;

internal static class OpenExchangeRates
{
    private static string AppId { get; } = App.GetOpenExchangeRatesAppId();
    private static readonly string BaseUrl = "https://openexchangerates.org/api/latest.json?app_id=" + AppId;

    public static async Task<int?> Compare(Money m1, Money m2)
    {
        if (m1.Currency == m2.Currency) return m1.Amount?.CompareTo(m2.Amount);
        
        // Convert m2 to m1's currency
        var exchangeRate = await GetExchangeRate(m1.Currency, m2.Currency);
        if (exchangeRate == null) throw new ArgumentException("Invalid currency");

        var m2InM1Currency = m2.Amount * exchangeRate.Value;
        return m1.Amount?.CompareTo(m2InM1Currency);

    }

    public record ExchangeRatesResponse(string? Base, DateTime Date, Dictionary<string, decimal>? Rates);

    public static async Task<decimal?> GetExchangeRate(string fromCurrency, string toCurrency)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync($"{BaseUrl}&symbols={toCurrency}&base={fromCurrency}");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var exchangeRates = JsonSerializer.Deserialize<ExchangeRatesResponse>(responseContent);

        return exchangeRates?.Rates?.GetValueOrDefault(toCurrency);
    }

    public static async Task<Dictionary<string, decimal>?> GetExchangeRates(string baseCurrency)
    {
        var url = $"https://openexchangerates.org/api/latest.json?app_id={AppId}&base={baseCurrency}";

        using var client = new HttpClient();
        using var response = await client.GetAsync(url);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new($"Failed to retrieve exchange rates: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var exchangeRates = JsonSerializer.Deserialize<ExchangeRatesResponse>(responseContent);

        return exchangeRates?.Rates;
    }


    public static async Task<bool?> IsValidCurrency(string currencyCode)
    {
        using var httpClient = new HttpClient();
        var url = $"https://openexchangerates.org/api/currencies.json?app_id={AppId}";
        
        var response = await httpClient.GetAsync(url);

        switch (response.IsSuccessStatusCode) {
            case true: {
                var responseString = await response.Content.ReadAsStringAsync();
                var currencies = JObject.Parse(responseString);

                return currencies.ContainsKey(currencyCode);
            }
            default:
                return null;
        }
    }
}