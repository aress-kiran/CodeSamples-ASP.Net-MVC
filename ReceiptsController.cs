//@filename: ReceiptsController.cs
//@description: This page contains following functionality.
//              1) Display list of Receipts
//              2) Edit receipt details
//              3) View receipt details
//@aboutauthor: Aress Softwares
//@copyright: Copyright (C) 2017, JustTap
//@created by: Aress Developer

using JustTap.Areas.Admin.Models;
using JustTap.Common;
using JustTap.Models;
using PagedList;
using System;
using PagedList.Mvc;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Data.Entity;

namespace JustTap.Areas.Admin.Controllers
{
    public class ReceiptsController : Controller
    {
        #region --- Global Variables ---

        JustTapDBEntities db = new JustTapDBEntities();
        int taxRate = Convert.ToInt32(ConfigurationManager.AppSettings["TaxRate"]);

        #endregion

        #region --- List of Receipts ---

        /// <summary>
        /// Displays receipt list to admin
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult ReceiptsList(string search, int? page)
        {
            try
            {
                int pageSize = 10;
                int pageNumber = (page ?? 1);

                Session["Search"] = search;

                Session["page"] = pageNumber;

                ReceiptViewModel model = new ReceiptViewModel();

                if (!string.IsNullOrEmpty(search))
                {
                    //search receipt by receipt no
                    model.ReceiptsList = db.GetAllReceiptsListToAdmin().Where(r => r.ReceiptNo.ToLower().Contains(search.ToLower())).OrderBy(r => r.BillingDate).ToList().ToPagedList(pageNumber, pageSize);
                }
                else
                {
                    model.ReceiptsList = db.GetAllReceiptsListToAdmin().OrderBy(r => r.BillingDate).ToList().ToPagedList(pageNumber, pageSize);
                }

                model.listcount = (pageNumber - 1) * 10;

                return View(model);
            }
            catch (Exception ex)
            {
                GlobalVariable.objWriteLog.PrintMessage(Server.MapPath("~/ErrorLog/"), ex.Message);

                throw ex;
            }
        }

        #endregion

