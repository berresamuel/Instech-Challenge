﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace InstechWebAPI
{
    /// <summary>
    /// Class with algorithm for finding the best way to store ships on as few anchorages as possible
    /// </summary>
    public class AlgorithmFleetsOnAnchorage : AnchorageAndFleets
    {
        public int[,] anchorage;
        int[,]? tempAnchorage;
        public ArrayList finalAnchorageList = new ArrayList();
        public List<FleetPlacement> fleetPlacements = new List<FleetPlacement>();
        public int shipNumber = 1; // used for visual representation of ships
        
        /// <summary>
        /// Constructor for class used to calculate optimal way to place fleets on anchorage, such that as few iterations of the anchorage given is used.
        /// </summary>
        [SetsRequiredMembers]
        public AlgorithmFleetsOnAnchorage(Dimensions anchorageSize, List<Fleet> fleets)
        {
            this.anchorageSize = anchorageSize;
            this.fleets = fleets;
            InitializeNewAnchorage();
        }

        /// <summary>
        /// Runs the algorithm for finding optimal way to place ships on as few anchorages as possible
        /// </summary>
        public void RunAlgorithm()
        // Runs through algorithm, returning a list of all anchorages with ships
        {
            SortFleets();
            while (MoreShipsRemaining())
            {
                while (AddShipToAnchorageIfPossible())
                { UpdateShipNumber(); };
                GoToNextAnchorage();
            }
        }

        private void UpdateShipNumber()
        // Updates ship number, used to visualize where different ships are on anchorage
        {
            if (++shipNumber > 9)
                shipNumber = 1;
        }

        private void SortFleets()
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

        private void GoToNextAnchorage()
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
                    if (TryToPlaceShipAllLocations(currentFleet))
                    {
                        currentFleet.shipCount -= 1;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TryToPlaceShipAllLocations(Fleet fleet)
        // Systematically goes through available spots on anchorage, left to right, then top to bottom, and tries to place ship there
        {
            for (int y = 0; y < anchorage.GetLength(0); y++)
            {
                for (int x = 0; x < anchorage.GetLength(1); x++)
                {
                    if (anchorage[y, x] == 0)
                    {
                        if (TryToPlaceShipAtCoordinates(y, x, fleet.singleShipDimensions.width, fleet.singleShipDimensions.height))
                        {
                            AddFleetPlacement(y, x, fleet);
                            return true;
                        }
                        else if (TryToPlaceShipAtCoordinates(y, x, fleet.singleShipDimensions.height, fleet.singleShipDimensions.width)) // Try to place ship tilted 90 degrees
                        {
                            AddFleetPlacement(y, x, fleet, true);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool TryToPlaceShipAtCoordinates(int anchorageY, int anchorageX, int shipWidth, int shipHeight)
        // Tries to place ship on specified anchorage coordinates
        {
            // if ship does not go out of anchorage bounds
            if (anchorageY + shipHeight <= anchorage.GetLength(0) && anchorageX + shipWidth <= anchorage.GetLength(1))
            {
                tempAnchorage = (int[,])anchorage.Clone();
                for (int y = anchorageY; y < anchorageY + shipHeight; y++)
                {
                    for (int x = anchorageX; x < anchorageX + shipWidth; x++)
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
        private void AddFleetPlacement(int anchorageY, int anchorageX, Fleet fleet, bool flipped90Degrees = false)
        {
            FleetPlacement newFleetPlaced = new FleetPlacement(fleet, anchorageY, anchorageX, finalAnchorageList.Count, flipped90Degrees);
            fleetPlacements.Add(newFleetPlaced);
        }
    }
}
