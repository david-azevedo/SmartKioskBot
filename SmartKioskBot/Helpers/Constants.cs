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

        public enum DialogType {MENU, FILTER, COMPARE, WISHLIST, RECOMMENDATION, STORE, ACCOUNT, TUTORIAL};

        public const string brand_filter = "marca";
        public const string ram_filter = "ram";
        public const string storage_type_filter = "tipo_armazenamento";
        public const string cpu_family_filter = "familia_cpu";
        public const string gpu_filter = "placa_grafica";
        public const string type_filter = "tipo";
        public const string price_filter = "preço";
        public const string storage_filter = "armazenamento";
        public const string screen_size_filter = "tamanho_ecra";
        public const string autonomy_filter = "autonomia";

        public static string getDialogName(DialogType d)
        {
            switch (d)
            {
                case DialogType.ACCOUNT:
                    return "account";
                case DialogType.COMPARE:
                    return "compare";
                case DialogType.FILTER:
                    return "filter";
                case DialogType.MENU:
                    return "menu";
                case DialogType.RECOMMENDATION:
                    return "recommendation";
                case DialogType.STORE:
                    return "store";
                case DialogType.WISHLIST:
                    return "wishlist";
                case DialogType.TUTORIAL:
                    return "tutorial";
            }
            return "";
        }

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