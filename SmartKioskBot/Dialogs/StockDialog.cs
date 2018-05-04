using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using SmartKioskBot.Controllers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class StockDialog
    {

        public static List<string> CheckAvailability(string message)
        {
            string[] parts = message.Split(':');

            if (parts.Length >= 2)
                return ContextController.CheckAvailability(parts[1]);

            return new List<string>();
        }
    }
}