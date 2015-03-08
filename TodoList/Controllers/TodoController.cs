using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TodoList.Models;

namespace TodoList.Controllers
{
    public class TodoController : Controller
    {
        private DatabaseEntities db = new DatabaseEntities();
        private static List<Todo> model =null;
        private static string ContentSort = "";
        private static string DueTimeSort = "";
        private static string PrioritySort = "";

        public ActionResult Index()
        {
            if (model ==null)
            {
                model = db.Todo.ToList();
            }
            ViewBag.PrioritySort = ViewBag.PrioritySort == null ? PrioritySort : ViewBag.PrioritySort;
            ViewBag.DueTimeSort = ViewBag.DueTimeSort == null ? DueTimeSort : ViewBag.DueTimeSort;
            ViewBag.ContentSort = ViewBag.ContentSort == null ? ContentSort : ViewBag.ContentSort;
            return View(model);
        }

        #region Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Content,DueTime,Priority,TodoFlag")] Todo todo)
        {
            if (ModelState.IsValid)
            {
                db.Todo.Add(todo);
                db.SaveChanges();
                model = db.Todo.ToList();
                return RedirectToAction("Index");
            }

            return View(todo);
        }
        #endregion

        #region Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Todo todo = db.Todo.Find(id);
            if (todo == null)
            {
                return HttpNotFound();
            }
            return View(todo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Content,DueTime,Priority,TodoFlag")] Todo todo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(todo).State = EntityState.Modified;
                db.SaveChanges();
                model = db.Todo.ToList();
                return RedirectToAction("Index");
            }
            return View(todo);
        }
        #endregion

        #region Delete
        [HttpPost]
        public ActionResult DeleteConfirmed(int pid)
        {
            Todo todo = db.Todo.Find(pid);
            db.Todo.Remove(todo);
            db.SaveChanges();
            model.Remove(todo);
            return View("~/Views/Todo/Index.cshtml", model);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Done
        [HttpPost]
        public ActionResult TodoFlagChanged(int pid, bool isChecked)
        {
            Todo todo = db.Todo.Find(pid);
            todo.TodoFlag = isChecked;
            db.Entry(todo).State = EntityState.Modified;
            db.SaveChanges();
            var result = (from p in model where p.Id == pid select p);
            result.First().TodoFlag=isChecked;
            return View("~/Views/Todo/Index.cshtml", model);
        }
        #endregion

        #region Query
        [HttpPost]
        public ActionResult QueryContent(string queryStr)
        {
            var result = (from p in db.Todo where p.Content.Contains(queryStr) select p);
            model = result.ToList();
            return View("~/Views/Todo/Index.cshtml", model);
        }
        #endregion

        #region Sort
        public ActionResult Sort(string sortOrder)
        {
            ViewBag.PrioritySort = sortOrder == "Priority" ? "Priority_desc" : "Priority";
            ViewBag.DueTimeSort = sortOrder == "DueTime" ? "DueTime_desc" : "DueTime";
            ViewBag.ContentSort = sortOrder == "Content" ? "Content_desc" : "Content";
            PrioritySort = ViewBag.PrioritySort;
            DueTimeSort = ViewBag.DueTimeSort;
            ContentSort = ViewBag.ContentSort;
            var result = from p in model
                         select p;
            switch (sortOrder)
            {
                case "Priority_desc":
                    result = result.OrderByDescending(p => p.Priority);
                    break;
                case "DueTime":
                    result = result.OrderBy(p => p.DueTime);
                    break;
                case "DueTime_desc":
                    result = result.OrderByDescending(p => p.DueTime);
                    break;
                case "Content":
                    result = result.OrderBy(p => p.Content);
                    break;
                case "Content_desc":
                    result = result.OrderByDescending(p => p.Content);
                    break;
                default:
                    result = result.OrderBy(p => p.Priority);
                    break;
            }
            model = result.ToList();
            return View("~/Views/Todo/Index.cshtml", model);
        }
        #endregion

    }
}
