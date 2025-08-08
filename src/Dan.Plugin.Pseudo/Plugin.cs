using Dan.Common.Exceptions;
using Dan.Common.Extensions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Pseudo.Config;
using Dan.Plugin.Pseudo.Models;
using Dan.Pseudo.Models;
using Dan.Pseudo.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
namespace Dan.Plugin.Pseudo;

public class Plugin
{
    private readonly IEvidenceSourceMetadata _evidenceSourceMetadata;
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly Settings _settings;
    private readonly IPluginMemoryCacheProvider _memoryCache;

    public Plugin(ILoggerFactory loggerFactory,IOptions<Settings> settings,IEvidenceSourceMetadata evidenceSourceMetadata,IPluginMemoryCacheProvider memoryCacheProvider)
    {        
        _logger = loggerFactory.CreateLogger<Plugin>();
        _settings = settings.Value;
        _evidenceSourceMetadata = evidenceSourceMetadata;
        _memoryCache = memoryCacheProvider;
    }

    [Function(PluginConstants.DatasetName_pk)]
    public async Task<HttpResponseData> GetKey(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext context)
    {  
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();

        return await EvidenceSourceResponse.CreateResponse(req,
            () => GetKeyEvidenceValues(evidenceHarvesterRequest));
    }

    [Function(PluginConstants.DatasetName_upk)]
    public async Task<HttpResponseData> GetValue(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext context)
    {
        var evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();

        return await EvidenceSourceResponse.CreateResponse(req,
            () => UseKeyEvidenceValues(evidenceHarvesterRequest));
    }

    private async Task<List<EvidenceValue>> GetKeyEvidenceValues(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        var refVal = evidenceHarvesterRequest.TryGetParameter("referenceValue", out string referenceValue) ? referenceValue : string.Empty;

        var (success, keymodel) = await _memoryCache.TryGet<KeyModel>(refVal);       

        if (!success)
        {
            keymodel = new KeyModel
            {
                Key = KeyGenerator.GenerateKey(),
                ReferenceValue = refVal,
                CreatedDateTime = DateTime.UtcNow
            };

            keymodel = await _memoryCache.SetCache<KeyModel>(refVal, keymodel, TimeSpan.FromDays(_settings.KeyCacheTimeToLiveDays));
        }

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, PluginConstants.DatasetName_pk);
        ecb.AddEvidenceValue("key", Convert.ToBase64String(keymodel.Key), PluginConstants.SourceName);
        ecb.AddEvidenceValue("referenceValue", keymodel.ReferenceValue, PluginConstants.SourceName);
        ecb.AddEvidenceValue("created", keymodel.CreatedDateTime, PluginConstants.SourceName);

        return ecb.GetEvidenceValues();
    }

    private async Task<List<EvidenceValue>> UseKeyEvidenceValues(EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        var refVal = evidenceHarvesterRequest.TryGetParameter("referenceValue", out string referenceValue) ? referenceValue : string.Empty;
        var inputdata = evidenceHarvesterRequest.TryGetParameter("identifier", out string data) ? data : string.Empty;

        var (success, keymodel) = await _memoryCache.TryGet<KeyModel>(refVal);

        if (!success)
        {
            _logger.LogError($"Plugin.Pseudo - reference value ({refVal}) has no matching key.");
            throw new EvidenceSourcePermanentServerException(PluginConstants.ErrorInvalidInput,
                "Invalid reference value - no key has been created yet.");
        }

        var result = Pseudonymizer.HashIdentifier(inputdata, keymodel.Key);

        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, PluginConstants.DatasetName_upk);
        ecb.AddEvidenceValue("key", Convert.ToBase64String(keymodel.Key), PluginConstants.SourceName);
        ecb.AddEvidenceValue("result", result, PluginConstants.SourceName);
        return ecb.GetEvidenceValues();
    }

}
