﻿using System;
using System.Web.Mvc;

namespace MANAGER.Controllers.InternationalCollaboration.Scholarship
{
    public class ScholarshipController : Controller
    {
        // GET: Scholarship
        public ActionResult List()
        {
            ViewBag.pageTitle = "Danh sách học bổng";
            return View();
        }

        public ActionResult Delete_Scholar(string id)
        {
            try
            {
                string result = id;
                return Json("", JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
    }
}
