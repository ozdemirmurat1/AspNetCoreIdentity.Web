using System.ComponentModel.DataAnnotations;

namespace AspNetCoreIdentity.Web.ViewModels
{
    public class UserEditViewModel
    {

        [Required(ErrorMessage = "Kullanıcı Ad alanı boş bırakılamaz.")]
        [Display(Name = "Kullanıcı Adı :")]
        public string UserName { get; set; }

        [EmailAddress(ErrorMessage = "Email formatı yanlıştır.")]
        [Required(ErrorMessage = "Email alanı boş bırakılamaz.")]
        [Display(Name = "Email :")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon alanı boş bırakılamaz.")]
        [Display(Name = "Telefon :")]
        public string Phone { get; set; }

        
        [Display(Name = "Doğum Tarihi :")]
        public string BirthDate { get; set; }

        [Display(Name = "Şehir :")]
        public string City { get; set; }

        [Display(Name = "Profil Resmi :")]
        public IFormFile? Picture { get; set; }

        [Display(Name = "Cinsiyet :")]
        public byte? Gender { get; set; }

    }
}
