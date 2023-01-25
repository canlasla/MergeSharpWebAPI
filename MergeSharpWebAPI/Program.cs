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
        IDMapping.Add(Guid.NewGuid(), 1);
        IDMapping.Add(Guid.NewGuid(), 2);
        IDMapping.Add(Guid.NewGuid(), 3);
        IDMapping.Add(Guid.NewGuid(), 4);
        IDMapping.Add(Guid.NewGuid(), 5);

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
            Console.WriteLine("Message received: ", byteMsg);
            //MergeSharp.LWWSetMsg<int> lwwMsg = new();
            //lwwMsg.Decode(byteMsg);
            //Console.WriteLine("lwwMsg.addSet:");
            //Console.WriteLine(string.Join(",", lwwMsg.addSet.Keys.ToList()));

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

            //var serializedLwwSet = JsonConvert.SerializeObject(myLWWSetService.Get(1));

            var requestData = new StringContent(serializedNodes, Encoding.UTF8, "application/json");
            //string myContent = await requestData.ReadAsStringAsync();
            //Console.WriteLine("im porinting data");
            //Console.WriteLine(myContent);

            Console.WriteLine("Sending Put Request for front-end");
            var result = await client.PutAsync(
                "https://localhost:7009/LWWSet/SendLWWSetToFrontEnd", requestData);
            //Console.WriteLine(result);

            Console.WriteLine("=====================");
        });

        // Start the connection
        try
        {
            await connection.StartAsync();
            Console.WriteLine("Connection started");

            //myLWWSetService.AddElement(1, 11);
            //myLWWSetService.AddElement(1, 22);
            //myLWWSetService.AddElement(1, 33);
            //myLWWSetService.RemoveElement(1, 11);
            //myLWWSetService.RemoveElement(1, 22);

            //var byteMsg = myLWWSetService.Get(1).LwwSet.GetLastSynchronizedUpdate().Encode();
            //MergeSharp.LWWSetMsg<int> lwwMsg = new();
            //lwwMsg.Decode(byteMsg);
            //Console.WriteLine("addset: " + string.Join(",", lwwMsg.addSet.Keys.ToList()));
            //Console.WriteLine("removeset: " + string.Join(",", lwwMsg.removeSet.Keys.ToList()));



            //myTPTPGraphService.AddVertex(1, Guid.NewGuid());
            //myTPTPGraphService.AddVertex(1, Guid.NewGuid());
            //myTPTPGraphService.AddVertex(1, Guid.NewGuid());

            //Console.WriteLine();
            //var byteMsgtptp = myTPTPGraphService.GetLastSynchronizedUpdate(1).Encode();
            //MergeSharp.TPTPGraphMsg tptpGraphMsg = new();
            //tptpGraphMsg.Decode(byteMsgtptp);
            //Console.WriteLine(string.Join(", ", myTPTPGraphService.LookupVertices(1)));
            //Console.WriteLine("Vertices from tptpgraphmsg: " + string.Join(", ", tptpGraphMsg._verticesMsg.addSet));
            //Console.WriteLine("Edges from tptpgraphmsg: " + string.Join(", ", tptpGraphMsg._edgesMsg.addSet));


            //Console.WriteLine(JsonConvert.SerializeObject(myTPTPGraphService.Get(1)));
            //Console.WriteLine(JsonConvert.SerializeObject(myTPTPGraphService.LookupVertices(1)));

            //if (connection.State == HubConnectionState.Connected)
            //{
            //    await MergeSharpWebAPI.Globals.connection.InvokeAsync("SendEncodedMessage", myTPTPGraphService.Get(1).TptpGraph.GetLastSynchronizedUpdate().Encode());
            //}
            //Console.WriteLine("Raised RecieveMessage event on all clients");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occured when connecting to server:");
            Console.WriteLine(ex.Message);
        }
    }
}
