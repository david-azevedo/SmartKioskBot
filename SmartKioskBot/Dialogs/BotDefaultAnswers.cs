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
         * Intents Dialog
         */

        public static String getGreeting(String user_name)
        {
            String[] greetings = {
                "Bem vindo à Technon " + user_name + "! O meu nome é Sr. Technon. Em que o posso ajudar?",
                "Olá " + user_name + "! O meu nome é Sr.Technon, procura alguma coisa em específico?",
                "Olá sou o Sr.Technon, em que o posso ajudar " + user_name + "?"
            };
            String br = "oi cara tudo bem? Meu nome é Technon e estou aqui para o ajudar.";
            return br;
        }
    }
}