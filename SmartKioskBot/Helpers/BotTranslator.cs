using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace SmartKioskBot.Helpers
{
    class BotTranslator
    {

        private string[] languageCodes;     // array of language codes

        // Dictionary to map language code from friendly name (sorted case-insensitively on language name)
        private SortedDictionary<string, string> languageCodesAndTitles =
            new SortedDictionary<string, string>(Comparer<string>.Create((a, b) => string.Compare(a, b, true)));

        public BotTranslator()
        {
            if (AppSettings.TEXT_TRANSLATION_API_SUBSCRIPTION_KEY.Length != 32
                || AppSettings.TEXT_ANALYTICS_API_SUBSCRIPTION_KEY.Length != 32
                || AppSettings.BING_SPELL_CHECK_API_SUBSCRIPTION_KEY.Length != 32)
            {
                //TODO Raise exception
            }
            else
            {
                GetLanguagesForTranslate();     // get codes of languages that can be translated
                GetLanguageNames();             // get friendly names of languages
            }
        }

        private string[] GetLanguageCodes()
        {
            return languageCodes;
        }

        // ***** DETECT LANGUAGE OF TEXT TO BE TRANSLATED
        private string DetectLanguage(string text)
        {
            string uri = AppSettings.TEXT_ANALYTICS_API_ENDPOINT + "languages?numberOfLanguagesToDetect=1";

            // create request to Text Analytics API
            HttpWebRequest detectLanguageWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            detectLanguageWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", AppSettings.TEXT_ANALYTICS_API_SUBSCRIPTION_KEY);
            detectLanguageWebRequest.Method = "POST";

            // create and send body of request
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            string jsonText = serializer.Serialize(text);

            string body = "{ \"documents\": [ { \"id\": \"0\", \"text\": " + jsonText + "} ] }";
            byte[] data = Encoding.UTF8.GetBytes(body);
            detectLanguageWebRequest.ContentLength = data.Length;

            using (var requestStream = detectLanguageWebRequest.GetRequestStream())
                requestStream.Write(data, 0, data.Length);

            HttpWebResponse response = (HttpWebResponse)detectLanguageWebRequest.GetResponse();

            // read and and parse JSON response
            var responseStream = response.GetResponseStream();
            var jsonString = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
            dynamic jsonResponse = serializer.DeserializeObject(jsonString);

            // fish out the detected language code
            var languageInfo = jsonResponse["documents"][0]["detectedLanguages"][0];
            if (languageInfo["score"] > (decimal)0.5)
                return languageInfo["iso6391Name"];
            else
                return "";
        }

        // ***** CORRECT SPELLING OF TEXT TO BE TRANSLATED
        private string CorrectSpelling(string text)
        {
            string uri = AppSettings.BING_SPELL_CHECK_API_ENDPOINT + "?mode=spell&mkt=en-US";

            // create request to Bing Spell Check API
            HttpWebRequest spellCheckWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            spellCheckWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", AppSettings.BING_SPELL_CHECK_API_SUBSCRIPTION_KEY);
            spellCheckWebRequest.Method = "POST";
            spellCheckWebRequest.ContentType = "application/x-www-form-urlencoded"; // doesn't work without this

            // create and send body of request
            string body = "text=" + System.Web.HttpUtility.UrlEncode(text);
            byte[] data = Encoding.UTF8.GetBytes(body);
            spellCheckWebRequest.ContentLength = data.Length;
            using (var requestStream = spellCheckWebRequest.GetRequestStream())
                requestStream.Write(data, 0, data.Length);
            HttpWebResponse response = (HttpWebResponse)spellCheckWebRequest.GetResponse();

            // read and parse JSON response and get spelling corrections
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var responseStream = response.GetResponseStream();
            var jsonString = new StreamReader(responseStream, Encoding.GetEncoding("utf-8")).ReadToEnd();
            dynamic jsonResponse = serializer.DeserializeObject(jsonString);
            var flaggedTokens = jsonResponse["flaggedTokens"];

            // construct sorted dictionary of corrections in reverse order in string (right to left)
            // so that making a correction can't affect later indexes
            var corrections = new SortedDictionary<int, string[]>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
            for (int i = 0; i < flaggedTokens.Length; i++)
            {
                var correction = flaggedTokens[i];
                var suggestion = correction["suggestions"][0];  // consider only first suggestion
                if (suggestion["score"] > (decimal)0.7)         // take it only if highly confident
                    corrections[(int)correction["offset"]] = new string[]   // dict key   = offset
                        { correction["token"], suggestion["suggestion"] };  // dict value = {error, correction}
            }

            // apply the corrections in order from right to left
            foreach (int i in corrections.Keys)
            {
                var oldtext = corrections[i][0];
                var newtext = corrections[i][1];

                // apply capitalization from original text to correction - all caps or initial caps
                if (text.Substring(i, oldtext.Length).All(char.IsUpper)) newtext = newtext.ToUpper();
                else if (char.IsUpper(text[i])) newtext = newtext[0].ToString().ToUpper() + newtext.Substring(1);

                text = text.Substring(0, i) + newtext + text.Substring(i + oldtext.Length);
            }

            return text;
        }

        // ***** GET TRANSLATABLE LANGUAGE CODES
        private void GetLanguagesForTranslate()
        {
            // send request to get supported language codes
            string uri = AppSettings.TEXT_TRANSLATION_API_ENDPOINT + "GetLanguagesForTranslate?scope=text";
            WebRequest WebRequest = WebRequest.Create(uri);
            WebRequest.Headers.Add("Ocp-Apim-Subscription-Key", AppSettings.TEXT_TRANSLATION_API_SUBSCRIPTION_KEY);
            WebResponse response = null;


            try
            {
                // read and parse the XML response
                response = WebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    System.Runtime.Serialization.DataContractSerializer dcs =
                        new System.Runtime.Serialization.DataContractSerializer(typeof(List<string>));
                    List<string> languagesForTranslate = (List<string>)dcs.ReadObject(stream);
                    languageCodes = languagesForTranslate.ToArray();
                }
            }
            catch (Exception e) {

            }
        }

        //***** GET FRIENDLY LANGUAGE NAMES
        private void GetLanguageNames()
        {

            try
            {
                // send request to get supported language names in English
                string uri = AppSettings.TEXT_TRANSLATION_API_ENDPOINT + "GetLanguageNames?locale=en";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", AppSettings.TEXT_TRANSLATION_API_SUBSCRIPTION_KEY);
                request.ContentType = "text/xml";
                request.Method = "POST";
                System.Runtime.Serialization.DataContractSerializer dcs =
                    new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String[]"));
                using (System.IO.Stream stream = request.GetRequestStream())
                    dcs.WriteObject(stream, languageCodes);

                // read and parse the XML response
                var response = request.GetResponse();
                string[] languageNames;
                using (Stream stream = response.GetResponseStream())
                    languageNames = (string[])dcs.ReadObject(stream);

                //load the dictionary for the combo box
                for (int i = 0; i < languageNames.Length; i++)
                    languageCodesAndTitles.Add(languageNames[i], languageCodes[i]);
            }
            catch (Exception) { }
        }

        // ***** PERFORM TRANSLATION ON BUTTON CLICK
        public string Translate(string textToTranslate, string fromLanguage, string toLanguage)
        {
            string fromLanguageCode;

            // auto-detect source language if requested
            if (fromLanguage == "Detect")
            {
                fromLanguageCode = DetectLanguage(textToTranslate);
                if (!languageCodes.Contains(fromLanguageCode))
                {
                    //TODO return Language Detection Failed
                    return "";
                }
            }
            else
                fromLanguageCode = languageCodesAndTitles[fromLanguage];

            string toLanguageCode = languageCodesAndTitles[toLanguage];

            // spell-check the source text if the source language is English
            if (fromLanguageCode == "en")
            {
                if (textToTranslate.StartsWith("-"))    // don't spell check in this case
                    textToTranslate = textToTranslate.Substring(1);
                else
                {
                    textToTranslate = CorrectSpelling(textToTranslate);
                }
            }

            // handle null operations: no text or same source/target languages
            if (textToTranslate == "" || fromLanguageCode == toLanguageCode)
            {
                //return something
                return "";
            }

            // send HTTP request to perform the translation
            string uri = string.Format(AppSettings.TEXT_TRANSLATION_API_ENDPOINT + "Translate?text=" +
                System.Web.HttpUtility.UrlEncode(textToTranslate) + "&from={0}&to={1}", fromLanguageCode, toLanguageCode);
            var translationWebRequest = HttpWebRequest.Create(uri);
            translationWebRequest.Headers.Add("Ocp-Apim-Subscription-Key", AppSettings.TEXT_TRANSLATION_API_SUBSCRIPTION_KEY);
            WebResponse response = null;
            response = translationWebRequest.GetResponse();

            // Parse the response XML
            Stream stream = response.GetResponseStream();
            StreamReader translatedStream = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
            System.Xml.XmlDocument xmlResponse = new System.Xml.XmlDocument();
            xmlResponse.LoadXml(translatedStream.ReadToEnd());

            // Update the translation field
            return (string)xmlResponse.InnerText;
        }


        /// <summary>
        /// //////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="args"></param>



        // ***** DETECT LANGUAGE OF TEXT TO BE TRANSLATED
        private async Task<string> DetectLanguageAsync(string text)
        {
            string uri = AppSettings.TEXT_ANALYTICS_API_ENDPOINT + "languages?numberOfLanguagesToDetect=1";
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AppSettings.TEXT_ANALYTICS_API_SUBSCRIPTION_KEY);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string jsonText = serializer.Serialize(text);

                StringContent body = new StringContent("{ \"documents\": [ { \"id\": \"0\", \"text\": " + jsonText + "} ] }", Encoding.UTF8, "application/json");

                HttpResponseMessage result = await client.PostAsync(uri, body);
                string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                dynamic jsonResponse = serializer.DeserializeObject(response);

                try
                {
                    // fish out the detected language code
                    var languageInfo = jsonResponse["documents"][0]["detectedLanguages"][0];
                    if (languageInfo["score"] > (decimal)0.5)
                    {
                        Console.WriteLine("\nLanguage: " + languageInfo["iso6391Name"]);
                        return languageInfo["iso6391Name"];
                    }
                    else
                    {
                        Console.WriteLine("\nLanguage not found ");
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return "";
                }
            }
        }

        // ***** CORRECT SPELLING OF TEXT TO BE TRANSLATED
        private async Task<string> CorrectSpellingAsync(string text, string languageCode)
        {
            string uri = AppSettings.BING_SPELL_CHECK_API_ENDPOINT + "?mode=spell&mkt=" + languageCode;

            Console.WriteLine("Starting Correct Spelling");

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AppSettings.BING_SPELL_CHECK_API_SUBSCRIPTION_KEY);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

                StringContent body = new StringContent("text=" + System.Web.HttpUtility.UrlEncode(text), Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage result = await client.PostAsync(uri, body);

                string response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                dynamic jsonResponse = serializer.DeserializeObject(response);

                try
                {
                    var flaggedTokens = jsonResponse["flaggedTokens"];
                    // construct sorted dictionary of corrections in reverse order in string (right to left)
                    // so that making a correction can't affect later indexes
                    var corrections = new SortedDictionary<int, string[]>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
                    for (int i = 0; i < flaggedTokens.Length; i++)
                    {
                        var correction = flaggedTokens[i];
                        var suggestion = correction["suggestions"][0];  // consider only first suggestion
                        if (suggestion["score"] > (decimal)0.7)         // take it only if highly confident
                            corrections[(int)correction["offset"]] = new string[]   // dict key   = offset
                                { correction["token"], suggestion["suggestion"] };  // dict value = {error, correction}
                    }

                    // apply the corrections in order from right to left
                    foreach (int i in corrections.Keys)
                    {
                        var oldtext = corrections[i][0];
                        var newtext = corrections[i][1];

                        // apply capitalization from original text to correction - all caps or initial caps
                        if (text.Substring(i, oldtext.Length).All(char.IsUpper)) newtext = newtext.ToUpper();
                        else if (char.IsUpper(text[i])) newtext = newtext[0].ToString().ToUpper() + newtext.Substring(1);

                        text = text.Substring(0, i) + newtext + text.Substring(i + oldtext.Length);
                    }

                    return text;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return "";
                }
            }
        }


        // ***** PERFORM TRANSLATION
        public async Task<Tuple<string, string>> TranslateAsync(string textToTranslate, string fromLanguage, string toLanguage)
        {
            string fromLanguageCode;

            // auto-detect source language if requested
            if (fromLanguage == "Detect")
            {
                Console.WriteLine("Starting Detection");
                fromLanguageCode = await DetectLanguageAsync(textToTranslate);
                if (!languageCodes.Contains(fromLanguageCode))
                {
                    //TODO return Language Detection Failed
                    return null;
                }
            }
            else
            {
                Console.WriteLine("Detected Language: " + fromLanguage);
                fromLanguageCode = languageCodesAndTitles[fromLanguage];
            }

            string toLanguageCode = languageCodesAndTitles[toLanguage];


            if (textToTranslate.StartsWith("-"))    // don't spell check in this case
                textToTranslate = textToTranslate.Substring(1);
            else
            {
                textToTranslate = await CorrectSpellingAsync(textToTranslate, fromLanguageCode);
                Console.WriteLine(textToTranslate);
            }
            

            // handle null operations: no text or same source/target languages
            if (textToTranslate == "")
            {
                //return something
                return null;
            }



            // send HTTP request to perform the translation
            string uri = string.Format(AppSettings.TEXT_TRANSLATION_API_ENDPOINT + "Translate?text=" +
                System.Web.HttpUtility.UrlEncode(textToTranslate) + "&from={0}&to={1}", fromLanguageCode, toLanguageCode);



            Console.WriteLine(uri);



            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AppSettings.TEXT_TRANSLATION_API_SUBSCRIPTION_KEY);

                HttpResponseMessage response = await client.GetAsync(uri);

                string result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(result);

                System.Xml.XmlDocument xmlResponse = new System.Xml.XmlDocument();
                xmlResponse.LoadXml(result);

                Console.WriteLine("Inner Text: " + xmlResponse.InnerText);

                return new Tuple <string, string> (xmlResponse.InnerText, fromLanguageCode);
            }

        }


        private static async Task<string> DirectTranslateAsync(string textToTranslate, string fromLanguageCode, string toLanguageCode)
        {
            

            // handle null operations: no text or same source/target languages
            if (textToTranslate == "")
            {
                //return something
                return "";
            }



            // send HTTP request to perform the translation
            string uri = string.Format(AppSettings.TEXT_TRANSLATION_API_ENDPOINT + "Translate?text=" +
                System.Web.HttpUtility.UrlEncode(textToTranslate) + "&from={0}&to={1}", fromLanguageCode, toLanguageCode);



            Console.WriteLine(uri);



            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AppSettings.TEXT_TRANSLATION_API_SUBSCRIPTION_KEY);

                HttpResponseMessage response = await client.GetAsync(uri);

                string result = await response.Content.ReadAsStringAsync();

                Console.WriteLine(result);

                System.Xml.XmlDocument xmlResponse = new System.Xml.XmlDocument();
                xmlResponse.LoadXml(result);

                Console.WriteLine("Inner Text: " + xmlResponse.InnerText);

                return (string) xmlResponse.InnerText;
            }

        }


        public static async Task PostTranslated(IDialogContext context, string reply, string userLanguageCode)
        {
            // Traduzir
            string text = await DirectTranslateAsync(reply, "pt", userLanguageCode);
            await context.PostAsync(reply);

        }
        public static async Task PostTranslated(IDialogContext context, IMessageActivity reply, string userLanguageCode)
        {
            // Traduzir
            //reply.Text = await DirectTranslateAsync(reply.Text, "pt", userLanguageCode);
            foreach(Attachment a in reply.Attachments)
            {
                if(a.ContentType == HeroCard.ContentType)
                {
                    foreach(CardAction b in ((HeroCard)a.Content).Buttons)
                    {
                        b.Text = await DirectTranslateAsync(b.Text, "pt", userLanguageCode);
                    }
                }
            }


            await context.PostAsync(reply);

        }
    }
}
