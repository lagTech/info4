using Microsoft.EntityFrameworkCore;
using MVC.Business;
using MVC.Models;

namespace MVC.Data
{
    public class EFRepositoryAPINoSQL : EFRepositoryAPI<ApplicationDbContextNoSQL>
    {
        public EFRepositoryAPINoSQL(ApplicationDbContextNoSQL context, EventHubController eventHubController) : base(context, eventHubController) { }

    }
}
