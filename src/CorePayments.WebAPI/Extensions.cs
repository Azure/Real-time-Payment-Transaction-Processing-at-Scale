using CorePayments.Infrastructure;

namespace CorePayments.WebAPI
{
    internal static class Extensions
    {
        public static (int, int) GetPagingQuery(this HttpRequest req)
        {
            int offset = int.TryParse(req.Query["offset"], out offset) ? offset : 0;
            int limit = int.TryParse(req.Query["limit"], out limit) ? limit : Constants.DefaultPageSize;

            return (offset, limit);
        }

        public static (DateTime?, DateTime?) GetDateRangeQuery(this HttpRequest req)
        {
            DateTime? startDate = DateTime.TryParse(req.Query["startDate"], out DateTime start) ? start : null;
            DateTime? endDate = DateTime.TryParse(req.Query["endDate"], out DateTime end) ? end : null;

            return (startDate, endDate);
        }
    }
}
