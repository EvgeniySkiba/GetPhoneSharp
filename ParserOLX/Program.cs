using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;

namespace ParserOLX
{
    class Program
    {

        static string _urlToLoad =// "https://www.olx.ua/obyavlenie/2-komn-zhk-omega-ot-sk-budova-krasivyy-vid-na-more-i-gorod-IDnjBuq.html";
                                     "https://www.olx.ua/obyavlenie/1-komn-vavilova-startovye-tseny-ot-zastroyschika-ID9027J.html";
        static string getPhoneBaseString = "https://www.olx.ua/ajax/misc/contact/phone/";// njBuq/";

        static void Main(string[] args)
        {
          
            int index = _urlToLoad.IndexOf("ID") + "ID".Length;
            string idString = _urlToLoad.Substring(index  , _urlToLoad.Length-index);
            string[] idNum = idString.Split('.');
            string id = idNum[0];


            List<string> phones = GetPhone(_urlToLoad,id);

            foreach(string item in phones)
            {
                Console.WriteLine(item);
            }

            Console.ReadLine();
        }

        static List<string> GetPhone(String urlToLoad, string id_)
        {
            List<string> phonesList = new List<string>();
            string pt = string.Empty;

            HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.OptionFixNestedTags = true;

            HttpWebRequest request = HttpWebRequest.Create(urlToLoad) as HttpWebRequest;
            request.Method = "GET";

            var _cookieContainer = new CookieContainer();
            request.CookieContainer = _cookieContainer;

            /* Sart browser signature */
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");
            /* Sart browser signature */

         
            WebResponse response = request.GetResponse();

            _cookieContainer = request.CookieContainer;

            htmlDoc.Load(response.GetResponseStream(), true);
            if (htmlDoc.DocumentNode != null)
            {
                var articleNodes =
                    htmlDoc.DocumentNode.SelectNodes("/html/body/div[@id='innerLayout']/section[@id='body-container']");

                var script = articleNodes.Descendants("script");

                foreach (var item in script)
                {
                    string input = item.InnerHtml.Trim();
                    string[] arrays = input.Split('=');
                    pt = arrays[arrays.Length - 1].Replace("\'", string.Empty).Replace(";", string.Empty).Trim();
                }

                string[] array = urlToLoad.Split('#');
                string referString = array[0];
          
                
                //  https://www.olx.ua/obyavlenie/1-komn-vavilova-startovye-tseny-ot-zastroyschika-ID9027J.html
                string getPhoneString = String.Concat(getPhoneBaseString,id_,"/", "?pt=", pt);

                HttpWebRequest phoneRequest = HttpWebRequest.Create(getPhoneString) as HttpWebRequest;
                phoneRequest.Method = "GET";
                phoneRequest.Referer = referString;
                phoneRequest.CookieContainer = _cookieContainer;

                phoneRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
                phoneRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                phoneRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us,en;q=0.5");

                WebResponse phoneResponse = phoneRequest.GetResponse();
                htmlDoc.Load(phoneResponse.GetResponseStream(), true);
                string res = htmlDoc.DocumentNode.InnerHtml;

                const string MatchPhonePattern = @"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}";
                Regex rx = new Regex(MatchPhonePattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                // Find matches.
                MatchCollection matches = rx.Matches(res);
                // Report the number of matches found.
                int noOfMatches = matches.Count;
                // Report on each match.


                foreach (Match match in matches)
                {
                    phonesList.Add(match.Value.ToString());
                }
            }
            return phonesList;

        }
    }
}
