using System;
using Newtonsoft.Json;

namespace Dan.Plugin.Pseudo.Models;

[JsonObject("keymodel")]
public class KeyModel
{

    [JsonProperty("key")]
    public byte[] Key { get; set; }

    [JsonProperty("referenceValue")]
    public string ReferenceValue { get; set; }

    [JsonProperty("createdDateTime")]
    public DateTime CreatedDateTime { get; set; }
}



