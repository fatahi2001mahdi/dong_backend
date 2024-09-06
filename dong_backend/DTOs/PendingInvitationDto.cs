namespace dong_backend.DTOs
{
    public class PendingInvitationDto
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public string InvitedByEmail { get; set; }
    }
}
