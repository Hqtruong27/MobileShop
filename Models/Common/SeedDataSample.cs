using Models.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
    public static class SeedDataSample
    {
        public static void BussinessSample(MobileShopContext context)
        {
            if (context.Businesses.Count() != 0)
            {
                context.Businesses.AddOrUpdate(
                    new Business { BusinessId = "Cart", BusinessName = "Giỏ Hàng" },
                    new Business { BusinessId = "Categories", BusinessName = "Danh mục sản phẩm, Thương Hiệu" },
                    new Business { BusinessId = "News", BusinessName = "Tin tức" },
                    new Business { BusinessId = "Products", BusinessName = "Sản phẩm" },
                    new Business { BusinessId = "TypeAttr", BusinessName = "Thuộc tính" },
                    new Business { BusinessId = "Users", BusinessName = "Quản lý nhân viên" });
                context.SaveChanges();
            }
        }
    }
}
