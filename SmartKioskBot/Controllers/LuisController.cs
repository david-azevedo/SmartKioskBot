using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace SmartKioskBot.Controllers
{
    public static class LuisController
    {
        static public async Task<LuisResult> GetLuisResult(string message)
        {
            var client = new HttpClient();
            var queryString = HttpUtility.ParseQueryString(string.Empty);

            // The request header contains the subscription key
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AppSettings.LuisSubscriptionKey);

            // The "q" parameter contains the utterance to send to LUIS
            queryString["q"] = message;

            // These optional request parameters are set to their default values
            queryString["timezoneOffset"] = "0";
            queryString["verbose"] = "false";
            queryString["spellCheck"] = "false";
            queryString["staging"] = "true";

            var uri = AppSettings.LuisBaseUri + AppSettings.LuisAppId + "?" + queryString;
            var response = await client.GetAsync(uri);

            var strResponseContent = await response.Content.ReadAsStringAsync();

            LuisResult result = JsonConvert.DeserializeObject<LuisResult>(strResponseContent);

            return result;
            // Display the JSON result from LUIS
            // await context.PostAsync(strResponseContent.ToString());
        }
    }
}