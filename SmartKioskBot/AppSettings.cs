namespace SmartKioskBot
{
    using System;
    using System.Configuration;

    public static class AppSettings
    {
        
        public static string DbName => "db";
        public static string CollectionName => "Products";
        public static string MongoDBConnectionString => "mongodb://skb-cosmos-mongo-v1:50CViIEwqQ2lJJXKc4Rr2FjwlKHo8xstnEl8tCEEsoqSMYBqavY2FGmdY7cGtP3T6MnAli6uLKqEcUrNLpR4Jg==@skb-cosmos-mongo-v1.documents.azure.com:10255/?ssl=true&replicaSet=globaldb";
    }
}