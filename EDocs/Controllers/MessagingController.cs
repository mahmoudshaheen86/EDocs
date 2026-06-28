using edocs.Models;
using edocs.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using edocs.helper;

namespace edocs.Controllers
{
    [Authorize]
    public class MessagingController : BaseController
    {
        private readonly IDocSentService _docSentService;
        private readonly IDocumentService _documentService;
        private readonly IFileService _fileService;
        private readonly IUserCategoryService _userCategoryService;

        public MessagingController(
            IDocSentService docSentService,
            IDocumentService documentService,
            IFileService fileService,
            IUserCategoryService userCategoryService)
        {
            _docSentService = docSentService;
            _documentService = documentService;
            _fileService = fileService;
            _userCategoryService = userCategoryService;
        }

        // GET: Messaging
        public async Task<ActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Redirect("~/Account/Login");

            var messages = await _docSentService.GetUserMessagesAsync(userId.Value);
            return View(messages);
        }

        public async Task<ActionResult> Outbox()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Redirect("~/Account/Login");

            var sentMessages = await _docSentService.GetSentMessagesAsync(userId.Value);

            if (sentMessages == null)
                sentMessages = new List<DocMessage>();

            var sentMessagesList = sentMessages.ToList();

            var firstMessage = sentMessagesList.FirstOrDefault();

            ViewBag.sendnote = firstMessage?.SENDNOTES;
            ViewBag.senddate = firstMessage?.SENDDATE;
            ViewBag.DocNum = firstMessage?.DocumentiFk?.NUMBEROF;
            ViewBag.UsersName = await _docSentService.GetAllUsersAsync();

            ViewBag.mode = sentMessagesList.Any() ? "Full" : "Empty";

