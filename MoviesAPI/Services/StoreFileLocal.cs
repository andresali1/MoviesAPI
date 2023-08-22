using MoviesAPI.Interfaces;

namespace MoviesAPI.Services
{
    public class StoreFileLocal : IStoreFile
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StoreFileLocal(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Method to delete a file from the application wwwroot folder
        /// </summary>
        /// <param name="path">path of the file</param>
        /// <param name="container">name of the folder</param>
        /// <returns></returns>
        public Task DeleteFile(string path, string container)
        {
            if (path != null)
            {
                var fileName = Path.GetFileName(path);
                string fileFolder = Path.Combine(_env.WebRootPath, container, fileName);

                if (File.Exists(fileFolder))
                {
                    File.Delete(fileFolder);
                }

            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Method to delete the previous file and save anew one in the application wwwroot
        /// </summary>
        /// <param name="content">File in bytes</param>
        /// <param name="extension">File extension</param>
        /// <param name="container">Folder name</param>
        /// <param name="path">path of the previous folder</param>
        /// <param name="contentType">File content type</param>
        /// <returns></returns>
        public async Task<string> EditFile(byte[] content, string extension, string container, string path, string contentType)
        {
            await DeleteFile(path, container);
            return await SaveFile(content, extension, container, contentType);
        }

        /// <summary>
        /// Method to save an image in the wwwroot application folder
        /// </summary>
        /// <param name="content">File in bytes</param>
        /// <param name="extension">File extension</param>
        /// <param name="container">Folder Name</param>
        /// <param name="contentType">File content type</param>
        /// <returns></returns>
        public async Task<string> SaveFile(byte[] content, string extension, string container, string contentType)
        {
            var fileName = $"{Guid.NewGuid()}{extension}";
            string folder = Path.Combine(_env.WebRootPath, container);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string path = Path.Combine(folder, fileName);
            await File.WriteAllBytesAsync(path, content);

            var currentUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
            var dataBaseUrl = Path.Combine(currentUrl, container, fileName).Replace("\\", "/");

            return dataBaseUrl;
        }
    }
}
