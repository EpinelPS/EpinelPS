// From: https://github.com/Ninka-Rex/CSharp-Search-and-Replace/blob/main/SearchReplace.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServerSelector;

public static class PatchUtility
{
    public static bool CanFindOffset(string filePath, string[] searchPatterns)
    {
        // Check if the file exists
        if (!File.Exists(filePath)) { Console.WriteLine("[ERROR] File not found: " + filePath); return false; }

        // Read the binary data from the file as an array of bytes
        byte[] fileData = File.ReadAllBytes(filePath);

        for (int k = 0; k < searchPatterns.Length; k++)
        {
            // Convert the hexadecimal strings to byte arrays using a helper method
            byte[] searchBytes = HexStringToBytes(searchPatterns[k]);

            // Find the index of the first occurrence of the search pattern in the file data using another helper method
            var results = FindPatternIndex(fileData, searchBytes);
            if (results.Count <= 1) return false;

            Console.WriteLine("offset: " + results[1].ToString("X"));

            // If the index is -1, it means the pattern was not found, so we return false and log an error message
            if (results[1] == -1)
            {
                Console.WriteLine("[ERROR] Search pattern not found: " + searchPatterns[k]);
                return false;
            }
        }
        return true;
    }
    // This method searches and replaces binary patterns in a given file
    // It takes three parameters:
    // - filePath: the path of the file to be patched
    // - searchPatterns: an array of hexadecimal strings representing the patterns to be searched for
    // - replacePatterns: an array of hexadecimal strings representing the patterns to be replaced with
    // It returns true if the patching was successful, or false if there was an error or a pattern was not found
    public static bool SearchAndReplace(string filePath, string[] searchPatterns, string[] replacePatterns)
    {
        try
        {
            // Check if the file exists
            if (!File.Exists(filePath)) { Console.WriteLine("[ERROR] File not found: " + filePath); return false; }

            // Read the binary data from the file as an array of bytes
            byte[] fileData = File.ReadAllBytes(filePath);

            // Backup the original file by copying it to a new file with a .bak extension
            string backupFilePath = filePath + ".bak";
            if (!File.Exists(backupFilePath))
            {
                File.Copy(filePath, backupFilePath);
            }

            // Loop through each pair of search and replace patterns and apply them to the file data
            for (int k = 0; k < searchPatterns.Length; k++)
            {
                // Convert the hexadecimal strings to byte arrays using a helper method
                byte[] searchBytes = HexStringToBytes(searchPatterns[k]);
                byte[] replaceBytes = HexStringToBytes(replacePatterns[k]);

                // Find the index of the first occurrence of the search pattern in the file data using another helper method
                int index = FindPatternIndex(fileData, searchBytes)[1];

                Console.WriteLine("offset: " + index.ToString("X"));

                // If the index is -1, it means the pattern was not found, so we return false and log an error message
                if (index == -1)
                {
                    Console.WriteLine("[ERROR] Search pattern not found: " + searchPatterns[k]);
                    return false;
                }

                // Replace the pattern at the found index with the replace pattern, preserving original values when wildcards are encountered
                // A wildcard is represented by either 00 or FF in the replace pattern, meaning that we keep the original value at that position
                for (int i = 0; i < replaceBytes.Length; i++)
                {
                    if (replaceBytes[i] != 0x00 && replaceBytes[i] != 0xFF)
                    {
                        fileData[index + i] = replaceBytes[i];
                    }
                    else if (replaceBytes[i] == 0x00)
                    {
                        fileData[index + i] = 0x00;
                    }
                }

                // Log a success message with the offset and file name where the patch was applied
                string exeName = Path.GetFileName(filePath);
                Console.WriteLine($"[Patch] Apply patch success at 0x{index:X} in {exeName}");
            }

            // Write the modified data back to the file, overwriting the original content
            File.WriteAllBytes(filePath, fileData);

            return true;
        }
        catch (Exception ex)
        {
            // If any exception occurs during the patching process, we return false and log an error message with the exception details
            Console.WriteLine("[ERROR] An error occurred while writing the file: " + ex.Message);
            return false;
        }
    }

    // This helper method converts a hexadecimal string to a byte array
    // It takes one parameter:
    // - hex: a string of hexadecimal digits, optionally separated by spaces or question marks
    // It returns a byte array corresponding to the hexadecimal values in the string
    private static byte[] HexStringToBytes(string hex)
    {
        hex = hex.Replace(" ", "").Replace("??", "FF"); // Replace ?? with FF for wildcards
        return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0) // Take every second character in the string
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16)) // Convert each pair of characters to a byte value in base 16
            .ToArray(); // Convert the result to an array of bytes
    }

    // This helper method finds the index of the first occurrence of a pattern in a data array
    // It takes two parameters:
    // - data: an array of bytes representing the data to be searched in
    // - pattern: an array of bytes representing the pattern to be searched for
    // It returns an integer representing the index where the pattern was found, or -1 if it was not found
    private static List<int> FindPatternIndex(byte[] data, byte[] pattern)
    {
        List<int> points = [];
        // Loop through each possible position in the data array where the pattern could start
        for (int i = 0; i < data.Length - pattern.Length + 1; i++)
        {
            bool found = true; // Assume that the pattern is found until proven otherwise
            // Loop through each byte in the pattern and compare it with the corresponding byte in the data array
            for (int j = 0; j < pattern.Length; j++)
            {
                // If the pattern byte is not FF (wildcard) and it does not match the data byte, then the pattern is not found at this position
                if (pattern[j] != 0xFF && data[i + j] != pattern[j])
                {
                    found = false;
                    break;
                }
            }
            // If the pattern was found at this position, return the index
            if (found)
            {
                points.Add(i);
            }
        }
        // If the pattern was not found in the entire data array, return -1
        return points;
    }
}