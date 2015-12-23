using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChromeListener
{
    public class WebListener
    {
        #region Variables

        private HttpListener _httpListener;
        private bool _keepGoing;
        private readonly string _baseUrl = "http://localhost:60024/";

        #endregion

        #region Constructor

        public WebListener()
        {
            _keepGoing = true;
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(_baseUrl);
            _httpListener.Start();
            Console.WriteLine($"Listener started on {_baseUrl}");
            Task.Run(() => ListenForRequests());
        }

        public void Stop()
        {
            _keepGoing = false;
            _httpListener.Stop();
            Console.WriteLine("Listener stopped");
        }

        #endregion

        #region Private Methods

        private void ListenForRequests()
        {
            while (_keepGoing)
            {
                try
                {
                    var data = _httpListener.GetContext();
                    Console.WriteLine("Request received - Processing");
                    Task.Run(() => ProcessRequest(data));
                }
                catch (Exception)
                {
                    //Bad!
                }

            }

        }

        private void ProcessRequest(HttpListenerContext request)
        {
            Console.WriteLine($"Method {request.Request.HttpMethod}");
            var origin = request.Request.Headers["origin"];
            if (request.Request.HttpMethod == "POST")
            {
                var url = request.Request.Url;
                var urlData = url.LocalPath.Split('/');
                //The URL contains a valid query
                if (urlData.Count() == 3)
                {
                    var reqType = urlData[1];
                    if (reqType == "question")
                    {
                        Console.WriteLine($"Request pushed [question - {urlData[2]}]");
                        HandleResponse(request.Response, 200, "Request Accepted", origin);
                    }
                    else
                    {
                        HandleResponse(request.Response, 400, "Unknown Request", origin);
                    }

                }
                else
                {
                    HandleResponse(request.Response, 400, "Invalid URL", origin);
                }
            }
            else
            {
                HandleResponse(request.Response, 405, "Server only supports POST", origin);
            }
        }

        private void HandleResponse(HttpListenerResponse response, int code, string msg, string origin)
        {
            response.AppendHeader("ContentType", "application/json");
            //How to set this correctly?
            response.AppendHeader("Access-Control-Allow-Origin", origin);
            response.StatusCode = code;
            response.StatusDescription = msg;
            var rspMsg = new { Message = msg };
            var rspJson = JsonConvert.SerializeObject(rspMsg);
            var bytes = Encoding.UTF8.GetBytes(rspJson);
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();
            response.Close();
        }

        #endregion
    }
}