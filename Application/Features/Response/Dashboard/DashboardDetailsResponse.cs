namespace BisleriumBlog.Application.Features.Response.Dashboard;

public class DashboardDetailsResponse
{
    public DashboardCount DashboardCount { get; set; }

    public List<FamousBlog> FamousBlogs { get; set; }

    public List<FamousBlogger> FamousBloggers { get; set; }
}

public class DashboardCount
{
    public int Posts { get; set; }
    public int Comments { get; set; }

    public int UpVotes { get; set; }

    public int DownVotes { get; set; }

}

public class FamousBlog
{
    public int BlogId { get; set; }

    public string Blog { get; set; }
}

public class FamousBlogger
{
    public long BloggerId { get; set; }

    public string BloggerName { get; set; }
}

public class BlogDetails : FamousBlog
{
    public long BloggerId { get; set; }

    public int Popularity { get; set; }
}

public class BloggerDetails : FamousBlogger
{
    public int Popularity { get; set; }
}