            return View(sentMessagesList);
        }

        public async Task<ActionResult> Inbox()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Redirect("~/Account/Login");

            var receivedMessages = await _docSentService.GetReceivedMessagesAsync(userId.Value);
            var sentMessages = await _docSentService.GetSentMessagesAsync(userId.Value);

            ViewBag.recmaes = sentMessages;
            ViewBag.currentuser = userId;
            ViewBag.UsersName = await _docSentService.GetAllUsersAsync();

            ViewBag.mode = receivedMessages.Any() ? "Full" : "Empty";

            return View(receivedMessages);
        }

        public ActionResult DocMessages()
        {
            return View();
        }

        public async Task<ActionResult> SearchForUser(string searchTerm)
        {
            var users = await _docSentService.GetAllUsersAsync();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.UserName.Contains(searchTerm)).ToList();
            }
            return PartialView("_UserSearchResults", users);
        }

        public async Task<ActionResult> replyperson(int id, int userrec)
        {
            ViewBag.DocId = id;
            ViewBag.userrec = userrec;
            ViewBag.UsersName = await _docSentService.GetAllUsersAsync();
            return View();
        }

        public async Task<ActionResult> myreply(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Redirect("~/Account/Login");

            var myReplies = await _docSentService.GetByDocumentIdAsync(id);
            var userReplies = myReplies.Where(m => m.USERSENDID == userId).OrderByDescending(m => m.SENDDATE);

            var firstReply = userReplies.FirstOrDefault();
            ViewBag.Docnum = firstReply?.DocumentiFk?.NUMBEROF;
            ViewBag.UsersName = await _docSentService.GetAllUsersAsync();

            return View(userReplies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> reply(int idd, int userrecd, string note)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Redirect("~/Account/Login");

            var message = new DocMessage
            {
                SENDNOTES = note,
                SENDDATE = DateTime.Now,
                USERSENDID = userId.Value,
                USERECIVEID = userrecd,
                Documenti = idd
            };

            await _docSentService.CreateMessageAsync(message);
            return RedirectToAction("Sentmasseges");
        }

        public async Task<ActionResult> SearchResult(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userId = GetCurrentUserId();
            var replies = await _docSentService.SearchByDocumentNumberAsync(number, userId ?? 0);
            ViewBag.docid = replies.FirstOrDefault()?.DocumentiFk?.ID;
            ViewBag.docnumber = number;
            ViewBag.UsersName = await _docSentService.GetAllUsersAsync();

            return View(replies.ToList());
        }

        // GET: Messaging/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var message = await _docSentService.GetMessageAsync(id.Value);
            if (message == null)
                return HttpNotFound();

            return View(message);
        }

        // GET: Messaging/DetailsDOc/5
        public async Task<ActionResult> DetailsDOc(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var document = await _documentService.GetDocumentDetailsAsync(id.Value);
            if (document == null)
            {
                return RedirectToAction("Index", new { Message = "No Document" });
            }

            var files = document.DocFile?.OrderBy(f => f.ID).ToList();
            var tempFiles = new List<DocFile>();

            if (files != null && files.Count > 0 && Session["pf"] != null && Session["pf"].ToString() != "")
            {
                foreach (var file in files)
                {
                    Encryption.DecryptFile(file, document, Server.MapPath("~"));
                    tempFiles.Add(file);
                }
                ViewBag.files = tempFiles;
            }
            else
            {
                ViewBag.files = null;
            }

            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                var user = await _userCategoryService.GetUserCategoryAsync(userId.Value, document.Category ?? 0);
                ViewBag.userrole = user?.TYPE;
            }

            return View(document);
        }

        private async System.Threading.Tasks.Task PopulateSelectListsAsync(int? documentId)
        {
            var documents = await _documentService.GetRecentDocumentsAsync(0, 1000); // get many
            ViewBag.DOCUMENTID = new SelectList(documents, "ID", "NUMBEROF", documentId);
            var users = await _docSentService.GetAllUsersAsync();
            ViewBag.users = users;
        }

        // GET: Messaging/Create
        public async Task<ActionResult> Create(int? id)
        {
            ViewBag.DocumentID = id;
            await PopulateSelectListsAsync(null);
            return View();
        }

        // GET: Messaging/saveSelectedItem
        public async Task<ActionResult> SaveSelectedItem()
        {
            await PopulateSelectListsAsync(null);
            return View();
        }

        // POST: Messaging/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DocMessage docMessage)
        {
            docMessage.SENDDATE = DateTime.Now;
            if (ModelState.IsValid)
            {
                await _docSentService.CreateMessageAsync(docMessage);
                return RedirectToAction("Index");
            }
            return View(docMessage);
        }

        // POST: Messaging/SendModel
        public async Task<ActionResult> SendModel(string users, string Note, int DocumentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Index");

            await _docSentService.BroadcastMessageAsync(users, Note, DocumentId, userId.Value);
            return RedirectToAction("Sentmasseges");
        }

        // GET: Messaging/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var message = await _docSentService.GetMessageAsync(id.Value);
            if (message == null)
                return HttpNotFound();

            return View(message);
        }

        // POST: Messaging/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DocMessage docMessage)
        {
            if (ModelState.IsValid)
            {
                await _docSentService.UpdateMessageAsync(docMessage);
                return RedirectToAction("Index");
            }
            return View(docMessage);
        }

        // GET: Messaging/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var message = await _docSentService.GetMessageAsync(id.Value);
            if (message == null)
                return HttpNotFound();

            return View(message);
        }

        // POST: Messaging/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _docSentService.DeleteMessageAsync(id);
            return RedirectToAction("Index");
        }

        private int? GetCurrentUserId()
        {
            return Session["UserId"] != null ? int.Parse(Session["UserId"].ToString()) : (int?)null;
        }

        private async Task UpdateDocSentAsync(DocMessage message)
        {
            var existing = await _docSentService.GetMessageAsync(message.ID);
            if (existing != null)
            {
                existing.SENDNOTES = message.SENDNOTES;
                existing.SENDDATE = message.SENDDATE;
                existing.USERSENDID = message.USERSENDID;
                existing.USERECIVEID = message.USERECIVEID;
                existing.Documenti = message.Documenti;
                await _docSentService.UpdateMessageAsync(existing);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);                
        }
    }
}
