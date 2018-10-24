using Microsoft.Bot.Builder.Luis.Models;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using SmartKioskBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using static SmartKioskBot.Helpers.AdaptiveCardHelper;
using static SmartKioskBot.Models.Context;
using static SmartKioskBot.Helpers.Constants;
using SmartKioskBot.Helpers;

namespace SmartKioskBot.Logic
{
    public class FilterLogic
    {
        /*
         * UI
         */

        public const string brand_filter = "marca";
        public const string ram_filter = "ram";
        public const string storage_type_filter = "tipo_armazenamento";
        public const string cpu_family_filter = "familia_cpu";
        public const string gpu_filter = "placa_grafica";
        public const string type_filter = "tipo";
        public const string price_filter = "preço";
        public const string storage_filter = "armazenamento";
        public const string screen_size_filter = "tamanho_ecra";
        public const string autonomy_filter = "autonomia";

        public static void SetFilterCardValue(JToken card, List<Filter> applied_filters)
        {
            List<JToken> card_fields = new List<JToken>();
            string last_retrieved = "";

            for (int i = 0; i < applied_filters.Count(); i++)
            {
                var f = applied_filters[i];

                if (last_retrieved != f.FilterName)
                    card_fields = GetFilterCardSection(card, f.FilterName);

                string lookup = f.FilterName + ":";
                bool checkbox = false;

                if (f.Operator.Equals("<"))
                    lookup += "max";
                else if (f.Operator.Equals(">"))
                    lookup += "min";
                else
                {
                    lookup += f.Value;
                    checkbox = true;
                }

                for (int j = 0; j < card_fields.Count; j++)
                {
                    if (card_fields[j]["id"].ToString().Equals(lookup))
                    {
                        if (!checkbox)
                            card_fields[j]["value"] = f.Value;
                        else
                            card_fields[j]["value"] = "true";
                        break;
                    }
                }
            }
        }

        private static List<JToken> GetFilterCardSection(JToken card, string section)
        {
            List<JToken> fields = new List<JToken>();

            switch (section)
            {
                case cpu_family_filter:
                    fields = card.SelectTokens("body[1].columns[0].items").Children().ToList();
                    fields.RemoveAt(0);
                    break;
                case gpu_filter:
                    fields = card.SelectTokens("body[1].columns[1].items").Children().ToList();
                    fields.RemoveAt(0);
                    break;
                case price_filter:
                    fields.Add(card.SelectToken("body[2].columns[0].items[1].columns[0].items[0]"));
                    fields.Add(card.SelectToken("body[2].columns[0].items[1].columns[1].items[0]"));
                    break;
                case storage_type_filter:
                    fields.Add(card.SelectToken("body[3].items[1]"));
                    fields.Add(card.SelectToken("body[3].items[2]"));
                    break;
                case storage_filter:
                    fields.Add(card.SelectToken("body[3].items[3].columns[0].items[0]"));
                    fields.Add(card.SelectToken("body[3].items[3].columns[1].items[0]"));
                    break;
                case ram_filter:
                    fields.Add(card.SelectToken("body[3].items[5].columns[0].items[0]"));
                    fields.Add(card.SelectToken("body[3].items[5].columns[1].items[0]"));
                    break;
                case brand_filter:
                    fields = card.SelectTokens("body[4].items[1].columns[0].items").Children().ToList();
                    fields = fields.Concat(card.SelectTokens("body[4].items[1].columns[1].items").Children().ToList()).ToList();
                    fields = fields.Concat(card.SelectTokens("body[4].items[1].columns[2].items").Children().ToList()).ToList();
                    break;
                case type_filter:
                    fields = card.SelectTokens("body[5].items[1].columns[0].items").Children().ToList();
                    fields = fields.Concat(card.SelectTokens("body[5].items[1].columns[1].items").Children().ToList()).ToList();
                    break;
            }

            return fields;
        }

