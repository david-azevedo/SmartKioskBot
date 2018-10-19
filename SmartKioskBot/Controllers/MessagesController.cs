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
                        
                        using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
                        {
                            /*
                            var botDataStore = scope.Resolve<IBotDataStore<BotData>>();
                            var key = new AddressKey()
                            {
                                BotId = activity.Recipient.Id,
                                ChannelId = activity.ChannelId,
                                UserId = activity.From.Id,
                                ConversationId = activity.Conversation.Id,
                                ServiceUrl = activity.ServiceUrl
                            };
                            var userData = await botDataStore.LoadAsync(key, BotStoreType.BotConversationData, CancellationToken.None);

                            //var varName = userData.GetProperty<string>("varName");
                            //userData.SetProperty<object>("varName", null);

                            await botDataStore.SaveAsync(key, BotStoreType.BotConversationData, userData, CancellationToken.None);
                            //await botDataStore.FlushAsync(key, CancellationToken.None);
                            */

                        }
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
                                        //initiate user data
                                        var botDataStore = scope.Resolve<IBotDataStore<BotData>>();
                                        var key = new AddressKey()
                                        {
                                            BotId = activity.Recipient.Id,
                                            ChannelId = activity.ChannelId,
                                            UserId = activity.From.Id,
                                            ConversationId = activity.Conversation.Id,
                                            ServiceUrl = activity.ServiceUrl
                                        };
                                        var userData = await botDataStore.LoadAsync(key, BotStoreType.BotConversationData, CancellationToken.None);

                                        //var varName = userData.GetProperty<string>("varName");
                                        userData.SetProperty<List<ObjectId>>(StateHelper.WISHLIST_ATR,new List<ObjectId>());
                                        userData.SetProperty<List<Filter>>(StateHelper.FILTERS_ATR, new List<Filter>());
                                        userData.SetProperty<List<ObjectId>>(StateHelper.COMPARATOR_ATR, new List<ObjectId>());
                                        userData.SetProperty<List<FilterCount>>(StateHelper.FILTER_COUNT_ATR, new List<FilterCount>());
                                        userData.SetProperty<List<ProductClicks>>(StateHelper.PRODUCT_CLICKS_ATR, new List<ProductClicks>());

                                        await botDataStore.SaveAsync(key, BotStoreType.BotConversationData, userData, CancellationToken.None);
                                        /*
                                        reply.Text = $"Welcome {newMember.Name}!";
                                        await client.Conversations.ReplyToActivityAsync(reply);
                                        */
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
    public class AddressKey : IAddress
    {
        public string BotId { get; set; }
        public string ChannelId { get; set; }
        public string ConversationId { get; set; }
        public string ServiceUrl { get; set; }
        public string UserId { get; set; }
    }
}