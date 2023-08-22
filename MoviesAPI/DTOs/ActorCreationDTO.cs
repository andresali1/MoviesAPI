using MoviesAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.DTOs
{
    public class ActorCreationDTO
    {
        [Required]
        [StringLength(120)]
        public string A_Name { get; set; }

        public DateTime BirthDate { get; set; }

        [FileSizeValidation(maxSizeInMegabytes: 4)]
        [FileTypeValidation(fileTypeGroup: FileTypeGroup.Image)]
        public IFormFile Photo { get; set; }
    }
}
