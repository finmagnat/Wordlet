namespace Core.Services.Shop
{
    public readonly struct PurchaseResult
    {
        public readonly bool Success;
        public readonly string Error;

        public PurchaseResult(bool success, string error = null)
        {
            Success = success;
            Error = error;
        }

        public static PurchaseResult Ok() => new(true);
        public static PurchaseResult Fail(string error) => new(false, error);
    }
}