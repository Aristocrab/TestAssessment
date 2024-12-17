using Microsoft.Extensions.Configuration;
using Serilog;
using TestAssessment;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var etl = new TripDataEtl(configuration, logger);

var tripData = etl.ReadCsv();
etl.CleanData(tripData);
var distinctRecords = etl.RemoveDuplicates(tripData);
etl.TransformData(distinctRecords);
etl.BulkInsertIntoDatabase(distinctRecords);

logger.Information("ETL process completed successfully");