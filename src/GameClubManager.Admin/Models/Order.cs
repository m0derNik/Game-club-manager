using System;

namespace GameClubManager.Admin.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ComputerName { get; set; }
        public string UserName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
    }
} 