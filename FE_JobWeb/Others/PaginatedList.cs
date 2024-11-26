namespace FE_JobWeb.Others
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public PaginatedList(List<T> items, int count, int currentPage, int pageSize)
        {
            Items = items != null ? items : new List<T>();
            TotalItems = count;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        public static PaginatedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            if (source == null)
            {
                source = Enumerable.Empty<T>(); // Nếu source null, trả về một danh sách rỗng
            }
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }

}
