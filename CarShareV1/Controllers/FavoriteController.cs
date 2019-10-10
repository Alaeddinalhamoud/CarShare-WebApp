using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace CarShareV1.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;

        public FavoriteController(ApplicationDbContext context, UserManager<UserApplication> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;
        }
        //get the current user fav car list
        public IActionResult Index()
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);

            IQueryable<FavoriteModelView> result = from fav in _context.Favorites.Where(i => i.UserId == currentUserId)
                                                          join p in _context.Vehicles on fav.VehicleId equals p.Id

                                                          select new FavoriteModelView
                                                          {
                                                              VehicleModel = p.Model,
                                                              VehicleId = p.Id,
                                                              Id = fav.Id,
                                                              CreateDate = fav.CreateTime
                                                          };
            return View(result.OrderBy(m=>m.CreateDate));
        }
    }
}