using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

// Classes used to create objects for fleets and anchorages

/// <summary>
/// Used to create fleet objects from API request
/// </summary>
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

/// <summary>
/// Represents dimension of anchorage, fleets, or even position of a fleet.
/// </summary>
public class Dimensions
{
    // Class used to represent dimensions of an object (or location of an object)
    public required int width { get; set; }
    public required int height { get; set; }

    [SetsRequiredMembers]
    public Dimensions(int width, int height)
    {
        this.width = width;
        this.height = height;
    }
}

/// <summary>
/// Fleet's position on anchorage, and which anchorage it is on
/// </summary>
public class FleetPlacement
{
    // Class that stores information about a fleet placed on an anchorage
    public required Dimensions singleShipDimensions { get; set; }
    public required string shipDesignation { get; set; }
    public required int anchorageIndex { get; set; }
    public required Dimensions location { get; set; }
    public required bool flipped { get; set; }

    /// <summary>
    /// Construct a fleet placement object, where <paramref name="locationX"/> and <paramref name="locationY"/> corresponds to location on anchorage,
    /// where top left part of the ship is placed, and <paramref name="flipped"/> is true if the ship has been flipped 90 degrees
    /// </summary>
    /// <param name="fleet"></param>
    /// <param name="locationY"></param>
    /// <param name="locationX"></param>
    /// <param name="anchorageIndex"></param>
    /// <param name="flipped"></param>
    [SetsRequiredMembers]
    public FleetPlacement(Fleet fleet, int locationY, int locationX, int anchorageIndex, bool flipped)
    {
        singleShipDimensions = fleet.singleShipDimensions;
        shipDesignation = fleet.shipDesignation;
        this.anchorageIndex = anchorageIndex;
        location = new Dimensions(locationX, locationY);
        this.flipped = flipped;
    }
}
/// <summary>
/// Used to construct a JSON response for API
/// </summary>
public class AnchorageResults
{
    // Class made to contain relevant data for API response
    public required Dimensions AnchorageSize { get; set; }
    public required int AmountOfAnchorageIterationsNeeded { get; set; }
    public required List<FleetPlacement> FleetPlacements { get; set; }
    public required string AnchoragesWithFleetsVisualized { get; set; }

    /// <summary>
    /// Construct JSON response for API
    /// </summary>
    [SetsRequiredMembers]
    public AnchorageResults(Dimensions AnchorageSize, List<FleetPlacement> FleetPlacements,  int AmountOfAnchorageIterationsNeeded, string AnchoragesWithFleetsVisualized)
    {
        this.AnchorageSize = AnchorageSize;
        this.FleetPlacements = FleetPlacements;
        this.AmountOfAnchorageIterationsNeeded = AmountOfAnchorageIterationsNeeded;
        this.AnchoragesWithFleetsVisualized = AnchoragesWithFleetsVisualized;
    }
}
/// <summary>
/// Object that includes anchorage dimensions and list of fleets
/// </summary>
public class AnchorageAndFleets
{
    public required Dimensions anchorageSize { get; set; }
    public required List<Fleet> fleets { get; set; }
}