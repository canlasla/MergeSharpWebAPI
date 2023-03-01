using MergeSharpWebAPI.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Text;
using static MergeSharpWebAPI.ServerConnection.Globals;
using MergeSharpWebAPI.Models;

internal class Program
{
    private static void StartServer()
    {
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

    private static void ConfigureConnectionReconnected()
    {
        connection.Reconnected += async connectionId =>
        {
            if (connection.State == HubConnectionState.Connected)
            {
                Console.WriteLine("RECONNECTED");
                // wait some random amount for case when multiple clients are coming back online at same time
                // want to stagger the SendEncodedMessage call. If SendEncodedMessage sent at same time,
                // one of them will get lost
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.InvokeAsync("SendEncodedMessage", myLWWSetService.Get(1).LwwSet.GetLastSynchronizedUpdate().Encode());
            }
        };
    }

    private static void ConfigureConnectionClosed()
    {
        connection.Closed += async (error) =>
                    {
                        await Task.Delay(new Random().Next(0, 5) * 1000);
                        Console.WriteLine("starting to connect to server again");

                        await connection.StartAsync();
                    };
    }

    private static void ConfigureConnectionOnReceiveEncodeMessage()
    {
        _ = connection.On<byte[]>("ReceiveEncodedMessage", async byteMsg =>
        {
            Console.WriteLine("Message received: ", byteMsg);
            //MergeSharp.LWWSetMsg<int> lwwMsg = new();
            //lwwMsg.Decode(byteMsg);

            // --- Print the received LWWSetMsg state ---

            //Console.WriteLine("lwwMsg.addSet:");
            //Console.WriteLine(string.Join(",", lwwMsg.addSet.Keys.ToList()));
            ////Console.WriteLine(string.Join(",", lwwMsg.addSet.Values.ToList()));

            //Console.WriteLine("lwwMsg.removeSet:");
            //Console.WriteLine(string.Join(",", lwwMsg.removeSet.Keys.ToList()));
            ////Console.WriteLine(string.Join(",", lwwMsg.removeSet.Values.ToList()));

            //// --- Merge received state with current state and print the result ---

            //myLWWSetService.MergeLWWSets(1, lwwMsg);
            //Console.WriteLine("LwwSet:");
            //Console.WriteLine(JsonConvert.SerializeObject(myLWWSetService.Get(1)));

            // --- Send the updated state to the frontend ---

            //Declare TPTPMsg
            MergeSharp.TPTPGraphMsg tptpgraphMsg = new MergeSharp.TPTPGraphMsg();

            //initialize TPTPMsg with decoded received bytemsg
            tptpgraphMsg.Decode(byteMsg);

            //merge TPTPMsg with local TPTPGraph
            myTPTPGraphService.MergeTPTPGraphs(1, tptpgraphMsg);

            Console.WriteLine("Graphs merged");

            //translate TPTPGraph node guids to a <Guid,int> dictionary
            IDMapping.Clear();
            int key = 1;
            foreach(var id in myTPTPGraphService.LookupVertices(1))
            {
                IDMapping.Add(id, key);
                key++;
            }

            //translate dictionary values into list of Node objects
            string[] types = { "and", "or", "not", "xor", "nand", "nor" };

            var nodeDataArray = new List<Node>();

            foreach (KeyValuePair<Guid, int> entry in IDMapping)
            {
                Random rnd = new Random(); ;

                var n = new Node(types[rnd.Next(0, types.Length)], entry.Value, $"{Math.Pow(-1, rnd.Next(1, 3)) * rnd.Next(0, 200)} {Math.Pow(-1, rnd.Next(1, 3)) * rnd.Next(0, 200)}");
                nodeDataArray.Add(n);
            }

            nodeDataArray.Add(new Node("and", 6, "0 0"));

            //serialize list of Node objects
            var serializedNodes = JsonConvert.SerializeObject(nodeDataArray);

            //send serilized list of node objects to frontend
            using HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Clear();

            var frontEndLwwSetJson = JsonConvert.SerializeObject(myLWWSetService.Get(1));

            var requestData = new StringContent(frontEndLwwSetJson, Encoding.UTF8, "application/json");

            Console.WriteLine("Sending Put Request for front-end");
            var result = await client.PutAsync(
                "https://localhost:7009/LWWSet/SendLWWSetToFrontEnd", requestData);
            Console.WriteLine(result);

            // TODO: check this result for errors
        });
    }

    private static async void ConnectToServer()
    {
        // Start the connection
        while (connection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await connection.StartAsync();
                Console.WriteLine("Connection started");
                await connection.InvokeAsync("SendEncodedMessage", myLWWSetService.Get(1).LwwSet.GetLastSynchronizedUpdate().Encode());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured when connecting to server:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(connection.State);
                await Task.Delay(5000);
            }
        }
    }
    private static void Main(string[] args)
    {
        Thread CRDTEndpoints = new Thread(StartServer);
        CRDTEndpoints.Start();

        ConfigureConnectionReconnected();

        ConfigureConnectionClosed();

        ConfigureConnectionOnReceiveEncodeMessage();

        ConnectToServer();

    }
}
