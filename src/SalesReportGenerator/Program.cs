using System;
using System.IO;
using System.Text;

namespace SalesReportGenerator
{
    class Program
    {
        private static Random _rndGenerator;

        static void Main(string[] args)
        {
            string path = args.Length > 0 ? args[0] : "SalesReport.csv";
            _rndGenerator = new Random(path.GetHashCode());
            const int defaultRowCount = 10;
            int rowCount = defaultRowCount;
            if (args.Length > 1)
                if (!int.TryParse(args[1], out rowCount))
                    rowCount = defaultRowCount;
            Console.WriteLine($"Generating {rowCount} rows ...");
            GenerateFile(rowCount, path);
            Console.WriteLine($"Generated: {new FileInfo(path).FullName}");
            Console.WriteLine();
        }

        private static void GenerateFile(int rowCount, string path)
        {
            using (StreamWriter writer = File.CreateText(path))
            {
                WriteHeader(writer);
                WriteDataRows(writer, rowCount);
            }
        }

        private static void WriteDataRows(StreamWriter writer, int rowCount)
        {
            var sb = new StringBuilder();
            var valueBuffer = new double[17];
            for (int i = 0; i < rowCount; i++)
            {
                string line = GenerateDataRow(sb, i, valueBuffer);
                writer.WriteLine(line);
            }
        }

        private static void WriteHeader(StreamWriter writer)
        {
            writer.WriteLine("ArticleName;ArticleNr;ArticleSize;SaleYear;SaleM01;SaleM02;SaleM03;SaleM04;SaleM05;SaleM06;SaleM07;SaleM08;SaleM09;SaleM10;SaleM11;SaleM12;SaleQ1;SaleQ2;SaleQ3;SaleQ4;SaleTotal;");
        }

        private static string GenerateDataRow(StringBuilder sb, int rowIndex, double[] valueBuffer)
        {
            sb.Clear();
            sb.Append(GenerateArticleName(rowIndex))
                .Append(";")
                .Append(GenerateArticleNr(rowIndex))
                .Append(";")
                .Append(GenerateArticleSize(rowIndex))
                .Append(";")
                .Append(GenerateYear(rowIndex))
                .Append(";");
            GenerateValues(valueBuffer);
            for (int i = 0; i < valueBuffer.Length; i++)
                sb.Append(valueBuffer[i]).Append(";");
            
            return sb.ToString();
        }

        private static string GenerateArticleName(int rowIndex)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int index1 = _rndGenerator.Next(letters.Length);
            int index2 = _rndGenerator.Next(letters.Length);
            int index3 = _rndGenerator.Next(letters.Length);
            int index4 = _rndGenerator.Next(letters.Length);
            int index5 = _rndGenerator.Next(letters.Length);
            int number1 = _rndGenerator.Next(100);
            int number2 = _rndGenerator.Next(1000);
            return
                $"{letters[index1]}{letters[index2]}{number1:D2}{letters[index3]}{number2:D4}{letters[index4]}{letters[index5]}";
        }

        private static string GenerateArticleNr(int rowIndex)
        {
            return _rndGenerator.Next(1_000_000_000, 2_000_000_001).ToString();
        }

        private static string GenerateArticleSize(int rowIndex)
        {
            int choice = (rowIndex / 5 + 4) % 7 + 1;
            switch (choice)
            {
                case 1:
                    return "XS";
                case 2:
                    return "S";
                case 3:
                    return "M";
                case 4:
                    return "L";
                case 5:
                    return "XL";
                case 6:
                    return "XXL";
                case 7:
                    return "XXXL";
                default:
                    return "";
            }
        }

        private static string GenerateYear(int rowIndex)
        {
            int year = (rowIndex / 50 + 2) % 3 + 2018;
            return year.ToString();
        }

        private static void GenerateValues(double[] valueBuffer)
        {
            for (int i = 0; i < 12; i++)
            {
                int faktor = _rndGenerator.Next(1_000, 1_000_001);
                double value = _rndGenerator.NextDouble() * faktor;
                value = Math.Truncate(value);
                value = value / 100;
                valueBuffer[i] = value;
            }

            valueBuffer[12] = valueBuffer[0] + valueBuffer[1] + valueBuffer[2];
            valueBuffer[13] = valueBuffer[3] + valueBuffer[4] + valueBuffer[5];
            valueBuffer[14] = valueBuffer[6] + valueBuffer[7] + valueBuffer[8];
            valueBuffer[15] = valueBuffer[9] + valueBuffer[10] + valueBuffer[11];
            valueBuffer[16] = valueBuffer[12] + valueBuffer[13] + valueBuffer[14] + valueBuffer[15];
        }
    }
}
