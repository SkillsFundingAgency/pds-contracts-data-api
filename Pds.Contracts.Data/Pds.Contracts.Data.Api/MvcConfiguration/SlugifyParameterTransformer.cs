using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace Pds.Contracts.Data.Api.MvcConfiguration
{
    /// <summary>
    /// Transforms route values to strings for use in URIs by slugifying the value.
    /// For example, an action "GetValue" will be represented in the URI as "get-value".
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-3.1#use-a-parameter-transformer-to-customize-token-replacement"/>.
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        private static readonly Regex _parameterPattern = new Regex("([a-z])([A-Z])", RegexOptions.Compiled);

        /// <inheritdoc/>
        public string TransformOutbound(object value)
        {
            if (value == null)
            {
                return null;
            }

            return _parameterPattern.Replace(value.ToString(), "$1-$2").ToLower();
        }
    }
}