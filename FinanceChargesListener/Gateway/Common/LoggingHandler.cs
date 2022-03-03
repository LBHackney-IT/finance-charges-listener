using Amazon.Lambda.Core;

namespace FinanceChargesListener.Gateway.Common
{
    public static class LoggingHandler
    {
        public static void LogError(string message)
        {
            LambdaLogger.Log($"[ERROR]: {message}");
        }

        public static void LogWarning(string message)
        {
            LambdaLogger.Log($"[WARNING]: {message}");
        }

        public static void LogInfo(string message)
        {
            LambdaLogger.Log($"[INFO]: {message}");
        }
    }
}
