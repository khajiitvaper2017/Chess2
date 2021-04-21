using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{
    public class ClientObject
    {
        private readonly TcpClient client;
        private readonly ServerObject server;
        private string userName;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        protected internal string Id { get; }
        protected internal NetworkStream Stream { get; private set; }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                var message = GetMessage();
                userName = message;

                message = userName + " вошел в чат";
                server.BroadcastMessage(message, Id);
                Console.WriteLine(message);
                while (true)
                    try
                    {
                        message = GetMessage();
                        Console.WriteLine($"{userName}: {message}");
                        server.BroadcastMessage(message, Id);
                    }
                    catch
                    {
                        message = $"{userName}: покинул чат";
                        Console.WriteLine($"{userName}: покинул чат");
                        server.BroadcastMessage(message, Id);
                        break;
                    }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(Id);
                Close();
            }
        }

        private string GetMessage()
        {
            var data = new byte[64];
            var builder = new StringBuilder();
            do
            {
                var bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
            } while (Stream.DataAvailable);

            return builder.ToString();
        }

        protected internal void Close()
        {
            Stream?.Close();
            client?.Close();
        }
    }

    public class ServerObject
    {
        private static TcpListener tcpListener;
        private readonly List<ClientObject> clients = new();

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }

        protected internal void RemoveConnection(string id)
        {
            var client = clients.FirstOrDefault(c => c.Id == id);
            if (client != null) clients.Remove(client);
        }

        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    var tcpClient = tcpListener.AcceptTcpClient();

                    var clientObject = new ClientObject(tcpClient, this);
                    var clientThread = new Thread(clientObject.Process);
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        protected internal void BroadcastMessage(string message, string id)
        {
            var data = Encoding.UTF8.GetBytes(message);
            foreach (var client in clients)
                if (client.Id != id)
                    client.Stream.Write(data, 0, data.Length);
        }

        protected internal void Disconnect()
        {
            tcpListener.Stop();

            foreach (var client in clients)
                client.Close();

            Environment.Exit(0);
        }
    }

    internal class Program
    {
        private static ServerObject server;
        private static Thread listenThread;

        private static void Main()
        {
            try
            {
                server = new ServerObject();
                listenThread = new Thread(server.Listen);
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}