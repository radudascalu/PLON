using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace PLON.Core
{
    public class PlonModelAttribute : ActionFilterAttribute
    {
        private const string ETagPlonHeader = "ETag-PLON";
        private const string IfMatchPlonHeader = "If-Match-PLON";

        private readonly Type modelType;

        public PlonModelAttribute(Type modelType)
        {
            this.modelType = modelType;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            IEnumerable<string> plonVersionHeader = new List<string>();
            var plonVersionHeaderExists = actionContext.ControllerContext.Request.Headers.TryGetValues(IfMatchPlonHeader, out plonVersionHeader);

            if (!plonVersionHeaderExists)
            {
                CreatePreconditionFailedResponse(actionContext);
                return;
            }

            int requestedVersion;
            var currentVersion = GetCurrentModelVersion();

            if (TryGetModelVersionFromETag(plonVersionHeader.First(), out requestedVersion)
                && requestedVersion != currentVersion)
            {
                CreatePreconditionFailedResponse(actionContext);
                return;
            }
        }

        private int GetCurrentModelVersion()
        {
            int hashCode = 0;
            foreach (var property in modelType.GetProperties())
                hashCode += property.GetHashCode();

            return hashCode;
        }

        private bool TryGetModelVersionFromETag(string eTag, out int version)
        {
            version = 0;
            if (eTag == null || eTag.Length < 5)
                return false;

            var versionString = eTag.Remove(eTag.Length - 1, 1).Remove(0, 3);

            return int.TryParse(versionString, out version);
        }

        private string GetModelPlonMetadata()
        {
            return Plon.Serialization.Metadata.getPlonMetadata(modelType);
        }

        private void CreatePreconditionFailedResponse(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.PreconditionFailed,
                GetModelPlonMetadata());
            actionContext.Response.Headers.Add(ETagPlonHeader, $"W/\"{GetCurrentModelVersion()}\"");
        }
    }
}