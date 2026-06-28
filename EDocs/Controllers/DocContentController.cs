using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using edocs.Models;
using edocs.Models;
using edocs;

namespace edocs.Controllers
{
    [NoDirectAccess]
    [Authorize]
    public class DocContentController : BaseController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: DocContent
        public async Task<ActionResult> Index(CONTENTMessageId? message)
        {
            ViewBag.StatusMessage =
            message == CONTENTMessageId.AddSuccess ? "<div class=\"alert alert-success\"><strong>  إضافة  </strong>تم عملية الإضافة بنجاح </div> "
            : message == CONTENTMessageId.AddError ? "<div class=\"alert alert-danger\"><strong> تنبيه</strong>فشل في عملية الإضافة </div> "
            : message == CONTENTMessageId.EditeSuccess ? "<div class=\"alert alert-info\"><strong>  تعديل  </strong>تم عملية التعديل بنجاح </div> "
            : message == CONTENTMessageId.DeleteSuccess ? "<div class=\"alert alert-danger\"><strong>  حذف  </strong>تم عملية الحذف بنجاح </div> "
            : "";
            ViewBag.columns = new Columns().names.ToString();
            var dOC_CONTENT = db.DocContent.Include(d => d.DOCFk);
            return View(await dOC_CONTENT.ToListAsync());
        }


        [HttpPost]
        public ActionResult GetList()
        {
            //server side parameter
            int start = 0;
            int length = 10; // default page size

            // Validate start parameter
            string startParam = Request["start"];
            if (!string.IsNullOrEmpty(startParam) && !int.TryParse(startParam, out start) || start < 0)
            {
                start = 0;
            }

            // Validate length parameter
            string lengthParam = Request["length"];
            if (!string.IsNullOrEmpty(lengthParam) && !int.TryParse(lengthParam, out length) || length <= 0)
            {
                length = 10;
            }

            // Limit to reasonable max to prevent DoS
            if (length > 500)
            {
                length = 500;
            }

            var draw = Request["draw"];
            int pagesize = length;
            int skip = start;
            int recordsTotl = 0;
            string searchValue = Request["search[value]"]?.ToLower() ?? "";
            // These are used for potential future sorting; currently unused
            // string sortColumnName = Request["columns[" + Request["order[0][column]"] + "][data]"];
            // string sortDirection = Request["order[0][dir]"];

            using (ApplicationDbContext dc = new ApplicationDbContext())
            {
                var v = (from a in dc.DocContent select a);

                //search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    v = v.Where(c => c.TYPE.ToLower().Contains(searchValue) 
                        || c.CONTENT.ToLower().Contains(searchValue) 
                        || c.DESCRIPTION.ToLower().Contains(searchValue) 
                        || c.NOTES.ToLower().Contains(searchValue) 
                        || (c.DOCFk != null && c.DOCFk.NUMBEROF.ToLower().Contains(searchValue)));
                }

                recordsTotl = v.Count();

                v = v.Skip(skip).Take(pagesize);
                var data = v.Select(DocContent => new
                {
                    NUMBERDOC = DocContent.NUMBERDOC,
                    NOTES = DocContent.NOTES,
                    TYPE = DocContent.TYPE,
                    DESCRIPTION = DocContent.DESCRIPTION,
                    CONTENT = DocContent.CONTENT,
                    DATEDOC = DocContent.DATEDOC,
                }).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotl, recordsTotal = recordsTotl, data }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: DocContent/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocContent dOC_CONTENT = await db.DocContent.FindAsync(id);
            if (dOC_CONTENT == null)
            {
                return HttpNotFound();
            }
            return View(dOC_CONTENT);
        }

        // GET: DocContent/Create
        public ActionResult Create()
        {
            ViewBag.DOCUMENTID = new SelectList(db.Documenti, "ID", "NUMBEROF");
            return View();
        }

        // POST: DocContent/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "NUMBERDOC,TYPE,CONTENT,NOTES,DESCRIPTION,DATEDOC,DOCUMENTID")] DocContent dOC_CONTENT)
        {
            if (ModelState.IsValid)
            {
                db.DocContent.Add(dOC_CONTENT);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.DOCUMENTID = new SelectList(db.Documenti, "ID", "NUMBEROF", dOC_CONTENT.DOCUMENTID);
            return View(dOC_CONTENT);
        }

        // GET: DocContent/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocContent dOC_CONTENT = await db.DocContent.FindAsync(id);
            if (dOC_CONTENT == null)
            {
                return HttpNotFound();
            }
            ViewBag.DOCUMENTID = new SelectList(db.Documenti, "ID", "NUMBEROF", dOC_CONTENT.DOCUMENTID);
            return View(dOC_CONTENT);
        }

        // POST: DocContent/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "NUMBERDOC,TYPE,CONTENT,NOTES,DESCRIPTION,DATEDOC,DOCUMENTID")] DocContent dOC_CONTENT)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dOC_CONTENT).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.DOCUMENTID = new SelectList(db.Documenti, "ID", "NUMBEROF", dOC_CONTENT.DOCUMENTID);
            return View(dOC_CONTENT);
        }

        // GET: DocContent/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DocContent dOC_CONTENT = await db.DocContent.FindAsync(id);
            if (dOC_CONTENT == null)
            {
                return HttpNotFound();
            }
            return View(dOC_CONTENT);
        }

        // POST: DocContent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            DocContent dOC_CONTENT = await db.DocContent.FindAsync(id);
            db.DocContent.Remove(dOC_CONTENT);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public enum CONTENTMessageId
        {
            AddSuccess,
            AddError,
            EditeSuccess,
            EditeError,
            DeleteSuccess,
            DeleteError
        }

    }


}
