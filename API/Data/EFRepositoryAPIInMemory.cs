using MVC.Business;

namespace MVC.Data
{
    public class EFRepositoryAPIInMemory : EFRepositoryAPI<ApplicationDbContextInMemory>
    {
        public EFRepositoryAPIInMemory(ApplicationDbContextInMemory context, EventHubController eventHubController) : base(context, eventHubController) { }

    }
}
