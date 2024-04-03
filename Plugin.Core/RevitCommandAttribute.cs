using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Plugin.Core;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RevitCommandAttribute(int order, string panel, string ribbon, string imageUri, string commandName, string buttonText) : Attribute
{
    public int Order { get; } = order;
    public string Panel { get; } = panel;
    public string Ribbon { get; } = ribbon;
    public string ImageUri { get; } = imageUri;
    public string CommandName { get; } = commandName;
    public string ButtonText { get; } = buttonText;
}

public static class StringHelper
{
    private static readonly Regex NonNumericRegex = new("[^0-9]+", RegexOptions.Compiled);

    public static List<long> GetLongs(string selectedItemString)
    {
        return selectedItemString
            .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .Select(line => NonNumericRegex.Replace(line, "")) // Replace non-numeric characters with empty string
            .Where(str => !string.IsNullOrEmpty(str)) // Ensure the string is not empty after replacing
            .Select(long.Parse) // Now it will only contain numbers, so it's safe to parse
            .ToList();

    }
}