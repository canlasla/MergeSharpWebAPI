using MergeSharpWebAPI.Hubs;
using MergeSharpWebAPI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using static MergeSharpWebAPI.Globals;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using MergeSharpWebAPI.Models;

internal class Program
{
    private static void StartServer()
    {
        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        var builder = WebApplication.CreateBuilder();

        // Add services to the container.

        _ = builder.Services.AddControllers();
        _ = builder.Services.AddSignalR(options =>
        {
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(1);
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        _ = builder.Services.AddEndpointsApiExplorer();
        _ = builder.Services.AddSwaggerGen();

        _ = builder.Services.AddCors(options => options.AddPolicy(name: MyAllowSpecificOrigins,
                              policy => _ = policy.AllowAnyHeader()
                                        .AllowAnyMethod()
                                        .SetIsOriginAllowed((host) => true)
                                        .AllowCredentials()
                                        ));
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI();
        }

        _ = app.UseHttpsRedirection();

        _ = app.UseCors(MyAllowSpecificOrigins);

        _ = app.UseRouting();

        _ = app.UseAuthorization();

        _ = app.MapControllers();
        _ = app.MapGet("/", () => "Hello world!");
        _ = app.MapHub<FrontEndHub>("/hubs/frontendmessage");

        app.Run();
    }

    private static async Task Main(string[] args)
    {
        Thread serverThread = new Thread(StartServer);
        serverThread.Start();

        // connection.ServerTimeout = TimeSpan.FromSeconds(5000);

        connection.Reconnecting += error =>
        {
            System.Diagnostics.Debug.Assert(connection.State == HubConnectionState.Reconnecting);
            // Notify users the connection was lost and the client is reconnecting.
            // Start queuing or dropping messages.
            // var result = await client.PutAsync(
            //     "https://localhost:7009/LWWSet/SendLWWSetToFrontEnd", requestData);
            return Task.CompletedTask;
        };

        connection.Reconnected += async connectionId =>
        {
            if (connection.State == HubConnectionState.Connected)
            {
                Console.WriteLine("RECONNECTED");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myLWWSetService.Get(1).LwwSet.GetLastSynchronizedUpdate().Encode());
                // Console.WriteLine("SENT?");
            }
            // return Task.CompletedTask;
        };

        connection.Closed += async (error) =>
                    {
                        await Task.Delay(new Random().Next(0, 5) * 1000);
                        Console.WriteLine("starting to connect to server again");

                        await connection.StartAsync();
                    };


        // Define behaviour on events from server
        _ = connection.On<byte[]>("ReceiveEncodedMessage", async byteMsg =>
        {
            MergeSharp.LWWSetMsg<int> lwwMsg = new();
            lwwMsg.Decode(byteMsg);

            Console.WriteLine("lwwMsg.addSet:");
            Console.WriteLine(string.Join(",", lwwMsg.addSet.Keys.ToList()));

            myLWWSetService.MergeLWWSets(1, lwwMsg);

            using HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Clear();

            var serializedLwwSet = JsonConvert.SerializeObject(myLWWSetService.Get(1));

            var requestData = new StringContent(serializedLwwSet, Encoding.UTF8, "application/json");
            //string myContent = await requestData.ReadAsStringAsync();
            //Console.WriteLine("im porinting data");
            //Console.WriteLine(myContent);

            Console.WriteLine("Sending Put Request for front-end");
            var result = await client.PutAsync(
                "https://localhost:7009/LWWSet/SendLWWSetToFrontEnd", requestData);
            Console.WriteLine(result);
        });

        // Start the connection

        while (connection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await connection.StartAsync();
                Console.WriteLine("Connection started");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured when connecting to server:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(connection.State);
                // await connection.StartAsync();
                await Task.Delay(5000);
            }
        }
    }
}
