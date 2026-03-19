namespace api.Exceptions
{
    public class FailedToFetchExchangeRateException : Exception
    {
        public FailedToFetchExchangeRateException()
            : base("Failed to fetch exchange rate.") { }
    }
}
