var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

var weatherApi = builder.AddProject<Projects.MyWeatherAPI>("weatherAPI")
    .WithReference(cache)
    .WaitFor(cache);

var umbWebsite = builder.AddProject<Projects.UmbObservability>("umbraco-site")
    .WithReference(weatherApi)
    .WaitFor(weatherApi);



builder.Build().Run();
