using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Validations
{
    public class FileSizeValidation : ValidationAttribute
    {
        private readonly int _maxSizeInMegabytes;

        public FileSizeValidation(int maxSizeInMegabytes)
        {
            _maxSizeInMegabytes = maxSizeInMegabytes;
        }

        /// <summary>
        /// Method to validate the size of the file
        /// </summary>
        /// <param name="value">File sended in the form</param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            IFormFile formFile = value as IFormFile;

            if(formFile == null)
            {
                return ValidationResult.Success;
            }

            if(formFile.Length > _maxSizeInMegabytes * 1024 * 1024)
            {
                return new ValidationResult($"El peso del archivo no debe se superior a {_maxSizeInMegabytes}mb");
            }

            return ValidationResult.Success;
        }
    }
}
