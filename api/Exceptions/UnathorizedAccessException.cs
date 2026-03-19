namespace api.Exceptions
{
    public class UnathorizedAccessException: Exception
    {
        public UnathorizedAccessException(): base("User does not have access to this bank account.")
        {
            
        }
    }
}
