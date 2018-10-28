using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System.Threading;
using MongoDB.Bson;
using static SmartKioskBot.Models.Context;
using static SmartKioskBot.Models.Customer;
using SmartKioskBot.Helpers;
using SmartKioskBot.Dialogs;

namespace SmartKioskBot.Controllers
{

    /// <summary>
    /// A sample ICredentialProvider that is configured by multiple MicrosoftAppIds and MicrosoftAppPasswords
    /// </summary>
    public class MultiCredentialProvider : ICredentialProvider
    {
        public Dictionary<string, string> credentials = new Dictionary<string, string>
        {
            { "347f0de7-3c0c-44c7-9788-4ec424eb943b", "gwbzILZ83@;cguIZSH028;[" }
        };

        public Task<bool> IsValidAppIdAsync(string appId)
        {
            return Task.FromResult(this.credentials.ContainsKey(appId));
        }

        public Task<string> GetAppPasswordAsync(string appId)
        {
            return Task.FromResult(this.credentials.ContainsKey(appId) ? this.credentials[appId] : null);
        }

        public Task<bool> IsAuthenticationDisabledAsync()
        {
            return Task.FromResult(!this.credentials.Any());
        }
    }

    /// Use the MultiCredentialProvider as credential provider for BotAuthentication
    [BotAuthentication(CredentialProviderType = typeof(MultiCredentialProvider))]
    public class MessagesController : ApiController
    {
        static MessagesController()
        {

            // Update the container to use the right MicorosftAppCredentials based on
            // Identity set by BotAuthentication
            var builder = new ContainerBuilder();

            builder.Register(c => ((ClaimsIdentity)HttpContext.Current.User.Identity).GetCredentialsFromClaims())
                .AsSelf()
                .InstancePerLifetimeScope();
            builder.Update(Conversation.Container);
        }

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity != null)
            {
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        //Tuple<string, string> nt = await botTranslator.TranslateAsync(activity.Text, "Detect", "Portuguese");
                        //activity.Text = nt.Item1;

                        activity.Locale = "pt-PT";
                        await Conversation.SendAsync(activity, () => new SmartKioskBot.Dialogs.RootDialog());

                        break;

                    case ActivityTypes.ConversationUpdate:
                        IConversationUpdateActivity update = activity;
                        // resolve the connector client from the container to make sure that it is 
                        // instantiated with the right MicrosoftAppCredentials
                        using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                        {
                            var client = scope.Resolve<IConnectorClient>();
                            if (update.MembersAdded.Any())
                            {
                                var reply = activity.CreateReply();
                                foreach (var newMember in update.MembersAdded)
                                {
                                    if (newMember.Id != activity.Recipient.Id)
                                    {
                                        reply.Text = Interactions.Greeting("");
                                        await client.Conversations.ReplyToActivityAsync(reply);
                                        Thread.Sleep(2000);
                                        reply.Text = "Não me estou a lembrar de si. Já nos apresentamos antes?\n" +
                                            "Se for do seu interesse, aceda ao meu menu principal para que se possa " +
                                            "apresentar ou identificar caso já se tenha apresentado antes. Se o fizer, poderei auxiliar-lhe de uma forma " +
                                            "mais eficiente e tornar a sua experiência de pré-compra mais pessoal e " +
                                            "personalizada.\nO menu principal permite-lhe conhecer e aceder às minhas áreas " +
                                            "de expertise para que o possa ajudar mais facilmente. Para ter acesso ao mesmo, basta me " +
                                            "pedir ajuda ou dizer-me que quer aceder ao menu principal. Poderá fazê-lo em qualquer altura.";
                                        await client.Conversations.ReplyToActivityAsync(reply);
                                    }
                                }
                            }
                        }
                        break;
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                        break;
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
}