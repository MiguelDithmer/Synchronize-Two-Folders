using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestTask
{
    internal class SyncService
    {
        public string SourcePath { get; }
        public string ReplicaPath { get; }
        public LogService LogService { get; }

        public SyncService(string sourcePath, string replicaPath, LogService logService)
        {
            SourcePath = sourcePath;
            ReplicaPath = replicaPath;
            LogService = logService;
        }

        // Main method to synchronize folders
        public async Task SynchronizeFoldersAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Start folder synchronization and log the action
                    LogService.Log($"Starting synchronization from {SourcePath} to {ReplicaPath}");
                    SyncDirectory(SourcePath, ReplicaPath);
                    LogService.Log($"Synchronization from {SourcePath} to {ReplicaPath} completed.");

                    // Clean up extra files and directories and log the action
                    LogService.Log($"Starting cleanup of extra files and directories in {ReplicaPath}");
                    CleanExtraFiles(ReplicaPath, SourcePath);
                    LogService.Log($"Cleanup of extra files and directories in {ReplicaPath} completed.");
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during synchronization
                    LogService.Log($"Error during synchronization: {ex.Message}");
                }
            });
        }

        //  synchronize files and directories from source to replica
        private void SyncDirectory(string sourceDir, string replicaDir)
        {
            // Ensure the replica directory exists and log the creation
            if (!Directory.Exists(replicaDir))
            {
                Directory.CreateDirectory(replicaDir);
                LogService.Log($"Directory created: {replicaDir}");
            }

            // Synchronize files from source to replica
            foreach (var sourceFile in Directory.GetFiles(sourceDir))
            {
                var fileName = Path.GetFileName(sourceFile);
                var destFile = Path.Combine(replicaDir, fileName);

                try
                {
                    // Check if the file needs to be copied (does not exist in replica or is different)
                    if (!File.Exists(destFile))
                    {
                        File.Copy(sourceFile, destFile, true);
                        LogService.Log($"File created: {destFile} (from {sourceFile})");
                    }
                    else if (!FilesAreEqual(sourceFile, destFile))
                    {
                        File.Copy(sourceFile, destFile, true);
                        LogService.Log($"File updated: {destFile} (from {sourceFile})");
                    }
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during file copy
                    LogService.Log($"Error copying file {sourceFile} to {destFile}: {ex.Message}");
                }
            }

            // synchronize subdirectories
            foreach (var sourceSubDir in Directory.GetDirectories(sourceDir))
            {
                var dirName = Path.GetFileName(sourceSubDir);
                var destSubDir = Path.Combine(replicaDir, dirName);
                SyncDirectory(sourceSubDir, destSubDir);
            }
        }

        // Cleans up extra files and directories in the replica that are not present in the source
        private void CleanExtraFiles(string replicaDir, string sourceDir)
        {
            // Delete files in the replica that are not in the source
            foreach (var replicaFile in Directory.GetFiles(replicaDir))
            {
                var fileName = Path.GetFileName(replicaFile);
                var sourceFile = Path.Combine(sourceDir, fileName);

                if (!File.Exists(sourceFile))
                {
                    try
                    {
                        File.Delete(replicaFile);
                        LogService.Log($"File deleted: {replicaFile}");
                    }
                    catch (Exception ex)
                    {
                        // Log any errors that occur during file deletion
                        LogService.Log($"Error deleting file {replicaFile}: {ex.Message}");
                    }
                }
            }

            // delete directories in the replica that are not in the source
            foreach (var replicaSubDir in Directory.GetDirectories(replicaDir))
            {
                var dirName = Path.GetFileName(replicaSubDir);
                var sourceSubDir = Path.Combine(sourceDir, dirName);

                if (!Directory.Exists(sourceSubDir))
                {
                    try
                    {
                        Directory.Delete(replicaSubDir, true);
                        LogService.Log($"Directory deleted: {replicaSubDir}");
                    }
                    catch (Exception ex)
                    {
                        // Log any errors that occur during directory deletion
                        LogService.Log($"Error deleting directory {replicaSubDir}: {ex.Message}");
                    }
                }
                else
                {
                    CleanExtraFiles(replicaSubDir, sourceSubDir);
                }
            }

            /* // Delete any empty directories in the replica
             foreach (var replicaSubDir in Directory.GetDirectories(replicaDir))
             {
                 if (Directory.GetFiles(replicaSubDir).Length == 0 && Directory.GetDirectories(replicaSubDir).Length == 0)
                 {
                     try
                     {
                         Directory.Delete(replicaSubDir, true);
                         LogService.Log($"Deleted empty directory: {replicaSubDir}");
                     }
                     catch (Exception ex)
                     {
                         // Log any errors that occur during empty directory deletion
                         LogService.Log($"Error deleting empty directory {replicaSubDir}: {ex.Message}");
                     }
                 }
             }*/
        }

        // Compares two files to see if they are equal by comparing their hashes
        private bool FilesAreEqual(string filePath1, string filePath2)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                {
                    byte[] hash1 = GetFileHash(sha256, filePath1);
                    byte[] hash2 = GetFileHash(sha256, filePath2);
                    return hash1.SequenceEqual(hash2);
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during file comparison
                LogService.Log($"Error comparing files {filePath1} and {filePath2}: {ex.Message}");
                return false;
            }
        }

        // Computes the hash of a file using the specified hash algorithm
        private byte[] GetFileHash(HashAlgorithm hashAlgorithm, string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }
    }
}
