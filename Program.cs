using InstechWebAPI;
using System.Collections;
using System.Dynamic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

AnchorageVisualizer Visualizer = new AnchorageVisualizer();

/// <summary>
/// Takes data from API request, calculated optimal anchorage layout and returns solution
/// </summary>
AnchorageResults runAlgorithmFromPost(AlgorithmFleetsOnAnchorage request)
{
    request.RunAlgorithm();
    String AnchorageVisualization = Visualizer.CreateStringAnswerBasedOnAnchorages(request.finalAnchorageList);
    AnchorageResults results = new AnchorageResults(request.anchorageSize, request.fleetPlacements, request.finalAnchorageList.Count, AnchorageVisualization);
    return results;
}

app.MapPost("/GetOptimalAnchorages", (AlgorithmFleetsOnAnchorage request) => runAlgorithmFromPost(request)
);

app.Run();