using System;

namespace MeawMarket.Models
{
    public class SoldCat
    {
        public int Id { get; set; }           // Primary Key
        public int CatId { get; set; }        // อ้างอิงถึง Cat ที่ถูกขาย
        public int BuyerId { get; set; }      // ไอดีของผู้ซื้อ
        public DateTime SoldDate { get; set; } // วันที่ขาย
    }
}
