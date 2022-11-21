using MergeSharpWebAPI.Hubs;
using MergeSharpWebAPI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using static MergeSharpWebAPI.Globals;
using MergeSharpWebAPI.Controllers;

internal class Program
{
    private static void StartServer() {
        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        var builder = WebApplication.CreateBuilder();

        // Add services to the container.

        _ = builder.Services.AddControllers();
        _ = builder.Services.AddSignalR();

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

        // Create a connection to the server
        const string propogationMessageServer = "https://localhost:709/test";
        //const string propogationMessageServer = "https://serverwebapi20221114203154.azurewebsites.net/hubs/propagationmessage";
        // var connection2 = new HubConnectionBuilder()
        //                .WithUrl(propogationMessageServer)
        //                .WithAutomaticReconnect()
        //                .Build();

        connection.Reconnecting += error =>
        {
            System.Diagnostics.Debug.Assert(connection.State == HubConnectionState.Reconnecting);

            // Notify users the connection was lost and the client is reconnecting.
            // Start queuing or dropping messages.

            return Task.CompletedTask;
        };

        connection.Closed += async (error) =>
                    {
                        await Task.Delay(new Random().Next(0, 5) * 1000);
                        await connection.StartAsync();
                    };

        // Define behaviour on events from server
        _ = connection.On<byte[]>("ReceiveEncodedMessage", async byteMsg =>
        {
            MergeSharp.LWWSetMsg<int> lwwMsg = new();
            lwwMsg.Decode(byteMsg);

            Console.WriteLine("lwwMsg.addSet:");
            Console.WriteLine(string.Join(",", lwwMsg.addSet.Keys.ToList()));

            //transfer data between threads
            // merge the data to the correct thread
            myLWWSetService.MergeLWWSets(1, lwwMsg);

            //sean is doin this
            //raise receivemessage on javascript frontend
            // the javscsript front end is subscribed to this
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:7009");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("1", "5")
                });

                var result = await client.PostAsync("AddElement/1", content);

                string resultContent = await result.Content.ReadAsStringAsync();

                Console.WriteLine(resultContent);
            }

        });

        // Start the connection
        try
        {
            await connection.StartAsync();
            Console.WriteLine("Connection started");
        }
        catch (Exception ex)
        {
            Console.WriteLine("dkjfshkdjh");
            Console.WriteLine(ex.Message);
            Console.WriteLine("lllllllllllll");
        }

        // Test sending a message to the server
        // try
        // {
        //     //MergeSharp.LWWSet<int> set1 = new();
        //     //set1.Add(5);
        //     //set1.Add(6);

        //     //myLWWSetService.AddElement(1, 69);

        //     //figure out how to get crdt data from the other thread

        //     byte[] byteMsg = myLWWSetService.GetLastSynchronizedUpdate(1).Encode();
        //     await connection.InvokeAsync("SendEncodedMessage", byteMsg);
        // }
        // catch (Exception ex)
        // {
        //     Console.Write(ex.Message);
        // }
    }
}
