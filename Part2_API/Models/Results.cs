namespace poe11.Models
{
    public class Results
    {
        public int ResultId { get; set; }
        public int EntranceId { get; set; }
        public RaceEntrance? RaceEntrance { get; set; }
        public string FinishStatus { get; set; } = "Finished";
        public int? OverallRank { get; set; }
        public int? CategoryRank { get; set; }
        public int? AgeGroupRank { get; set; }
    }
}
