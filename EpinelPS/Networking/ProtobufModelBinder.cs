using Google.Protobuf;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;

namespace EpinelPS.Networking;

public class ProtobufModelBinder : IModelBinder
{
    public async Task BindModelAsync(
       ModelBindingContext bindingContext)
    {
        var request =
            bindingContext.HttpContext.Request;

        var modelType =
            bindingContext.ModelType;
        if (!string.Equals(
              request.ContentType,
              "application/octet-stream+protobuf",
              StringComparison.OrdinalIgnoreCase))
        {
            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName,
                $"Expected correct content type.");

            bindingContext.Result =
                ModelBindingResult.Failed();

            bindingContext.HttpContext.Response.StatusCode =
                StatusCodes.Status415UnsupportedMediaType;

            return;
        }

        try
        {
            IMessage model = (IMessage)Activator.CreateInstance(modelType)!;
            if (request.ContentLength != 0)
            {
                model.MergeFrom(request.Body);
            }

            bindingContext.Result = ModelBindingResult.Success(model);
        }
        catch (Exception ex)
        {
            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName,
                ex.Message);

            bindingContext.Result = ModelBindingResult.Failed();
        }
    }
}
