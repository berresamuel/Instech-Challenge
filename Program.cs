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

///

var httpClient = new HttpClient();

string runAlgorithmFromPost(AnchorageAndFleets request)
    // Takes POST data, calculated optimal anchorage layout, sends back solution
{
    request.InitializeNewAnchorage();
    ArrayList test = request.RunAlgorithm();
    return PrintFittedAnchorageInfo(test);
}

async Task<bool> InitializeRandomFleetsAsync()
// Gets random fleets and anchorage dimensions from API, sorts and runs algorithm
{
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

            anchorageAndFleets.SortFleets();

            foreach (Fleet fleet in anchorageAndFleets.fleets)
            {
                Console.WriteLine(fleet.singleShipDimensions.width + " " + fleet.singleShipDimensions.height);
            }

            anchorageAndFleets.InitializeNewAnchorage();

            anchorageAndFleets.RunAlgorithm();

            PrintFittedAnchorageInfo(anchorageAndFleets.finalAnchorageList);

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

string PrintFittedAnchorageInfo(ArrayList anchorage)
// Prints number of anchorages, and where ships are placed
{
    string finalString = "";
    Console.WriteLine($"According to the algorithm, we need {anchorage.Count} iterations\n");
    finalString += $"According to the algorithm, we need {anchorage.Count} iterations\n";
    foreach (int[,] iteration in anchorage)
    {
        //Console.WriteLine("\n------------ New anchorage ------------\n");
        finalString += "\n------------ New anchorage ------------\n\n";
        for (int y = 0; y < iteration.GetLength(0); y++)
        {
            for (int x = 0; x < iteration.GetLength(1); x++)
            {
                if (iteration[y, x] == 0)
                {
                    //Console.Write("X");
                    finalString += ".";
                }
                else
                    //Console.Write(iteration[y, x]);
                    finalString += iteration[y, x];
            }
            //Console.WriteLine();
            finalString += "\n";
        }
    }
    return finalString;
}

app.MapPost("/GetOptimalAnchorages", (AnchorageAndFleets request) => runAlgorithmFromPost(request)
);

app.Run();