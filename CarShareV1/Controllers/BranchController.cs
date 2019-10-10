using CarShare.Data;
using CarShareV1.Data;
using CarShareV1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarShareV1.Controllers
{
    [Authorize]
    public class BranchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserApplication> _userManager;

        public BranchController(ApplicationDbContext context, UserManager<UserApplication> _UserManager)
        {
            _context = context;
            _userManager = _UserManager;
        }

        // GET: Branches
        public async Task<IActionResult> Index()
        {
            return View(await _context.Branches.ToListAsync());
        }

        // GET: Branches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(m => m.Id == id);
            if (branch == null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(branch);
        }

        // GET: Branches/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Branches/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BranchName,CreateTime,UpdateTime,UserId")] Branch branch)
        {
            string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId
            branch.UserId = currentUserId;
            branch.UpdateTime = DateTime.Now;
            branch.CreateTime = DateTime.Now;
            if (ModelState.IsValid)
            {
                _context.Add(branch);
                await _context.SaveChangesAsync();
                int Id = branch.Id;//Return the Id after Add
                // return RedirectToAction(nameof(Index));
                //Redirect to Address page
                return RedirectToAction("Create", "BranchAddress", new { id = Id });
            }
            return View(branch);
        }

        // GET: Branches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", "Home");
            }

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(branch);
        }

        // POST: Branches/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BranchName,UpdateTime")] Branch branch)
        {
            if (id != branch.Id)
            {
                return RedirectToAction("Error", "Home");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string currentUserId = _userManager.GetUserId(HttpContext.User);//Get UserId

                    Branch model =  await _context.Branches.FindAsync(id);
                    model.BranchName = branch.BranchName;
                    model.UpdateTime = DateTime.Now;
                    model.UserId = currentUserId;


                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BranchExists(branch.Id))
                    {
                        return RedirectToAction("Error", "Home");
                    }
                     
                }
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        
        private bool BranchExists(int id)
        {
            return _context.Branches.Any(e => e.Id == id);
        }


        [HttpGet("/Branch/DeleteBranch/{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            POJOMsgs model = new POJOMsgs();

            try
            {
                Branch _Branch = await _context.Branches.FindAsync(id);
                _context.Branches.Remove(_Branch);
                await _context.SaveChangesAsync();
                model.Flag = true;
                model.Msg = "Has Been Deleted it";
            }
            catch (Exception e)
            {
                model.Flag = false;
                model.Msg = e.ToString();
            }
            return Json(model);
        }

    }
}
