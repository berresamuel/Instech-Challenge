using System.Collections;
using System.Dynamic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

// Code to get list of ships, anchorage
var httpClient = new HttpClient();
//string randomFleetsAndAnchorage; // JSON format
AnchorageAndFleets anchorageAndFleets;
int[,] anchorage;
int[,] tempAnchorage;
ArrayList finalAnchorageList = new ArrayList();
int shipNumber = 1; // used for visual representation of ships

/* if (await InitializeRandomFleetsAsync())
    RunAlgorithm(); */

await InitializeRandomFleetsAsync();

ArrayList RunAlgorithm()
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

void PrintFittedAnchorageInfo(ArrayList anchorage)
// Prints number of anchorages, and how they look
{
    Console.WriteLine("According to the algorithm, we need " + anchorage.Count + " iterations");
    Console.WriteLine();
    foreach (int[,] iteration in anchorage)
    {
        Console.WriteLine("\n------------ New anchorage ------------\n");
        for (int y = 0; y < iteration.GetLength(0); y++)
        {
            for (int x = 0; x < iteration.GetLength(1); x++)
            {
                if (iteration[y, x] == 0)
                {
                    Console.Write("X");
                }
                else
                    Console.Write(iteration[y, x]);
            }
            Console.WriteLine();
        }
    }
}

// Code to format the list of fleets
void SortFleets()
{
    // Sorts list of fleets by their longest side, from shortest to longest
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

// Algorithm to find a good combination of fitting the ships, return true if ship placed
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
{
    if (++shipNumber > 9)
        shipNumber = 1;
}

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