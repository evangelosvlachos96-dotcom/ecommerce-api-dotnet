using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EcommerceApp.Filters
{
    public class FromJwtUserIdAttribute : ModelBinderAttribute
    {
        public FromJwtUserIdAttribute() : base(typeof(JwtUserIdBinder)) {}
    }

    public class JwtUserIdBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var user = bindingContext.HttpContext.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "userid");

                if (userIdClaim != null)
                {
                    // Set the result for the parameter
                    bindingContext.Result = ModelBindingResult.Success(userIdClaim.Value);
                    return Task.CompletedTask;
                }
            }

            // If no valid userId is found
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }
    }
}

