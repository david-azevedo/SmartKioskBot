using Microsoft.Bot.Connector;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartKioskBot.Dialogs;
using MongoDB.Driver;

namespace SmartKioskBot.UI
{
    public abstract class ProductCard
    {
        public static HeroCard getProductCard(Product p)
        {
            return new HeroCard
            {
                // title of the card  
                Title = p.Brand + "\n\n Modelo: " + p.Model,
                //subtitle of the card  
                Subtitle = p.Price + "€",
                // navigate to page , while tab on card  
                //Tap = new CardAction(ActionTypes.OpenUrl, "Learn More", value: "http://www.devenvexe.com"),
                //Detail Text  
                Text = p.Name,
                // list of  Large Image  
                Images = new List<CardImage> { new CardImage(p.Photo) },
                // list of buttons   
                Buttons = new List<CardAction> {
                        new CardAction(ActionTypes.ImBack, "Ver Detalhes", value: BotDefaultAnswers.show_product_details + " " + p.Id.ToString()),
                        new CardAction(ActionTypes.ImBack, "Adicionar à Wish List", value: BotDefaultAnswers.add_wish_list + " " + p.Id.ToString()),
                        new CardAction(ActionTypes.ImBack, "Adicionar ao Comparador", value: BotDefaultAnswers.add_to_comparator + " " + p.Id.ToString())}
            };
        }

        internal static object getProductDetailsCard(IFindFluent<Product, Product> product)
        {
            throw new NotImplementedException();
        }

        public static HeroCard getProductDetailsCard(Product p)
        {
            string details = "";
            details += "Nome: " + p.Name + "\n\n" +
                        "Processador: " + p.CPU + "\n\n" +
                        "Família de Processador: " + p.CPUFamily + "\n\n" +
                        "Velocidade do Processador: " + p.CPUSpeed + "\n\n" +
                        "Número de núcleos: " + p.CoreNr + "\n\n" +
                        "RAM: " + p.RAM + "\n\n" +
                        "Tipo de Armazenamento: " + p.StorageType + "\n\n" +
                        "Armazenamento: " + p.StorageAmount + "\n\n" +
                        "Tipo de Placa Gráfica: " + p.GraphicsCardType + "\n\n" +
                        "Placa Gráfica: " + p.GraphicsCard + "\n\n" +
                        "Memória Gráfica (Máximo): " + p.MaxVideoMem + "\n\n" +
                        "Autonomia: " + p.Autonomy + "\n\n" +
                        "Placa de Som: " + p.SoundCard + "\n\n" + 
                        "Tem Câmara: " + p.HasCamera + "\n\n" + 
                        "Teclado Númerico: " + p.NumPad + "\n\n" +
                        "Touch Bar: " + p.TouchBar + "\n\n" +
                        "Teclado Retroiluminado: " + p.BacklitKeybr + "\n\n" +
                        "Teclado Mecânico: " + p.MechKeybr + "\n\n" +
                        "Software: " + p.Software + "\n\n" +
                        "Sistema Operativo: " + p.OS + "\n\n" +
                        "Ecrã: " + p.Screen + "\n\n" +
                        "Diagonal do Ecrã: " + p.ScreenDiagonal + "\n\n" +
                        "Resolução do Ecrã: " + p.ScreenResolution + "\n\n" +
                        "Ecrã Tatil: " + p.TouchScreen + "\n\n" +
                        "EAN: " + p.EAN + "\n\n" +
                        "Marca: " + p.Brand + "\n\n" +
                        "Modelo: " + p.Model + "\n\n" +
                        "Garantia: " + p.Warranty + "\n\n" +
                        "Peso: " + p.Weight + "\n\n" +
                        "Cor: " + p.Colour + "\n\n" +
                        "Altura: " + p.Height + "\n\n" +
                        "Largura: " + p.Width + "\n\n" +
                        "Profundidade: " + p.Depth + "\n\n" +
                        "Garantia da Bateria: " + p.BatteryWarranty + "\n\n" +
                        "Conteúdo extra: " + p.ExtraContent + "\n\n" +
                        "Tipo: " + p.Type + "\n\n" +
                        "Drive: " + p.Drive + "\n" +
                        "Conetividade: " + p.Connectivity + "\n\n" +
                        "Ligações: " + p.Connections + "\n\n" +
                        "Mais Informações: " + p.MoreInfo + "\n\n" +
                        "Part Number: " + p.PartNr + "\n\n";

            return new HeroCard
            {
                Title = p.Brand + "\n\n Modelo: " + p.Model,
                Subtitle = p.Price + "€",
                Text = details,
                Images = new List<CardImage> { new CardImage(p.Photo) },
                // list of buttons   
                Buttons = new List<CardAction> {
                        new CardAction(ActionTypes.ImBack, "Adicionar à Wish List", value: BotDefaultAnswers.add_wish_list + ": " + p.Id),
                        new CardAction(ActionTypes.ImBack, "Adicionar ao Comparador", value: BotDefaultAnswers.add_to_comparator + ": " + p.Id),
                        new CardAction(ActionTypes.ImBack, "Ver Pacotes", value: BotDefaultAnswers.add_to_comparator + ": " + p.Id),
                        new CardAction(ActionTypes.ImBack, "Produtos Relacionados", value: BotDefaultAnswers.add_to_comparator + ": " + p.Id)}
            };
        }

    }
}