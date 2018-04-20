using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SmartKioskBot.Services
{
    public static class LUIS
    {
        private static string LUIS_APP_ID = "bdfe5d93-c03f-4eda-b3b6-679611555932";
        private static string LUIS_SUBSCRIPTION_KEY = "b17f8347b5874cdcbf4a867adf34db7f";
        private static string LUIS_BASE_URI = "https://westeurope.api.cognitive.microsoft.com/luis/v2.0/apps/";

        static public async Task<LuisResult> GetLuisResult(string message)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // The request header contains the subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", LUIS_SUBSCRIPTION_KEY);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["q"] = message;

            // These optional request parameters are set to their default values
            queryString["timezoneOffset"] = "0";
            queryString["verbose"] = "false";
            queryString["spellCheck"] = "false";
            queryString["staging"] = "true";

            var uri = LUIS_BASE_URI + LUIS_APP_ID + "?" + queryString;
            var response = await client.GetAsync(uri);

            var strResponseContent = await response.Content.ReadAsStringAsync();

            LuisResult result = JsonConvert.DeserializeObject<LuisResult>(strResponseContent);

            return result;
            // Display the JSON result from LUIS
            // await context.PostAsync(strResponseContent.ToString());
        }
    }
}