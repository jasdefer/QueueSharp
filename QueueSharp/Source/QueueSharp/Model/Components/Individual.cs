namespace QueueSharp.Model.Components;
internal class Individual
{
    public required int Id { get; init; }
    public required Cohort Cohort { get; set; }
}
