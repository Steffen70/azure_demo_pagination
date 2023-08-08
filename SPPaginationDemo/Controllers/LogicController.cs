using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;

namespace SPPaginationDemo.Controllers;

[ApiController]
[Route("server-call/")]
public class LogicController : Controller
{
    [HttpPost("callback/{typeName}/{methodName}")]
    public async Task<ActionResult> ServerCall(string typeName, string methodName)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);

        var json = await reader.ReadToEndAsync();

        // deserialize Params from json
        var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        // create instance of type by typeName
        var type = GetType(typeName);
        var instance = Activator.CreateInstance(type);

        // get method by methodName
        var method = type.GetMethod(methodName);

        // if method is null return bad request with error message
        if (method == null)
            return BadRequest($"Method {methodName} not found");

        var result = parameters == null ? method.Invoke(instance, null) : method.Invoke(instance, parameters.Values.ToArray());
        return Ok(result);
    }

    private static readonly List<Type> CachedTypes = new();
    private Type GetType(string typeName)
    {
        var type = CachedTypes.FirstOrDefault(c => c.FullName == typeName);

        if (type != null)
            return type;

        type = Type.GetType(typeName);

        if (type != null)
            return type;

        var namespaceString = typeName[..typeName.LastIndexOf('.')];
        var assembly = Assembly.Load(namespaceString);

        // get Type from assembly by typeName

        type = assembly.GetType(typeName);

        if (type == null) throw new Exception($"Type {typeName} not found");

        CachedTypes.Add(type);
        return type;
    }
}