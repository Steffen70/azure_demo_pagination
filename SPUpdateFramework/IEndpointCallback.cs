using Microsoft.AspNetCore.Mvc;

namespace SPUpdateFramework;

public interface IEndpointCallback
{
    ActionResult OnEndpointCallback(string json);
}