using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using MongoDB.Driver;
using SmartKioskBot.Controllers;
using SmartKioskBot.Helpers;
using SmartKioskBot.Models;
using SmartKioskBot.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static SmartKioskBot.Models.Context;

namespace SmartKioskBot.Dialogs
{
    [Serializable]
    public class FilterDialog : LuisDialog<object>
    {

        public static IMessageActivity CleanAllFilters(IDialogContext context, User user)
        {
            ContextController.CleanFilters(user);
            var reply = context.MakeMessage();
            reply.Text = "";
            return reply;
        }

        public static IMessageActivity CleanFilter(IDialogContext _context, User user, Context context, IList<EntityRecommendation> entities)
        {
            var filtername = "";
            
            foreach(EntityRecommendation e in entities)
            {
                if(e.Type == "filter")
                {
                    filtername = e.Entity;
                    break;
                }
            }

            ContextController.RemFilter(user, filtername);

            var reply = _context.MakeMessage();
            reply.Text = "Os filtros que estão a ser aplicados à busca são: \n\n";

            foreach (Filter f in context.Filters)
            {
                reply.Text = f.FilterName + "\n\n";
            }
            
            return reply;
        }

        public static IMessageActivity Filter(IDialogContext _context, User user, Context context, IList<EntityRecommendation> entities)
        {
            var filters = new List<FilterDefinition<Product>>();

            /*var reply = _context.MakeMessage();

            foreach(EntityRecommendation e in entities)
            {
                reply.Text += e.Type + "\n\n" +
                    e.Entity + "\n\n" +
                    "\n\n";
            }*/

            //Filters from context
            foreach (Filter f in context.Filters)
            {
                filters.Add(GetFilter(f.FilterName, f.Operator, f.Value));
            }

            //Filters from search
            foreach(Filter f in GetEntitiesFilter(entities.ToList()))
            {
                var filtername = f.FilterName;
                var op = f.Operator;
                var v = f.Value;
                filters.Add(GetFilter(f.FilterName,f.Operator,f.Value));
                ContextController.AddFilter(user,f.FilterName,f.Operator,f.Value);
            }

            List<Product> products = GetProductsForUser(filters);

            return ShowProducts(products, _context);
           //return reply;
        }

        private static List<Product> GetProductsForUser(List<FilterDefinition<Product>> filters)
        {
             var total_filter = Builders<Product>.Filter.Empty;

             //combine all filters
             foreach (FilterDefinition<Product> f in filters)
             {
                 total_filter = total_filter & f;
             }

            return ProductController.getProductsFilter(total_filter);
        }

        private static IMessageActivity ShowProducts(List<Product> products, IDialogContext context)
        {
            var reply = context.MakeMessage();

            if (products.Count == 0)
            {
                reply.AttachmentLayout = AttachmentLayoutTypes.List;
                reply.Text = BotDefaultAnswers.getFilterFail();
            }
            else
            {
                reply.Text = BotDefaultAnswers.getFilterSuccess();
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                List<Attachment> cards = new List<Attachment>();

                foreach (Product p in products)
                {
                    cards.Add(ProductCard.GetProductCard(p,ProductCard.CardType.SEARCH).ToAttachment());
                }

                reply.Attachments = cards;
            }
            return reply;
        }

