using MVC.Business;

namespace MVC.Data
{
    public class EFRepositoryAPISQL : EFRepositoryAPI<ApplicationDbContextSQL>
    {
        public EFRepositoryAPISQL(ApplicationDbContextSQL context, EventHubController eventHubController) : base(context, eventHubController) { }

    }
}
