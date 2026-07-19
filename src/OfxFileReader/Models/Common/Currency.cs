namespace OfxFileReader.Models.Common;

public sealed record Currency(string Code, decimal? Rate = null);
