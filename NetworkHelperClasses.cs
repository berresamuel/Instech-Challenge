using System.Text.Json;

namespace InstechWebAPI
{
    /// <summary>
    /// Retrieves fleets from instech API. Returns null if failing to retrieve.
    /// </summary>
    public class RetrieveFleets
    {
        AnchorageVisualizer Visualizer = new AnchorageVisualizer();
        async Task<AlgorithmFleetsOnAnchorage> InitializeRandomFleetsAsync()
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
                    AlgorithmFleetsOnAnchorage anchorageAndFleets = JsonSerializer.Deserialize<AlgorithmFleetsOnAnchorage>(responseAsString) ??
                    throw new InvalidOperationException();

                    return anchorageAndFleets;

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
            return null;
        }
    }
}
