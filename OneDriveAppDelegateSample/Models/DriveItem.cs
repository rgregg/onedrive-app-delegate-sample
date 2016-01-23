using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace OneDriveAppDelegateSample.Models
{
    public class DriveItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("webUrl")]
        public string WebUrl { get; set; }

        [JsonProperty("size")]
        public Int64 Size { get; set; }

        [JsonProperty("@content.downloadUrl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DownloadUrl { get; set; }

        [JsonProperty("children", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DriveItem[] Children { get; set; }

        [JsonProperty("folder", DefaultValueHandling =DefaultValueHandling.Ignore)]
        public FolderFacet Folder { get; set; }

        [JsonProperty("file", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public FileFacet File { get; set; }

        [JsonProperty("parentReference", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DriveItemReference ParentReference { get; set; }
    }

    public class FolderFacet
    {
        [JsonProperty("childCount")]
        public int ChildCount { get; set; }
    }

    public class FileFacet
    {

    }

    public class DriveItemReference
    {
        [JsonProperty("driveId")]
        public string DriveId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }


}