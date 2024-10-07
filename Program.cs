using System;
using System.Timers;
using System.Threading.Tasks;
using TestTask;
using Timer = System.Timers.Timer;


// Check if the correct number of arguments are provided

string sourcePath;
string replicaPath;
string logFilePath;
double syncInterval;

// Check if the correct number of arguments are provided
if (args.Length != 4)
{
    Console.WriteLine("Usage: FolderSynchronizer <sourcePath> <replicaPath> <logFilePath> <syncIntervalInSeconds>");

    // Prompt for user input
    Console.Write("Enter Source Path: ");
    sourcePath = Console.ReadLine();

    Console.Write("Enter Replica Path: ");
    replicaPath = Console.ReadLine();

    Console.Write("Enter Log File Path: ");
    logFilePath = Console.ReadLine();

    Console.Write("Enter Sync Interval in Seconds: ");
    while (!double.TryParse(Console.ReadLine(), out syncInterval) || syncInterval <= 0)
    {
        Console.WriteLine("Invalid sync interval. Please provide a valid number of seconds.");
    }
}
else
{
    sourcePath = args[0];
    replicaPath = args[1];
    logFilePath = args[2];

    if (!double.TryParse(args[3], out syncInterval) || syncInterval <= 0)
    {
        Console.WriteLine("Invalid sync interval. Please provide a valid number of seconds.");
        return;
    }
}


// Create a new LogService instance to handle logging
var logService = new LogService(logFilePath);

// Create a new SyncService instance to handle synchronization
var syncService = new SyncService(sourcePath, replicaPath, logService);

// Set up a Timer to trigger synchronization at the specified interval
var syncTimer = new Timer(syncInterval * 1000); // Convert seconds to milliseconds
syncTimer.Elapsed += async (sender, e) => await SyncFoldersAsync(syncService);
syncTimer.AutoReset = true;
syncTimer.Enabled = true;

Console.WriteLine($"Synchronization started. Interval: {syncInterval} seconds");
logService.Log("Synchronization started.");

// Perform an initial synchronization before starting periodic synchronization
await SyncFoldersAsync(syncService);

// Keep the application running to allow periodic synchronization
Console.ReadLine();

// Asynchronous method to handle synchronization and logging
async Task SyncFoldersAsync(SyncService syncService)
{
    try
    {
        // Perform folder synchronization
        await syncService.SynchronizeFoldersAsync();
        syncService.LogService.Log("Synchronization completed.");
    }
    catch (Exception ex)
    {
        // Log any errors that occur during synchronization
        syncService.LogService.Log($"Error during synchronization: {ex.Message}");
    }
}
