using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Papyrus.Models
{
    public class UploadDocumentViewModel
    {
        [Required(ErrorMessage = "Файл обязателен")]
        public IFormFile File { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
