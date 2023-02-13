namespace Cowboys.Contracts;

public record GameResultsDto
{
    public string Status { get; set; }
    public IEnumerable<string> Logs { get; set; }
}