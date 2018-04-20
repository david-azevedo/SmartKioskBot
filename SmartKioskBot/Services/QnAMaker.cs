using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;

namespace SmartKioskBot.Services
{
    public class QnAMaker
    {
        private static string KNOWLEDGE_BASE_ID = "bf9355c7-2edc-46c2-96b5-85e040a2da07";
        private static string QNAMAKER_SUBSCRIPTION_KEY = "2a10f8ce8c324d8a81cfff5a86d837b3";
        private static string QNAMAKER_BASE_URI = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0";

        public static Result MakeRequest(string query)
        {
            string responseString = string.Empty;

            //Build the URI
            var builder = new UriBuilder($"{QNAMAKER_BASE_URI}/knowledgebases/{KNOWLEDGE_BASE_ID}/generateAnswer");

            //Add the question as part of the body
            var postBody = $"{{\"question\": \"{query}\"}}";

            //Send the POST request
            using (WebClient client = new WebClient())
            {
                //Set the encoding to UTF8
                client.Encoding = System.Text.Encoding.UTF8;

                //Add the subscription key header
                client.Headers.Add("Ocp-Apim-Subscription-Key", QNAMAKER_SUBSCRIPTION_KEY);
                client.Headers.Add("Content-Type", "application/json");
                responseString = client.UploadString(builder.Uri, postBody);
            }

            return GetResultFromResponse(responseString);
        }

        public static Result GetResultFromResponse(string responseString)
        {
            //De-serialize the response
            Result response;
            try
            {
                response = JsonConvert.DeserializeObject<Result>(responseString);
            }
            catch
            {
                throw new Exception("Unable to deserialize QnA Maker response string.");
            }

            return response;
        }

        public class Result
        {
            /// <summary>
            /// The top answer found in the QnA Service.
            /// </summary>
            [JsonProperty(PropertyName = "answer")]
            public string Answer { get; set; }

            /// <summary>
            /// The score in range [0, 100] corresponding to the top answer found in the QnA    Service.
            /// </summary>
            [JsonProperty(PropertyName = "score")]
            public double Score { get; set; }
        }
    }
}