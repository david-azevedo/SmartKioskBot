using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace SmartKioskBot.Models
{
    [Serializable, JsonObject]
    [BsonIgnoreExtraElements]
    public class Product
    {
        [BsonId]
        public MongoDB.Bson.ObjectId Id { get; set; }

        [BsonElement("nome")]
        public string Name { get; set; }

        [BsonElement("preço")]
        public double Price { get; set; }

        [BsonElement("foto")]
        public string Photo { get; set; }

        [BsonElement("Processador")]
        public string CPU { get; set; }

        [BsonElement("Familia de Processador")]
        public string CPUFamily { get; set; }

        [BsonElement("Velocidade Processador")]
        public double CPUSpeed { get; set; }        //ghz

        [BsonElement("Quantidade de Núcleos Core")]
        public string CoreNr { get; set; }

        [BsonElement("RAM")]
        public double RAM { get; set; }             //gb

        [BsonElement("Tipo de Armazenamento")]
        public string StorageType { get; set; }

        [BsonElement("Armazenamento")]
        public double StorageAmount { get; set; }   //gb

        [BsonElement("Tipo de Placa Gráfica")]
        public string GraphicsCardType { get; set; }

        [BsonElement("Gráfica")]
        public string GraphicsCard { get; set; }

        [BsonElement("Memória Gráfica (Máx)")]
        public string MaxVideoMem { get; set; }

        [BsonElement("Autonomia (Estimada)")]
        public double Autonomy { get; set; }            //hours

        [BsonElement("Placa de Som")]
        public string SoundCard { get; set; }

        [BsonElement("Câmara Incorporada")]
        public string HasCamera { get; set; }

        [BsonElement("Teclado numérico")]
        public string NumPad { get; set; }

        [BsonElement("Touch Bar")]
        public string TouchBar { get; set; }

        [BsonElement("Teclado Retroiluminado")]
        public string BacklitKeybr { get; set; }

        [BsonElement("Teclado Mecânico")]
        public string MechKeybr { get; set; }

        [BsonElement("Software")]
        public string Software { get; set; }

        [BsonElement("Sistema Operativo")]
        public string OS { get; set; }

        [BsonElement("Ecrã")]
        public string Screen { get; set; }

        [BsonElement("Diagonal do Ecrã ('')")]
        public double ScreenDiagonal { get; set; }      //inches

        [BsonElement("Resolução do Ecrã")]
        public string ScreenResolution { get; set; }

        [BsonElement("Ecrã tatil")]
        public string TouchScreen { get; set; }

        [BsonElement("Referência Worten")]
        public string WortenRef { get; set; }

        [BsonElement("EAN")]
        public string EAN { get; set; }

        [BsonElement("Marca")]
        public string Brand { get; set; }

        [BsonElement("Modelo")]
        public string Model { get; set; }

        [BsonElement("Garantia")]
        public double Warranty { get; set; }        //years

        [BsonElement("Peso")]
        public double Weight { get; set; }          //kg

        [BsonElement("Cor")]
        public string Colour { get; set; }

        [BsonElement("Altura")]
        public double Height { get; set; }          //cm

        [BsonElement("Largura")]
        public double Width { get; set; }           //cm
        
        [BsonElement("Profundidade")]
        public double Depth { get; set; }           //cm

        [BsonElement("Garantia Bateria")]
        public string BatteryWarranty { get; set; }

        [BsonElement("Conteúdo Extra Incluído na Caixa")]
        public string ExtraContent { get; set; }

        [BsonElement("Tipo")]
        public string Type { get; set; }

        [BsonElement("Drive")]
        public string Drive { get; set; }

        [BsonElement("Conetividade")]
        public string Connectivity { get; set; }

        [BsonElement("Ligações")]
        public string Connections { get; set; }

        [BsonElement("Mais Informações")]
        public string MoreInfo { get; set; }

        [BsonElement("Part Number")]
        public string PartNr { get; set; }

    }
}