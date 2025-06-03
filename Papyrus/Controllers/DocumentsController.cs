using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Papyrus.Data;
using Papyrus.Models;

namespace Papyrus.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DocumentsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Documents
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Подключаем связанные данные (Author и Status) и фильтруем по AuthorId
            var documents = await _context.Documents
                .Include(d => d.Author)
                .Include(d => d.Status)
                .Where(d => d.AuthorId == userId)
                .ToListAsync();

            return View(documents);
        }

        // GET: Documents/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Author)
                .Include(d => d.Status)
                .Include(d => d.Routes).ThenInclude(r => r.Approver)
                .Include(d => d.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // GET: Documents/Create
        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id");
            ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Id");
            return View();
        }

        // POST: Documents/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UploadDocumentViewModel model)
        {
            /*
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Проверьте данные.";
                return RedirectToAction("Index");
            }
            */
            try
            {
                // 0. Проверка авторизации.
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Вы не авторизованы.";
                    return RedirectToAction("Login", "Account");
                }

                var file = model.File;
                // 1. Валидация: файл должен быть выбран
                if (file == null || file.Length == 0)
                {
                    TempData["ErrorMessage"] = "Файл не выбран.";
                    return RedirectToAction("Index");
                }

                // 2. Проверка формата файла
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".txt", ".doc", ".xls" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Недопустимый тип файла.";
                    return RedirectToAction("Index");
                }

                // 3. Создание папки для хранения файлов, если её нет
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "documents");


                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 4. Уникальное имя файла
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 5. Сохранение файла на диск
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 6. Создание объекта Document
                var document = new Document
                {
                    Name = string.IsNullOrWhiteSpace(model.Name) ? file.FileName : model.Name,
                    Description = model.Description,
                    FilePath = "/documents/" + uniqueFileName,
                    FileSize = file.Length,
                    StatusId = 1, // На согласовании
                    AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    CreatedDate = DateTime.Now
                };

                // 7. Сохранение в БД
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                // 8. Перенаправление
                TempData["SuccessMessage"] = "Файл успешно загружен.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // 9. Логирование и обработка ошибок
                TempData["ErrorMessage"] = $"Ошибка при загрузке файла: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Documents/Download
        public async Task<IActionResult> Download(int id)
        {
            var document = await _context.Documents.FindAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, document.FilePath.TrimStart('/'));
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(document.FilePath), Path.GetFileName(document.FilePath));
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word.document.12"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            };
        }

        // GET: Documents/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", document.AuthorId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Id", document.StatusId);
            return View(document);
        }

        // POST: Documents/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,FilePath,StatusId,AuthorId,CreatedDate")] Document document)
        {
            if (id != document.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(document);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentExists(document.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", document.AuthorId);
            ViewData["StatusId"] = new SelectList(_context.Statuses, "Id", "Id", document.StatusId);
            return View(document);
        }

        // GET: Documents/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.Documents
                .Include(d => d.Author)
                .Include(d => d.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (document == null)
            {
                return NotFound();
            }

            return View(document);
        }

        // POST: Documents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Documents/Approver
        [HttpPost]
        [Authorize(Roles = "Approver")]
        public async Task<IActionResult> Approve(int id, bool isApproved)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Вы не авторизованы.";
                return RedirectToAction("Login", "Account");
            }
            var route = _context.Routes
                .Where(r => r.DocumentId == id && !r.IsApproved)
                .OrderBy(r => r.Order)
                .FirstOrDefault(r => r.ApproverId == userId);

            if (route == null) return Forbid();

            route.IsApproved = isApproved;
            route.ApprovalDate = DateTime.Now;

            // Если последний этап — меняем статус документа
            var nextRoute = _context.Routes
                .OrderBy(r => r.Order)
                .FirstOrDefault(r => r.DocumentId == id && r.Order > route.Order);

            if (nextRoute == null)
            {
                var doc = await _context.Documents.FindAsync(id);
                doc.StatusId = isApproved ? 2 : 3; // 2 - Одобрен, 3 - Отклонен
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool DocumentExists(int id)
        {
            return _context.Documents.Any(e => e.Id == id);
        }
    }
}
