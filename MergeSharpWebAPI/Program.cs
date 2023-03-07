using MergeSharpWebAPI.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System.Text;
using static MergeSharpWebAPI.ServerConnection.Globals;
using MergeSharpWebAPI.Models;

internal class Program
{
    private static void CRDTEndpointsForFrontend()
    {
        var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        var builder = WebApplication.CreateBuilder();

        // Add services to the container.

        _ = builder.Services.AddControllers();
        _ = builder.Services.AddSignalR();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        _ = builder.Services.AddEndpointsApiExplorer();
        _ = builder.Services.AddSwaggerGen();

        _ = builder.Services.AddCors(options => options.AddPolicy(
                                        name: MyAllowSpecificOrigins,
                                        policy => _ = policy.AllowAnyHeader()
                                                            .AllowAnyMethod()
                                                            .SetIsOriginAllowed((host) => true)
                                                            .AllowCredentials()
                                        )
                                    );
        var frontendEndpoints = builder.Build();

        // Configure the HTTP request pipeline.
        if (frontendEndpoints.Environment.IsDevelopment())
        {
            _ = frontendEndpoints.UseSwagger();
            _ = frontendEndpoints.UseSwaggerUI();
        }

        _ = frontendEndpoints.UseHttpsRedirection();

        _ = frontendEndpoints.UseCors(MyAllowSpecificOrigins);

        _ = frontendEndpoints.UseRouting();

        _ = frontendEndpoints.UseAuthorization();

        _ = frontendEndpoints.MapControllers();
        _ = frontendEndpoints.MapGet("/", () => "Hello world!");
        _ = frontendEndpoints.MapHub<FrontEndHub>("/hubs/frontendmessage");

        frontendEndpoints.Run();
    }

    private static void ConfigureServerConnectionReconnected()
    {
        serverConnection.Reconnected += async connectionId =>
        {
            if (serverConnection.State == HubConnectionState.Connected)
            {
                Console.WriteLine("RECONNECTED");
                // wait some random amount for case when multiple clients are coming back online at same time
                // want to stagger the SendEncodedMessage call. If SendEncodedMessage sent at same time,
                // one of them will get lost
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await serverConnection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
            }
        };
    }

    private static void ConfigureServerConnectionClosed()
    {
        serverConnection.Closed += async (error) =>
                    {
                        await Task.Delay(new Random().Next(0, 5) * 1000);
                        Console.WriteLine("starting to connect to server again");

                        await serverConnection.StartAsync();
                    };
    }

    private static async void ProcessAndDisplayLwwSetMsg(byte[] byteMsg)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Clear();
        DecodeLWWSetMsgAndMerge(byteMsg);
        Console.WriteLine(JsonConvert.SerializeObject(myLWWSetService.Get(1)));
        var frontEndLwwSetJson = JsonConvert.SerializeObject(myLWWSetService.Get(1));
        var requestDatalwwset = new StringContent(frontEndLwwSetJson, Encoding.UTF8, "application/json");
        var resultLww = await client.PutAsync(
            "https://localhost:7009/LWWSet/SendLWWSetToFrontEnd", requestDatalwwset);

        // TODO: check this result for errors
    }

    private static async void ProcessAndDisplayTptpGraphMsg(byte[] byteMsg)
    {
        Console.WriteLine("Message received: ", byteMsg);

        //Declare TPTPMsg
        MergeSharp.TPTPGraphMsg tptpgraphMsg = new MergeSharp.TPTPGraphMsg();
        tptpgraphMsg.Decode(byteMsg);

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

    }

    private static void ConfigureServerConnectionOnReceiveEncodeMessage()
    {
        _ = serverConnection.On<byte[]>("ReceiveEncodedMessage", async byteMsg =>
        {
            //Merge Graphs using applysynchronizedupdate from graphservice
            myGraphService.ApplySynchronizedUpdate(byteMsg);
            
            //set up http client
            using HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Clear();

            Console.WriteLine("Sending Put Request for front-end");
            var result = await client.PutAsync(
                "https://localhost:7009/Graph/SendGraphToFrontEnd", null);
            Console.WriteLine(result);

            // ProcessAndDisplayTptpGraphMsg(byteMsg);
            // ProcessAndDisplayLwwSetMsg(byteMsg);
        });
    }

    private static void ConfigureServerConnectionOnSendMessageToNewConnection()
    {
        _ = serverConnection.On<string>("SendMessageToNewConnection", async newConnectionID =>
        {
            Console.WriteLine(JsonConvert.SerializeObject(myGraphService.GetGraph()));
            await serverConnection.InvokeAsync("SendClientNewState", myGraphService.GetLastSynchronizedUpdate(), newConnectionID);

            // TODO: check this result for errors
        });
    }
    private static async void ConnectToServer()
    {
        while (serverConnection.State == HubConnectionState.Disconnected)
        {
            try
            {
                await serverConnection.StartAsync();
                Console.WriteLine("Connection started");
                await serverConnection.InvokeAsync("RequestStateFromServer", serverConnection.ConnectionId);
                await serverConnection.InvokeAsync("SendEncodedMessage", myGraphService.GetLastSynchronizedUpdate());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured when connecting to server:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(serverConnection.State);
                await Task.Delay(5000);
            }
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

    private static void Main(string[] args)
    {
        Thread frontEndCRDTEndpoints = new Thread(CRDTEndpointsForFrontend);
        frontEndCRDTEndpoints.Start();

        ConfigureServerConnectionReconnected();

        ConfigureServerConnectionClosed();

        ConfigureServerConnectionOnReceiveEncodeMessage();

        ConfigureServerConnectionOnSendMessageToNewConnection();

        ConnectToServer();
    }
}
