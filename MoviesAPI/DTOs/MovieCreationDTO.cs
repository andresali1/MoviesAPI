using Microsoft.AspNetCore.Mvc;
using MoviesAPI.Helpers;
using MoviesAPI.Validations;

namespace MoviesAPI.DTOs
{
    public class MovieCreationDTO : MoviePatchDTO
    {
        [FileSizeValidation(maxSizeInMegabytes: 4)]
        [FileTypeValidation(fileTypeGroup: FileTypeGroup.Image)]
        public IFormFile Poster { get; set; }

        [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
        public List<int> GenreIds { get; set; }

        [ModelBinder(BinderType = typeof(TypeBinder<List<ActorMovieCreationDTO>>))]
        public List<ActorMovieCreationDTO> Actors { get; set; }
    }
}
