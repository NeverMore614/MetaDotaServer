namespace MetaDotaServer.Tool
{
    public class MDSTokenValidator
    {
        public static bool Validate(string token, out string accountId, out int expireTime)
        {
            accountId = token;
            expireTime = 2592000;
            return true;
        }
    }
}
