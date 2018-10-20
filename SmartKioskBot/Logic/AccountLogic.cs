using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json.Linq;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;

namespace SmartKioskBot.Logic
{
    public class AccountLogic
    {
        public enum CardField{ NAME, CLIENT_NUMBER, EMAIL, GENDER};

        //card field ids
        public const string name_field = "name";
        public const string email_field = "email";
        public const string client_id_field = "client_id";
        public const string gender_field = "gender";

        /*
         * UI Cards
         */

        public static void SetAccountCardFields(JToken card, IDialogContext context, bool edit_card)
        {
            User user = StateHelper.GetUser(context);
            var customer_card = user.CustomerCard;
            var info_tag = "text";
            if (customer_card == "")
                customer_card = "Desconhecido";

            if (edit_card)
                info_tag = "value";

            GetAccountCardSection(card, CardField.NAME)[info_tag] = user.Name;
            GetAccountCardSection(card, CardField.CLIENT_NUMBER)[info_tag] = user.CustomerCard;
            GetAccountCardSection(card, CardField.GENDER)[info_tag] = user.Gender;
            GetAccountCardSection(card, CardField.EMAIL)[info_tag] = user.Email;
        }

        private static JToken GetAccountCardSection(JToken card, CardField field)
        {
            switch (field)
            {
                case CardField.NAME:
                    return card.SelectToken("body[1].items[1]");
                case CardField.EMAIL:
                    return card.SelectToken("body[2].items[1]");
                case CardField.CLIENT_NUMBER:
                    return card.SelectToken("body[3].items[1]");
                case CardField.GENDER:
                    return card.SelectToken("body[4].items[1]");
            }

            return null;
        }

        public static string SaveAccountInfo(List<InputData> fields, IDialogContext context)
        {
            User user = StateHelper.GetUser(context);

            string fail = "";

            foreach(InputData f in fields)
            {
                switch (f.attribute)
                {
                    case name_field:
                        if(f.value != "")
                            user.Name = f.value;
                        break;
                    case email_field:
                        if(f.value != "")
                        {
                            try
                            {
                                var m = new MailAddress(f.value);
                                user.Email = f.value;
                            }
                            catch
                            {
                                fail += "O email não está no correcto formato (exemplo: user123@mail.com). ";
                            }
                        }                        
                        break;
                    case gender_field:
                        user.Gender = f.value;
                        break;
                    case client_id_field:
                        if (f.value != "")
                        {
                            if (UserController.getUserByCard(f.value) == null)
                                user.CustomerCard = f.value;
                            else
                                fail += "Já existe um outro email associado a este cartão de cliente. ";
                        }
                        break;

                }
            }

            UserController.SetUserInfo(user.Id,user.Country,user.Name,user.Email,user.CustomerCard,user.Gender);
            StateHelper.SetUser(context,user);
            return fail;
        }

        public static string Register(List<InputData> fields, IDialogContext context)
        {
            string fail = "";

            string name = "";
            string email = "";
            string card = "";
            string gender = "";

            foreach (InputData f in fields)
            {
                switch (f.attribute)
                {
                    case name_field:
                        if (f.value != "")
                            name = f.value;
                        else
                            fail += "O campo 'Nome' é obrigatório.\n";
                        break;
                    case email_field:
                        if (f.value != "")
                        {
                            try
                            {
                                var m = new MailAddress(f.value);
                                email = f.value;
                            }
                            catch
                            {
                                fail += "O email não está no formato correcto (exemplo: user123@mail.com).\n";
                            }
                        }
                        else
                            fail += "O campo 'Email' é obrigatório.\n";
                        break;
                    case gender_field:
                        gender = f.value;
                        break;
                    case client_id_field:
                        if (f.value != "")
                            card = f.value;
                        break;

                }
            }

            if (UserController.getUserByEmail(email) != null)
                fail += "O email que inseriu já está associado a uma conta.\n";
            else
            {
                Random r = new Random();
                UserController.CreateUser(email, name,(r.Next(25) + 1).ToString(), gender);
                User u = UserController.getUserByEmail(email);
                ContextController.CreateContext(u);
                CRMController.AddCustomer(u);

                if (card != "0")
                {
                    UserController.SetCustomerCard(u, card);
                    u.CustomerCard = card;
                }

                StateHelper.SetUser(context,u);
            }
            return fail;
        }

        public static string Login(List<InputData> fields, IDialogContext context)
        {
            foreach(InputData data in fields)
            {
                if (data.attribute.Equals(email_field))
                {
                    User u = UserController.getUserByEmail(data.value);

                    if (u != null)
                    {
                        StateHelper.Login(context, u);
                        return "";
                    }
                }
            }

            return "Não conheço ninguém com esse email. Deseja registar-se para que o possa " +
                            "conhecer melhor? Caso contrário pode tentar novamente. Por favor selecione uma das " +
                            "opções abaixo. Obrigada";
        }
    }
}