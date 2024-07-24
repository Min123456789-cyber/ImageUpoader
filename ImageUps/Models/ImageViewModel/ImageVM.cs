using System.ComponentModel.DataAnnotations;

namespace ImageUps.Models.ImageViewModel
{
    public class ImageVM
    {

        public string Title { get; set; }

        public string Description { get; set; }

        public IFormFile? ImageUrl { get; set; }
    }
}
