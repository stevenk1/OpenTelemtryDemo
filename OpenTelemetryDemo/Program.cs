using Flurl.Http;

await "http://localhost:5141/Weather"
    .PostJsonAsync(new
    {
        Location = "Diepenbeek",
        Country = "Belgium"
    });