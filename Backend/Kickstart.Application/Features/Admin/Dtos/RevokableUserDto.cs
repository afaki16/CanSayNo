namespace Kickstart.Application.Features.Admin.Dtos
{
    public class RevokableUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
