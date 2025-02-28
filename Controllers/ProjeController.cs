using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeYonetimTakipSistem.Data;
using ProjeYonetimTakipSistem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjeYonetimTakipSistem.Models.ViewModels;

namespace ProjeYonetimTakipSistem.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class ProjeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Projeleri Listele
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            IQueryable<Proje> projeler;

            if (User.IsInRole("Admin"))
            {
                projeler = _context.Projeler
                    .Include(p => p.ProjeKullanicilari)
                    .ThenInclude(pk => pk.Kullanici);
            }
            else
            {
                projeler = _context.Projeler
                    .Include(p => p.ProjeKullanicilari)
                    .ThenInclude(pk => pk.Kullanici)
                    .Where(p => p.ProjeKullanicilari.Any(pk => pk.KullaniciId == currentUser.Id));
            }

            return View(await projeler.ToListAsync());
        }

        // Proje Detayı
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proje = await _context.Projeler
                .Include(p => p.ProjeKullanicilari)
                .ThenInclude(pk => pk.Kullanici)
                .Include(p => p.Gorevler)
                .ThenInclude(g => g.AtananKullanici)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proje == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") && !proje.ProjeKullanicilari.Any(pk => pk.KullaniciId == currentUser.Id))
            {
                return Forbid();
            }

            return View(proje);
        }

        // Yeni Proje Oluşturma Formu
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // Yeni Proje Oluştur
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Ad,Aciklama,BaslangicTarihi,BitisTarihi,Durum")] Proje proje)
        {
            if (ModelState.IsValid)
            {
                _context.Add(proje);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(proje);
        }

        // Proje Düzenleme Formu
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proje = await _context.Projeler
                .Include(p => p.ProjeKullanicilari)
                .ThenInclude(pk => pk.Kullanici)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proje == null)
            {
                return NotFound();
            }

            return View(proje);
        }

        // Proje Düzenle
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Aciklama,BaslangicTarihi,BitisTarihi,Durum")] Proje proje)
        {
            if (id != proje.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proje);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjeExists(proje.Id))
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
            return View(proje);
        }

        // Proje Silme Onay Sayfası
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proje = await _context.Projeler
                .Include(p => p.ProjeKullanicilari)
                .ThenInclude(pk => pk.Kullanici)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proje == null)
            {
                return NotFound();
            }

            return View(proje);
        }

        // Proje Sil
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proje = await _context.Projeler.FindAsync(id);
            if (proje == null)
            {
                return NotFound();
            }

            _context.Projeler.Remove(proje);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Kullanıcı Atama Sayfası
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> KullaniciAta(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proje = await _context.Projeler
                .Include(p => p.ProjeKullanicilari)
                .ThenInclude(pk => pk.Kullanici)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proje == null)
            {
                return NotFound();
            }

            var kullanicilar = await _userManager.Users.ToListAsync();
            var model = new KullaniciAtaViewModel
            {
                ProjeId = proje.Id,
                Proje = proje,
                Kullanicilar = kullanicilar
            };

            return View(model);
        }

        // Kullanıcı Ata
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> KullaniciAta(int id, string[] selectedKullanicilar)
        {
            var proje = await _context.Projeler
                .Include(p => p.ProjeKullanicilari)
                .ThenInclude(pk => pk.Kullanici)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (proje == null)
            {
                return NotFound();
            }

            var mevcutKullanicilar = new HashSet<string>(proje.ProjeKullanicilari.Select(pk => pk.KullaniciId));
            var secilenKullanicilar = new HashSet<string>(selectedKullanicilar);

            foreach (var kullanici in _context.Users)
            {
                if (secilenKullanicilar.Contains(kullanici.Id))
                {
                    if (!mevcutKullanicilar.Contains(kullanici.Id))
                    {
                        proje.ProjeKullanicilari.Add(new ProjeKullanici { ProjeId = proje.Id, KullaniciId = kullanici.Id });
                    }
                }
                else
                {
                    if (mevcutKullanicilar.Contains(kullanici.Id))
                    {
                        var toRemove = proje.ProjeKullanicilari.FirstOrDefault(pk => pk.KullaniciId == kullanici.Id);
                        if (toRemove != null)
                        {
                            _context.ProjeKullanicilar.Remove(toRemove);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = id });
        }

        private bool ProjeExists(int id)
        {
            return _context.Projeler.Any(e => e.Id == id);
        }

        // Kullanıcı Çıkar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> KullaniciCikar(int projeId, string kullaniciId)
        {
            if (string.IsNullOrEmpty(kullaniciId))
            {
                return BadRequest("Kullanıcı ID gereklidir.");
            }

            var projeKullanici = await _context.ProjeKullanicilar
                .FirstOrDefaultAsync(pk => pk.ProjeId == projeId && pk.KullaniciId == kullaniciId);

            if (projeKullanici == null)
            {
                return NotFound("Kullanıcı bu projede bulunamadı.");
            }

            try
            {
                // Önce bu kullanıcıya atanmış görevleri kontrol et
                var atanmisGorevler = await _context.Gorevler
                    .Where(g => g.ProjeId == projeId && g.AtananKullaniciId == kullaniciId)
                    .ToListAsync();

                // Atanmış görevleri null yap
                foreach (var gorev in atanmisGorevler)
                {
                    gorev.AtananKullaniciId = null;
                }

                // Kullanıcıyı projeden çıkar
                _context.ProjeKullanicilar.Remove(projeKullanici);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Kullanıcı projeden başarıyla çıkarıldı.";
                return RedirectToAction(nameof(Details), new { id = projeId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Kullanıcı çıkarılırken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 