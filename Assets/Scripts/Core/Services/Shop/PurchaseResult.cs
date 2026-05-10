namespace Core.Services.Shop
{
    public readonly struct PurchaseResult
    {
        public readonly bool Success;
        public readonly string Error;
        public readonly bool IsError;

        public PurchaseResult(bool success, string error = null, bool isError = false)
        {
            Success = success;
            Error = error;
            IsError = isError;
        }

        public static PurchaseResult Ok() => new(true);
        public static PurchaseResult Fail(string error) => new(false, error);
        public static PurchaseResult ErrorResult(string error) => new(false, error, true);
    }
}
