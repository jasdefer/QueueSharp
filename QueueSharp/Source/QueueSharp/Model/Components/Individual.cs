namespace QueueSharp.Model.Components;
public class Individual
{
    public required int Id { get; init; }
    public required Cohort Cohort { get; set; }
}
