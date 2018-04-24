using MongoDB.Bson.Serialization.Attributes;

namespace SmartKioskBot.Models
{
    public class Product
    {
        [BsonId]
        public MongoDB.Bson.ObjectId Id { get; set; }

        [BsonElement("nome")]
        public string Name { get; set; }

        [BsonElement("preço")]
        public string Price { get; set; }

        [BsonElement("foto")]
        public string Photo { get; set; }

        [BsonElement("Processador")]
        public string CPU { get; set; }

        [BsonElement("Familia de Processador")]
        public string CPUFamily { get; set; }

        [BsonElement("Velocidade Processador")]
        public string CPUSpeed { get; set; }

        [BsonElement("Quantidade de Núcleos Core")]
        public string CoreNr { get; set; }

        [BsonElement("RAM")]
        public string RAM { get; set; }

        [BsonElement("Tipo de Armazenamento")]
        public string StorageType { get; set; }

        [BsonElement("Armazenamento")]
        public string StorageAmount { get; set; }

        [BsonElement("Tipo de Placa Gráfica")]
        public string GraphicsCardType { get; set; }

        [BsonElement("Gráfica")]
        public string GraphicsCard { get; set; }

        [BsonElement("Memória Gráfica (Máx)")]
        public string MaxVideoMem { get; set; }

        [BsonElement("Autonomia (Estimada)")]
        public string Autonomy { get; set; }

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
        public string ScreenDiagonal { get; set; }

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
        public string Warranty { get; set; }

        [BsonElement("Peso")]
        public string Weight { get; set; }

        [BsonElement("Cor")]
        public string Colour { get; set; }

        [BsonElement("Altura")]
        public string Height { get; set; }

        [BsonElement("Largura")]
        public string Width { get; set; }
        
        [BsonElement("Profundidade")]
        public string Depth { get; set; }

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