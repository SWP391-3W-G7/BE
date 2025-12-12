using System.ComponentModel;

namespace DAL.Models
{
    public enum CampusEnum
    {
        [Description("Hà Nội")]
        HaNoi = 1,
        [Description("Hồ Chí Minh")]
        HoChiMinh = 2,
        [Description("Đà Nẵng")]
        DaNang = 3,
        [Description("Cần Thơ")]
        CanTho = 4,
        [Description("Nhà Văn Hóa Sinh Viên")]
        NhaVanHoaSinhVien = 5,
        [Description("Quy Nhơn")]
        QuyNhon = 6
    }
}
