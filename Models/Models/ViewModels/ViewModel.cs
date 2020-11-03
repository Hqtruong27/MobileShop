using Models.Models.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Models.ViewModels
{
    //Customer
    //Register and login Viewmodel Customer.....
    public class CustomerViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ Tên")]
        public string FullName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [RegularExpression("^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@" + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$", ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Mật khẩu chứa ít nhất 6 ký tự")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Vui lòng nhập lại Password")]
        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginCustomer
    {
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [RegularExpression("^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@" + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$", ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
        [DataType(DataType.Password)]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Mật khẩu chứa ít nhất 6 ký tự")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Mật khẩu chứa ít nhất 6 ký tự")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmNewPassword { get; set; }
    }
    /// <summary>
    /// Models Reset pwd
    /// </summary>
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Mật khẩu chứa ít nhất 6 ký tự")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không khớp")]
        [DataType(DataType.Password)]
        public string confrimPassword { get; set; }

        [Required]
        public string ResetCode { get; set; }
    }

    //Admin
    /// <summary>
    /// Product Create View Models
    /// </summary>
    public class ProductViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        public string ProductName { get; set; }

        public int CategoryId { get; set; }

        public int ProviderId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá nhập")]
        [Range(0.0, Double.MaxValue, ErrorMessage = "Giá nhập vào không được nhỏ hơn 0")]
        public double PriceIn { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán")]
        [Range(0.0, Double.MaxValue, ErrorMessage = "Giá xuất không được nhỏ hơn 0")]
        public double PriceOut { get; set; }

        public int Discount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ảnh")]
        public string FeatureImage { get; set; }

        public string Images { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thông số kỹ thuật")]
        public string Specifications { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thông tin chi tiết")]
        public string ProductDetail { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái sản phẩm")]
        public byte Condition { get; set; }

        public bool? Status { get; set; }
        public virtual ICollection<ProductAttr> ProductAttrs { get; set; }
    }

    /// <summary>
    /// Login, create and Edit Users
    /// </summary>
    public class UserLogin
    {
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [RegularExpression("^[_A-Za-z0-9-\\+]+(\\.[_A-Za-z0-9-]+)*@" + "[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*(\\.[A-Za-z]{2,})$", ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Mật khẩu chứa ít nhất 6 ký tự")]
        public string Password { get; set; }
    }
    public class EditUser
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        public string ConfirmPassword { get; set; }
        public string Phone { get; set; }
    }
    public class CreateUser
    {
        [Required(ErrorMessage ="Vui lòng nhập họ và tên")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu không khớp")]
        [Required(ErrorMessage = "Nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression("^[0][1-9][0-9]{8}$", ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò cho nhân viên")]
        public string GroupId { get; set; }
    }
    public class ChangePassword
    {
        
        public string Password { get; set; }
        
        public string NewPassword { get; set; }
        
        public string ConfirmPassword { get; set; }
    }
    public class CreateCategories
    {
        public string CategoryName { get; set; }
        public int OrderBy { get; set; }
        public byte Status { get; set; }
        public int? ParentId { get; set; }
        public int? CategoryId { get; set; }
    }

    /// <summary>
    /// Table User Join Table groups
    /// </summary>
    public class UsersGroup
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Avatar { get; set; }

        public string GroupName { get; set; }

        public byte Status { get; set; }

        public string Background { get; set; }

        public string GroupId { get; set; }
    }

    //Table typeAttr Join Table Attribute
    public class AttributeJoinTypeAttr
    {
        public int AttrId { get; set; }
        public string TypeName { get; set; }
        public string AttrName { get; set; }
        public string Value { get; set; }
        public byte Status { get; set; }
    }

    //List News Join admin get Name admin
    public class NewsJoinAdmin
    {
        public int NewsId { get; set; }

        public string NewsTitle { get; set; }

        [DisplayName("Ảnh minh hoạ")]
        public string FeatureImage { get; set; }

        [DisplayName("Mô tả ngắn")]
        public string ShortDescription { get; set; }

        [DisplayName("Mô tả chi tiết")]
        public string Description { get; set; }

        [DisplayName("Lượt xem")]
        public int? CountView { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm tt}")]
        public DateTime Created { get; set; }

        [DisplayName("Trạng thái")]
        public int Status { get; set; }

        public int UserId { get; set; }

        public string FullName { get; set; }
    }


    //Chart data
    public class ChartData
    {
        public string MonthOfYear { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public string MonthName
        {
            get
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.Month);
            }
        }
        public int Total { get; set; }
        public double TotalPrice { get; set; }

    }
}
