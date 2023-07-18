using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetCoreIdentity.Web.TagHelpers
{
    // class'ın sonu TagHelper ile bitmesi önemli miras alan sınıfın TagHelper olarak algılaması için
    // TagHelper _ViewImports klasörüne eklenir.
    public class UserPictureThumbnailTagHelper:TagHelper
    {
        public string? PictureUrl { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "img";

            if(string.IsNullOrEmpty(PictureUrl) )
            {
                output.Attributes.SetAttribute("src", "/userpictures/default_user_picture.JPG");
            }
            else
            {
                output.Attributes.SetAttribute("src", $"/userpictures/{PictureUrl}");
            }


            
        }
    }
}