        public static List<Filter> GetFilterFromForm(List<InputData> data)
        {
            List<Filter> filters = new List<Filter>();

            for (int i = 0; i < data.Count(); i++)
            {
                Filter f = new Filter();
                bool add_filter = true;

                f.Operator = "=";                   //default
                f.FilterName = data[i].attribute;

                if (data[i].value == "min")
                {
                    f.Operator = ">";
                    f.Value = data[i].input;
                }
                else if (data[i].value == "max")
                {
                    f.Operator = "<";
                    f.Value = data[i].input;
                }
                else
                {
                    f.Value = data[i].value;
                    if (data[i].input.Equals("false"))
                        add_filter = false;
                }

                if (f.Value == "")
                    add_filter = false;

                if(add_filter)
                    filters.Add(f);
            }

            return filters;
        }

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
                                    f1.FilterName = ram_filter;
                                else
                                {
                                    f1.FilterName = storage_type_filter;
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
                                    f1.FilterName = price_filter;

                                if (ch.Type == "storage" && f1.FilterName == "")
                                    f1.FilterName = storage_filter;

                                if (f1.FilterName == storage_type_filter)
                                {
                                    filters.Add(f1);
                                    f1 = new Filter();
                                    f1.FilterName = storage_filter;
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
                        f.FilterName = cpu_family_filter;
                        break;
                    case "gpu":
                        f.FilterName = gpu_filter;
                        break;
                    case "builtin.currency":
                        f.FilterName = price_filter;
                        f.Value = Regex.Match(entities[i].Entity, @"[\d]*([\,,\.][\d]*)?").Value.Replace(',', '.');
                        break;
                    case "storage":
                        f.FilterName = storage_filter;
                        f.Value = Regex.Match(entities[i].Entity, @"[\d]*([\,,\.][\d]*)?").Value.Replace(',', '.');
                        break;
                    case "brand":
                        f.FilterName = brand_filter;
                        break;
                    case "pc-type::advanced":
                        f.FilterName = type_filter;
                        f.Value = "avançado";
                        break;
                    case "pc-type::convertible":
                        f.FilterName = type_filter;
                        f.Value = "convertível 2 em 1";
                        break;
                    case "pc-type::essencial":
                        f.FilterName = type_filter;
                        f.Value = "essencial";
                        break;
                    case "pc-type::gaming":
                        f.FilterName = type_filter;
                        f.Value = "gaming";
                        break;
                    case "pc-type::mobility":
                        f.FilterName = type_filter;
                        f.Value = "mobilidade";
                        break;
                    case "pc-type::performance":
                        f.FilterName = type_filter;
                        f.Value = "performance";
                        break;
                    case "pc-type::slim":
                        f.FilterName = type_filter;
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
                case price_filter:
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.Price, Convert.ToDouble(f.Value));
                    if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.Price, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.Price, Convert.ToDouble(f.Value));
                    break;
                case brand_filter:
                    return Builders<Product>.Filter.Where(x => x.Brand.ToLower() == f.Value.ToLower());
                case cpu_family_filter:
                    return Builders<Product>.Filter.Where(x => x.CPUFamily.ToLower().Contains(f.Value.ToLower()));
                case ram_filter:
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.RAM, Convert.ToDouble(f.Value));
                    else if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.RAM, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.RAM, Convert.ToDouble(f.Value));
                    break;
                case storage_type_filter:
                    return Builders<Product>.Filter.Where(x => x.StorageType.ToLower() == f.Value.ToLower());
                case storage_filter:
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.StorageAmount, Convert.ToDouble(f.Value));
                    else if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.StorageAmount, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.StorageAmount, Convert.ToDouble(f.Value));
                    break;
                case gpu_filter:
                    return Builders<Product>.Filter.Where(x => x.GraphicsCardType.ToLower().Contains(f.Value.ToLower()));
                case autonomy_filter:
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.Autonomy, Convert.ToDouble(f.Value));
                    else if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.Autonomy, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.Autonomy, Convert.ToDouble(f.Value));
                    break;
                case screen_size_filter:
                    if (f.Operator == "=")
                        return Builders<Product>.Filter.Eq(x => x.ScreenDiagonal, Convert.ToDouble(f.Value));
                    else if (f.Operator == ">")
                        return Builders<Product>.Filter.Gte(x => x.ScreenDiagonal, Convert.ToDouble(f.Value));
                    else if (f.Operator == "<")
                        return Builders<Product>.Filter.Lte(x => x.ScreenDiagonal, Convert.ToDouble(f.Value));
                    break;
                case type_filter:
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
                if ((filters[i].FilterName == storage_type_filter ||
                    filters[i].FilterName == cpu_family_filter ||
                    filters[i].FilterName == gpu_filter ||
                    filters[i].FilterName == brand_filter ||
                    filters[i].FilterName == type_filter)
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