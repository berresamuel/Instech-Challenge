using System.Collections;
using System.Diagnostics.CodeAnalysis;

// Classes used to create objects for fleets and anchorages

public class Fleet
{
    public required Dimensions singleShipDimensions { get; set; }
    public required string shipDesignation { get; set; }
    public required int shipCount { get; set; }

    [SetsRequiredMembers]
    public Fleet(Dimensions singleShipDimensions, string shipDesignation, int shipCount)
    {
        this.singleShipDimensions = singleShipDimensions;
        this.shipDesignation = shipDesignation;
        this.shipCount = shipCount;
    }
}

public class Dimensions
{
    public required int width { get; set; }
    public required int height { get; set; }

    [SetsRequiredMembers]
    public Dimensions(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
}

public class AnchorageResults
{
    public required Dimensions AnchorageSize { get; set; }
    public required List<Fleet> Fleets { get; set; }
    public required int AmountOfAnchorageIterationsNeeded { get; set; }
    public required string AnchoragesWithFleetsVisualized { get; set; }

    [SetsRequiredMembers]
    public AnchorageResults(Dimensions AnchorageSize, List<Fleet> Fleets,  int AmountOfAnchorageIterationsNeeded, string AnchoragesWithFleetsVisualized)
    {
        this.AnchorageSize = AnchorageSize;
        this.Fleets = Fleets;
        this.AmountOfAnchorageIterationsNeeded = AmountOfAnchorageIterationsNeeded;
        this.AnchoragesWithFleetsVisualized = AnchoragesWithFleetsVisualized;
    }
}

public class AnchorageAndFleets
    // Contains everything needed to store and manipulate data on fleets and anchorages.
{
    public required Dimensions anchorageSize { get; set; }
    public required List<Fleet> fleets { get; set; }

    public int[,] anchorage;
    int[,]? tempAnchorage;
    public ArrayList finalAnchorageList = new ArrayList();
    public int shipNumber = 1; // used for visual representation of ships

    [SetsRequiredMembers]
    public AnchorageAndFleets(Dimensions anchorageSize, List<Fleet> fleets)
    {
        this.anchorageSize = anchorageSize;
        this.fleets = fleets;
        InitializeNewAnchorage();
    }

    public ArrayList RunAlgorithm()
    // Runs through algorithm, returning a list of all anchorages with ships
    {
        SortFleets();
        while (MoreShipsRemaining())
        {
            while (AddShipToAnchorageIfPossible())
            { UpdateShipNumber(); };
            GoToNextAnchorage();
        }
        return finalAnchorageList;
    }

    public void UpdateShipNumber()
    // Updates ship number, used to visualize where different ships are on anchorage
    {
        if (++shipNumber > 9)
            shipNumber = 1;
    }

    public void SortFleets()
    // Sorts list of fleets by their longest side, from shortest to longest
    {
        fleets.Sort((y, x) => (
            Math.Max(x.singleShipDimensions.width, x.singleShipDimensions.height).CompareTo(
                Math.Max(y.singleShipDimensions.width, y.singleShipDimensions.height))
            ));
    }

    private void InitializeNewAnchorage()
    // Sets variable anchorage to empty anchorage with correct dimensions
    {
        anchorage = new int[anchorageSize.height, anchorageSize.width];
    }

    public void GoToNextAnchorage()
    // Saves anchorage and starts a new iteration
    {
        finalAnchorageList.Add(anchorage);
        InitializeNewAnchorage();
    }

    private bool MoreShipsRemaining()
    // Returns true if there are more ships of any type, false if all ships are placed
    {
        foreach (Fleet fleet in fleets)
        {
            if (fleet.shipCount > 0)
            {
                return true;
            }
        }
        return false;
    }

    private bool AddShipToAnchorageIfPossible()
    // Goes through all ships and tries to place one. Returns false if anchorage is full
    {
        foreach (Fleet currentFleet in fleets)
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

    private bool TryToPlaceShipAllLocations(int shipWidth, int shipHeight)
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

    private bool TryToPlaceShipAtCoordinates(int anchorageY, int anchorageX, int shipWidth, int shipHeight)
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
}