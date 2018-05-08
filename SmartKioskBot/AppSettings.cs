namespace SmartKioskBot
{
    public static class AppSettings
    {

        public static string DbName => "db";
        public static string ProductsCollection => "Products";
        public static string UserCollection => "User";
        public static string ContextCollection => "Context";
        public static string MongoDBConnectionString => "mongodb://skb-cosmos-mongo-v1:mD9P6JY7iA863jozlyaXWjTdkvEMJyG6N3mHFF9iTCYyUfN0dYfrLQXx3Fkg4meW9EVGnwWSMlJGNpg34OepnA==@skb-cosmos-mongo-v1.documents.azure.com:10255/?ssl=true&replicaSet=globaldb";
        
        public const string LuisAppId = "e597ae7c-2c5a-45ec-a117-446b58bfdc05";
        public const string LuisSubscriptionKey = "b17f8347b5874cdcbf4a867adf34db7f";
        public const string LuisBaseUri = "https://westeurope.api.cognitive.microsoft.com/luis/v2.0/apps/";
        public const string LuisDomain = "westeurope.api.cognitive.microsoft.com";

        public const string TEXT_TRANSLATION_API_SUBSCRIPTION_KEY = "3857a4b96cfe4f77981bad9911a820e8";
        public const string TEXT_ANALYTICS_API_SUBSCRIPTION_KEY = "a3d7916391904f0c85ce50d4dae582dd";
        public const string BING_SPELL_CHECK_API_SUBSCRIPTION_KEY = "fc3b8ec4609742028551d85793300af3";

        public const string TEXT_TRANSLATION_API_ENDPOINT = "https://api.microsofttranslator.com/v2/Http.svc/";
        public const string TEXT_ANALYTICS_API_ENDPOINT = "https://northeurope.api.cognitive.microsoft.com/text/analytics/v2.0/";
        public const string BING_SPELL_CHECK_API_ENDPOINT = "https://api.cognitive.microsoft.com/bing/v7.0/spellcheck/";

        public static string QNA_MAKER_URI = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/knowledgebases/74ade3b2-a647-44c2-ad04-3143be79af05/generateAnswer";
        public static string QNA_MAKER_SUBSCRIPTION_KEY = "cedae07238df4fa79fba4185cbe2780f";
        public static string QNA_MAKER_CONTENT_TYPE = "application/json";

    }
}