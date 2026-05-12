using Microsoft.AspNetCore.Authorization;

namespace WebApiShop.Attributes
{
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        public AdminOnlyAttribute() : base()
        {
            Roles = "Admin";
        }
    }
}
