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

        /// <summary>
        /// Constructor
        /// </summary>
        public WebListener()
        {
            _keepGoing = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the HttpListener
        /// </summary>
        public void Start()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(_baseUrl);
            _httpListener.Start();
            Console.WriteLine($"Listener started on {_baseUrl}");
            Task.Run(() => ListenForRequests());
        }

        /// <summary>
        /// Stop the HttpListener
        /// </summary>
        public void Stop()
        {
            _keepGoing = false;
            _httpListener.Stop();
            Console.WriteLine("Listener stopped");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Listen for incoming Http Requests
        /// </summary>
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occured while listening for requests: {ex}");
                }

            }

        }

        /// <summary>
        /// Handle the request
        /// </summary>
        /// <param name="request">The request to handle</param>
        private void ProcessRequest(HttpListenerContext request)
        {
            Console.WriteLine($"Method {request.Request.HttpMethod}");
            //We need the origin if we want our extension to be able to use the response
            var origin = request.Request.Headers["origin"];

            //We only handle POST requests
            if (request.Request.HttpMethod == "POST")
            {
                var url = request.Request.Url;
                var urlData = url.LocalPath.Split('/');
                //The URL contains a valid query - We are looking for url in the form /questions/{number}
                if (urlData.Count() == 3)
                {
                    var reqType = urlData[1];
                    //Match the request type - In this example we only work with question
                    if (reqType == "question")
                    {
                        Console.WriteLine($"Request pushed [question - {urlData[2]}]");
                        HandleResponse(request.Response, 200, "Request Accepted", origin);
                    }
                    //For any other type of request give an error
                    else
                    {
                        HandleResponse(request.Response, 400, "Unknown Request", origin);
                    }

                }
                //If the URL contains more or less parts than expected return an error
                else
                {
                    HandleResponse(request.Response, 400, "Invalid URL", origin);
                }
            }
            //Return an error for any other request type
            else
            {
                HandleResponse(request.Response, 405, "Server only supports POST", origin);
            }
        }

        /// <summary>
        /// Send a response to the caller to indicate the state of the request
        /// </summary>
        /// <param name="response">Response Object</param>
        /// <param name="code">Http Status Code</param>
        /// <param name="msg">Status Message</param>
        /// <param name="origin">Origin of the call - Without this Chrome extensions won't accept a response</param>
        private void HandleResponse(HttpListenerResponse response, int code, string msg, string origin)
        {
            //Set the headers
            response.AppendHeader("ContentType", "application/json");
            response.AppendHeader("Access-Control-Allow-Origin", origin);

            //Set the response code and message
            response.StatusCode = code;
            response.StatusDescription = msg;

            //Set the response content body
            var rspMsg = new { Message = msg };
            var rspJson = JsonConvert.SerializeObject(rspMsg);
            var bytes = Encoding.UTF8.GetBytes(rspJson);
            response.OutputStream.Write(bytes, 0, bytes.Length);
            response.OutputStream.Close();

            //Close the response so it can be sent back to the caller
            response.Close();
        }

        #endregion
    }
}