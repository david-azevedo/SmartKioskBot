using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SmartKioskBot.Models;
using System;
using System.Threading.Tasks;
using static SmartKioskBot.Helpers.Constants;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;
using Newtonsoft.Json.Linq;
using SmartKioskBot.Logic;
using System.Collections.Generic;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using System.Threading;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class AccountDialog : IDialog<object>
    {
        private State state;

        public enum State { INIT, INPUT_HANDLER };

        public AccountDialog(State state)
        {
            this.state = state;
        }

        public async Task StartAsync(IDialogContext context)
        {
            switch (state)
            {
                case State.INIT:
                    if (StateHelper.IsLoggedIn(context))
                        await ViewAccountDialog(context, null);
                    else
                        await NotLoginDialog(context, null);
                    break;
                case State.INPUT_HANDLER:
                    context.Wait(InputHandler);
                    break;
            }
        }

        public async Task NotLoginDialog(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            string msg = "Já nos apresentamos antes? Se sim, por favor identifique-se para que me possa lembrar de si e oferecer-lhe uma experiência " +
                "mais personalizada e adequada a si.\nCaso ainda não nos tenhamos conhecido não há problema. Clique no botão abaixo para que se possa apresentar.";

            await Interactions.SendMessage(context, msg, 0, 1500);

            var reply = context.MakeMessage();
            Attachment att = await getCardAttachment(CardType.NOT_LOGIN);

            reply.Attachments.Add(att);
            await context.PostAsync(reply);

            await Interactions.SendMessage(context, "Não se sinta obrigado a se identificar ou a apresentar. Posso sempre tentar ajudá-lo na mesma.", 3000,0);

            context.Wait(InputHandler);
        }

        public async Task ViewAccountDialog(IDialogContext context, IAwaitable<IMessageActivity> activity)
        {
            string msg = "Esta é a informação que me deu sobre você. Se há algo que não está certo por favor avise-me, para isso, clique no " +
                    "botão para que eu possamos alterar a informação incorrecta.\nSe esta informação não pertencer a você por favor termine a conversa. Obrigado";
            await Interactions.SendMessage(context, msg, 0, 2000);

            var reply = context.MakeMessage();
            Attachment att = await getCardAttachment(CardType.VIEW_ACCOUNT);

            JObject json = att.Content as JObject;
            AccountLogic.SetAccountCardFields(json,context, false);

            reply.Attachments.Add(att);
            await context.PostAsync(reply);

            await Interactions.SendMessage(context, "Existe alguma questão em que lhe possa ser útil? Com o menu principal é mais fácil mostrar-lhe as áreas em que o posso ajudar.", 0, 3000);
            context.Call(new MenuDialog(MenuDialog.State.INIT), ResumeAfterDialogCall);
        }

        public async Task InputHandler(IDialogContext context, IAwaitable<object> argument)
        {
            var activity = await argument as Activity;

            //received message
            if (activity.Text != null)
                context.Done(new CODE(DIALOG_CODE.PROCESS_LUIS, activity));
            //received event
            else if (activity.Value != null)
                await EventHandler(context, activity);
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        private async Task EventHandler(IDialogContext context, Activity activity)
        {

            JObject json = activity.Value as JObject;
            List<InputData> data = getReplyData(json);

            //have mandatory info
            if (data.Count >= 2)
            {
                //json structure is correct
                if (data[0].attribute == REPLY_ATR && data[1].attribute == DIALOG_ATR)
                {
                    ClickType event_click = getClickType(data[0].value);
                    DialogType event_dialog = getDialogType(data[1].value);

                    //process in this dialog
                    if (event_dialog == DialogType.ACCOUNT &&
                        event_click != ClickType.NONE)
                    {
                        switch (event_click)
                        {
                            case ClickType.LOGIN:
                                await Login(context);
                                break;
                            case ClickType.LOGIN_START:
                                await LoginStart(context, data);
                                break;
                            case ClickType.REGISTER:
                                await Register(context);
                                break;
                            case ClickType.REGISTER_SAVE:
                                await RegisterSave(context, data);
                                break;
                            case ClickType.ACCOUNT_EDIT:
                                await EditAccount(context);
                                break;
                            case ClickType.ACCOUNT_SAVE:
                                await SaveAccount(context, data);
                                break;
                            case ClickType.LOGOUT:
                                await Logout(context);
                                break;
                        }
                    }
                    //process in parent dialog
                    else
                        context.Done(new CODE(DIALOG_CODE.PROCESS_EVENT, activity,event_dialog));
                }
                else
                    context.Done(new CODE(DIALOG_CODE.DONE));
            }
            else
                context.Done(new CODE(DIALOG_CODE.DONE));
        }

        public async Task Login(IDialogContext context)
        {
            string msg = "Poderia-me dizer o seu email?\nAssim será mais fácil recordar-me de si.";
            await Interactions.SendMessage(context, msg, 0, 1500);

            Attachment att = await getCardAttachment(CardType.LOGIN);

            var reply = context.MakeMessage();
            reply.Attachments.Add(att);
            await context.PostAsync(reply);

            context.Wait(InputHandler);
        }

        public async Task LoginStart(IDialogContext context, List<InputData> data)
        {
            var fail_text = AccountLogic.Login(data, context);
            if (fail_text != "")
                await context.PostAsync(fail_text);

            state = State.INIT;
            string msg = StateHelper.GetUser(context).Name + " agora já me lembro de você!";
            await Interactions.SendMessage(context, msg, 0, 1500);

            await StartAsync(context);
        }

        public async Task Register(IDialogContext context)
        {
            string msg = "Poderia preencher o formulário abaixo? Irá-me ajudar a conhecê-lo melhor e a lembrar-me de si numa próxima vez.";
            await Interactions.SendMessage(context, msg, 0, 1500);

            Attachment att = await getCardAttachment(CardType.REGISTER);

            var reply = context.MakeMessage();
            reply.Attachments.Add(att);
            await context.PostAsync(reply);

            context.Wait(InputHandler);
        }

        public async Task RegisterSave(IDialogContext context, List<InputData> data)
        {
            await Interactions.SendMessage(context, "Estou a analizar os seus dados ...", 0, 0);
            string fail_text = AccountLogic.Register(data, context);
            if (fail_text != "")
                await context.PostAsync(fail_text);
            else
                await Interactions.SendMessage(context, Interactions.Register(StateHelper.GetUser(context)), 0, 3000);

            state = State.INIT;

            await Interactions.SendMessage(context, "Existe alguma questão em que lhe possa ser útil? Com o menu principal é mais fácil mostrar-lhe as áreas em que o posso ajudar.", 0, 3000);
            context.Call(new MenuDialog(MenuDialog.State.INIT), ResumeAfterDialogCall);
        }

        public async Task EditAccount(IDialogContext context)
        {
            string msg = "Posso ter percebido mal alguma informação sobre si. Importar-se-ia de a corrigir no formulário abaixo?\nObrigado.";
            await Interactions.SendMessage(context, msg, 0, 1500);

            Attachment att = await getCardAttachment(CardType.EDIT_ACCOUNT);
            JObject content = att.Content as JObject;

            AccountLogic.SetAccountCardFields(content, context, true);

            var reply = context.MakeMessage();
            reply.Attachments.Add(att);
            await context.PostAsync(reply);
            context.Wait(InputHandler);
        }

        public async Task SaveAccount(IDialogContext context, List<InputData> data)
        {
            string fail_text = AccountLogic.SaveAccountInfo(data, context);
            if (fail_text != "")
                await context.PostAsync(fail_text);

            state = State.INIT;
            await Interactions.SendMessage(context, "Obrigado por me ter corrigido.", 0, 3000);
            await Interactions.SendMessage(context, "Existe alguma questão em que lhe possa ser útil? Com o menu principal é mais fácil mostrar-lhe as áreas em que o posso ajudar.", 1000, 3000);

            context.Call(new MenuDialog(MenuDialog.State.INIT), ResumeAfterDialogCall);
        }

        public async Task Logout(IDialogContext context)
        {
            await Interactions.SendMessage(context, "A nossa conversa foi terminada.\n Mas não se preocupe, podemos sempre continuá-la numa outra ocasião.", 0, 0);
            context.Done<CODE>(new CODE(DIALOG_CODE.RESET));
        }

        private async Task ResumeAfterDialogCall(IDialogContext context, IAwaitable<object> result)
        {
            CODE code = await result as CODE;

            //child dialog invoked an event of this dialog
            if (code.dialog == DialogType.ACCOUNT)
                await EventHandler(context, code.activity);
            else
                context.Done(code);
        }
    }
}
