namespace api.Features.Currency
{
    public interface ICurrencyService
    {
        Task<decimal> GetExchangeRateAsync(Enum.Currency fromCurrency, Enum.Currency toCurrency);
    }
}
