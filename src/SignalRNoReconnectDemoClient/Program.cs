using Communication.Core;
using Communication.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Signalr.Client;

namespace SignalRNoReconnectDemoClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            AddDependencies(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var serverCommunicator = serviceProvider.GetRequiredService<ServerCommunicator>();
            serverCommunicator.MessageReceived += ServerCommunicator_MessageReceived;
            serverCommunicator.ConnectToServer().Wait();
            

            var messageCounter = 0;
            Task.Delay(1000).Wait();
            Console.WriteLine("Press 'Enter' to send a message to SignalR server. Press 'C' to close the application.");
            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    serverCommunicator.SendMessage(new TestMessage { Id = (++messageCounter).ToString(), Text = "Client message for server" });
                }
                else if (key.Key == ConsoleKey.C)
                {
                    Environment.Exit(0);
                }
            }
        }

        private static void ServerCommunicator_MessageReceived(TestMessage message)
        {
            Console.WriteLine($"Received message id {message.Id} and text {message.Text}");
        }

        private static void AddDependencies(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(x =>
            {
                x.AddNLog();
                x.SetMinimumLevel(LogLevel.Trace);
            });
            serviceCollection.AddSingleton<ServerCommunicator>();
            serviceCollection.AddSingleton<IClientController, ClientController>();
            serviceCollection.AddSingleton<IClientSettings>(x => new ClientSettings { ClientId = "1" });
            serviceCollection.AddSingleton<IConnectionSettings>(x => new ConnectionSettings
            {
                ServerAddress = "192.168.0.121",
                ServerPort = 5001,
                ConnectionTimeoutInSeconds = 30,
                ConnectionWatchDogInSeconds = 45,
                PingSizeBytes = 64,
                PingTimeoutInSeconds = 10,
                PingIntervalInSeconds = 5,
                ReconnectDelaysInSeconds = 3,
            });
        }
    }
}