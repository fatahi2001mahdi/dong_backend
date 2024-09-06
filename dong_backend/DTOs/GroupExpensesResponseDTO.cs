namespace dong_backend.DTOs
{
    public class GroupExpensesResponseDTO
    {
        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime AddedAt { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public decimal ShareAmount { get; set; }
        public int Status { get; set; }
    }
}
