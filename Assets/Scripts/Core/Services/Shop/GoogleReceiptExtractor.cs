using System;
using System.Collections.Generic;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.MiniJSON;

namespace Core.Services.Shop
{
    public static class GoogleReceiptExtractor
    {
        /// <summary>
        /// Извлекает данные из Unity IAP receipt для Google Play
        /// </summary>
        public static (string receiptJson, string signature, string transactionId) Extract(Product product)
        {
            if (string.IsNullOrEmpty(product.receipt))
                throw new Exception("Product receipt is empty");

            // receipt — это JSON:
            // { "Store":"GooglePlay", "TransactionID":"...", "Payload":"{...}" }
            var receiptWrapper = Json.Deserialize(product.receipt) as Dictionary<string, object>;
            if (receiptWrapper == null)
                throw new Exception("Invalid receipt wrapper");

            var transactionId = product.transactionID ?? string.Empty;

            if (!receiptWrapper.TryGetValue("Payload", out var payloadObj))
                throw new Exception("Receipt has no Payload");

            var payloadStr = payloadObj as string;
            if (string.IsNullOrEmpty(payloadStr))
                throw new Exception("Payload is empty");

            // Payload для Google — это JSON:
            // { "json":"{...}", "signature":"..." }
            var payload = Json.Deserialize(payloadStr) as Dictionary<string, object>;
            if (payload == null)
                throw new Exception("Invalid Google payload");

            if (!payload.TryGetValue("json", out var jsonObj) || jsonObj is not string receiptJson)
                throw new Exception("Google payload has no 'json'");

            if (!payload.TryGetValue("signature", out var sigObj) || sigObj is not string signature)
                throw new Exception("Google payload has no 'signature'");

            return (receiptJson, signature, transactionId);
        }
    }
}