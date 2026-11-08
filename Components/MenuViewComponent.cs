using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Components
{
    [ViewComponent(Name = "MenuView")]
    public class MenuViewComponent : ViewComponent
    {
        private readonly DataContext _context;

        public MenuViewComponent(DataContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var listOfMenu = await _context.tblMenu
                .Where(m => m.IsActive == true && m.Position == 1)
                .OrderBy(m => m.MenuOrder).ToListAsync();

            return View("Default", listOfMenu);
        }
    }
}