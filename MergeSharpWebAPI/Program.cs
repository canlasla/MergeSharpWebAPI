using MergeSharpWebAPI.Hubs;
using Microsoft.AspNetCore.SignalR.Client;

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
        _ = app.MapHub<ChatHub>("/hubs/chat");

        app.Run();
    }

    private static async Task Main(string[] args)
    {
        Thread serverThread = new Thread(StartServer);
        serverThread.Start();

        // Create a connection to the server
        const string propogationMessageServer = "https://localhost:7106/hubs/propagationmessage";
        HubConnection connection = new HubConnectionBuilder()
                        .WithUrl(propogationMessageServer)
                        .WithAutomaticReconnect()
                        .Build();

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
        _ = connection.On<byte[]>("ReceiveEncodedMessage", byteMsg =>
        {
            MergeSharp.LWWSetMsg<int> lwwMsg = new();
            lwwMsg.Decode(byteMsg);
            Console.WriteLine("lwwMsg.addSet:");
            Console.WriteLine(string.Join(",", lwwMsg.addSet.Keys.ToList()));
        });

        // Start the connection
        try
        {
            await connection.StartAsync();
            Console.WriteLine("Connection started");
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
        }

        // Test sending a message to the server
        try
        {
            MergeSharp.LWWSet<int> set1 = new();
            set1.Add(5);
            set1.Add(6);

            byte[] byteMsg = set1.GetLastSynchronizedUpdate().Encode();
            await connection.InvokeAsync("SendEncodedMessage", byteMsg);
        }
        catch (Exception ex)
        {
            Console.Write(ex.Message);
        }
    }
}
