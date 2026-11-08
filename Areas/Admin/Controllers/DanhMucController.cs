using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Areas.Admin.Controllers;

[Area("Admin")]
public class DanhMucController(DataContext context) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (HttpContext.Session.GetInt32("TaiKhoanID") == null)
            filterContext.Result = RedirectToAction("Index", "DangNhap", new { area = "" });
        else if (HttpContext.Session.GetString("VaiTro") != "Admin")
            filterContext.Result = StatusCode(StatusCodes.Status403Forbidden);
        base.OnActionExecuting(filterContext);
    }

    // 1. Danh sách danh mục (GET: Admin/DanhMuc)
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, bool? trangThai)
    {
        var query = context.tblDanhMucTaiSan.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(x =>
                x.DMTS_MaDanhMuc.ToLower().Contains(kw) ||
                x.DMTS_TenDanhMuc.ToLower().Contains(kw));
        }

        if (trangThai.HasValue)
        {
            query = query.Where(x => x.DMTS_TrangThai == trangThai.Value);
        }

        ViewBag.Keyword = keyword;
        ViewBag.TrangThai = trangThai;

        var list = await query
            .OrderByDescending(x => x.DMTS_ID)
            .ToListAsync();

        return View(list);
    }

    // 2. Giao diện thêm mới (GET: Admin/DanhMuc/Create)
    [HttpGet]
    public IActionResult Create()
    {
        return View(new tblDanhMucTaiSan { DMTS_TrangThai = true });
    }

    // 3. Xử lý thêm mới (POST: Admin/DanhMuc/Create)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(tblDanhMucTaiSan model)
    {
        if (!string.IsNullOrWhiteSpace(model.DMTS_MaDanhMuc))
        {
            var ma = model.DMTS_MaDanhMuc.Trim();
            var exists = await context.tblDanhMucTaiSan.AnyAsync(x => x.DMTS_MaDanhMuc == ma);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.DMTS_MaDanhMuc), "Mã danh mục này đã tồn tại.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.DMTS_MaDanhMuc = model.DMTS_MaDanhMuc.Trim();
        model.DMTS_TenDanhMuc = model.DMTS_TenDanhMuc.Trim();
        model.DMTS_MoTa = model.DMTS_MoTa?.Trim();
        model.DMTS_NgayTao = DateTime.Now;
        model.DMTS_TrangThai ??= true;

        context.tblDanhMucTaiSan.Add(model);
        await context.SaveChangesAsync();

        TempData["Success"] = "Thêm danh mục tài sản thành công.";
        return RedirectToAction(nameof(Index));
    }

    // 4. Giao diện cập nhật (GET: Admin/DanhMuc/Edit/5)
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await context.tblDanhMucTaiSan.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }
        return View(item);
    }

    // 5. Xử lý cập nhật (POST: Admin/DanhMuc/Edit/5)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, tblDanhMucTaiSan model)
    {
        if (id != model.DMTS_ID)
        {
            return BadRequest();
        }

        if (!string.IsNullOrWhiteSpace(model.DMTS_MaDanhMuc))
        {
            var ma = model.DMTS_MaDanhMuc.Trim();
            var exists = await context.tblDanhMucTaiSan
                .AnyAsync(x => x.DMTS_MaDanhMuc == ma && x.DMTS_ID != model.DMTS_ID);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.DMTS_MaDanhMuc), "Mã danh mục này đã tồn tại.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existing = await context.tblDanhMucTaiSan.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.DMTS_MaDanhMuc = model.DMTS_MaDanhMuc.Trim();
        existing.DMTS_TenDanhMuc = model.DMTS_TenDanhMuc.Trim();
        existing.DMTS_MoTa = model.DMTS_MoTa?.Trim();
        existing.DMTS_TrangThai = model.DMTS_TrangThai;

        await context.SaveChangesAsync();

        TempData["Success"] = "Cập nhật danh mục tài sản thành công.";
        return RedirectToAction(nameof(Index));
    }

    // 6. Xử lý xóa (POST: Admin/DanhMuc/Delete/5)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await context.tblDanhMucTaiSan.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        // Kiểm tra ràng buộc khóa ngoại với bảng Tài sản (DeleteBehavior.Restrict)
        var hasAssets = await context.tblTaiSan.AnyAsync(x => x.DMTS_ID == id);
        if (hasAssets)
        {
            TempData["Error"] = "Không thể xóa danh mục này vì đang có tài sản thuộc danh mục.";
            return RedirectToAction(nameof(Index));
        }

        context.tblDanhMucTaiSan.Remove(item);
        await context.SaveChangesAsync();

        TempData["Success"] = "Xóa danh mục tài sản thành công.";
        return RedirectToAction(nameof(Index));
    }
}