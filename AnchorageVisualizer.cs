using System.Collections;

namespace InstechWebAPI
{
    /// <summary>
    /// Visualize the filled anchorages
    /// </summary>
    public class AnchorageVisualizer
    {
        /// <summary>
        /// Visualize every anchorage in given list with string
        /// </summary>
        /// <param name="anchorage"></param>
        /// <returns>String representation of every anchorage</returns>
        public string CreateStringAnswerBasedOnAnchorages(ArrayList anchorage)
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
    }
}
