using SlowPerfLib;

namespace SlowPerfWpfApp
{
    public class ArticleSale
    {
        public string Description { get; set; }
        [TableColumn("Artikel")]
        public string ArticleName { get; set; }
        [TableColumn("Nr")]
        public int ArticleNr { get; set; }
        [TableColumn("Größe")]
        public ArticleSize ArticleSize { get; set; }
        [TableColumn("Jahr")]
        public int SaleYear { get; set; }
        [TableColumn("Jan.")]
        public double SaleM01 { get; set; }
        [TableColumn("Feb.")]
        public double SaleM02 { get; set; }
        [TableColumn("Mär.")]
        public double SaleM03 { get; set; }
        [TableColumn("Apr.")]
        public double SaleM04 { get; set; }
        [TableColumn("Mai")]
        public double SaleM05 { get; set; }
        [TableColumn("Jun.")]
        public double SaleM06 { get; set; }
        [TableColumn("Jul.")]
        public double SaleM07 { get; set; }
        [TableColumn("Aug.")]
        public double SaleM08 { get; set; }
        [TableColumn("Sep.")]
        public double SaleM09 { get; set; }
        [TableColumn("Okt.")]
        public double SaleM10 { get; set; }
        [TableColumn("Nov.")]
        public double SaleM11 { get; set; }
        [TableColumn("Dez.")]
        public double SaleM12 { get; set; }
        [TableColumn("Quartal 1")]
        public double SaleQ1 { get; set; }
        [TableColumn("Quartal 2")]
        public double SaleQ2 { get; set; }
        [TableColumn("Quartal 3")]
        public double SaleQ3 { get; set; }
        [TableColumn("Quartal 4")]
        public double SaleQ4 { get; set; }
        [TableColumn("Gesamt")]
        public double SaleTotal { get; set; }
    }
}