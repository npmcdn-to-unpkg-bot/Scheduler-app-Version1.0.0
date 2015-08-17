using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using SchedulerWebApp.Models;
using SchedulerWebApp.Models.DBContext;

namespace SchedulerWebApp.Controllers
{
    [Authorize]
    public class ContactsController : Controller
    {
        private readonly SchedulerDbContext _db = new SchedulerDbContext();

        // GET: Contacts
        public ActionResult Index()
        {
            return View(GetUserContacts(GetCurrentUserId()));
        }

        // GET: Contacts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var contact = GetUserContacts(GetCurrentUserId()).Find(c => c.ContactId == id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // GET: Contacts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ContactId,FirstName,LastName,Email,PhoneNumber")] Contact contact)
        {
            contact.SchedulerUserId = GetCurrentUserId();

            if (ModelState.IsValid)
            {
                GetUserContacts(GetCurrentUserId()).Add(contact);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(contact);
        }

        public ActionResult AddUnsaved(Contact contact)
        {
            return View("Create", contact);
        }

        // GET: Contacts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = _db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ContactId,FirstName,LastName,Email,PhoneNumber,SchedulerUserId")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(contact).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = _db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Contact contact = _db.Contacts.Find(id);
            _db.Contacts.Remove(contact);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public List<Contact> GetUserContacts(string currentUser)
        {
            var contants = _db.Users.Single(u => u.Id == currentUser).Contacts;
            return contants;
        }

        public JsonResult SearchContact(string term)
        {
            var t = term.ToLower();
            var contacts = GetUserContacts(GetCurrentUserId())
                .Where(c => (c.FirstName.ToLower().Contains(t) || c.LastName.ToLower().Contains(t) || c.Email.ToLower().Contains(t)))
                                    .Select(c => new
                                         {
                                             label = (c.FirstName + " " + c.LastName + " <" + c.Email + ">"),
                                             value = c.Email
                                         });

            return Json(contacts, JsonRequestBehavior.AllowGet);
        }

        private string GetCurrentUserId()
        {
            return User.Identity.GetUserId();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
