using Microsoft.AspNetCore.Mvc;

namespace SPPaginationDemo.Model
{
    public interface IEndpointCallback
    {
        ActionResult OnEndpointCallback(string json);
    }
}
