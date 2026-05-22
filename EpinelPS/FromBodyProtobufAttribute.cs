using EpinelPS.Networking;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EpinelPS;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromBodyProtobufAttribute : ModelBinderAttribute
{
    public FromBodyProtobufAttribute() : base(typeof(ProtobufModelBinder))
    {
        BindingSource = BindingSource.Body;
    }
}
