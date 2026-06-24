using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudClinic.Doctor.Models.DrProgressModels
{
    public class MonthlyProgressViewModel
    {
        public int Month { get; set; }
        public List<DailyTotalAppointments> TotalAppointments { get; set; }
        public List<DailyTotalVisits> TotalVisits { get; set; }
        public List<DailyTotalDrugsProfit> TotalDrugsProfit { get; set; }
        public List<DailyTotalLabFee> TotalLabFee { get; set; }
        public List<DailyTotalXrayFee> TotalXrayFee { get; set; }
        public List<DailyTotalDrFee> TotalDrFee { get; set; }
    }

    public class DailyTotalAppointments
    {
        public int DayOfMonth { get; set; }
        public int Sum { get; set; }
    }

    public class DailyTotalVisits
    {
        public int DayOfMonth { get; set; }
        public int Sum { get; set; }
    }

    public class DailyTotalDrugsProfit
    {
        public int DayOfMonth { get; set; }
        public int Sum { get; set; }
    }    
    
    public class DailyTotalDrFee
    {
        public int DayOfMonth { get; set; }
        public int Sum { get; set; }
    }

    public class DailyTotalLabFee
    {
        public int DayOfMonth { get; set; }
        public int Sum { get; set; }
    }

    public class DailyTotalXrayFee
    {
        public int DayOfMonth { get; set; }
        public int Sum { get; set; }
    }

    /// for yearly progress
    /// 
    public class YearlyProgressViewModel
    {
        public int Year { get; set; }
        public List<MonthlyTotalAppointments> TotalAppointments { get; set; }
        public List<MonthlyTotalVisits> TotalVisits { get; set; }
        public List<MonthlyTotalDrFee> TotalDrFee { get; set; }
        public List<MonthlyTotalDrugsProfit> TotalDrugsProfit { get; set; }
        public List<MonthlyTotalLabFee> TotalLabFee { get; set; }
        public List<MonthlyTotalXrayFee> TotalXrayFee { get; set; }
    }

    public class MonthlyTotalAppointments
    {
        public int MonthOfYear { get; set; }
        public int Sum { get; set; }
    }

    public class MonthlyTotalVisits
    {
        public int MonthOfYear { get; set; }
        public int Sum { get; set; }
    }

    public class MonthlyTotalDrugsProfit
    {
        public int MonthOfYear { get; set; }
        public int Sum { get; set; }
    }

    public class MonthlyTotalDrFee
    {
        public int MonthOfYear { get; set; }
        public int Sum { get; set; }
    }

    public class MonthlyTotalLabFee
    {
        public int MonthOfYear { get; set; }
        public int Sum { get; set; }
    }

    public class MonthlyTotalXrayFee
    {
        public int MonthOfYear { get; set; }
        public int Sum { get; set; }
    }
}
