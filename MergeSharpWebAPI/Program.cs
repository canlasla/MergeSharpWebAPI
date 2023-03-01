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
            //Console.WriteLine("Message received: ", byteMsg);

            //Declare TPTPMsg
            MergeSharp.TPTPGraphMsg tptpgraphMsg = new MergeSharp.TPTPGraphMsg();

            //initialize TPTPMsg with decoded received bytemsg
            tptpgraphMsg.Decode(byteMsg);

            //merge TPTPMsg with local TPTPGraph
            myTPTPGraphService.MergeTPTPGraphs(1, tptpgraphMsg);

            Console.WriteLine("Graphs merged");

            //translate TPTPGraph node guids to a <Guid,int> dictionary
            //clear existing mapping because state has been updated
            TranslateGuidstoKeys();

            //translate dictionary values into list of Node objects
            var nodeDataArray = TranslateKeystoNodes();

            //set uo http client
            using HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Clear();


            //serialize list of Node objects
            var serializedNodes = JsonConvert.SerializeObject(nodeDataArray);
            Console.WriteLine("serialized nodes: " + serializedNodes);
            var requestData = new StringContent(serializedNodes, Encoding.UTF8, "application/json");
            //send serilized list of node objects to frontend
            //TODO: Send updated state to frontend
            Console.WriteLine("Sending Put Request for front-end");
            var result = await client.PutAsync(
                "https://localhost:7009/TPTPGraph/SendTPTPGraphToFrontEnd", requestData);
            Console.WriteLine(result);

            // decodeLWWSetMsgAndMerge(byteMsg);
            // Console.WriteLine(JsonConvert.SerializeObject(myLWWSetService.Get(1)));
            // var serializedLwwSet = JsonConvert.SerializeObject(myLWWSetService.Get(1));
            // var requestDatalwwset = new StringContent(serializedLwwSet, Encoding.UTF8, "application/json");
            // var result = await client.PutAsync(
            //     "https://localhost:7009/LWWSet/SendLWWSetToFrontEnd", requestDatalwwset);   

            Console.WriteLine("=====================");
        });

        // Start the connection
        try
        {
            await connection.StartAsync();
            Console.WriteLine("Connection started");

            //Console.WriteLine("Raised RecieveMessage event on all clients");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occured when connecting to server:");
            Console.WriteLine(ex.Message);
        }
    }

    private static List<Node> TranslateKeystoNodes()
    {
        string[] types = { "and", "or", "not", "xor", "nand", "nor" };
        var nodeDataArray = new List<Node>();

        foreach (KeyValuePair<Guid, int> entry in IDMapping)
        {
            Random rnd = new Random(); ;

            var n = new Node(types[rnd.Next(0, types.Length)], entry.Value, $"{Math.Pow(-1, rnd.Next(1, 3)) * rnd.Next(0, 200)} {Math.Pow(-1, rnd.Next(1, 3)) * rnd.Next(0, 200)}");
            nodeDataArray.Add(n);
        }

        // nodeDataArray.Add(new Node("and", 6, "0 0"));

        return nodeDataArray;
    }

    private static void TranslateGuidstoKeys()
    {
        IDMapping.Clear();
        int key = 1;
        foreach (var id in myTPTPGraphService.LookupVertices(1))
        {
            IDMapping.Add(id, key);
            key++;
        }
    }

    //Function to hold code used to decode and merge byteMsg containing lwwsetmsg
    private static void DecodeLWWSetMsgAndMerge(byte[] byteMsg)
    {
        MergeSharp.LWWSetMsg<int> lwwMsg = new();
        lwwMsg.Decode(byteMsg);

        // --- Print the received LWWSetMsg state ---

        Console.WriteLine("lwwMsg.addSet:");
        Console.WriteLine(string.Join(",", lwwMsg.addSet.Keys.ToList()));
        //Console.WriteLine(string.Join(",", lwwMsg.addSet.Values.ToList()));

        Console.WriteLine("lwwMsg.removeSet:");
        Console.WriteLine(string.Join(",", lwwMsg.removeSet.Keys.ToList()));
        //Console.WriteLine(string.Join(",", lwwMsg.removeSet.Values.ToList()));

        // --- Merge received state with current state and print the result ---

        myLWWSetService.MergeLWWSets(1, lwwMsg);
        Console.WriteLine("LwwSet:");
        Console.WriteLine(JsonConvert.SerializeObject(myLWWSetService.Get(1)));

        // --- Send the updated state to the frontend ---
    }
}
