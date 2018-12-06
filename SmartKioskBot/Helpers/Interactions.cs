using System;
using System.Collections.Generic;
using SmartKioskBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace SmartKioskBot.Dialogs
{
    public abstract class Interactions
    {
        /*
         * Common
         */
        public const string invalid_option = "Opção Inválida";
        public const string tries_exceeded = "Ooops! Ultrapassou o número máximo de tentativas!";
        public static List<string> Yes = new List<string> { "sim", "Sim", "Ok", "ok" };
        public static List<string> No = new List<string> { "não", "Não" };
        public enum State { SUCCESS, FAIL };
        public const string next_pagination = "Ver Mais";
        public const string unknown_intention = "Não entendi aquilo que disse, poderia refrasear?";


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
        public static string show_store_with_stock = "verificar disponibilidade do produto:";
        public static string in_store_location1 = "localização dentro da loja:";

        public const string email = "Email";
        public const string store_card = "Cartão da Loja";

        public static async Task SendMessage(IDialogContext context, string msg, int delay_before, int delay_after)
        {
            Thread.Sleep(delay_before);
            await context.PostAsync(msg);
            Thread.Sleep(delay_after);
        }

        /*
         * Intents Dialog
         */

        public static String Greeting(String user_name)
        {
            String[] greetings = {
                "Bem-vindo à Technon " + user_name + "! O meu nome é Sr. Technon. Em que o posso ajudar?",
                "Olá " + user_name + "! O meu nome é Sr.Technon, procura alguma coisa em específico?",
                "Olá sou o Sr.Technon, em que o posso ajudar " + user_name + "?"
            };
            return greetings[new Random().Next(0, greetings.Length)];
        }

        public static String MainMenu()
        {
            String[] info = { "Este é o menu principal.\n" + 
                    "Aqui poderei conseguir identificá-lo como cliente, " +
                "ajudar na procura de produtos do nosso catálogo, opinar sobre quais os melhores e até dar-lhe " +
                "algumas recomendações.\n" +
                "Também lhe posso  mostrar os seus produtos favoritos e, caso esteja interessado," +
                "também mostrar as lojas Technon mais próximas.",

                "Por favor, selecione uma das opções do menu principal para que eu o possa ajudar.",
                "Este menu principal irá me ajudar a guiá-lo melhor. Por favor selecione a opção que deseja."
                };

            return info[new Random().Next(0, info.Length)];
        }

        //ACCOUNT
        public static String NotLoggedIn()
        {
            String[] registered = {
                "É um cliente da Technon? Por favor identifique-se, assim poderei ajudá-lo melhor!",
                "Já nos conhecemos? Se sim, por favor identifique-se para que eu me possa lembrar de quem é.",
                "Não me estou a lembrar de si. Poderia-se identificar? Assim já me vou recordar de si."
                };

            String[] not_registered =
            {
                "Se ainda não nos conhecemos não há problema, clique no botão abaixo para que eu me lembre de si numa próxima vez."
            };

            return registered[new Random().Next(0, registered.Length)] + "\n" + not_registered[new Random().Next(0, not_registered.Length)];
        }

        public static String AccountInfo(){
            String[] interaction =
                {
                "Esta é a informação que me deu sobre você. Se há algo que não está certo por favor me corrija, clique no " +
                    "botão para que eu possa alterar esta informação.\nSe você não for esta pessoa por favor termine a conversa. Obrigada"
            };

            return interaction[new Random().Next(0, interaction.Length)];
        }

        public static String Login(string name)
        {
            String[] interaction =
                {
                name + " nem sei como é que não me lembrei de si mais cedo!",
                "Bem-vindo de volta " + name + "desculpe não me ter lembrado de você."
            };

            return interaction[new Random().Next(0, interaction.Length)];
        }
        
        public static String Register(User u)
        {
            string msg = "Prazer em conhecê-l";
            if (u.Gender== "Feminino")
                msg += "a";
            else
                msg += "o";

            msg += " " + u.Name + ", eu sou o Sr.Technon.";

            return msg;
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
        public static String getFilter(State state, int page)
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
            return "Página " + page + " - " + result[new Random().Next(0, result.Length)];
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
        public static String getWishList(State state,int page)
        {
            String[] result = new String[] { };

            if(state == State.SUCCESS)
            {
                result = new string[]{
                    "Página " + page + " - Aqui está a sua lista de desejos:"
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
                "Melhor " + comparison + ":"
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
                "Produto adicionado com sucesso ao comparador!"
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

        //Store info
        public static String getStockFail()
        {
            String[] fail = {
                "Nenhuma das nossas lojas tem esse produto em stock.",
            };
            return fail[new Random().Next(0, fail.Length)];
        }

        public static String getStockSuccess()
        {
            String[] success = {
                "Temos esse produto em stock nas seguintes lojas:",
                "As seguintes lojas têm esse produto em stock:"
            };
            return success[new Random().Next(0, success.Length)];
        }

        public static String getClosesStore()
        {
            String[] success = {
                "Estas são as lojas mais próximas de si:"
            };
            return success[new Random().Next(0, success.Length)];
        }
    }
}
