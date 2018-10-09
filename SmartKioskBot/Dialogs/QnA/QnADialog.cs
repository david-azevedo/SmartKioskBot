using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SmartKioskBot.Dialogs.QnA
{
    public class QnADialog
    {
        public static async Task<QnAMakerResult> MakeRequest(string question)
        {
            var client = new HttpClient();

            // Request headers
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AppSettings.QNA_MAKER_SUBSCRIPTION_KEY);

            HttpResponseMessage response;
            
            // Request body
            byte[] byteData = Encoding.UTF8.GetBytes(String.Format("{{ \"question\": \"{0}\"}}", question));

            using (var content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue(AppSettings.QNA_MAKER_CONTENT_TYPE); ;
                response = await client.PostAsync(AppSettings.QNA_MAKER_URI, content);
            }

            List<QnAMakerResult> results = new List<QnAMakerResult>();

            if (response.IsSuccessStatusCode)
            {

                string res = response.Content.ReadAsStringAsync().Result;
                if (res.Contains("No good match found in the KB"))
                    return null;
                DataSet dataSet = JsonConvert.DeserializeObject<DataSet>(res);

                DataTable dataTable = dataSet.Tables["answers"];

                foreach (DataRow row in dataTable.Rows)
                {
                    results.Add(new QnAMakerResult(row["answer"].ToString(), Double.Parse(row["score"].ToString())));
                }

                return results[0];
            }

            return null;
        }
    }
}