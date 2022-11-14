using MergeSharpWebAPI.Hubs;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy => _ = policy.AllowAnyHeader()
                                .AllowAnyMethod()
                                // .WithOrigins("http://localhost:3000", "https://localhost:7010", "http://localhost:5010")
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

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Hello world!");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
