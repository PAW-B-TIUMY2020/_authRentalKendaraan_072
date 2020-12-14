using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentalKendaraan_072.Models;
using Microsoft.AspNetCore.Authorization;

namespace RentalKendaraan_072.Controllers
{
    public class PeminjamenController : Controller
    {
        private readonly RentKendaraanContext _context;

        public PeminjamenController(RentKendaraanContext context)
        {
            _context = context;
        }

        [Authorize(Policy = "readonlypolicy")]
        // GET: Peminjamen
        public async Task<IActionResult> Index(string ktsd, string searchString, string sortOrder, string currentFilter, int? pageNumber)
        {
            //buat list ketersediaan
            var ktsdList = new List<string>();

            //query mengambil data
            var ktsdQuery = from d in _context.Peminjaman orderby d.IdKendaraanNavigation.NamaKendaraan select d.IdKendaraanNavigation.NamaKendaraan;
            ktsdList.AddRange(ktsdQuery.Distinct());

            //untuk nampilkan di view
            ViewBag.ktsd = new SelectList(ktsdList);

            //panggil db context
            var menu = from m in _context.Peminjaman.Include(p => p.IdCostumerNavigation).Include(p => p.IdJaminanNavigation).Include(p => p.IdKendaraanNavigation) select m;
            //var rentKendaraanContext = _context.Peminjaman.Include(p => p.IdCostumerNavigation).Include(p => p.IdJaminanNavigation).Include(p => p.IdKendaraanNavigation);
            // return View(await rentKendaraanContext.ToListAsync());

            //untuk memilih dropdownlist ketersediaan
            if (!string.IsNullOrEmpty(ktsd))
            {
                menu = menu.Where(x => x.IdKendaraanNavigation.NamaKendaraan == ktsd);
            }

            //untuk search data
            if (!string.IsNullOrEmpty(searchString))
            {
                menu = menu.Where(s => s.Biaya.ToString().Contains(searchString) || s.IdCostumerNavigation.NamaCostumer.Contains(searchString)
                || s.IdJaminanNavigation.NamaJaminan.Contains(searchString));
            }

            //untuk sorting
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            switch (sortOrder)
            {
                case "name_desc":
                    menu = menu.OrderByDescending(s => s.IdCostumerNavigation.NamaCostumer);
                    break;
                case "Date":
                    menu = menu.OrderBy(s => s.TglPeminjaman);
                    break;
                case "date_desc":
                    menu = menu.OrderByDescending(s => s.TglPeminjaman);
                    break;
                default: // name asscending
                    menu = menu.OrderBy(s => s.IdCostumerNavigation.NamaCostumer);
                    break;
            }

            //membuat pagedkist
            ViewData["CurrentSort"] = sortOrder;
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;

            }
            ViewData["CurrentFilter"] = searchString;
            //return View(await menu.ToListAsync());

            //definisi jumlah data pada halaman
            int pageSize = 5;

            return View(await PaginatedList<Peminjaman>.CreateAsync(menu.AsNoTracking(), pageNumber ?? 1, pageSize));




        }

        // GET: Peminjamen/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var peminjaman = await _context.Peminjaman
                .Include(p => p.IdCostumerNavigation)
                .Include(p => p.IdJaminanNavigation)
                .Include(p => p.IdKendaraanNavigation)
                .FirstOrDefaultAsync(m => m.IdPeminjaman == id);
            if (peminjaman == null)
            {
                return NotFound();
            }

            return View(peminjaman);
        }

        [Authorize(Policy = "writepolicy")]
        // GET: Peminjamen/Create
        public IActionResult Create()
        {
            ViewData["IdCostumer"] = new SelectList(_context.Customer, "IdCustomer", "NamaCustomer");
            ViewData["IdJaminan"] = new SelectList(_context.Jaminan, "IdJaminan", "NamaJaminan");
            ViewData["IdKendaraan"] = new SelectList(_context.Kendaraan, "IdKendaraan", "NamaKendaraan");
            return View();
        }

        // POST: Peminjamen/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPeminjaman,TglPeminjaman,IdKendaraan,IdCostumer,IdJaminan,Biaya")] Peminjaman peminjaman)
        {
            if (ModelState.IsValid)
            {
                _context.Add(peminjaman);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCostumer"] = new SelectList(_context.Customer, "IdCustomer", "NamaCustomer", peminjaman.IdCostumer);
            ViewData["IdJaminan"] = new SelectList(_context.Jaminan, "IdJaminan", "NamaJaminan", peminjaman.IdJaminan);
            ViewData["IdKendaraan"] = new SelectList(_context.Kendaraan, "IdKendaraan", "NamaKendaraan", peminjaman.IdKendaraan);
            return View(peminjaman);
        }

        [Authorize(Policy = "editpolicy")]
        // GET: Peminjamen/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var peminjaman = await _context.Peminjaman.FindAsync(id);
            if (peminjaman == null)
            {
                return NotFound();
            }
            ViewData["IdCostumer"] = new SelectList(_context.Customer, "IdCustomer", "NamaCustomer", peminjaman.IdCostumer);
            ViewData["IdJaminan"] = new SelectList(_context.Jaminan, "IdJaminan", "NamaJaminan", peminjaman.IdJaminan);
            ViewData["IdKendaraan"] = new SelectList(_context.Kendaraan, "IdKendaraan", "NamaKendaraan", peminjaman.IdKendaraan);
            return View(peminjaman);
        }

        // POST: Peminjamen/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPeminjaman,TglPeminjaman,IdKendaraan,IdCostumer,IdJaminan,Biaya")] Peminjaman peminjaman)
        {
            if (id != peminjaman.IdPeminjaman)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(peminjaman);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PeminjamanExists(peminjaman.IdPeminjaman))
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
            ViewData["IdCostumer"] = new SelectList(_context.Customer, "IdCustomer", "NamaCustomer", peminjaman.IdCostumer);
            ViewData["IdJaminan"] = new SelectList(_context.Jaminan, "IdJaminan", "NamaJaminan", peminjaman.IdJaminan);
            ViewData["IdKendaraan"] = new SelectList(_context.Kendaraan, "IdKendaraan", "NamaKendaraan", peminjaman.IdKendaraan);
            return View(peminjaman);
        }

        [Authorize(Policy = "deletepolicy")]
        // GET: Peminjamen/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var peminjaman = await _context.Peminjaman
                .Include(p => p.IdCostumerNavigation)
                .Include(p => p.IdJaminanNavigation)
                .Include(p => p.IdKendaraanNavigation)
                .FirstOrDefaultAsync(m => m.IdPeminjaman == id);
            if (peminjaman == null)
            {
                return NotFound();
            }

            return View(peminjaman);
        }

        // POST: Peminjamen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var peminjaman = await _context.Peminjaman.FindAsync(id);
            _context.Peminjaman.Remove(peminjaman);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PeminjamanExists(int id)
        {
            return _context.Peminjaman.Any(e => e.IdPeminjaman == id);
        }
    }
}