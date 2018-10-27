using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using MongoDB.Bson;
using MongoDB.Driver;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static SmartKioskBot.UI.ProductCard;

namespace SmartKioskBot.Logic
{
    public class ProductLogic
    {
        public async static Task ShowProductMessage(IDialogContext context, string id)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Product>(AppSettings.ProductsCollection);

            //get product
            var query_id = Builders<Product>.Filter.Eq("_id", ObjectId.Parse(id));
            var entity = collection.Find(query_id).ToList();

            var product = entity[0];

            var image = context.MakeMessage();
            image.Attachments.Add(new HeroCard
            {
                Images = new List<CardImage> { new CardImage(product.Photo) }
            }.ToAttachment());

            await context.PostAsync(image);

            var reply = context.MakeMessage();
            reply.Text = "## " + product.Name + "  \n" +
                        "### Preço: " + product.Price + " euros \n  \n" +

                        "#### *Referências*  \n" +
                        "EAN: " + product.EAN + "  \n" +
                        "Marca: " + product.Brand + "  \n" +
                        "Modelo: " + product.Model + "  \n" +
                        "Garantia: " + product.Warranty + " anos  \n  \n" +

                        "#### *Processador*  \n" +
                        "Processador: " + product.CPU + "  \n" +
                        "Família de Processador: " + product.CPUFamily + "  \n" +
                        "Velocidade do Processador: " + product.CPUSpeed + " GHz  \n  \n" +

                        "#### *Memória e Armazenamento*  \n" +
                        "Número de núcleos: " + product.CoreNr + "  \n" +
                        "RAM: " + product.RAM + " GB  \n" +
                        "Tipo de Armazenamento: " + product.StorageType + "  \n" +
                        "Armazenamento: " + product.StorageAmount + " GB  \n  \n" +

                        "#### *Performance*  \n" +
                        "Tipo de Placa Gráfica: " + product.GraphicsCardType + "  \n" +
                        "Placa Gráfica: " + product.GraphicsCard + "  \n" +
                        "Memória Gráfica (Máximo): " + product.MaxVideoMem + "  \n" +
                        "Autonomia: " + product.Autonomy + " horas  \n" +
                        "Placa de Som: " + product.SoundCard + "  \n  \n" +

                        "#### *Sistema Operativo*  \n" +
                        "Software: " + product.Software + "  \n" +
                        "Sistema Operativo: " + product.OS + "  \n  \n" +

                        "#### *Ecrã*  \n" +
                        "Ecrã: " + product.Screen + "  \n" +
                        "Diagonal do Ecrã: " + product.ScreenDiagonal + "  \n" +
                        "Resolução do Ecrã: " + product.ScreenResolution + "  \n" +
                        "Ecrã Tatil: " + product.TouchScreen + "  \n  \n" +

                        "#### *Características Físicas*  \n" +
                        "Peso: " + product.Weight + " kg  \n" +
                        "Cor: " + product.Colour + "  \n" +
                        "Altura: " + product.Height + " cm  \n" +
                        "Largura: " + product.Width + " cm  \n" +
                        "Profundidade: " + product.Depth + " cm  \n" +
                        "Tem Câmara: " + product.HasCamera + "  \n" +
                        "Teclado Númerico: " + product.NumPad + "  \n" +
                        "Touch Bar: " + product.TouchBar + "  \n" +
                        "Teclado Retroiluminado: " + product.BacklitKeybr + "  \n" +
                        "Teclado Mecânico: " + product.MechKeybr + "  \n  \n" +

                        "#### *Outras Informações:*  \n" +
                        "Garantia da Bateria: " + product.BatteryWarranty + "  \n" +
                        "Conteúdo extra: " + product.ExtraContent + "  \n" +
                        "Tipo: " + product.Type + "  \n" +
                        "Drive: " + product.Drive + "  \n" +
                        "Conetividade: " + product.Connectivity + "  \n" +
                        "Ligações: " + product.Connections + "  \n" +
                        "Mais Informações: " + product.MoreInfo + "  \n" +
                        "Part Number: " + product.PartNr + "  \n  \n";

            await context.PostAsync(reply);

            //butons
            var buttons = context.MakeMessage();
            buttons.Attachments.Add(new HeroCard
            {
                Buttons = ProductCard.getButtonsCardType(CardType.PRODUCT_DETAILS, product.Id.ToString())
            }.ToAttachment());

            await context.PostAsync(buttons);
        }

        public async static Task ShowInStoreLocation(IDialogContext context, string productId, string storeId)
        {
            var collection = DbSingleton.GetDatabase().GetCollection<Store>(AppSettings.StoreCollection);

            //get store
            var query_id = Builders<Store>.Filter.Eq("_id", ObjectId.Parse(storeId));
            var entity = collection.Find(query_id).ToList();

            var store = entity[0];
            var message = "";

            for (int i = 0; i < store.ProductsInStock.Count(); i++)
            {
                if (store.ProductsInStock[i].ProductId.ToString().Equals(productId))
                {
                    message += "Corredor(" + store.ProductsInStock[i].InStoreLocation.Corridor + "), ";
                    message += "Secção(" + store.ProductsInStock[i].InStoreLocation.Section + "), ";
                    message += "Prateleira(" + store.ProductsInStock[i].InStoreLocation.Shelf + "). ";
                }
            }

            await context.PostAsync(message);
        }

    }
}