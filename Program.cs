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

//InitializeRandomFleetsAsync();

AnchorageResults runAlgorithmFromPost(AnchorageAndFleets request)
    // Takes POST data, calculated optimal anchorage layout, sends back solution
{
    // Creates a clone of the original fleets
    List<Fleet> fleetsWithOriginalShipCount = request.fleets.ConvertAll(fleet => new Fleet(fleet.singleShipDimensions, fleet.shipDesignation, fleet.shipCount));

    request.RunAlgorithm();
    String AnchorageVisualization = CreateStringAnswerBasedOnAnchorages(request.finalAnchorageList);
    AnchorageResults results = new AnchorageResults(request.anchorageSize, fleetsWithOriginalShipCount, request.finalAnchorageList.Count, AnchorageVisualization);
    return results;
}

string CreateStringAnswerBasedOnAnchorages(ArrayList anchorage)
// Prints number of anchorages, and where ships are placed
{
    string finalString = "";
    finalString += $"According to the algorithm, we need {anchorage.Count} iterations\n";
    foreach (int[,] iteration in anchorage)
    {
        finalString += "\n------------ New anchorage ------------\n\n";

        for (int y = 0; y < iteration.GetLength(0); y++)
        {
            for (int x = 0; x < iteration.GetLength(1); x++)
            {
                if (iteration[y, x] == 0)
                {
                    finalString += ".";
                }
                else
                    finalString += iteration[y, x];
            }
            finalString += "\n";
        }
    }
    return finalString;
}

async Task<bool> InitializeRandomFleetsAsync()
// Gets random fleets and anchorage dimensions from API, sorts and runs algorithm
{
    var httpClient = new HttpClient();
    try
    {
        HttpResponseMessage httpResponse = await httpClient.GetAsync("https://esa.instech.no/api/fleets/random");

        if (httpResponse.IsSuccessStatusCode)
        {
            Console.WriteLine("Response received from random fleet api");

            string responseAsString = await httpResponse.Content.ReadAsStringAsync();

            // Why do I get a warning here ??
            AnchorageAndFleets anchorageAndFleets = JsonSerializer.Deserialize<AnchorageAndFleets>(responseAsString) ??
            throw new InvalidOperationException();

            anchorageAndFleets.RunAlgorithm();

            Console.WriteLine(CreateStringAnswerBasedOnAnchorages(anchorageAndFleets.finalAnchorageList));

            return true;

        }
        else
        {
            Console.WriteLine($"Failed with status code: {httpResponse.StatusCode}");
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Error occured while retrieving data: {ex.Message}");
    }
    return false;
}

app.MapPost("/GetOptimalAnchorages", (AnchorageAndFleets request) => runAlgorithmFromPost(request)
);

app.Run();