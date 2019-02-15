using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Console;

namespace ImageUrlScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> templateList = new List<string>();
            string connection = "<connstring>";
            string query = "<query>";
            string path = @"filepath";

            using (OracleConnection con = new OracleConnection(connection))
            {
                using (OracleCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = query;
                    OracleDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string template = Convert.ToString(reader.GetValue(reader.GetOrdinal("VALUE")));
                        templateList.Add(template);
                    }
                }
            }
            
            if (!File.Exists(path))
                File.Create(path);

            TextWriter tw = new StreamWriter(path);
            List<Uri> imageUrlList = ScrapeImageUrls(templateList);
            foreach (Uri imageUri in imageUrlList)
                tw.WriteLine(Convert.ToString(imageUri));

            WriteLine("Done");
            tw.Close();

            ReadLine();
        }

        public static List<Uri> ScrapeImageUrls(List<string> templateList)
        {
            List<Uri> links = new List<Uri>();
            string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";

            foreach (string htmlTemplate in templateList)
            {
                MatchCollection matchesImgSrc = Regex.Matches(htmlTemplate, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                foreach (Match m in matchesImgSrc)
                {
                    string href = m.Groups[1].Value;
                    links.Add(new Uri(href));
                }
            }

            return links;
        }
    }
}
