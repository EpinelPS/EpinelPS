using Google.Protobuf;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Paseto;
using Paseto.Builder;
using EpinelPS.Database;

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

        if (request.Headers.ContainsKey("Authorization"))
        {
            try
            {
                PasetoTokenValidationResult encryptionToken = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Local)
                           .WithKey(JsonDb.Instance.LauncherTokenKey, Encryption.SymmetricKey)
                           .Decode(request.Headers.Authorization.ToString().Replace("Bearer ", ""), new PasetoTokenValidationParameters() { ValidateLifetime = true });

                if (encryptionToken.IsValid)
                {
                    var id = ((System.Text.Json.JsonElement)encryptionToken.Paseto.Payload["userId"]).GetUInt64();

                    if (id == 0) throw new Exception("403");

                    bindingContext.HttpContext.Items["UserID"] = id;
                }
            }
            catch
            {

            }
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
