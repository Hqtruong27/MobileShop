using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Models.DataModels
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [DisplayName("Tên người dùng")]
        [Required(ErrorMessage ="Vui lòng nhập Họ Tên")]
        public string FullName { get; set; }
        public string Password { get; set; }
        [DisplayName("Địa chỉ Email")]
        public string Email { get; set; }
        [DisplayName("Số điện thoại")]
        public string Phone { get; set; }
        [DisplayName("Địa chỉ")]
        public string Address { get; set; }
        public string Avatar { get; set; }
        [DisplayName("Ngày tạo")]
        public DateTime CreateDate { get; set; }

        [DisplayName("Ngày Sinh")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{dd/MM/yyyy}")]
        public DateTime? DateofBirth { get; set; }
        
        public DateTime? ExpiredTime { get; set; }

        [DisplayName("Giới tính")]
        public byte? Gender { get; set; }

        [DisplayName("Trạng thái")]
        public byte? Status { get; set; }
        public bool isEmailVerified { get; set; }

        public System.Guid ActiveCode { get; set; }

        public string ResetPasswordCode { get; set; }
    }

    public class DateMinimumAgeAttribute : ValidationAttribute
    {
        public DateMinimumAgeAttribute(int minimumAge)
        {
            MinimumAge = minimumAge;
            ErrorMessage = "Yêu cầu {1} tuổi trở lên.";
        }

        public override bool IsValid(object value)
        {
            DateTime date;
            if ((value != null && DateTime.TryParse(value.ToString(), out date)))
            {
                return date.AddYears(MinimumAge) < DateTime.Now;
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, MinimumAge);
        }

        public int MinimumAge { get; }
    }
}
