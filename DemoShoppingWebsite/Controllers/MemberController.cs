using DemoShoppingWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace DemoShoppingWebsite.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        dbShoppingCarEntities db = new dbShoppingCarEntities();

        // GET: Member
        public ActionResult Index()
        {
            var products = db.table_Product.OrderByDescending(m => m.Id).ToList();
            return View("../Home/Index", "_LayoutMember", products);

            // viewName也可以將完整路徑寫出來，但上方寫法較為簡潔
            //return View("~/Views/Home/Index.cshtml", "_LayoutMember", products);
        }
        public ActionResult Logout()
        {
            //using System.Web.Security;
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }
        public ActionResult ShoppingCar()
        {
            string UserId = User.Identity.Name;

            var orderDetails = db.table_OrderDetail.Where(m => m.UserId == UserId && m.IsApproved == "否").ToList();
            return View(orderDetails);
        }
        [HttpPost]
        public ActionResult ShoppingCar(string Receiver, string Email, string Address)
        {
            string userId = User.Identity.Name;
            string guid = Guid.NewGuid().ToString(); //產生隨機訂單編號

            //加入訂單至 table_Order 資料表
            var order = new table_Order();
            order.OrderGuid = guid;
            order.UserId = userId;
            order.Receiver = Receiver;
            order.Email = Email;
            order.Address = Address;
            order.Date = DateTime.Now;
            db.table_Order.Add(order);

            //訂單加入後，需一併更新訂單明細內容
            var carList = db.table_OrderDetail.Where(m => m.IsApproved == "否" && m.UserId == userId).ToList();
            foreach (var item in carList)
            {
                item.OrderGuid = guid;
                item.IsApproved = "是";
            }
            db.SaveChanges();
            return RedirectToAction("OrderList");
        }
        public ActionResult AddCar(string ProductId)
        {
            //取得目前通過驗證的使用者名稱
            string userId = User.Identity.Name;

            //取得該使用者目前購物車內是否已有此商品，且尚未形成訂單的資料
            var currentCar = db.table_OrderDetail
                .Where(m => m.ProductId == ProductId && m.IsApproved == "否" && m.UserId == userId).FirstOrDefault();
            if (currentCar == null)
            {
                //如果篩選條件資料為null，代表要新增全新一筆訂單明細資料
                //將產品資料欄位一一對照至訂單明細的欄位
                var product = db.table_Product.Where(m => m.ProductId == ProductId).FirstOrDefault();
                var orderDetail = new table_OrderDetail();
                orderDetail.UserId = userId;
                orderDetail.ProductId = product.ProductId;
                orderDetail.Name = product.Name;
                orderDetail.Price = product.Price;
                orderDetail.Quantity = 1;
                orderDetail.IsApproved = "否";
                db.table_OrderDetail.Add(orderDetail);
            }
            else
            {
                //如果購物車已有此商品，僅需將數量加1
                currentCar.Quantity++;
            }

            //儲存資料庫並導至購物車檢視頁面
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }
        public ActionResult DeleteCar(int Id)
        {
            var orderDetails = db.table_OrderDetail.Where(m => m.Id == Id).FirstOrDefault();

            db.table_OrderDetail.Remove(orderDetails);
            db.SaveChanges();
            return RedirectToAction("ShoppingCar");
        }
        public ActionResult OrderList()
        {
            string userId = User.Identity.Name;
            var orders = db.table_Order.Where(m => m.UserId == userId).OrderByDescending(m => m.Date).ToList();
            return View(orders);
        }
        public ActionResult OrderDetail(string OrderGuid)
        {
            var orderDetails = db.table_OrderDetail.Where(m => m.OrderGuid == OrderGuid).ToList();
            return View(orderDetails);
        }
    }
}