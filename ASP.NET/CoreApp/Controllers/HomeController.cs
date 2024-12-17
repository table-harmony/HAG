using ImageProcessing.Services;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace CoreApp.Controllers;

public class HomeController : Controller {
    public IActionResult Index() {
        return View();
    }

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> Convert(IFormFile file, string targetFormat) {
        if (file == null || file.Length == 0) {
            TempData["Error"] = "Please select a file";
            return RedirectToAction("Index");
        }

        try {
            var originalExtension = Path.GetExtension(file.FileName);

            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{originalExtension}");

            using (var stream = System.IO.File.Create(tempPath)) {
                await file.CopyToAsync(stream);
            }

            var outputFormat = ImageConverter.GetFormatFromExtension($"dummy.{targetFormat}");

            var outputPath = ImageConverter.Convert(
                tempPath,
                outputFormat
            );

            var memory = new MemoryStream();
            using (var stream = new FileStream(outputPath, FileMode.Open)) {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            System.IO.File.Delete(tempPath);
            System.IO.File.Delete(outputPath);

            return File(memory, "application/octet-stream", $"converted.{targetFormat}");
        } catch (Exception ex) {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
}
