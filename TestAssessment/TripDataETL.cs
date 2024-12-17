using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using TestAssessment.CsvMaps;
using TestAssessment.Models;

namespace TestAssessment;

public class TripDataEtl
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public TripDataEtl(IConfiguration configuration, ILogger logger)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public List<TripData> ReadCsv()
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
        };
        using var reader = new StreamReader(_configuration["ETLSettings:InputFilePath"]!);
        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<TripDataMap>();
        var records = csv
            .GetRecords<TripData>()
            .ToList();
        
        _logger.Information("Read {Count} records from CSV", records.Count);

        return records;
    }
    
    public void CleanData(List<TripData> tripData)
    {    
        var removed = tripData.Where(x => x.PassengerCount == null 
                                || x.FareAmount < 0 
                                || x.TipAmount < 0).ToList();
        
        var writeConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };
        using var writer = new StreamWriter(_configuration["ETLSettings:RemovedFilePath"]!);
        using var csvWriter = new CsvWriter(writer, writeConfig);
        csvWriter.Context.RegisterClassMap<TripDataMap>();
        csvWriter.WriteRecords(removed);
        
        var removedCount = tripData.RemoveAll(x => x.PassengerCount == null 
                            || x.FareAmount < 0 
                            || x.TipAmount < 0);
        
        _logger.Information("Removed {Count} records with missing or invalid data", removedCount);
    }

    public List<TripData> RemoveDuplicates(List<TripData> records)
    {
        var groupedRecords = records
            .GroupBy(x => new
            {
                x.PickupDatetime,
                x.DropoffDatetime,
                x.PassengerCount
            })
            .ToList();

        var distinct = groupedRecords
            .Select(x => x.First())
            .ToList();
        var duplicates = groupedRecords
            .Where(x => x.Count() > 1)
            .SelectMany(x => x.Skip(1))
            .ToList();

        var writeConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };
        using var writer = new StreamWriter(_configuration["ETLSettings:DuplicatesFilePath"]!);
        using var csvWriter = new CsvWriter(writer, writeConfig);
        csvWriter.Context.RegisterClassMap<TripDataMap>();
        csvWriter.WriteRecords(duplicates);
        
        _logger.Information("Removed {Count} duplicate records", duplicates.Count);

        return distinct;
    }

    public void TransformData(List<TripData> records)
    {
        var easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        foreach (var record in records)
        {
            record.StoreAndFwdFlag = record.StoreAndFwdFlag == "Y" ? "Yes" : "No";
            record.PickupDatetime = TimeZoneInfo.ConvertTimeToUtc(record.PickupDatetime, easternTimeZone);
            record.DropoffDatetime = TimeZoneInfo.ConvertTimeToUtc(record.DropoffDatetime, easternTimeZone);
        }
        
        _logger.Information("Transformed data");
    }

    public void BulkInsertIntoDatabase(List<TripData> records)
    {
        var dataTable = ConvertToDataTable(records);

        using var connection = new SqlConnection(_configuration["ETLSettings:ConnectionString"]!);
        connection.Open();

        using var bulkCopy = new SqlBulkCopy(_configuration["ETLSettings:ConnectionString"]!);
        bulkCopy.DestinationTableName = "TripData";
        
        bulkCopy.ColumnMappings.Add("PickupDatetime", "PickupDatetime");
        bulkCopy.ColumnMappings.Add("DropoffDatetime", "DropoffDatetime");
        bulkCopy.ColumnMappings.Add("PassengerCount", "PassengerCount");
        bulkCopy.ColumnMappings.Add("TripDistance", "TripDistance");
        bulkCopy.ColumnMappings.Add("StoreAndFwdFlag", "StoreAndFwdFlag");
        bulkCopy.ColumnMappings.Add("PuLocationId", "PuLocationId");
        bulkCopy.ColumnMappings.Add("DoLocationId", "DoLocationId");
        bulkCopy.ColumnMappings.Add("FareAmount", "FareAmount");
        bulkCopy.ColumnMappings.Add("TipAmount", "TipAmount");
        
        bulkCopy.WriteToServer(dataTable);
        
        _logger.Information("Inserted {Count} records into database", records.Count);
    }

    private static DataTable ConvertToDataTable(List<TripData> records)
    {
        var table = new DataTable();
        table.Columns.Add("PickupDatetime", typeof(DateTime));
        table.Columns.Add("DropoffDatetime", typeof(DateTime));
        table.Columns.Add("PassengerCount", typeof(int));
        table.Columns.Add("TripDistance", typeof(double));
        table.Columns.Add("StoreAndFwdFlag", typeof(string));
        table.Columns.Add("PuLocationId", typeof(int));
        table.Columns.Add("DoLocationId", typeof(int));
        table.Columns.Add("FareAmount", typeof(decimal));
        table.Columns.Add("TipAmount", typeof(decimal));

        foreach (var record in records)
        {
            table.Rows.Add(
                record.PickupDatetime,
                record.DropoffDatetime,
                record.PassengerCount,
                record.TripDistance,
                record.StoreAndFwdFlag,
                record.PuLocationId,
                record.DoLocationId,
                record.FareAmount,
                record.TipAmount
            );
        }

        return table;
    }
}