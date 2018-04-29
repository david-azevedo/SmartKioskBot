using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using SmartKioskBot.Models;

namespace SmartKioskBot.Dialogs
{
    public abstract class BotDefaultAnswers
    {
        public static string[] Yes = new string[] { "sim", "Sim", "Ok", "ok" };
        public static string[] No = new string[] { "não", "Não" };
        /*
         * Activity Dialog
         */

        public static String getMemberAdded()
        {
            String[] welcomes =
            {
                "Bem-vindo de volta, reparei agora que está online! Posso ajudá-lo ?",
                "Estou contente que tenha voltado! Necessita de alguma coisa?"
            };

            return welcomes[new Random().Next(0, welcomes.Length)];
        }

        public static String getMemberRem()
        {
            //TODO
            return null;
        }

        /*
         * Button Click Message
         */
        public static string show_product_details = "Detalhes";
        public static string add_wish_list = "AdicionarWishList";
        public static string add_to_comparator = "AdicionarComparador";
        public static string rem_wish_list = "RemoverWishList";
        public static string rem_comparator = "RemoverComparador";
        public static string set_customer_email = "SaveEmail";
        public static string set_customer_card = "SaveCard";
        public static string add_channel = "AddChannel";
        public static string set_customer_name = "SaveName";
        public static string set_customer_country = "SaveCountry";
        /*
         * Intents Dialog
         */

        public static String getGreeting(String user_name)
        {
            String[] greetings = {
                "Bem vindo à Technon " + user_name + "! O meu nome é Sr. Technon. Em que o posso ajudar?",
                "Olá " + user_name + "! O meu nome é Sr.Technon, procura alguma coisa em específico?",
                "Olá sou o Sr.Technon, em que o posso ajudar " + user_name + "?"
            };
            return greetings[new Random().Next(0,greetings.Length)];
        }

        //FILTERS
        public static String getFilterFail()
        {
            String[] fail = {
                "Não temos nenhum produto com tais características.",
                "Não existem produtos com essas especificações"
            };
            return fail[new Random().Next(0, fail.Length)];
        }

        public static String getFilterSuccess()
        {
            String[] success = {
                "Temos os seguintes produtos com essas características:",
                "Temos várias opções com essas especificações:"
            };
            return success[new Random().Next(0, success.Length)];
        }

        //WISHLIST
        public static String getWishList()
        {
            String[] success =
            {
                "Aqui está a sua lista de desejos:"
            };
            return success[new Random().Next(0, success.Length)];
        }

        public static String getEmptyWishList()
        {
            String[] success =
            {
                "De momentos não tem nada na sua lista de desejos."
            };
            return success[new Random().Next(0, success.Length)];
        }

        public static String getAddWishList()
        {
            String[] success =
            {
                "Produto adicionado com sucesso à sua lista de desejos!"
            };
            return success[new Random().Next(0, success.Length)];
        }

        public static String getRemWishList()
        {
            String[] success =
            {
                "Produto removido com sucesso da sua lista de desejos!"
            };
            return success[new Random().Next(0, success.Length)];
        }

        //IDENTIFICATION
        public static String getIdentification()
        {
            String[] dialog =
            {
                "Olá, já nos conhecemos? Pode indicar o seu email ou numero de cliente?"
            };

            return dialog[new Random().Next(0, dialog.Length)];
        }
        public static String getCustomerCardOrEmail(String n)
        {
            String[] dialog =
            {
                "Olá " + n + ", pode introduzir o seu email ou cartão cliente?"
            };

            return dialog[new Random().Next(0, dialog.Length)];
        }
        public static String getAddIdentifier(String identifier, String value)
        {
            String[] dialog =
            {
               "O seu " + identifier + " foi atualizado para : " + value
            };

            return dialog[new Random().Next(0, dialog.Length)];
        }
        public static String getActionCanceled()
        {
            String[] dialog =
            {
               "Acção cancelada!"
            };

            return dialog[new Random().Next(0, dialog.Length)];
        }
        public static String getCustomerInfo(User user)
        {
            String[] dialog =
            {
               "INFORMAÇÃO DO UTILIZADOR \n\nNome: " + user.Name + "\n\n Email: " + user.Email + "\n\n Cartão cliente: " + user.CustomerCard + "\n\n País: " + user.Country
            };

            return dialog[new Random().Next(0, dialog.Length)];
        }
        public static String getCountry()
        {
            String[] dialog =
            {
               "Introduza o seu país"
            };

            return dialog[new Random().Next(0, dialog.Length)];
        }
        public static String getAddUser()
        {
            String[] dialog =
            {
               "A sua conta foi criada com sucesso!"
            };

            return dialog[new Random().Next(0, dialog.Length)];
        }
        
    }
}
