using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;

namespace ScanData
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] ListInput = File.ReadAllLines(Application.StartupPath + "//import.txt");
            string[] Listtdea = File.ReadAllLines(Application.StartupPath + "//tdea.txt");
            string[] Configs = File.ReadAllLines(Application.StartupPath + "//config.txt");
            int Delay1 = Convert.ToInt32( Configs[0]);
            int Delay2 = Convert.ToInt32(Configs[1]);
            int Retry = Convert.ToInt32(Configs[2]);

            int error = 0;

            for (int loop = 0; loop < ListInput.Length; loop++)
            {
                string CustomerCode = ListInput[loop].Split('|')[0];
                string FirstName = ListInput[loop].Split('|')[1];
                string LastName = ListInput[loop].Split('|')[2];
                string LastName2 = LastName.Replace(" ", "+");
                string[] Randomtdea = Listtdea[new Random().Next(0, Listtdea.Length)].Split('|');
                string Randomt = Randomtdea[0];
                string Randomd = Randomtdea[0];
                string Randome = Randomtdea[0];
                string Randoma = Randomtdea[0];
                string result = sendRequest("https://mapi.vietjetair.com/apimobileweb/get-reservation.php", "ReservationNumber=" + CustomerCode + "&PaxFirstName=" + LastName2 + "&PaxLastName=" + FirstName + "&Itemnumber=18391422&Language=vi&step=flightstatus&_t=" + Randomt + "&_d=" + Randomd + "&_e=" + Randome + "&_a=" + Randoma);
                //File.WriteAllText(Application.StartupPath + "//txt.txt", result);
                if (Regex.IsMatch(result, "\"OperationMessage\":\"OK\""))
                {
                    error = 0;
                    string Export = CustomerCode + "|" + FirstName + "|" + LastName + "|";
                    if (result.Split(new[] { "ArrivalLocal" }, StringSplitOptions.None).Length == 2)
                    {
                        MatchCollection coll = Regex.Matches(result, "\"ArrivalLocal\":\"(\\d{4})-(\\d{2})-(\\d{2})T\\d{2}:\\d{2}:\\d{2}\"");
                        string year = coll[0].Groups[1].Value;
                        string month = coll[0].Groups[2].Value;
                        string day = coll[0].Groups[3].Value;
                        string dateflight1 = day + "/" + month + "/" + year;
                        coll = Regex.Matches(result, "\"Flight\":\"([^\"]*)\"");
                        string flight1 = coll[0].Groups[1].Value;
                        Export += dateflight1 + "|" + flight1 + "|";
                        //Console.WriteLine(dateflight1 + flight1);

                    }
                    else if (result.Split(new[] { "ArrivalLocal" }, StringSplitOptions.None).Length == 3)
                    {
                        MatchCollection coll = Regex.Matches(result, "\"ArrivalLocal\":\"(\\d{4})-(\\d{2})-(\\d{2})T\\d{2}:\\d{2}:\\d{2}\".*\"ArrivalLocal\":\"(\\d{4})-(\\d{2})-(\\d{2})T\\d{2}:\\d{2}:\\d{2}\"");
                        string year = coll[0].Groups[1].Value;
                        string month = coll[0].Groups[2].Value;
                        string day = coll[0].Groups[3].Value;
                        string year2 = coll[0].Groups[4].Value;
                        string month2 = coll[0].Groups[5].Value;
                        string day2 = coll[0].Groups[6].Value;
                        string dateflight1 = day + "/" + month + "/" + year;
                        string dateflight2 = day2 + "/" + month2 + "/" + year2;
                        coll = Regex.Matches(result, "\"Flight\":\"([^\"]*)\".*\"Flight\":\"([^\"]*)\"");
                        string flight1 = coll[0].Groups[1].Value;
                        string flight2 = coll[0].Groups[1].Value;
                        Export += dateflight1 + "|" + flight1 + "|" + dateflight2 + "|" + flight2 + "|";
                        //Console.WriteLine(dateflight1 + flight1 + dateflight2 + flight2);

                    }
                    if (Regex.IsMatch(result, "Email"))
                    {
                        MatchCollection coll = Regex.Matches(result, "\"Email\":\"([^\"]*)\"");
                        string Email = coll[0].Groups[1].Value;
                        Export += Email + "|";
                        //Console.WriteLine(Email);

                    }
                    if (Regex.IsMatch(result, "Telephone"))
                    {
                        MatchCollection coll = Regex.Matches(result, "\"Telephone\":\"([^\"]*)\"");
                        string Phone = coll[0].Groups[1].Value;
                        Export += Phone + "|";
                        //Console.WriteLine(Phone);

                    }
                    if (Regex.IsMatch(result, "StatusCode"))
                    {
                        MatchCollection coll = Regex.Matches(result, "\"StatusCode\":\"([^\"]*)\"");
                        string StatusCode = coll[0].Groups[1].Value;
                        Export += StatusCode + "|";
                        //Console.WriteLine(StatusCode);
                    }
                    if (Regex.IsMatch(result, "Payer"))
                    {
                        Export += "DTT|";
                    }
                    else
                    {
                        Export += "CTT|";
                    }
                    Console.WriteLine(Export);
                    File.AppendAllText(Application.StartupPath + "//export.txt", Export + Environment.NewLine);
                }else if (error<Retry) { error++;loop--; } else
                {
                    error = 0;
                }
                Thread.Sleep(new Random().Next(Delay1, Delay2));
            }
            Console.ReadKey();
        }
        private static string sendRequest(string url, string postdata)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.ASCII.GetBytes(postdata);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseString;
        }
    }
}
