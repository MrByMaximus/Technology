using System;
using RBS_Core.Helpers;

namespace RBS_Core.Features.Technology
{
    public partial class TechnologyController : BaseController
    {
        public override void RefreshSession()
        {
            var now = DateTime.Now;

            var dateBegin = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            var dateEnd = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1);

            if (now.Hour >= 0 && now.Hour < 8)
            {
                dateBegin = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0).AddDays(-1);
                dateEnd = new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
            }
            else if (now.Hour >= 8 && now.Hour < 16)
            {
                dateBegin = new DateTime(now.Year, now.Month, now.Day, 7, 0, 0);
                dateEnd = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0);
            }
            else if (now.Hour >= 16)
            {
                dateBegin = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0);
                dateEnd = new DateTime(now.Year, now.Month, now.Day, 23, 0, 0);
            }

            HttpContext.Session.Set("DateBegin", dateBegin);
            HttpContext.Session.Set("DateEnd", dateEnd);
        }

    }
}