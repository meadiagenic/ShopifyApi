using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Dynamic;
using System.Web.Script.Serialization;

namespace Shopify {
    public class ShopifyObject:DynamicObject {
        string _objectType;
        string _apiKey;
        string _password;
        string _shopUrl;

        /// <summary>
        /// This is a special, psuedo-wrapper for Shopify objects such as Products, Customers, and so on
        /// </summary>
        public ShopifyObject(string objectType, string storeUrl, string apiKey, string password) {
            _objectType = objectType;
            _shopUrl = storeUrl;
            _apiKey = apiKey;
            _password = password;
        }

        /// <summary>
        /// A Dynamic catcher - allows you to invoke Save, Delete, Add, etc on this object
        /// </summary>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result) {

            //the args should be expandos
            //loop them and spin up some JSON
            var sb = new StringBuilder();
            var name = _objectType;
            if (name.EndsWith("s"))
                name = name.TrimEnd('s');

            //what are we doing?
            if (binder.Name == "Save") {

                var item = args[0];
                //var dc = (IDictionary<string, object>)item;
                var serializer = new JavaScriptSerializer();
                var json = serializer.Serialize(item);
                
                //wrap this as we need an outer identifier
                json = "{ " + name + ": " + json + "}";


                bool isNew = !item.GetType().GetProperties().Any(x => x.Name == "id");

                //adjust the root to be the name here
                //var outer = new Dictionary<string, object>();
                //outer.Add(name, item);

                if (isNew) {
                    Post(json);
                    Console.WriteLine("{0} added...", name);
                } else {
                    //pull the id
                    var id = item.GetType().GetProperty("id").GetValue(item, null).ToString();
                    Put(id, json);
                    Console.WriteLine("{0} updated...", name);
                }

            } else if (binder.Name == "Delete" || binder.Name == "Destroy") {
                Delete(args[0].ToString());
                Console.WriteLine("Blog {0} deleted ...", args[0].ToString());
            } else {
                throw new InvalidDataException("Can't tell what it is you want to do - try using Save or Delete instead");
            }
            result = this;
            return true;
        }

        /// <summary>
        /// Executes a PUT to Shopify - used for Updates
        /// </summary>
        dynamic Put(string id, string json) {
            //build the URL
            var url = _shopUrl + this._objectType + "/" + id + ".json";
            var result = ExecuteRequest(url, "PUT", json);
            return JsonHelper.Decode(result);
        }
        /// <summary>
        /// Executes an HTTP DELETE to Shopify... guess what it does!
        /// </summary>
        void Delete(string id) {
            var url = _shopUrl + this._objectType + "/" + id + ".json";
            ExecuteRequest(url, "DELETE", "");
        }
        /// <summary>
        /// Executes an HTTP POST - which adds an item to the Shopify DB
        /// </summary>
        dynamic Post(string json) {
            //build the URL
            var url = _shopUrl + this._objectType + ".json";
            var result = ExecuteRequest(url, "POST", json);
            //the result will be a pile of JSON
            //deserialize it and return
            return JsonHelper.Decode(result);

        }

        /// <summary>
        /// The core executor for sending off requests
        /// </summary>
        string ExecuteRequest(string url, string verb, string data) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = verb;
            request.ContentType = "application/json";
            var creds = new NetworkCredential(userName: _apiKey, password: _password);
            request.Credentials = creds;

            //add the data if needed
            if (!String.IsNullOrEmpty(data)) {
                using (var ms = new MemoryStream()) {
                    using (var writer = new StreamWriter(request.GetRequestStream())) {
                        writer.Write(data);
                        writer.Close();
                    }
                }
            }

            var response = (HttpWebResponse)request.GetResponse();
            string result = "";

            using (Stream stream = response.GetResponseStream()) {
                StreamReader sr = new StreamReader(stream);
                result = sr.ReadToEnd();
                sr.Close();
            }
            return result;
        }
    }
}
