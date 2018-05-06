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
        public enum State { SUCCESS, FAIL };
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
        public static string add_wish_list = "Adicionar à lista de desejos o produto: ";
        public static string add_to_comparator = "Adicionar ao comparador o produto: ";
        public static string rem_wish_list = "Remover da lista de desejos o produto: ";
        public static string rem_comparator = "Remover do comparador o produto: ";
        public static string add_comparator = "Adicionar ao comparador o produto: ";
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
        public static String getViewFilters(State state)
        {
            String[] result = new string[] { };
            if(state == State.SUCCESS)
            {
                result = new string[]{
                    "Os filtros que estão a ser aplicados na pesquisa são:"
                };
            }
            else
            {
                result = new string[]{
                    "Não tem filtros aplicados na pesquisa."
                };
            }
            return result[new Random().Next(0, result.Length)];
        }
        public static String getFilter(State state)
        {
            String[] result = new string[] { };

            if(state == State.SUCCESS)
            {
                result = new string[]{
                    "Temos os seguintes produtos com essas características:",
                "Temos várias opções com essas especificações:"
            };
            }
            else
            {
                result = new string[]{
                    "Não temos nenhum produto com tais características.",
                "Não existem produtos com essas especificações"
            };
            }
            return result[new Random().Next(0, result.Length)];
        }
        public static String getRemovedFilter(State state, string filtername)
        {
            String[] result = new string[] { };

            if (state == State.SUCCESS)
            {
                result = new string[]{
                    "O filtro " + filtername + " foi removido com successo da pesquisa."
            };
            }
            else
            {
                result = new string[]{
                    "Não existe nenhum filtro " + filtername + " aplicado na pesquisa."
            };
            }
            return result[new Random().Next(0, result.Length)];
        }
        public static String getCleanAllFilters()
        {
            String[] result  = new string[]{
                    "Todos os filtros foram removidos.",
                    "Todos os filtros foram retirados."
            };
            
            return result[new Random().Next(0, result.Length)];
        }

        //WISHLIST
        public static String getWishList(State state)
        {
            String[] result = new String[] { };

            if(state == State.SUCCESS)
            {
                result = new string[]{
                    "Aqui está a sua lista de desejos:"
               };

            }
            else
            {
                result = new string[] {
                    "De momentos não tem nada na sua lista de desejos."
                };
            }

            return result[new Random().Next(0, result.Length)];
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

        //COMPARATOR
        public static String getComparator(string comparison)
        {
            String[] success =
            {
                "Top score para " + comparison + ":"
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

        //COMPARATOR
        public static String getOngoingComp()
        {
            String[] success =
            {
                "Estou a analisar os computadores..."
            };
            return success[new Random().Next(0, success.Length)];
        }
        public static String getResultComp()
        {
            String[] success =
            {
                "Aqui está o resultado da comparação:\n\n"
            };
            return success[new Random().Next(0, success.Length)];
        }
        public static String getAddComparator()
        {
            String[] success =
            {
                "Produto adicionado com sucesso ao comparador!\n\n"
            };
            return success[new Random().Next(0, success.Length)];
        }
        public static String getRemComparator()
        {
            String[] success =
            {
                "Produto removido com sucesso do comparador!"
            };
            return success[new Random().Next(0, success.Length)];
        }

    }
}
