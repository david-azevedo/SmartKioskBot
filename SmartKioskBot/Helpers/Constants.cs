using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;

namespace SmartKioskBot.Helpers
{
    public abstract class Constants
    {
        public static int N_ITEMS_CARROUSSEL = 7;
        public static int MAX_N_FILTERS_RECOMM = 10;
        public static double INTENT_SCORE_THRESHOLD = 0.4;

        public enum DialogType {MENU, FILTER, COMPARE, WISHLIST, RECOMMENDATION, STORE, ACCOUNT, TUTORIAL, NONE};

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

        public static DialogType getDialogType(string name)
        {
            switch (name)
            {
                case "account":
                    return DialogType.ACCOUNT;
                case "compare":
                    return DialogType.COMPARE;
                case "filter":
                    return DialogType.FILTER;
                case "menu":
                    return DialogType.MENU;
                case "recommendation":
                    return DialogType.RECOMMENDATION;
                case "store":
                    return DialogType.STORE;
                case "wishlist":
                    return DialogType.WISHLIST;
                case "tutorial":
                    return DialogType.TUTORIAL;
            }
            return DialogType.NONE;
        }

        //Dialogue Response when exiting
        public enum DIALOG_CODE {
            PROCESS_EVENT,  //event to be processed
            PROCESS_LUIS,   //message needs to be processed by luis
            DONE,           //do nothing
        }        

        public class CODE
        {
            public DIALOG_CODE code;
            public Activity activity;
            public DialogType dialog = DialogType.NONE;

            public CODE(DIALOG_CODE value)
            {
                this.code = value;
            }

            public CODE(DIALOG_CODE value, Activity message)
            {
                this.code = value;
                this.activity = message;
            }

            public CODE(DIALOG_CODE value, Activity message, DialogType dialog)
            {
                this.code = value;
                this.activity = message;
                this.dialog = dialog;
            }
        }
    }
}