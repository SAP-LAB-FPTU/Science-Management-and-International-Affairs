﻿using BLL.Authen;
using BLL.ScienceManagement.Comment;
using BLL.ScienceManagement.MasterData;
using BLL.ScienceManagement.Paper;
using ENTITIES;
using ENTITIES.CustomModels;
using ENTITIES.CustomModels.ScienceManagement.Comment;
using ENTITIES.CustomModels.ScienceManagement.Paper;
using ENTITIES.CustomModels.ScienceManagement.ScientificProduct;
using GUEST.Models;
using GUEST.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using User.Models;

namespace GUEST.Controllers
{
    public class PaperController : Controller
    {
        private readonly PaperRepo pr = new PaperRepo();
        private readonly MasterDataRepo md = new MasterDataRepo();
        private readonly CommentRepo cr = new CommentRepo();

        [Auther(RightID = "26")]
        public ActionResult AddRequest()
        {
            ViewBag.title = "Đăng ký khen thưởng bài báo";
            var pagesTree = new List<PageTree>
            {
                new PageTree("Đăng ký khen thưởng bài báo","/Paper/AddRequest"),
            };
            ViewBag.pagesTree = pagesTree;

            string lang = "";
            if (Request.Cookies["language_name"] != null)
            {
                lang = Request.Cookies["language_name"].Value;
            }
            List<SpecializationLanguage> listSpec = md.GetSpec(lang);
            ViewBag.listSpec = listSpec;

            List<PaperCriteria> listCriteria = md.GetPaperCriteria();
            ViewBag.listCriteria = listCriteria;

            string link = pr.GetLinkPolicy();
            ViewBag.link = link;

            return View();
        }

        [HttpPost]
        public JsonResult addFile(HttpPostedFileBase file, string name, DetailPaper paper)
        {
            Account acc = CurrentAccount.Account(Session);
            Google.Apis.Drive.v3.Data.File f = GoogleDriveService.UploadResearcherFile(file, name, 2, acc.email);
            File fl = new File
            {
                link = f.WebViewLink,
                file_drive_id = f.Id,
                name = name
            };
            string mess = pr.AddFile(fl);
            return Json(new { mess, id = fl.file_id });
        }

