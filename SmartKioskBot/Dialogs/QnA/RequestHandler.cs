using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace SmartKioskBot.Dialogs.QnA
{
    public class RequestHandler
    {
        public QnAMakerResult MakeRequest(string query)
        {
            string responseString = string.Empty;

            var qnamakerHost = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0";

            var knowledgebaseId = "bf9355c7-2edc-46c2-96b5-85e040a2da07"; // Use knowledge base id created.
            var qnamakerSubscriptionKey = "2a10f8ce8c324d8a81cfff5a86d837b3"; //Use subscription key assigned to you.

            //Build the URI
            var builder = new UriBuilder($"{qnamakerHost}/knowledgebases/{knowledgebaseId}/generateAnswer");

            //Add the question as part of the body
            var postBody = $"{{\"question\": \"{query}\"}}";

            //Send the POST request
            using (WebClient client = new WebClient())
            {
                //Set the encoding to UTF8
                client.Encoding = System.Text.Encoding.UTF8;

                //Add the subscription key header
                client.Headers.Add("Ocp-Apim-Subscription-Key", qnamakerSubscriptionKey);
                client.Headers.Add("Content-Type", "application/json");
                responseString = client.UploadString(builder.Uri, postBody);
            }

            return QnAMakerResult.getResultFromResponse(responseString);
        }
    }
}