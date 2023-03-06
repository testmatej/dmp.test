using System.Globalization;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using rssreader.Data;
using rssreader.Models;
using System.Linq;
using System;

namespace rssreader.Controllers
{
    public class FeedsController : Controller
    {
        private readonly DataContext _context;

        public FeedsController(DataContext context)
        {
            _context = context;
        }

        // POST: Feeds/Load/5
        [HttpPost, ActionName("Load")]
        public async Task<IActionResult> Load(int? id)
        {
            if (id == null || _context.Feeds == null)
            {
                return NotFound();
            }

            var feed = await _context.Feeds
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feed == null || feed.URL == null)
            {
                return NotFound();
            }

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(feed.URL);
            XmlNodeList nl = xdoc.SelectNodes("rss/channel/item");
            
            if(nl == null)
                return NotFound();

            foreach (XmlNode xnode in nl)
            {
                XmlNode subNode = xnode.SelectSingleNode("title");
                string title = subNode != null ? subNode.InnerText : "";

                subNode = xnode.SelectSingleNode("link");
                string link = subNode != null ? subNode.InnerText : "";

                subNode = xnode.SelectSingleNode("description");
                string description = subNode != null ? subNode.InnerText : "";

                subNode = xnode.SelectSingleNode("pubDate");
                string pubDateStr = subNode != null ? subNode.InnerText : "";

                DateTime pubDate = DateTime.Parse(pubDateStr);

                var exists = _context.Articles.Any(a => a.URL == link);           

                if (!exists)
                {
                    var article = new Article
                    {
                        FeedId = feed.Id,
                        Title = title,
                        URL = link,
                        Description = description,
                        PubDate = pubDate
                    };

                    _context.Add(article);
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Details", new { id = feed.Id });
        }

        // GET: Feeds
        public async Task<IActionResult> Index(string sortOrder, string searchString)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CurrentFilter"] = searchString;
            var feeds = from f in _context.Feeds
                           select f;

            if (!String.IsNullOrEmpty(searchString))
            {
                feeds = feeds.Where(s => s.Name.ToLower().Contains(searchString.ToLower())
                                       || s.Description.ToLower().Contains(searchString.ToLower()));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    feeds = feeds.OrderByDescending(f => f.Name);
                    break;
                default:
                    feeds = feeds.OrderBy(f => f.Name);
                    break;
            }
            return View(await feeds.AsNoTracking().ToListAsync());
        }

        // GET: Feeds/Details/5
        public async Task<IActionResult> Details(int? id, string? sortOrder, string? searchString, 
            DateTime? fromDate, DateTime? toDate)
        {
            if (id == null || _context.Feeds == null)
            {
                return NotFound();
            }
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["CurrentFilter"] = searchString;
            ViewData["FromDate"] = fromDate;
            ViewData["ToDate"] = toDate;

            var feed = from f in _context.Feeds
                        .Include(a => a.Articles)
                        where f.Id == id
                        select f;

            var firstFeed = feed.First();

            if (!String.IsNullOrEmpty(searchString))
            {
                firstFeed.Articles = firstFeed.Articles.Where(s => s.Title.ToLower().Contains(searchString.ToLower())
                                       || s.Description.ToLower().Contains(searchString.ToLower())).ToList();
            }

            if (fromDate != null && toDate != null)
            {
                DateTime fDate = fromDate.Value.Date;

                firstFeed.Articles = firstFeed.Articles.Where(d => d.PubDate.Date >= fromDate.Value.Date
                                        && d.PubDate.Date <= toDate.Value.Date).ToList();
            }

            if (firstFeed == null)
                return NotFound();

            switch (sortOrder)
            {
                case "name_desc":
                    firstFeed.Articles = firstFeed.Articles.OrderByDescending(a => a.Title).ToList();
                    break;
                case "date": firstFeed.Articles = firstFeed.Articles.OrderBy(a => a.PubDate).ToList();
                    break;
                case "date_desc":
                    firstFeed.Articles = firstFeed.Articles.OrderByDescending(a => a.PubDate).ToList();
                    break;
                default:
                    firstFeed.Articles = firstFeed.Articles.OrderBy(a => a.Title).ToList();
                    break;
            }

            return View(firstFeed);
        }




    // GET: Feeds/Create
    public IActionResult Create()
        {
            return View();
        }

        // POST: Feeds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("URL,Name,Description")] Feed feed)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(feed);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. ");
            }
            return View(feed);
        }

        // GET: Feeds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Feeds == null)
            {
                return NotFound();
            }

            var feed = await _context.Feeds.FindAsync(id);
            if (feed == null)
            {
                return NotFound();
            }
            return View(feed);
        }

        // POST: Feeds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedToUpdate = await _context.Feeds.FirstOrDefaultAsync(o => o.Id == id);

            if (await TryUpdateModelAsync<Feed>(
                feedToUpdate,
                "",
                o => o.URL, o => o.Name, o => o.Description))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View(feedToUpdate);
        }

        // GET: Feeds/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feed = await _context.Feeds
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feed == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }

            return View(feed);
        }


        // POST: Feeds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feed = await _context.Feeds.FindAsync(id);
            if (feed == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Feeds.Remove(feed);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });

            }
        }

        private bool FeedExists(int id)
        {
          return _context.Feeds.Any(e => e.Id == id);
        }

        [HttpPost, ActionName("Selection")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Selection(FeedSelection userSelection)
        {
            if(userSelection.SelectedFeeds == null )
                return RedirectToAction(nameof(Index));

            List<Feed> feedsToRemove = new List<Feed>();
            foreach (var id in userSelection.SelectedFeeds)
            {
                feedsToRemove.Add(new Feed() { Id = id });
            }
            _context.Feeds.RemoveRange(feedsToRemove.ToArray());
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
