namespace AspNetCoreIdentity.Core.ViewModels
{
    public class UserViewModel
    {
        // null! null olmayacak demek
        public string UserName { get; set; } 
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? PictureUrl { get; set; }
    }
}
