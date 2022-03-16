using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ParserJohanesburg
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static List<string> AlphaCode = new List<string>();
        public static List<string> Name = new List<string>();
        public static List<string> Price = new List<string>();
        public static List<string> Low = new List<string>();
        public static List<string> High = new List<string>();
        public static List<string> Change = new List<string>();
        public static List<string> Volume = new List<string>();
        public static List<string> Type = new List<string>();
        [STAThread]
        static void Main(string[] args)
        {
            string[] NameStock = new string[] { "Equities", "Foreign Exchange", "Preference Shares", "ETF", "Fixed Interest", "Indices", "Traded Securities", "Warrants", "Commodities"};
            string[] IdStock = new string[] { "853", "919", "916", "874", "855", "231", "851", "850", "900"};
            logger.Info("НАЧАЛО РАБОТЫ ПАРСЕРА");
            string url = "https://www.jse.co.za/ajax/share-watchlist-instruments-ajax?type_id=";
            try
            {
                for (int i = 0; i < IdStock.Length; i++)
                {
                    WebRequest webRequest;
                    string json = "";
                    webRequest = (HttpWebRequest)WebRequest.Create(url + IdStock[i]);
                    webRequest.Timeout = 40000;
                    webRequest.ContentType = "/application/text";
                    webRequest.Method = "GET";
                    using (var response = webRequest.GetResponse())
                    {
                        var result = ((HttpWebResponse)response).StatusCode;
                        using (var responseStream = response.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(responseStream))
                            {
                                json = streamReader.ReadToEnd();
                            }
                        }
                    }
                    dynamic dataJson = JsonConvert.DeserializeObject(json);
                    if (dataJson.data.Count != 0)
                    {
                        try
                        {
                            foreach (dynamic str in dataJson.data)
                            {
                                AlphaCode.Add(str.attributes.field_alpha_code.Value + ",");
                                Name.Add(str.attributes.field_name_long.Value.Replace(',', '.') + ",");
                                Price.Add(" " + Convert.ToString(str.prices.price.Value).Replace(',', '.') + ",");
                                Volume.Add(" " + ConvertingToString(str.prices.field_volume.Value) + ",");
                                High.Add(" " + ConvertingToString(str.prices.field_high.Value) + ",");
                                Low.Add(" " + ConvertingToString(str.prices.field_low.Value) + ",");
                                Change.Add(" " + Convert.ToString(str.prices.field_percentage_change.Value).Replace(',', '.') + ",");
                                Type.Add(NameStock[i]);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex);
                        }
                    }
                    logger.Info("Загрузка " + (i + 1) + " страницы заверешена");
                }
            }
            catch (Exception ex)
            {

                logger.Debug(ex);
            }            
            try
            {
                using (StreamWriter stream = new StreamWriter(args[0] + "\\" + DateTime.Today.ToString("dd" + "MM" + "yyyy") + ".csv"))
                {
                    stream.WriteLine("sep=,");
                    stream.WriteLine("Symbol, Name, Last, Change, Volume, High, Low, Type");
                    int k = 0;
                    foreach (var item in Name)
                    {
                        stream.WriteLine(AlphaCode[k] + item + Price[k] + Change[k] + Volume[k] + High[k] + Low[k] + Type[k]);
                        k++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Debug(ex);
            }
            logger.Info("ОКОНЧАНИЕ РАБОТЫ ПАРСЕРА");
        }
        static string ConvertingToString(string str)
        {
            try
            {
                if (str.Substring(str.Length - 1) == "K")
                {
                    str = Convert.ToString(Convert.ToDouble(str.Remove(str.Length - 1).Replace('.', ',')) * 1000).Replace(',', '.');
                }
                else if (str.Substring(str.Length - 1) == "M")
                {
                    str = Convert.ToString(Convert.ToDouble(str.Remove(str.Length - 1).Replace('.', ',')) * 1000000).Replace(',', '.');
                }
                else
                {
                    str = str.Replace(',', '.');
                }
            }
            catch (Exception ex)
            {

                logger.Debug(ex);
            }           
            return str;
        }
    }
}
