using DemoShoppingWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DemoShoppingWebsite.Controllers
{
    public class HomeController : Controller
    {
        dbShoppingCarEntities db = new dbShoppingCarEntities();
        public ActionResult Index()
        {
            var products = db.table_Product.OrderByDescending(m => m.Id).ToList();
            return View(products);
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(table_Member Member)
        {
            //如果資料驗證未通過則回傳原本的View
            if (!ModelState.IsValid)
            {
                return View();
            }

            // 如果註冊的帳號不存在，才能執行新增與儲存
            var member = db.table_Member.Where(m => m.UserId == Member.UserId).FirstOrDefault();
            if (member == null)
            {
                db.table_Member.Add(Member);
                db.SaveChanges();

                return RedirectToAction("Login");
            }
            ViewBag.Message = "帳號已被使用，請重新註冊";
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string UserId, string Password)
        {
            //找出符合登入帳號與密碼的 Member資料
            var member = db.table_Member.Where(m => m.UserId == UserId && m.Password == Password).FirstOrDefault();
            if (member == null)
            {
                ViewBag.Message = "帳號or密碼錯誤，請重新確認登入";
                return View();
            }

            Session["Welcome"] = $"{member.Name} 您好";

            FormsAuthentication.RedirectFromLoginPage(UserId, true);

            return RedirectToAction("Index", "Member");
        }
        public ActionResult ShoppingCar()
        {
            string UserId = User.Identity.Name;

            var orderDetails = db.table_OrderDetail.Where(m => m.UserId == UserId && m.IsApproved == "否").ToList();
            return View(orderDetails);
        }

    }
}