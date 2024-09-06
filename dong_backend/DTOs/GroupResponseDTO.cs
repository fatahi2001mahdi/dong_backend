namespace dong_backend.DTOs
{
    public class GroupResponseDTO
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
