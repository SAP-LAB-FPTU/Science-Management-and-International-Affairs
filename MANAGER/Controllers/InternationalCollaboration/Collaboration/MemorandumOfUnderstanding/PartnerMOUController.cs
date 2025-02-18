﻿using BLL.InternationalCollaboration.Collaboration.MemorandumOfUnderstanding;
using ENTITIES.CustomModels.InternationalCollaboration.Collaboration.MemorandumOfUnderstanding.MOU;
using ENTITIES.CustomModels.InternationalCollaboration.Collaboration.MemorandumOfUnderstanding.MOUPartner;
using MANAGER.Support;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace MANAGER.Controllers.InternationalCollaboration.Collaboration.MemorandumOfUnderstanding
{
    public class PartnerMOUController : Controller
    {
        private static readonly PartnerMOURepo mou = new PartnerMOURepo();
        [Auther(RightID = "6")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Get_MOU_History(string mou_partner_id)
        {
            try
            {
                List<PartnerHistory> historyList = mou.ListMOUPartnerHistory(int.Parse(mou_partner_id));
                return Json(new { success = true, data = historyList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Get_Data_Partner_Detail(string mou_partner_id)
        {
            try
            {
                string result = mou_partner_id;
                ListMOUPartner mouObj = mou.GetMOUPartnerDetail(int.Parse(mou_partner_id));
                return Json(mouObj, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ViewListMOUPartner(string partner_name, string nation, string specialization)
        {
            try
            {
                if (Session["mou_detail_id"] is null)
                {
                    return Redirect("../MOU/List");
                }
                else
                {
                    string id = Session["mou_detail_id"].ToString();
                    string nation_name = nation is null ? "" : nation;
                    string specialization_name = specialization is null ? "" : specialization;
                    List<ListMOUPartner> mouList = mou.listAllMOUPartner(partner_name, nation_name, specialization_name, int.Parse(id));
                    return Json(new { success = true, data = mouList }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        [Auther(RightID = "6")]
        public ActionResult Add_Mou_Partner(PartnerInfo input)
        {
            try
            {
                if (Session["mou_detail_id"] is null)
                {
                    return Redirect("../MOU/List");
                }
                else
                {
                    BLL.Authen.LoginRepo.User user = (BLL.Authen.LoginRepo.User)Session["User"];
                    string id = Session["mou_detail_id"].ToString();
                    mou.AddMOUPartner(input, int.Parse(id), user);
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        [Auther(RightID = "6")]
        public ActionResult Edit_Mou_Partner(PartnerInfo input)
        {
            try
            {
                if (Session["mou_detail_id"] is null)
                {
                    return Redirect("../MOU/List");
                }
                else
                {
                    BLL.Authen.LoginRepo.User user = (BLL.Authen.LoginRepo.User)Session["User"];
                    string id = Session["mou_detail_id"].ToString();
                    mou.EditMOUPartner(input, int.Parse(id), input.mou_partner_id, user);
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        [Auther(RightID = "6")]
        public ActionResult deletePartnerMOU(int mou_bonus_id)
        {
            try
            {
                if (Session["mou_detail_id"] is null)
                {
                    return Redirect("../MOU/List");
                }
                else
                {
                    string id = Session["mou_detail_id"].ToString();
                    mou.DeleteMOUPartner(int.Parse(id), mou_bonus_id);
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new HttpStatusCodeResult(400);
            }
        }
        public ActionResult isLastPartnerMOU()
        {
            try
            {
                if (Session["mou_detail_id"] is null)
                {
                    return Redirect("../MOU/List");
                }
                else
                {
                    string id = Session["mou_detail_id"].ToString();
                    bool isLastPartner = mou.CheckLastPartner(int.Parse(id));
                    return Json(isLastPartner);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new HttpStatusCodeResult(400);
            }
        }
        public ActionResult CheckPartnerExistedInMOU(string partner_name)
        {
            try
            {
                if (Session["mou_detail_id"] is null)
                {
                    return Redirect("../MOU/List");
                }
                else
                {
                    string id = Session["mou_detail_id"].ToString();
                    string partner = mou.CheckPartnerExistedInMOU(int.Parse(id), partner_name);
                    return Json(partner);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new HttpStatusCodeResult(400);
            }
        }
        public ActionResult CheckPartnerExistedInEditMOU(string partner_name, int mou_partner_id)
        {
            try
            {
                string partner = mou.CheckPartnerExistedInMOU(mou_partner_id, partner_name);
                return Json(partner);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new HttpStatusCodeResult(400);
            }
        }
        public ActionResult CheckMOAPartnerExistedInMOU(int mou_partner_id)
        {
            try
            {
                if (Session["mou_detail_id"] is null)
                {
                    return Redirect("../MOU/List");
                }
                else
                {
                    bool isExisted = mou.CheckMOAPartnerExistedInMOU(mou_partner_id);
                    return Json(isExisted);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return new HttpStatusCodeResult(400);
            }
        }
        public ActionResult CheckMOUBonusExisted(int mou_partner_id)
        {
            try
            {
                bool isInvalid = mou.IsMOUBonusExisted(mou_partner_id);
                return Json(isInvalid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //return Json("", JsonRequestBehavior.AllowGet);
                return new HttpStatusCodeResult(400);
            }
        }
    }
}