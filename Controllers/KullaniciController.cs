using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeYonetimTakipSistem.Data;
using ProjeYonetimTakipSistem.Models;
using ProjeYonetimTakipSistem.Models.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ProjeYonetimTakipSistem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KullaniciController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public KullaniciController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Kullanıcı Listesi
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<KullaniciListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new KullaniciListViewModel
                {
                    Id = user.Id!,
                    Email = user.Email!,
                    FirstName = user.FirstName!,
                    LastName = user.LastName!,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        // Kullanıcı Detayı
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new KullaniciListViewModel
            {
                Id = user.Id!,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Roles = roles.ToList()
            };

            return View(model);
        }

        // Kullanıcı Düzenleme Formu
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var viewModel = new KullaniciDuzenleViewModel
            {
                Id = user.Id!,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                CurrentRoles = roles.ToList(),
                AllRoles = allRoles
            };

            return View(viewModel);
        }

        // Kullanıcı Düzenle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, KullaniciDuzenleViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Mevcut rolleri kaldır
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    // Yeni rolleri ekle
                    if (model.SelectedRoles != null)
                    {
                        await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Hata durumunda rolleri tekrar yükle
            model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // Kullanıcı Silme Onay Sayfası
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new KullaniciListViewModel
            {
                Id = user.Id!,
                Email = user.Email!,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                Roles = roles.ToList()
            };

            return View(viewModel);
        }

        // Kullanıcı Sil
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(Index));
        }

        // Yeni Kullanıcı Oluşturma Formu
        public IActionResult Create()
        {
            var model = new KullaniciOlusturViewModel
            {
                Email = "",
                FirstName = "",
                LastName = "",
                Password = "",
                ConfirmPassword = ""
            };
            return View(model);
        }

        // POST: Kullanici/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KullaniciOlusturViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                    return View(model);
                }

                // E-posta adresi kontrolü
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Bu e-posta adresi zaten kullanımda.");
                    model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                    return View(model);
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Her zaman User rolünü ekle
                    var rolesToAdd = new List<string> { "User" };

                    // Eğer başka roller seçildiyse onları da ekle
                    if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    {
                        foreach (var role in model.SelectedRoles.Where(r => r != "User"))
                        {
                            if (!rolesToAdd.Contains(role))
                            {
                                rolesToAdd.Add(role);
                            }
                        }
                    }

                    var roleResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        await _userManager.DeleteAsync(user);
                        model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                        return View(model);
                    }

                    TempData["Message"] = "Kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı oluşturulurken bir hata oluştu: " + ex.Message);
                model.AllRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }
        }
    }
} 