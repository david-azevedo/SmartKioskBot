using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace SmartKioskBot.Dialogs
{
    public abstract class BotDefaultAnswers
    {
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
        public static string show_product_details = "Ver detalhes do produto: ";
        public static string add_wish_list = "Adicionar à lista de desejos o produto:";
        public static string add_to_comparator = "Adicionar ao comparador o produto:";
        public static string rem_wish_list = "Remover da lista de desejos o produto:";
        public static string rem_comparator = "Remover do comparador o produto:";


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
    }
}