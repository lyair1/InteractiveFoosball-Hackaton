
namespace Foosball.UI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;

    public class HttpCommandsManager : IDisposable
    {
        private readonly Dictionary<string, HttpEvent> events;
        private readonly HttpListener listener;

        private readonly List<string> prefixes = new List<string> { "http://localhost/foosballApi/" };
        private HttpData possessionEvent;

        public HttpCommandsManager(HttpData possessionEvent, Dictionary<string, HttpEvent> events)
        {
            this.possessionEvent = possessionEvent;
            this.events = events;
            this.listener = new HttpListener();
            foreach (string prefix in this.prefixes)
            {
                this.listener.Prefixes.Add(prefix);
            }

            this.listener.Start();
            Console.WriteLine("Listening...");

            Task.Run((Action) Listen);
        }

        private void Listen()
        {
            try
            {
                while (this.listener.IsListening)
                {
                    // Blocking command
                    HttpListenerContext context = this.listener.GetContext();
                    try
                    {
                        string rawPayload = new StreamReader(context.Request.InputStream).ReadToEnd();
                        if (rawPayload.StartsWith("Possession"))
                        {
                            string[] payload = rawPayload.Split('*');
                            possessionEvent(new[] 
                            {
                                long.Parse(payload[1]),
                                long.Parse(payload[2]),
                                long.Parse(payload[3]),
                            });
                        }
                        if(rawPayload.StartsWith("HotSpots"))
                        {

                        }
                        else // Regular commands
                        {
                            string[] payload = rawPayload.Split('*');
                            Team team = (Team)Enum.Parse(typeof(Team), payload[1], true);

                            // Handle command asynchronously
                            Task.Run(() => this.events[payload[0]](team));
                        }

                        // Return OK response
                        context.Response.StatusCode = 200;
                        using (StreamWriter outputStream = new StreamWriter(context.Response.OutputStream))
                        {
                            outputStream.Write("OK");
                        }
                    }
                    catch (Exception e)
                    {
                        // Return error response
                        context.Response.StatusCode = 500;
                        using (StreamWriter outputStream = new StreamWriter(context.Response.OutputStream))
                        {
                            outputStream.Write(e.ToString());
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error occurred: {0}", exception);
            }
        }

        public void Dispose()
        {
            this.listener.Stop();
            this.listener.Close();
        }
    }
}