        [HttpPost]
        public JsonResult AddPaper(HttpPostedFileBase file, string name, DetailPaper paper, string input)
        {
            if (paper == null)
            {
                paper = new DetailPaper();
                JObject @object = JObject.Parse(input);

                paper.name = (string)@object["name"];
                paper.date_string = (string)@object["date_string"];
                paper.link_doi = (string)@object["link_doi"];
                paper.link_scholar = (string)@object["link_scholar"];
                paper.paper_type_id = (int)@object["paper_type_id"];
                paper.journal_name = (string)@object["journal_name"];
                paper.vol = (string)@object["vol"];
                paper.page = (string)@object["page"];
                paper.company = (string)@object["company"];
                paper.index = (string)@object["index"];
                paper.note_domestic = (string)@object["note_domestic"];

                //paper = @object["DetailPaper"].ToObject<DetailPaper>();
            }

            if (file != null)
            {
                Account acc = CurrentAccount.Account(Session);
                Google.Apis.Drive.v3.Data.File f = GoogleDriveService.UploadResearcherFile(file, name, 2, acc.email);
                File fl = new File
                {
                    link = f.WebViewLink,
                    file_drive_id = f.Id,
                    name = name
                };
                string mess = pr.AddFile(fl);
                if (mess == "ss") paper.file_id = fl.file_id;
            }
            paper.publish_date = DateTime.ParseExact(paper.date_string, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            Paper p = pr.AddPaper(paper);
            if (p != null) return Json(new { id = p.paper_id, mess = "ss" }, JsonRequestBehavior.AllowGet);
            else return Json(new { mess = "ff" });
        }

        [HttpPost]
        public JsonResult AddRequest(RequestPaper request, string daidien)
        {
            int account_id = CurrentAccount.AccountID(Session);
            BaseRequest b = pr.AddBaseRequest(account_id);
            string mess = pr.AddRequestPaper(b.request_id, request, daidien);
            return Json(new { mess });
        }

        [HttpPost]
        public JsonResult AddAuthor(List<AddAuthor> people, string paper_id)
        {
            string mess = pr.AddAuthor(people, paper_id);
            return Json(new { mess });
        }

        [HttpPost]
        public JsonResult AddCriteria(List<CustomCriteria> criteria, string paper_id)
        {
            string mess = pr.AddCriteria(criteria, paper_id);
            return Json(new { mess });
        }

        //[HttpPost]
        [Auther(RightID = "26")]
        public ActionResult Edit(string id)
        {
            ViewBag.title = "Chỉnh sửa khen thưởng bài báo";
            var pagesTree = new List<PageTree>
            {
                new PageTree("Chỉnh sửa khen thưởng bài báo","/Paper/Edit"),
            };
            ViewBag.pagesTree = pagesTree;

            DetailPaper item = pr.GetDetail(id);
            ViewBag.Paper = item;
            ViewBag.ckEdit = item.status_id;

            int request_id = item.request_id;

            string lang = "";
            if (Request.Cookies["language_name"] != null)
            {
                lang = Request.Cookies["language_name"].Value;
            }
            List<SpecializationLanguage> listSpec = md.GetSpec(lang);
            ViewBag.listSpec = listSpec;

            List<PaperCriteria> listCriteria = md.GetPaperCriteria();
            ViewBag.listCriteria = listCriteria;

            List<ListCriteriaOfOnePaper> listCriteriaOne = pr.GetCriteria(id);
            ViewBag.listCriteriaOne = listCriteriaOne;

            //List<DetailComment> listCmt = cr.GetComment(request_id);
            //ViewBag.cmt = listCmt;
            ViewBag.request_id = request_id;
            ViewBag.id = id;

            return View();
        }
        [HttpPost]
        public JsonResult editPaper(string paper_id, Paper paper)
        {
            string mess = pr.UpdatePaper(paper_id, paper);
            return Json(new { mess });
        }

        [HttpPost]
        public JsonResult editRequest(RequestPaper item, string daidien)
        {
            string mess = pr.UpdateRequest(item, daidien);
            return Json(new { mess, id = item.paper_id });
        }

        [HttpPost]
        public JsonResult editCriteria(List<CustomCriteria> criteria, string paper_id)
        {
            string mess = pr.UpdateCriteria(criteria, paper_id);
            return Json(new { mess });
        }

        [HttpPost]
        public JsonResult editAuthor(List<AddAuthor> people, string paper_id)
        {
            string mess = pr.UpdateAuthor(people, paper_id);
            return Json(new { mess, id = paper_id });
        }

        [HttpPost]
        public JsonResult listAuthor(string id)
        {
            string lang = "";
            if (Request.Cookies["language_name"] != null)
            {
                lang = Request.Cookies["language_name"].Value;
            }
            List<AuthorInfoWithNull> listAuthor = pr.GetAuthorPaper(id, lang);
            string ms = pr.GetAuthorReceived(id);
            if (ms == null) ms = "";
            return Json(new { author = listAuthor, ms });
        }

        [HttpPost]
        public JsonResult editPaperAuthorReward(List<AddAuthor> people, int paper_id)
        {
            string mess = pr.UpdateRewardAuthorAfterDecision(people, paper_id);
            return Json(new { mess });
        }

        [HttpPost]
        public JsonResult getDecision(int id)
        {
            List<string> link = pr.GetDecisionLink(id);
            return Json(new { link }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddPaper_Refactor(HttpPostedFileBase file, string paper_input, string criteria_input, string author_input, string request_input, string daidien)
        {
            JObject @object_paper = JObject.Parse(paper_input);
            JObject @object_criteria = JObject.Parse(criteria_input);
            JObject @object_author = JObject.Parse(author_input);
            JObject @object_request = JObject.Parse(request_input);

            DetailPaper paper = new DetailPaper
            {
                name = (string)@object_paper["name"],
                date_string = (string)@object_paper["date_string"],
                link_doi = (string)@object_paper["link_doi"],
                link_scholar = (string)@object_paper["link_scholar"],
                paper_type_id = (int)@object_paper["paper_type_id"],
                journal_name = (string)@object_paper["journal_name"],
                vol = (string)@object_paper["vol"],
                page = (string)@object_paper["page"],
                company = (string)@object_paper["company"],
                index = (string)@object_paper["index"],
                note_domestic = (string)@object_paper["note_domestic"],
            };
            if (paper.date_string != null && paper.date_string != "")
            {
                DateTime temp_date = DateTime.ParseExact(paper.date_string, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                paper.publish_date = temp_date;
            }

            List<CustomCriteria> criteria = new List<CustomCriteria>();
            foreach (var item in object_criteria["criteria"])
            {
                CustomCriteria cc = new CustomCriteria()
                {
                    name = (string)item["name"],
                    link = (string)item["link"]
                };
                criteria.Add(cc);
            }

            List<AddAuthor> author = new List<AddAuthor>();
            foreach (var item in @object_author["people"])
            {
                AddAuthor temp = new AddAuthor()
                {
                    name = (string)item["name"],
                    email = (string)item["email"],
                };
                if ((int)item["office_id"] != 0)
                {
                    temp.bank_number = (string)item["bank_number"].ToString();
                    temp.bank_branch = (string)item["bank_branch"];
                    temp.tax_code = (string)item["tax_code"].ToString();
                    temp.identification_number = (string)item["identification_number"];
                    temp.mssv_msnv = (string)item["mssv_msnv"];
                    temp.office_id = (int)item["office_id"];
                    temp.contract_id = 1;
                    temp.title_id = (int)item["title_id"];
                    temp.is_reseacher = (bool)item["is_reseacher"];
                    temp.identification_file_link = (string)item["identification_file_link"];
                }
                author.Add(temp);
            }

            RequestPaper request = new RequestPaper()
            {
                specialization_id = (int)object_request["specialization_id"],
                type = Int32.Parse((string)object_request["type"]),
                reward_type = Int32.Parse((string)object_request["reward_type"]),
            };

            File fl = new File();
            Account acc = CurrentAccount.Account(Session);
            if (file != null)
            {
                Google.Apis.Drive.v3.Data.File f = GoogleDriveService.UploadResearcherFile(file, paper.name, 2, acc.email);
                fl.link = f.WebViewLink;
                fl.file_drive_id = f.Id;
                fl.name = paper.name;
                //ENTITIES.File fl = new ENTITIES.File
                //{
                //    link = f.WebViewLink,
                //    file_drive_id = f.Id,
                //    name = paper.name
                //};
                string m = pr.AddFile(fl);
                if (m == "ss") paper.file_id = fl.file_id;
            }

            int id = pr.AddPaper_Refactor(paper, criteria, author, request, acc.account_id, fl, daidien);
            bool mess = true;
            if (id == 0) mess = false;

            return Json(new { mess, id }, JsonRequestBehavior.AllowGet);
        }
    }
}