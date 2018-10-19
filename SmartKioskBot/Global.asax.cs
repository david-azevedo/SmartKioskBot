using System;
using Autofac;
using System.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using System.Web.Http;

namespace SmartKioskBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            var store = new TableBotDataStore("DefaultEndpointsProtocol=https;AccountName=skbstate;AccountKey=Ft/dju7+FVoYvfAos0DCWz/bSSo5weHlVovSS2i7AhsBhw/yJI3KyYDr+87CauHX+898N5Gbz1CPYvBvI9XZzw==;EndpointSuffix=core.windows.net");
            //var uri = new Uri(ConfigurationManager.AppSettings["DocumentDbUrl"]);
            //var key = ConfigurationManager.AppSettings["DocumentDbKey"];
            //var store = new DocumentDbBotDataStore(uri, key);

           Conversation.UpdateContainer(
           builder =>
           {
               builder.Register(c => store)
                         .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                         .AsSelf()
                         .SingleInstance();

               builder.Register(c => new CachingBotDataStore(store,
                          CachingBotDataStoreConsistencyPolicy
                          .ETagBasedConsistency))
                          .As<IBotDataStore<BotData>>()
                          .AsSelf()
                          .InstancePerLifetimeScope();

               
           });
        }
    }
}