using Dan.Common;
using Dan.Common.Enums;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Plugin.Pseudo.Config;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Dan.Plugin.Pseudo;

/// <summary>
/// All plugins must implement IEvidenceSourceMetadata, which describes that datasets returned by this plugin. An example is implemented below.
/// </summary>
public class Metadata : IEvidenceSourceMetadata
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    public List<EvidenceCode> GetEvidenceCodes()
    {
        return
        [
            new EvidenceCode
            {
                EvidenceCodeName = PluginConstants.DatasetName_pk,
                EvidenceSource = PluginConstants.SourceName,
                BelongsToServiceContexts = [PluginConstants.PluginServiceContext],
                IsPublic = true,
                Values =
                [
                    new EvidenceValue
                    {
                        EvidenceValueName = "key",
                        ValueType = EvidenceValueType.String
                    },

                    new EvidenceValue
                    {
                        EvidenceValueName = "referenceValue",
                        ValueType = EvidenceValueType.String
                    },
                    new EvidenceValue
                    {
                        EvidenceValueName = "created",
                        ValueType = EvidenceValueType.DateTime
                    }
                ],
                Parameters =                 [
                    new EvidenceParameter
                    {
                        EvidenceParamName = "referenceValue",
                        ParamType = EvidenceParamType.String,
                        Required = true
                    }
                ],
            },
            new EvidenceCode
            {
                EvidenceCodeName = PluginConstants.DatasetName_upk,
                EvidenceSource = PluginConstants.SourceName,
                BelongsToServiceContexts = [PluginConstants.PluginServiceContext],
                IsPublic = true,
                Values =
                [
                    new EvidenceValue
                    {
                        EvidenceValueName = "key",
                        ValueType = EvidenceValueType.String
                    },

                    new EvidenceValue
                    {
                        EvidenceValueName = "result",
                        ValueType = EvidenceValueType.String
                    }
                ],
                Parameters =                 [
                    new EvidenceParameter
                    {
                        EvidenceParamName = "referenceValue",
                        ParamType = EvidenceParamType.String,
                        Required = true
                    },
                    new EvidenceParameter
                    {
                        EvidenceParamName = "identifier",
                        ParamType = EvidenceParamType.String,
                        Required = true
                    }
                ],
            }
        ];
    }


    /// <summary>
    /// This function must be defined in all DAN plugins, and is used by core to enumerate the available datasets across all plugins.
    /// Normally this should not be changed.
    /// </summary>
    /// <param name="req"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [Function(Constants.EvidenceSourceMetadataFunctionName)]
    public async Task<HttpResponseData> GetMetadataAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
        FunctionContext context)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(GetEvidenceCodes());
        return response;
    }

}