        private static List<Filter> GetEntitiesFilter(List<EntityRecommendation> entities)
        {
            var filters = new List<Filter>();

            bool price = false;
            bool brand = false;
            bool cpu = false;
            bool storage_type = false;
            bool graphics = false;
            bool type = false;

            for (int i = 0; i < entities.Count; i++)
            {

                if (entities[i].Type == "filter")
                {
                    switch (entities[i].Entity.ToLower())
                    {
                        //number values
                        case "preço":
                        case "ram":
                        case "armazenamento":
                        case "autonomia":
                        case "ecrã":
                            if (!(entities[i].Entity == "preço" && !price)) {
                                var f = new Filter();
                                f.FilterName = entities[i].Entity.ToString();
                                f.Operator = "=";
                                //Tries to find the value and the position
                                for (int j = i + 1; j < entities.Count; j++)
                                {
                                    if (entities[j].Type == "position::min")
                                        f.Operator = ">";
                                    else if (entities[j].Type == "position::max")
                                        f.Operator = "<";
                                    else if (entities[j].Type == "buildin.number")
                                    {
                                        f.Value = entities[j].Entity;
                                        filters.Add(f);
                                        break;
                                    }
                                }
                            }
                            break;
                        case "marca":
                            if (!brand)
                            {
                                var f = new Filter();
                                f.FilterName = "marca";
                                f.Operator = "=";
                                for (int j = i+1; j < entities.Count; j++)
                                {
                                    if (entities[j].Type == "brands")
                                    {
                                        f.Value = entities[j].Entity;
                                        filters.Add(f);
                                        brand = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        case "cpu":
                            if (!cpu)
                            {
                                var f = new Filter();
                                f.Operator = "=";
                                f.FilterName = "familia_cpu";
                                for (int j = i + 1; j < entities.Count; j++)
                                {
                                    if (entities[j].Type == "CPU")
                                    {
                                        f.Value = entities[j].Entity.ToString();
                                        filters.Add(f);
                                        cpu = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        case "tipo de armazenamento":
                            if (!storage_type) { 
                                var f = new Filter();
                                f.Operator = "=";
                                f.FilterName = "tipo_armazenamento";
                                for (int j = i + 1; j < entities.Count; j++)
                                {
                                    if (entities[j].Type == "memoryType")
                                    {
                                        f.Value = entities[j].Entity.ToString();
                                        filters.Add(f);
                                        storage_type = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        case "gráfica":
                            if (!graphics)
                            {
                                var f = new Filter();
                                f.Operator = "=";
                                f.FilterName = "placa_grafica";
                                for (int j = 0; j < entities.Count; j++)
                                {
                                    if (entities[j].Type == "GPU")
                                    {
                                        f.Value = entities[j].Entity.ToString();
                                        filters.Add(f);
                                        graphics = true;
                                        break;
                                    }
                                }
                            }
                            break;
                        case "tipo":
                            if (!type)
                            {
                                var f = new Filter();
                                f.Operator = "=";
                                f.FilterName = "tipo";
                                for (int j = 0; j < entities.Count; j++)
                                {
                                    if (entities[j].Type == "pcType")
                                    {
                                        f.Value = entities[j].Resolution.Values.ElementAt(0).ToString();
                                        filters.Add(f);
                                        type = true;
                                        break;
                                    }
                                }
                            }
                            break;
                    }

                }
                else if (entities[i].Type == "builtin.money" && !price)
                {
                    var f = new Filter();
                    f.FilterName = "preço";
                    for(var j = i-1; j< entities.Count; j++)
                    {
                        if (entities[j].Type == "position::min")
                            f.Operator = "<";
                        else if (entities[j].Type == "position::max")
                            f.Operator = ">";
                    }
                    f.Value = Regex.Match(entities[i].Entity, @"\d+").Value; 
                    filters.Add(f);
                    price = true;
                }
                else if (entities[i].Type == "brands" && !brand)
                {
                    var f = new Filter();
                    f.FilterName = "marca";
                    f.Operator = "=";
                    f.Value = entities[i].Entity;
                    filters.Add(f);
                    brand = true;
                }
                else if (entities[i].Type == "CPU" && !cpu)
                {
                    var f = new Filter();
                    f.FilterName = "familia_cpu";
                    f.Operator = "=";
                    f.Value = entities[i].Entity;
                    filters.Add(f);
                    cpu = true;
                }
                else if (entities[i].Type == "memoryType" && !storage_type)
                {
                    var f = new Filter();
                    f.FilterName = "tipo_armazenamento";
                    f.Operator = "=";
                    f.Value = entities[i].Entity;
                    filters.Add(f);
                    storage_type = true;
                }
                else if (entities[i].Type == "GPU" && !graphics)
                {
                    var f = new Filter();
                    f.FilterName = "placa_grafica";
                    f.Operator = "=";
                    f.Value = entities[i].Entity;
                    filters.Add(f);
                    graphics = true;
                }
                else if (entities[i].Type == "pcType" && !type)
                {
                    var f = new Filter();
                    f.FilterName = "tipo";
                    f.Operator = "=";
                    /*if (entities[i].Resolution.Count > 0)
                    {
                        var a = entities[i];
                        var b = a.Resolution;
                        var c = b.Values.ToList();
                        var d = c[0];
                        f.Value = d.ToString();
                    }
                    else
                        f.Value = entities[i].Entity;
                    filters.Add(f);*/
                    f.Value = entities[i].Entity;
                    graphics = true;
                }
            }

            return filters;
        }

        private static FilterDefinition<Product> GetFilter(string filter, string op,string value){
            switch (filter.ToLower())
            {
                case "preço":
                       if (op == "=")
                            return Builders<Product>.Filter.Eq(x => x.Price, Convert.ToDouble(value));
                        if (op == ">") 
                            return Builders<Product>.Filter.Gte(x => x.Price, Convert.ToDouble(value));
                        else if (op == "<")
                            return Builders<Product>.Filter.Lte(x => x.Price, Convert.ToDouble(value));
                        break;
                case "marca":
                    return Builders<Product>.Filter.Where(x => x.Brand.ToLower() == value.ToLower());
                case "familia_cpu":
                    return Builders<Product>.Filter.Where(x => x.CPUFamily.ToLower() == value.ToLower());
                case "ram":
                    if (op == "=")
                        return Builders<Product>.Filter.Eq(x => x.RAM, Convert.ToDouble(value));
                    else if (op == ">")
                        return Builders<Product>.Filter.Gte(x => x.RAM, Convert.ToDouble(value));
                    else if (op == "<")
                        return Builders<Product>.Filter.Lte(x => x.RAM, Convert.ToDouble(value));
                    break;
                case "tipo_armazenamento":
                    return Builders<Product>.Filter.Where(x => x.StorageType.ToLower() == value.ToLower());
                case "armazenamento":
                    if (op == "=")
                        return Builders<Product>.Filter.Eq(x => x.StorageAmount, Convert.ToDouble(value));
                    else if (op == ">")
                        return Builders<Product>.Filter.Gte(x => x.StorageAmount, Convert.ToDouble(value));
                    else if (op == "<")
                        return Builders<Product>.Filter.Lte(x => x.StorageAmount, Convert.ToDouble(value));
                    break;
                case "placa_grafica":
                    return Builders<Product>.Filter.Where(x => x.GraphicsCardType.ToLower() == value.ToLower());
                case "autonomia":
                    if (op == "=")
                        return Builders<Product>.Filter.Eq(x => x.Autonomy, Convert.ToDouble(value));
                    else if (op == ">")
                        return Builders<Product>.Filter.Gte(x => x.Autonomy, Convert.ToDouble(value));
                    else if (op == "<")
                        return Builders<Product>.Filter.Lte(x => x.Autonomy, Convert.ToDouble(value));
                    break;
                case "tamanho_ecra":
                    if (op == "=")
                        return Builders<Product>.Filter.Eq(x => x.ScreenDiagonal, Convert.ToDouble(value));
                    else if (op == ">")
                        return Builders<Product>.Filter.Gte(x => x.ScreenDiagonal, Convert.ToDouble(value));
                    else if (op == "<")
                        return Builders<Product>.Filter.Lte(x => x.ScreenDiagonal, Convert.ToDouble(value));
                    break;
                case "tipo":
                    return Builders<Product>.Filter.Where(x => x.Type.ToLower() == value.ToLower());
            }

            return null;
        }
    }
}
