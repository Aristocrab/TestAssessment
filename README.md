# TestAssessment

## Main steps
- Read all data from csv
- Clean data
- Remove duplicates
- Insert into db

## Result
- Read `30000` records from CSV
- Removed `145` records with missing or invalid data
- Removed `15` duplicate records
- Inserted `29840` records into database

## Possible changes for a 10GB input file
- Multithreading for some tasks
- Streaming approach instead of loading the entire CSV into memory
- Batch inserts

## Used technologies
- C#
- CsvHelper
- Microsoft.Data.SqlClient
- Microsoft.Extensions.Configuration
- Serilog
