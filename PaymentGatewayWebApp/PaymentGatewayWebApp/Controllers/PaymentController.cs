using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Newtonsoft.Json;
using PaymentGatewayWebApp.Models;

namespace PaymentGatewayWebApp.Controllers
{
    public class PaymentController : Controller
    {
        private string paymentGatewayBase = WebConfigurationManager.AppSettings["baseUrl"];
        private string confirmationBase = WebConfigurationManager.AppSettings["success_failed_cancle_base"];

        string Baseurl = WebConfigurationManager.AppSettings["baseUrl"] + "/request.php";
        
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            string messgae = "";
            try
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();

                var person = new TransactionModel();
                person.tran_id = RandomString(10);
                person.amount = "100";
                person.cus_name = "Mr Customer";
                person.cus_email = "customer@gmail.com";
                person.cus_phone = "012266584457";



                person.success_url = confirmationBase + "/Payment/Success";
                person.fail_url = confirmationBase + "/Payment/Failed";
                person.cancel_url = confirmationBase + "/Payment/Cancelled";
                PropertyInfo[] infos = person.GetType().GetProperties();

                foreach (PropertyInfo pair in infos)
                {
                    string name = pair.Name;
                    var value = pair.GetValue(person, null);
                    
                    parameters.Add(pair.Name, value.ToString());
                }
                using (var client = new HttpClient())
                {
                    HttpContent DictionaryItems = new FormUrlEncodedContent(parameters);

                    using (
                        var result =
                            await client.PostAsync(Baseurl, DictionaryItems))
                    {
                        var input = await result.Content.ReadAsStringAsync();
                        var trans = input.Remove(0, 2).Split('"')[0];

                        //string url = "https://sandbox.aamarpay.com" + trans;
                        string url = paymentGatewayBase + trans;

                        //\/ paynow.php ? track = AAM1581391191203665
                        return Redirect(url);
                    }
                }
            }
            catch (Exception e)
            {

                string message = e.Message;
                messgae = message;
            }

            return new ContentResult() { Content = messgae };
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string Success()
        {
            return "Success";
        }

        public string Failed()
        {
            return "Failed";
        }

        public string Cancelled()
        {
            return "Cancelled";
        }
    }
}