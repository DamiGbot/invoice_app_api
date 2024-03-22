
using InvoiceApp.Services.IServices;
using Microsoft.Extensions.Logging;

namespace InvoiceApp.Services.Services
{
    public class PokemonService : IPokemonService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PokemonService> _logger;

        public PokemonService(HttpClient httpClient, ILogger<PokemonService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetRandomPokemonNameAsync()
        {
            var randomPokemonId = new Random().Next(1, 898); 
            try
            {
                var response = await _httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/{randomPokemonId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    dynamic pokemon = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                    var pokemonName = pokemon?.name.ToString();
                    if (!string.IsNullOrEmpty(pokemonName))
                    {
                        _logger.LogInformation($"Successfully retrieved Pokémon name: {pokemonName}");
                        return pokemonName;
                    }
                    else
                    {
                        _logger.LogWarning($"Pokémon API response did not contain a valid name for ID {randomPokemonId}.");
                    }
                }
                else
                {
                    _logger.LogWarning($"Pokémon API request failed with status code: {response.StatusCode} for ID {randomPokemonId}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching Pokémon name for ID {randomPokemonId}.");
            }

            // Fallback or default behavior in case of failure
            return "PokeUser" + new Random().Next(1000, 9999); // Example fallback username
        }
    }
}
