using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Dynamic;

namespace Shopify {
    public class Api : DynamicObject {

        public string _apiKey { get; set; }
        public string _password { get; set; }
        public string _baseUrl { get; set; }
        public Api(string apiKey, string password, string storeUrl)
	    {
            //https://api_key:password@some-store.myshopify.com/admin/some-resource
            _apiKey = apiKey;
            _password = password;
            _baseUrl = storeUrl; //some-store.myshopify.com

            if (_baseUrl.StartsWith("http"))
                _baseUrl = _baseUrl.Replace("http", "https");

            if (!_baseUrl.StartsWith("https://"))
                _baseUrl = "https://" + _baseUrl;

            if (!_baseUrl.EndsWith("/"))
                _baseUrl += "/admin/";
            else
                _baseUrl += "admin/";
	    }
        /// <summary>
        /// A simple GET request to the Shopify API
        /// </summary>
        string Send(string url) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
	    request.PreAuthenticate = true;
            var creds = new NetworkCredential(userName: _apiKey, password: _password);
            request.Credentials = creds;
            var response = (HttpWebResponse)request.GetResponse();
            string result = "";

            using (Stream stream = response.GetResponseStream()) {
                StreamReader sr = new StreamReader(stream);
                result = sr.ReadToEnd();
                sr.Close();
            }
            return result;
        }

        /// <summary>
        /// This allows you to work with the data at Shopify using a Property, which represents Products, Customers, etc
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result) {

            var name = binder.Name.ToLower();

            //we can do this because the Shopify stuff is all pluralized with "s" :)
            if (!name.EndsWith("s"))
                name += "s";

            result = new ShopifyObject(name,_baseUrl,_apiKey,_password);
            return true;
        }

        /// <summary>
        /// This builds a query with the passed in named arguments - shopify.Products(collection_id:121212)
        /// </summary>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {
            var name = binder.Name.ToLower() + ".json";
            var url = _baseUrl + name;

            //params?
            var info = binder.CallInfo;
            var looper = 0;
            if (info.ArgumentNames.Count > 0) {

                for (int i = 0; i < args.Length; i++) {
                    var argName = info.ArgumentNames[i].ToLower();
                    var val = args[i];
                    //the ID is a singular call
                    //with a special format
                    if (argName == "id") {
                        url = url.Replace(".json", "/" + val + ".json");
                    } else {
                        if (looper == 0)
                            url += "?";
                        else
                            url += "&";
                        url += string.Format("{0}={1}", argName, val);
                    }
                    looper++;
                }
            }
            var json = Send(url);
            result = JsonHelper.Decode(json);
            return true;
        }


    }
}
