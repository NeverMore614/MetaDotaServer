﻿namespace MetaDotaServer.Tool
{
    public class TokenValidator
    {
        public static bool Validate(string token, out string accountId, out int expireTime)
        {
            accountId = token;
            expireTime = 120;
            return true;
        }
    }
}
