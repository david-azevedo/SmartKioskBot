using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartKioskBot.Helpers
{
    public abstract class Constants
    {
        public static int N_ITEMS_CARROUSSEL = 7;
        public static int MAX_N_FILTERS_RECOMM = 10;
        public static double INTENT_SCORE_THRESHOLD = 0.4;

        //Dialogue Response when exiting
        public enum DIALOG_CODE {
            PROCESS_LUIS,   //message needs to be processed by luis
            DONE }          //nothing to do 

        public class CODE
        {
            public DIALOG_CODE value;
            public IMessageActivity message;

            public CODE(DIALOG_CODE value)
            {
                this.value = value;
            }

            public CODE(DIALOG_CODE value, IMessageActivity message)
            {
                this.value = value;
                this.message = message;
            }
        }
    }
}