
using api.Exceptions;
using Azure;
using System.Text.Json;

namespace api.Features.Currency
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CurrencyService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["CurrencyService:ApiKey"]!;
        }
        public async Task<decimal> GetExchangeRateAsync(Enum.Currency fromCurrency, Enum.Currency toCurrency)
        {
            var url = $"https://v6.exchangerate-api.com/v6/{_apiKey}/pair/{fromCurrency.ToString()}/{toCurrency.ToString()}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new FailedToFetchExchangeRateException();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse>(content);

            if (result == null || result.Result != "success")
            {
                throw new FailedToFetchExchangeRateException();
            }

            return result.ConversionRate;
        }
    }
}
