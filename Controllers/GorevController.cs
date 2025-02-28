using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjeYonetimTakipSistem.Data;
using ProjeYonetimTakipSistem.Models;
using Microsoft.Extensions.Logging;

namespace ProjeYonetimTakipSistem.Controllers
{
    [Authorize]
    public class GorevController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GorevController> _logger;

        public GorevController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<GorevController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // Görevleri Listele
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            
            if (await _userManager.IsInRoleAsync(user!, "Admin"))
            {
                // Admin tüm görevleri görebilir
                return View(await _context.Gorevler
                    .Include(g => g.Proje)
                    .Include(g => g.AtananKullanici)
                    .ToListAsync());
            }

            // Normal kullanıcı sadece kendisine atanan görevleri görebilir
            var kullaniciGorevleri = await _context.Gorevler
                .Include(g => g.Proje)
                .Include(g => g.AtananKullanici)
                .Where(g => g.AtananKullaniciId == user.Id)
                .ToListAsync();

            return View(kullaniciGorevleri);
        }

        // Görev Detayı
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gorev = await _context.Gorevler
                .Include(g => g.Proje)
                .Include(g => g.AtananKullanici)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gorev == null)
            {
                return NotFound();
            }

            // Kullanıcının yetkisi kontrol edilir
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            var isAdmin = await _userManager.IsInRoleAsync(currentUser!, "Admin");
            var isAssigned = gorev.AtananKullaniciId == currentUser.Id;

            if (!isAdmin && !isAssigned)
            {
                return Forbid();
            }

            return View(gorev);
        }

        // Yeni Görev Oluşturma Formu
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? projeId = null)
        {
            var projeler = await _context.Projeler.ToListAsync();
            ViewBag.ProjeId = new SelectList(projeler, "Id", "Ad", projeId);
            
            // Başlangıçta boş kullanıcı listesi
            ViewBag.AtananKullaniciId = new SelectList(new List<ApplicationUser>(), "Id", "Email");
            
            // Durum ve Öncelik seçeneklerini hazırla
            var durumlar = Enum.GetValues(typeof(GorevDurumu))
                .Cast<GorevDurumu>()
                .Select(d => new { Id = (int)d, Name = d.ToString() });
            ViewBag.Durum = new SelectList(durumlar, "Id", "Name");

            var oncelikler = Enum.GetValues(typeof(GorevOnceligi))
                .Cast<GorevOnceligi>()
                .Select(o => new { Id = (int)o, Name = o.ToString() });
            ViewBag.Oncelik = new SelectList(oncelikler, "Id", "Name");

            var gorev = new Gorev
            {
                BaslangicTarihi = DateTime.Today,
                BitisTarihi = DateTime.Today.AddDays(7),
                Durum = GorevDurumu.Baslamadi,
                Oncelik = GorevOnceligi.Orta
            };

            if (projeId.HasValue)
            {
                gorev.ProjeId = projeId.Value;
                // Eğer proje ID varsa, o projenin kullanıcılarını getir
                var projeKullanicilari = await _context.ProjeKullanicilar
                    .Include(pk => pk.Kullanici)
                    .Where(pk => pk.ProjeId == projeId.Value)
                    .Select(pk => pk.Kullanici)
                    .ToListAsync();
                ViewBag.AtananKullaniciId = new SelectList(projeKullanicilari, "Id", "Email");
            }

            return View(gorev);
        }

        // Yeni Görev Oluştur
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm][Bind("Id,Ad,Aciklama,BaslangicTarihi,BitisTarihi,Durum,Oncelik,ProjeId,AtananKullaniciId")] Gorev gorev)
        {
            try
            {
                _logger.LogInformation("Görev oluşturma isteği alındı: {@Gorev}", gorev);

                // Form verilerini logla
                foreach (var key in Request.Form.Keys)
                {
                    _logger.LogInformation("Form verisi: {Key} = {Value}", key, Request.Form[key].ToString());
                }

                // Model durumunu kontrol et ve hataları logla
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model durumu geçersiz");
                    foreach (var modelState in ModelState)
                    {
                        foreach (var error in modelState.Value.Errors)
                        {
                            _logger.LogWarning("Validation hatası - {Key}: {Error}", modelState.Key, error.ErrorMessage);
                        }
                    }
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Geçersiz form verileri.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
                    }
                    
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                // Proje kontrolü
                if (gorev.ProjeId <= 0)
                {
                    _logger.LogWarning("Geçersiz proje ID: {ProjeId}", gorev.ProjeId);
                    ModelState.AddModelError("ProjeId", "Proje seçimi zorunludur.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Proje seçimi zorunludur." });
                    }
                    
                    await PrepareCreateEditViewBag(null);
                    return View(gorev);
                }

                var proje = await _context.Projeler.FindAsync(gorev.ProjeId);
                if (proje == null)
                {
                    _logger.LogWarning("Proje bulunamadı: {ProjeId}", gorev.ProjeId);
                    ModelState.AddModelError("ProjeId", "Seçilen proje bulunamadı.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Seçilen proje bulunamadı." });
                    }
                    
                    await PrepareCreateEditViewBag(null);
                    return View(gorev);
                }

                // Kullanıcı kontrolü
                if (string.IsNullOrEmpty(gorev.AtananKullaniciId))
                {
                    _logger.LogWarning("Kullanıcı ID boş");
                    ModelState.AddModelError("AtananKullaniciId", "Kullanıcı seçimi zorunludur.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Kullanıcı seçimi zorunludur." });
                    }
                    
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                var kullanici = await _userManager.FindByIdAsync(gorev.AtananKullaniciId);
                if (kullanici == null)
                {
                    _logger.LogWarning("Kullanıcı bulunamadı: {KullaniciId}", gorev.AtananKullaniciId);
                    ModelState.AddModelError("AtananKullaniciId", "Seçilen kullanıcı bulunamadı.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Seçilen kullanıcı bulunamadı." });
                    }
                    
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                // Proje-Kullanıcı ilişkisi kontrolü
                var projeKullanici = await _context.ProjeKullanicilar
                    .AnyAsync(pk => pk.ProjeId == gorev.ProjeId && pk.KullaniciId == gorev.AtananKullaniciId);

                if (!projeKullanici)
                {
                    _logger.LogWarning("Kullanıcı projede bulunamadı: Proje={ProjeId}, Kullanıcı={KullaniciId}", 
                        gorev.ProjeId, gorev.AtananKullaniciId);
                    ModelState.AddModelError("AtananKullaniciId", "Seçilen kullanıcı bu projede yer almıyor.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Seçilen kullanıcı bu projede yer almıyor." });
                    }
                    
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                // Tarih validasyonları
                if (gorev.BaslangicTarihi.Date < DateTime.Today)
                {
                    _logger.LogWarning("Geçersiz başlangıç tarihi: {Tarih}", gorev.BaslangicTarihi);
                    ModelState.AddModelError("BaslangicTarihi", "Başlangıç tarihi bugünden önce olamaz.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Başlangıç tarihi bugünden önce olamaz." });
                    }
                    
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                if (gorev.BitisTarihi.Date < gorev.BaslangicTarihi.Date)
                {
                    _logger.LogWarning("Geçersiz bitiş tarihi: Başlangıç={Baslangic}, Bitiş={Bitis}", 
                        gorev.BaslangicTarihi, gorev.BitisTarihi);
                    ModelState.AddModelError("BitisTarihi", "Bitiş tarihi başlangıç tarihinden önce olamaz.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Bitiş tarihi başlangıç tarihinden önce olamaz." });
                    }
                    
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                // Durum kontrolü
                if (!Enum.IsDefined(typeof(GorevDurumu), gorev.Durum))
                {
                    _logger.LogWarning("Geçersiz durum değeri: {Durum}", gorev.Durum);
                    ModelState.AddModelError("Durum", "Geçerli bir durum seçiniz.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Geçerli bir durum seçiniz." });
                    }
                    
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                // Öncelik kontrolü
                if (!Enum.IsDefined(typeof(GorevOnceligi), gorev.Oncelik))
                {
                    _logger.LogWarning("Geçersiz öncelik değeri: {Oncelik}", gorev.Oncelik);
                    ModelState.AddModelError("Oncelik", "Geçerli bir öncelik seçiniz.");
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Geçerli bir öncelik seçiniz." });
                    }
                    
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                // Görevi oluştur
                try
                {
                    _context.Gorevler.Add(gorev);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Görev başarıyla oluşturuldu: {GorevId}", gorev.Id);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true, message = "Görev başarıyla oluşturuldu." });
                    }

                    TempData["Message"] = "Görev başarıyla oluşturuldu.";
                    TempData["MessageType"] = "success";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Görev oluşturulurken hata oluştu");
                    ModelState.AddModelError("", "Görev oluşturulurken bir hata oluştu: " + ex.Message);
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Görev oluşturulurken bir hata oluştu: " + ex.Message });
                    }
                    
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görev oluşturma işleminde beklenmeyen hata");
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Beklenmeyen bir hata oluştu: " + ex.Message });
                }
                
                await PrepareCreateEditViewBag(gorev.ProjeId);
                return View(gorev);
            }
        }

        // Görev Düzenleme Formu
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gorev = await _context.Gorevler.FindAsync(id);
            if (gorev == null)
            {
                return NotFound();
            }

            await PrepareCreateEditViewBag(gorev.ProjeId);
            return View(gorev);
        }

        // Görev Düzenle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Aciklama,BaslangicTarihi,BitisTarihi,Durum,ProjeId,AtananKullaniciId")] Gorev gorev)
        {
            if (id != gorev.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Proje ve kullanıcı kontrolü
                var projeKullanici = await _context.ProjeKullanicilar
                    .AnyAsync(pk => pk.ProjeId == gorev.ProjeId && pk.KullaniciId == gorev.AtananKullaniciId);

                if (!projeKullanici)
                {
                    ModelState.AddModelError("AtananKullaniciId", "Seçilen kullanıcı bu projede yer almıyor.");
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                // Tarih validasyonları
                if (gorev.BaslangicTarihi.Date < DateTime.Today)
                {
                    ModelState.AddModelError("BaslangicTarihi", "Başlangıç tarihi bugünden önce olamaz.");
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                if (gorev.BitisTarihi.Date < gorev.BaslangicTarihi.Date)
                {
                    ModelState.AddModelError("BitisTarihi", "Bitiş tarihi başlangıç tarihinden önce olamaz.");
                    await PrepareCreateEditViewBag(gorev.ProjeId);
                    return View(gorev);
                }

                try
                {
                    _context.Update(gorev);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GorevExists(gorev.Id))
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

            await PrepareCreateEditViewBag(gorev.ProjeId);
            return View(gorev);
        }

        // Görev Durumu Güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDurum(int id, GorevDurumu durum)
        {
            var gorev = await _context.Gorevler
                .Include(g => g.AtananKullanici)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gorev == null)
            {
                return NotFound();
            }

            // Kullanıcının yetkisi kontrol edilir
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            var isAdmin = await _userManager.IsInRoleAsync(currentUser!, "Admin");
            var isAssigned = gorev.AtananKullaniciId == currentUser.Id;

            if (!isAdmin && !isAssigned)
            {
                return Forbid();
            }

            // Durum güncellemesi yapılır
            gorev.Durum = durum;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Görev durumu başarıyla güncellendi.";
            return RedirectToAction(nameof(Details), new { id = gorev.Id });
        }

        // Görev Silme Onay Sayfası
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gorev = await _context.Gorevler
                .Include(g => g.Proje)
                .Include(g => g.AtananKullanici)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gorev == null)
            {
                return NotFound();
            }

            return View(gorev);
        }

        // Görev Sil
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gorev = await _context.Gorevler.FindAsync(id);
            if (gorev != null)
            {
                _context.Gorevler.Remove(gorev);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GorevExists(int id)
        {
            return _context.Gorevler.Any(e => e.Id == id);
        }

        // ViewBag hazırlama yardımcı metodu
        private async Task PrepareCreateEditViewBag(int? projeId = null)
        {
            // Admin tüm projeleri görebilir
            var projeler = await _context.Projeler.ToListAsync();
            ViewBag.ProjeId = new SelectList(projeler, "Id", "Ad", projeId);

            // Başlangıçta boş kullanıcı listesi
            ViewBag.AtananKullaniciId = new SelectList(new List<ApplicationUser>(), "Id", "Email");

            // Eğer proje seçiliyse, sadece o projenin üyelerini listele
            if (projeId.HasValue)
            {
                var projeKullanicilari = await _context.ProjeKullanicilar
                    .Include(pk => pk.Kullanici)
                    .Where(pk => pk.ProjeId == projeId.Value)
                    .Select(pk => pk.Kullanici)
                    .ToListAsync();

                ViewBag.AtananKullaniciId = new SelectList(projeKullanicilari, "Id", "Email");
            }

            // Durum ve Öncelik seçeneklerini hazırla
            var durumlar = Enum.GetValues(typeof(GorevDurumu))
                .Cast<GorevDurumu>()
                .Select(d => new { Id = (int)d, Name = d.ToString() });
            ViewBag.Durum = new SelectList(durumlar, "Id", "Name");

            var oncelikler = Enum.GetValues(typeof(GorevOnceligi))
                .Cast<GorevOnceligi>()
                .Select(o => new { Id = (int)o, Name = o.ToString() });
            ViewBag.Oncelik = new SelectList(oncelikler, "Id", "Name");
        }

        // Proje kullanıcılarını getir
        [HttpGet]
        public async Task<IActionResult> GetProjeKullanicilari(int projeId)
        {
            try
            {
                _logger.LogInformation("GetProjeKullanicilari çağrıldı: {ProjeId}", projeId);

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Kullanıcı bulunamadı");
                    return Json(new { success = false, message = "Oturum bulunamadı" });
                }

                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    _logger.LogWarning("Yetkisiz erişim denemesi: {UserId}", user.Id);
                    return Json(new { success = false, message = "Bu işlem için yetkiniz yok" });
                }

                if (projeId <= 0)
                {
                    _logger.LogWarning("Geçersiz proje ID: {ProjeId}", projeId);
                    return Json(new { success = false, message = "Geçersiz proje ID" });
                }

                _logger.LogInformation("Proje aranıyor: {ProjeId}", projeId);

                // Önce projenin varlığını kontrol et
                var proje = await _context.Projeler
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == projeId);

                if (proje == null)
                {
                    _logger.LogWarning("Proje bulunamadı: {ProjeId}", projeId);
                    return Json(new { success = false, message = "Proje bulunamadı" });
                }

                _logger.LogInformation("Proje bulundu: {ProjeId} - {ProjeAdi}", projeId, proje.Ad);
                
                var projeKullanicilari = await _context.ProjeKullanicilar
                    .Include(pk => pk.Kullanici)
                    .Where(pk => pk.ProjeId == projeId)
                    .Select(pk => new { id = pk.KullaniciId, email = pk.Kullanici.Email })
                    .ToListAsync();

                _logger.LogInformation("Projede bulunan kullanıcı sayısı: {KullaniciSayisi}", projeKullanicilari.Count);

                if (!projeKullanicilari.Any())
                {
                    _logger.LogWarning("Projede kullanıcı bulunamadı: {ProjeId}", projeId);
                    return Json(new { success = false, message = "Bu projeye atanmış kullanıcı bulunmamaktadır" });
                }

                _logger.LogInformation("Kullanıcılar başarıyla getirildi: {@Kullanicilar}", 
                    projeKullanicilari.Select(k => new { k.email }));

                return Json(new { success = true, data = projeKullanicilari });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Proje kullanıcıları getirilirken hata oluştu: {ProjeId}", projeId);
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        private async Task<bool> IsUserAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return false;
            }
            return await _userManager.IsInRoleAsync(user, "Admin");
        }
    }
} 