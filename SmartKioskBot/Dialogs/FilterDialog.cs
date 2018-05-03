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
                if(e.Type.Contains("filter-type")) //--> alterar
                {
                    filtername = e.Type.Remove(0, e.Type.LastIndexOf(":") + 1);
                }
                else if(e.Type == "memory-type")
                {
                    if (e.Entity == "ram") filtername = "ram";
                    else filtername = "tipo_armazenamento";
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

        public static IMessageActivity Filter(IDialogContext _context, User user, Context context, LuisResult result)
        {
            var filters = new List<FilterDefinition<Product>>();

           /* var reply = _context.MakeMessage();

            foreach (CompositeEntity c in result.CompositeEntities)
            {
                reply.Text += c.ParentType + "\n\n";
                foreach(CompositeChild ch in c.Children)
                {
                    reply.Text += ch.Type + "\n\n";
                    reply.Text += ch.Value + "\n\n";
                }
            }
            reply.Text += "====================";

            foreach (EntityRecommendation e in result.Entities)
            {
                reply.Text += e.Type + "\n\n" +
                    e.Entity + "\n\n";
                reply.Text += "\n\n\n\n";
            }*/

            //Filters from context
            foreach (Filter f in context.Filters)
            {
                filters.Add(GetFilter(f.FilterName, f.Operator, f.Value));
            }

            //Filters from search
            foreach(Filter f in GetEntitiesFilter(result))
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

        private static List<Filter> GetEntitiesFilter(LuisResult result)
        {
            var composite = result.CompositeEntities.ToList();
            var entities = result.Entities.ToList();

            var filters = new List<Filter>();

            //Handle composite entities
            for (int i = 0; i < composite.Count; i++)
            {
                var c = composite[i];

                Filter f1 = new Filter();
                Filter f2 = new Filter();
                bool between = false;
                f1.FilterName = "";
                f1.Operator = "=";
                f1.Value = "";

                f2.FilterName = "";
                f2.Operator = "";
                f2.Value = "";

                //check entities
                for (int j = 0; j < c.Children.Count(); j++){
                    var ch = c.Children[j];

                    switch (ch.Type)
                    {
                        case "memory-type":
                            {
                                if (ch.Value == "ram")
                                    f1.FilterName = "ram";
                                else
                                {
                                    f1.FilterName = "tipo_armazenamento";
                                    f1.Value = ch.Value;
                                }
                                break;
                            }
                        case "filter-type":
                            {
                                f1.FilterName = ch.Value.ToString();
                                break;
                            }
                        case "position::between":
                            {
                                between = true;
                                f1.Operator = ">";
                                f2.Operator = "<";
                                break;
                            }
                        case "position::lower":
                            {
                                f1.Operator = "<";
                                break;
                            }
                        case "position::higher":
                            {
                                f1.Operator = ">";
                                break;
                            }
                        case "builtin.number":
                        case "builtin.money":
                        case "storage":
                            {
                                var v = ch.Value;

                                if(ch.Type == "storage" || ch.Type == "builtin.money" || ch.Type == "builtin.number")
                                    v = Regex.Match(ch.Value, @"[\d]*([\,,\.][\d]*)?").Value;    //extract number

                                if (v == "")
                                    break;

                                if (ch.Type == "builtin.money" && f1.FilterName == "")
                                    f1.FilterName = "preço";

                                if (ch.Type == "storage" && f1.FilterName == "")
                                    f1.FilterName = "armazenamento";
                                
                                if (f1.FilterName == "tipo_armazenamento")
                                {
                                    filters.Add(f1);
                                    f1 = new Filter();
                                    f1.FilterName = "armazenamento";
                                    f1.Operator = "=";
                                }

                                if (f1.Value == "")
                                    f1.Value = v;
                                else if(Double.Parse(f1.Value) <= Double.Parse(v))
                                    f2.Value = v;
                                else if (Double.Parse(f1.Value) >= Double.Parse(v))
                                {
                                    f2.Value = f1.Value;
                                    f1.Value = v;
                                }
                                break;
                            }
                    }
                    //remove from the list of the single entities
                    RemoveProcessedEntity(entities, ch.Type, ch.Value);
                }
                filters.Add(f1);
                if (between)
                {
                    f2.FilterName = f1.FilterName;
                    filters.Add(f2);
                }

                //remove from the list of single entities
                RemoveProcessedEntity(entities, c.ParentType, c.Value);
            }

            //Handle single entities
            for (int i = 0; i < entities.Count; i++)
            {
                var f = new Filter();
                f.FilterName = "";
                f.Operator = "=";
                f.Value = entities[i].Entity;
                switch(entities[i].Type.ToLower()){
                    case "cpu":
                        f.FilterName = "cpu";
                        break;
                    case "gpu":
                        f.FilterName = "gpu";
                        break;
                    case "builtin::money":
                        f.FilterName = "preço";
                        f.Value = Regex.Match(entities[i].Entity, @"[\d]*([\,,\.][\d]*)?").Value;
                        break;
                    case "storage":
                        f.FilterName = "armazenamento";
                        f.Value = Regex.Match(entities[i].Entity, @"[\d]*([\,,\.][\d]*)?").Value;
                        break;
                    case "brand":
                        f.FilterName = "marca";
                        break;
                    case "pc-type::advanced":
                        f.FilterName = "tipo";
                        f.Value = "avançado";
                        break;
                    case "pc-type::convertible":
                        f.FilterName = "tipo";
                        f.Value = "convertível 2 em 1";
                        break;
                    case "pc-type::essencial":
                        f.FilterName = "tipo";
                        f.Value = "essencial";
                        break;
                    case "pc-type::gaming":
                        f.FilterName = "tipo";
                        f.Value = "gaming";
                        break;
                    case "pc-type::mobility":
                        f.FilterName = "tipo";
                        f.Value = "mobilidade";
                        break;
                    case "pc-type::performance":
                        f.FilterName = "tipo";
                        f.Value = "performance";
                        break;
                    case "pc-type::slim":
                        f.FilterName = "tipo";
                        f.Value = "ultra fino";
                        break;
                }
                if (f.FilterName != "" && f.Operator != "" && f.Value != "")
                    filters.Add(f);
            }

            return filters;
        }

        public static void RemoveProcessedEntity(List<EntityRecommendation> entities, string type,string value)
        {
            int i = 0;

            for(i = 0; i < entities.Count(); i++)
            {
                var a = entities[i].Type;
                var b = type;
                var c = entities[i].Entity;
                var d = value;

                if (entities[i].Type == type && entities[i].Entity == value)
                    break;                    
            }
            if(i != entities.Count())
                entities.RemoveAt(i);
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