        #region -- Edit Receipt ---
        /// <summary>
        /// Edit receipt details
        /// </summary>
        /// <param name="receiptId"></param>
        /// <returns></returns>
        public ActionResult EditReceipt(int receiptId)
        {
            try
            {
                if (string.IsNullOrEmpty(Session["Admin"].ToString()))
                {
                    return RedirectToAction("Login", "Login", new { area = "Admin" });
                }
                else
                {
                    int userId = ((tblUser)Session["Admin"]).UserId;

                    ReceiptViewModel oReceiptViewModel = (from r in db.GetReceiptById(receiptId)
                                                          select new ReceiptViewModel
                                                          {
                                                              ReceiptNo = r.ReceiptNo,
                                                              BillingDate = r.BillingDate,
                                                              Latitude = r.Latitude,
                                                              Longitude = r.Longitude,
                                                              LocationName = r.LocationName,
                                                              Note = r.Note,
                                                              Name = r.Name,
                                                              ReceiptId = r.ReceiptId,
                                                              TotalBillAmount = r.TotalBillAmount.Value,
                                                              SupplierName = r.SupplierName,
                                                              TaxId = r.TaxId,
                                                              TaxName = r.TaxName,
                                                              CreatedDate = r.CreatedDate,
                                                              ModifiedDate = r.ModifiedDate,

                                                              ListOfReceiptImages = (from m in db.tblReceiptImages
                                                                                     where m.ReceiptId == r.ReceiptId
                                                                                     select new ReceiptImageModel
                                                                                     {
                                                                                         ImageId = m.ImageId,
                                                                                         ImagePath = m.ImagePath
                                                                                     }).ToList(),

                                                              ListOfReceiptItem = (from p in db.tblItems
                                                                                   where p.ReceiptId == r.ReceiptId
                                                                                   select new ReceiptItemModel
                                                                                   {
                                                                                       ItemId = p.ItemId,
                                                                                       ItemName = p.ItemName,
                                                                                       Quantity = p.Quantity.Value,
                                                                                       Price = p.Price.Value,
                                                                                       IsDeleted = p.IsDeleted
                                                                                   }).ToList(),

                                                              TagList = (from urt in db.tblUserReceiptTags
                                                                         join tg in db.tblTags on urt.TagsId equals tg.TagId
                                                                         where urt.ReceiptId == r.ReceiptId
                                                                         select new Tags
                                                                         {
                                                                             TagId = tg.TagId,
                                                                             TagName = tg.TagName
                                                                         }).ToList()
                                                          }).FirstOrDefault();

                    List<tblTag> tags = (from t in db.tblTags
                                         where t.IsDeleted == false
                                         select t).ToList();

                    oReceiptViewModel.tagsList = new List<SelectListItem>();

                    List<SelectListItem> lsttag = new List<SelectListItem>();

                    Nullable<int>[] tagId = new Nullable<int>[tags.Count];

                    int j = 0;

                    // Binds tag
                    foreach (tblTag ta in tags)
                    {
                        Tags utag = oReceiptViewModel.TagList.Where(m => m.TagId == ta.TagId).FirstOrDefault();

                        SelectListItem i = new SelectListItem();
                        if (utag != null)
                        {
                            i.Text = ta.TagName.ToString();
                            i.Value = ta.TagId.ToString();
                            i.Selected = true;
                            tagId[j] = ta.TagId;
                            j++;
                        }
                        else
                        {
                            i.Text = ta.TagName.ToString();
                            i.Value = ta.TagId.ToString();
                            i.Selected = false;
                        }
                        lsttag.Add(i);
                    }

                    oReceiptViewModel.tagsList = lsttag;
                    oReceiptViewModel.TagIds = tagId;

                    return View(oReceiptViewModel);
                }
            }
            catch (Exception ex)
            {
                GlobalVariable.objWriteLog.PrintMessage(Server.MapPath("~/ErrorLog/"), ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Update receipt details
        /// </summary>
        /// <param name="oReceiptViewModel"></param>
        /// <param name="formCollection"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateReceipt(ReceiptViewModel oReceiptViewModel)
        {
            try
            {
                int userId = ((tblUser)Session["Admin"]).UserId;               

                // updates receipt details
                tblReceipt receipt = db.tblReceipts.Where(m => m.ReceiptId == oReceiptViewModel.ReceiptId).FirstOrDefault();

                receipt.SupplierName = oReceiptViewModel.SupplierName;
                receipt.BillingDate = oReceiptViewModel.BillingDate;
                receipt.LocationName = oReceiptViewModel.LocationName;
                receipt.Note = oReceiptViewModel.Note;
                receipt.TotalBillAmount = oReceiptViewModel.TotalBillAmount;

                db.Entry(receipt).State = EntityState.Modified;
                db.SaveChanges();

                // updates receipt items
                if (oReceiptViewModel.ListOfReceiptItem != null)
                {
                    foreach (var item in oReceiptViewModel.ListOfReceiptItem)
                    {
                        tblItem otblItem = db.tblItems.Where(m => m.ItemId == item.ItemId && m.ReceiptId == oReceiptViewModel.ReceiptId).FirstOrDefault();

                        otblItem.ItemName = item.ItemName;
                        otblItem.Quantity = item.Quantity;
                        otblItem.Price = item.Price;

                        db.Entry(otblItem).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                List<tblUserReceiptTag> userTags = (from d in db.tblUserReceiptTags
                                                    where d.ReceiptId == oReceiptViewModel.ReceiptId
                                                    select d).ToList();

                // if user tags does not have any tags then add all items
                if (userTags.Count == 0)
                {
                    if (oReceiptViewModel.TagIds != null)
                    {
                        foreach (int tid in oReceiptViewModel.TagIds)
                        {
                            tblUserReceiptTag receiptTag = new tblUserReceiptTag();

                            receiptTag.TagsId = tid;
                            receiptTag.ReceiptId = oReceiptViewModel.ReceiptId;
                            receiptTag.UserId = userId;

                            db.tblUserReceiptTags.Add(receiptTag);
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    // if tagids dont have any tag then delete all tags
                    if (oReceiptViewModel.TagIds == null)
                    {
                        foreach (tblUserReceiptTag usertag in userTags)
                        {
                            db.tblUserReceiptTags.Remove(usertag);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        foreach (tblUserReceiptTag usertag in userTags)
                        {
                            bool isAvailable = oReceiptViewModel.TagIds.Contains(usertag.TagsId.Value);

                            if (isAvailable == false)
                            {
                                tblUserReceiptTag receiptTag = (from y in db.tblUserReceiptTags
                                                                where y.TagsId == usertag.TagsId
                                                                select y).FirstOrDefault();

                                db.tblUserReceiptTags.Remove(receiptTag);
                                db.SaveChanges();
                            }
                        }

                        foreach (int tagId in oReceiptViewModel.TagIds)
                        {
                            var checkIsAvailable = userTags.Where(m => m.TagsId == tagId).FirstOrDefault();

                            // if not availble
                            if (checkIsAvailable == null)
                            {
                                tblUserReceiptTag receiptTag = new tblUserReceiptTag();

                                receiptTag.TagsId = tagId;
                                receiptTag.ReceiptId = oReceiptViewModel.ReceiptId;
                                receiptTag.UserId = userId;

                                db.tblUserReceiptTags.Add(receiptTag);
                                db.SaveChanges();
                            }
                        }
                    }
                }

                TempData["Success"] = "Receipt updated successfully.";
                return RedirectToAction("ReceiptsList", "Receipts", new { page = Convert.ToInt32(Session["page"]), search = Session["Search"] , area = "Admin" });
            }
            catch (Exception ex)
            {
                GlobalVariable.objWriteLog.PrintMessage(Server.MapPath("~/ErrorLog/"), ex.Message);
                throw ex;
            }            
        }
        #endregion

        #region --- Receipts Details ---

        /// <summary>
        /// receipt details
        /// </summary>
        /// <param name="receiptId"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReceiptDetails(int receiptId)
        {
            try
            {
                ReceiptViewModel oReceiptViewModel = (from r in db.GetReceiptById(receiptId)
                                                      select new ReceiptViewModel
                                                      {
                                                          BillingDate = r.BillingDate,
                                                          Latitude = r.Latitude,
                                                          Longitude = r.Longitude,
                                                          LocationName = r.LocationName,
                                                          Note = r.Note,
                                                          ReceiptId = r.ReceiptId,
                                                          TotalBillAmount = r.TotalBillAmount.Value,
                                                          SupplierName = r.SupplierName,
                                                          TaxId = r.TaxId,
                                                          CreatedDate = r.CreatedDate,
                                                          ModifiedDate = r.ModifiedDate,

                                                          ListOfReceiptImages = (from i in db.tblReceiptImages
                                                                                 where i.ReceiptId == r.ReceiptId
                                                                                 select new ReceiptImageModel
                                                                                 {
                                                                                     ImageId = i.ImageId,
                                                                                     ImagePath = i.ImagePath
                                                                                 }).ToList(),

                                                          ListOfReceiptItem = (from p in db.tblItems
                                                                               where p.ReceiptId == r.ReceiptId
                                                                               select new ReceiptItemModel
                                                                               {
                                                                                   ItemId = p.ItemId,
                                                                                   ItemName = p.ItemName,
                                                                                   Quantity = p.Quantity.Value,
                                                                                   Price = p.Price.Value,
                                                                                   IsDeleted = p.IsDeleted
                                                                               }).ToList()

                                                      }).FirstOrDefault();

                oReceiptViewModel.TaxAmount = CalculateTax(oReceiptViewModel.TotalBillAmount.Value);
                oReceiptViewModel.TotalBill = oReceiptViewModel.TaxAmount + oReceiptViewModel.TotalBillAmount;

                return View(oReceiptViewModel);
            }
            catch (Exception ex)
            {
                GlobalVariable.objWriteLog.PrintMessage(Server.MapPath("~/ErrorLog/"), ex.Message);

                throw ex;
            }
        }
        #endregion

        #region --- Calculate tax ---
        /// <summary>
        /// Calculates tax on total bill amount
        /// </summary>
        /// <param name="totalBillAmount"></param>
        /// <returns></returns>
        internal decimal CalculateTax(decimal totalBillAmount)
        {
            decimal tax = (totalBillAmount * taxRate) / 100;

            return (tax);
        }
        #endregion
    }
}