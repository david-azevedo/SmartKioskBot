using Microsoft.Bot.Builder.Luis.Models;
using MongoDB.Driver;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using static SmartKioskBot.Models.Context;

namespace SmartKioskBot.Logic
{
    public class FilterLogic
    {
        public static Filter DEFAULT_RECOMMENDATION_FILTER = new Filter()
        {
            FilterName = "marca",
            Operator = "=",
            Value = "asus"
        };

        //luis => filter
        public static List<Filter> GetEntitiesFilter(LuisResult result)
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
                for (int j = 0; j < c.Children.Count(); j++)
                {
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
                        case "builtin.currency":
                        case "storage":
                            {
                                var v = ch.Value;

                                if (ch.Type == "storage" || ch.Type == "builtin.currency" || ch.Type == "builtin.number")
                                    v = Regex.Match(ch.Value, @"[\d]*([\,,\.][\d]*)?").Value.Replace(',', '.');    //extract number

                                if (v == "")
                                    break;

                                if ((ch.Type == "builtin.currency" || ch.Type == "builtin.currency") && f1.FilterName == "")
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
                                else if (Double.Parse(f1.Value) < Double.Parse(v))
                                    f2.Value = v;
                                else if (Double.Parse(f1.Value) > Double.Parse(v))
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

                if (f1.FilterName != "" && f1.Value != "")
                    filters.Add(f1);

                if (between && f2.Value != "")
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
                switch (entities[i].Type.ToLower())
                {
                    case "cpu":
                        f.FilterName = "familia_cpu";
                        break;
                    case "gpu":
                        f.FilterName = "placa_grafica";
                        break;
                    case "builtin.currency":
                        f.FilterName = "preço";
                        f.Value = Regex.Match(entities[i].Entity, @"[\d]*([\,,\.][\d]*)?").Value.Replace(',', '.');
                        break;
                    case "storage":
                        f.FilterName = "armazenamento";
                        f.Value = Regex.Match(entities[i].Entity, @"[\d]*([\,,\.][\d]*)?").Value.Replace(',', '.');
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
                if (f.FilterName != "" && f.Value != "")
                    filters.Add(f);
            }

            return filters;
        }

        private static void RemoveProcessedEntity(List<EntityRecommendation> entities, string type, string value)
        {
            int i = 0;

            for (i = 0; i < entities.Count(); i++)
            {
                var a = entities[i].Type;
                var b = type;
                var c = entities[i].Entity;
                var d = value;

                if (entities[i].Type == type && entities[i].Entity == value)
                    break;
            }
            if (i != entities.Count())
                entities.RemoveAt(i);
        }

        //filter => filter definition
        public static FilterDefinition<Product> GetFilter(Filter f)
        {
            switch (f.FilterName.ToLower())
            {
                case "preço":
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.Price, Convert.ToDouble(f.Value));
                    if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.Price, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.Price, Convert.ToDouble(f.Value));
                    break;
                case "marca":
                    return Builders<Product>.Filter.Where(x => x.Brand.ToLower() == f.Value.ToLower());
                case "familia_cpu":
                    return Builders<Product>.Filter.Where(x => x.CPUFamily.ToLower().Contains(f.Value.ToLower()));
                case "ram":
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.RAM, Convert.ToDouble(f.Value));
                    else if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.RAM, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.RAM, Convert.ToDouble(f.Value));
                    break;
                case "tipo_armazenamento":
                    return Builders<Product>.Filter.Where(x => x.StorageType.ToLower() == f.Value.ToLower());
                case "armazenamento":
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.StorageAmount, Convert.ToDouble(f.Value));
                    else if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.StorageAmount, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.StorageAmount, Convert.ToDouble(f.Value));
                    break;
                case "placa_grafica":
                    return Builders<Product>.Filter.Where(x => x.GraphicsCardType.ToLower().Contains(f.Value.ToLower()));
                case "autonomia":
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.Autonomy, Convert.ToDouble(f.Value));
                    else if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.Autonomy, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.Autonomy, Convert.ToDouble(f.Value));
                    break;
                case "tamanho_ecra":
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.ScreenDiagonal, Convert.ToDouble(f.Value));
                    else if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.ScreenDiagonal, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.ScreenDiagonal, Convert.ToDouble(f.Value));
                    break;
                case "tipo":
                    return Builders<Product>.Filter.Where(x => x.Type.ToLower() == f.Value.ToLower());
            }

            return null;
        }

        public static FilterDefinition<Product> GetJoinedFilter(List<Filter> filters)
        {
            var total_filter = Builders<Product>.Filter.Empty;
            List<int> treatedIdx = new List<int>();

            //combine all filters
            for (var i = 0; i < filters.Count(); i++)
            {
                if ((filters[i].FilterName == "tipo_armazenamento" ||
                    filters[i].FilterName == "familia_cpu" ||
                    filters[i].FilterName == "placa_grafica" ||
                    filters[i].FilterName == "marca" ||
                    filters[i].FilterName == "tipo")
                    && !treatedIdx.Contains(i))
                {
                    var filters_tmp = new List<FilterDefinition<Product>>();

                    //check if there are equal filters
                    for (var j = i; j < filters.Count(); j++)
                    {
                        if (filters[j].FilterName == filters[i].FilterName)
                        {
                            filters_tmp.Add(FilterLogic.GetFilter(filters[j]));
                            treatedIdx.Add(j);
                        }
                    }

                    //filters with the same filtername are joined with OR
                    total_filter &= Builders<Product>.Filter.Or(filters_tmp);
                }
                else if (!treatedIdx.Contains(i))
                    total_filter &= FilterLogic.GetFilter(filters[i]);
            }

            return total_filter;
        }
    }
}