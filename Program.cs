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

// Code to get list of ships, anchorage
var httpClient = new HttpClient();
//string randomFleetsAndAnchorage; // JSON format
AnchorageAndFleets anchorageAndFleets;
int[,] anchorage;
int[,] tempAnchorage;
ArrayList finalAnchorageList = new ArrayList();
int shipNumber = 1; // used for visual representation of ships

string runAlgorithmFromPost(AnchorageAndFleets request)
    // Takes POST data, calculated optimal anchorage layout, sends back solution
{
    anchorageAndFleets = request;
    tempAnchorage = new int[anchorageAndFleets.anchorageSize.height, anchorageAndFleets.anchorageSize.width];
    finalAnchorageList = new ArrayList();
    InitializeNewAnchorage();
    ArrayList test = RunAlgorithm();
    Console.WriteLine(test[0]);
    return PrintFittedAnchorageInfo(test);
}

ArrayList RunAlgorithm()
// Runs through algorithm, returning a list of all anchorages with ships
{
    while (MoreShipsRemaining())
    {
        while (AddShipToAnchorageIfPossible())
        { UpdateShipNumber(); };
        GoToNextAnchorage();
    }
    return finalAnchorageList;
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
            anchorageAndFleets = JsonSerializer.Deserialize<AnchorageAndFleets>(responseAsString) ??
            throw new InvalidOperationException();

            SortFleets();

            foreach (Fleet fleet in anchorageAndFleets.fleets)
            {
                Console.WriteLine(fleet.singleShipDimensions.width + " " + fleet.singleShipDimensions.height);
            }

            InitializeNewAnchorage();

            RunAlgorithm();

            PrintFittedAnchorageInfo(finalAnchorageList);

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

void SortFleets()
// Sorts list of fleets by their longest side, from shortest to longest
{
    anchorageAndFleets.fleets.Sort((y, x) => (
        Math.Max(x.singleShipDimensions.width, x.singleShipDimensions.height).CompareTo(
            Math.Max(y.singleShipDimensions.width, y.singleShipDimensions.height))
        ));
}

void InitializeNewAnchorage()
// Sets variable anchorage to empty anchorage with correct dimensions
{
    anchorage = new int[anchorageAndFleets.anchorageSize.height, anchorageAndFleets.anchorageSize.width];
}

void GoToNextAnchorage()
// Saves anchorage and starts a new iteration
{
    finalAnchorageList.Add(anchorage);
    InitializeNewAnchorage();
}

bool MoreShipsRemaining()
// Returns true if there are more ships of any type, false if all ships are placed
{
    foreach (Fleet fleet in anchorageAndFleets.fleets)
    {
        if (fleet.shipCount > 0)
        {
            return true;
        }
    }
    return false;
}

bool AddShipToAnchorageIfPossible()
// Goes through all ships and tries to place one. Returns false if anchorage is full
{
    foreach (Fleet currentFleet in anchorageAndFleets.fleets)
    {
        if (currentFleet.shipCount > 0)
        {
            if (TryToPlaceShipAllLocations(currentFleet.singleShipDimensions.width, currentFleet.singleShipDimensions.height))
            {
                currentFleet.shipCount -= 1;
                return true;
            }
        }
    }
    return false;
}

bool TryToPlaceShipAllLocations(int shipWidth, int shipHeight)
// Systematically goes through available spots on anchorage, left to right, then top to bottom, and tries to place ship there
{
    for (int y = 0; y < anchorage.GetLength(0); y++)
    {
        for (int x = 0; x < anchorage.GetLength(1); x++)
        {
            if (anchorage[y, x] == 0)
            {
                if (TryToPlaceShipAtCoordinates(y, x, shipWidth, shipHeight))
                    return true;
                else if (TryToPlaceShipAtCoordinates(y, x, shipHeight, shipWidth)) // Try to place ship tilted 90 degrees
                    return true;
            }
        }
    }
    return false;
}

bool TryToPlaceShipAtCoordinates(int anchorageY, int anchorageX, int shipWidth, int shipHeight)
// Tries to place ship on specified anchorage coordinates
{
    // if ship does not go out of anchorage bounds
    if (anchorageY + shipWidth <= anchorage.GetLength(0) && anchorageX + shipHeight <= anchorage.GetLength(1))
    {
        tempAnchorage = (int[,])anchorage.Clone();
        for (int y = anchorageY; y < anchorageY + shipWidth; y++)
        {
            for (int x = anchorageX; x < anchorageX + shipHeight; x++)
            {
                if (tempAnchorage[y, x] == 0)
                    tempAnchorage[y, x] = shipNumber;
                else
                    return false;
            }
        }
        anchorage = (int[,])tempAnchorage.Clone(); // Place ship if there is sufficient space
        return true;
    }
    return false;
}

void UpdateShipNumber()
// Updates ship number, used to visualize where different ships are on anchorage
{
    if (++shipNumber > 9)
        shipNumber = 1;
}

app.MapPost("/GetOptimalAnchorages", (AnchorageAndFleets request) => runAlgorithmFromPost(request)
);

app.Run();

// Return list 

// Classes used to create objects for fleets and anchorage
public class Fleet
{
    public required Dimensions singleShipDimensions { get; set; }
    public required string shipDesignation { get; set; }
    public required int shipCount { get; set; }
}

public class Dimensions
{
    public required int width { get; set; }
    public required int height { get; set; }
}

public class AnchorageAndFleets
{
    public required Dimensions anchorageSize { get; set; }
    public required List<Fleet> fleets { get; set; }
}

///