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

        [JsonProperty("createdDateTime")]
        public DateTimeOffset CreatedDateTime
        {
            get; set;
        }

        [JsonProperty("lastModifiedDateTime")]
        public DateTimeOffset LastModifiedDateTime
        {
            get; set;
        }

        [JsonProperty("permissions", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Permission[] Permissions
        {
            get; set;
        }

        [JsonProperty("shared")]
        public SharedFacet Shared
        {
            get; set;
        }

    }

    public class FolderFacet
    {
        [JsonProperty("childCount")]
        public int ChildCount { get; set; }
    }

    public class FileFacet
    {

    }

    public class IdentitySet
    {
        [JsonProperty("application", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Identity Application
        {
            get; set;
        }

        [JsonProperty("user", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Identity User
        {
            get; set;
        }
    }

    public class Identity
    {
        [JsonProperty("displayName")]
        public string DisplayName
        {
            get; set;
        }

        [JsonProperty("id")]
        public string Id
        {
            get; set;
        }
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

    public class Permission
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("roles")]
        public string[] Roles { get; set; }

        [JsonProperty("link", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public LinkFacet Link { get; set; }

        [JsonProperty("grantedTo", DefaultValueHandling =DefaultValueHandling.Ignore)]
        public IdentitySet GrantedTo { get; set; }

    }

    public class LinkFacet
    {
        /// <summary>
        /// Scope for the sharingLink, either anonymous, or organization
        /// </summary>
        public string Scope
        {
            get; set;
        }

        /// <summary>
        /// Type of sharing link, either view or edit
        /// </summary>
        public string Type
        {
            get; set;
        }

        /// <summary>
        /// The actual sharing link URL
        /// </summary>
        public string webUrl
        {
            get; set;
        }

    }
    

    

    public class SharedFacet
    {
        [JsonProperty("scope")]
        public string Scope
        {
            get; set;
        }

        internal static SharedFacet CreateFromPermissions(Permission[] permissions)
        {
            if (null == permissions || permissions.Length == 0)
                return null;

            SharedFacet facet = new SharedFacet();
            SharedScope discoveredScope = SharedScope.None;
            foreach (var perm in permissions)
            {
                if (perm.Link != null && perm.Link.Scope == "organization")
                {
                    discoveredScope = (SharedScope.Tenant > discoveredScope) ? SharedScope.Tenant : discoveredScope;
                }
                else if (perm.Link != null && perm.Link.Scope == "anonymous")
                {
                    discoveredScope = (SharedScope.Anonymous > discoveredScope) ? SharedScope.Anonymous : discoveredScope;
                }

                if (perm.GrantedTo != null)
                {
                    discoveredScope = (SharedScope.Users > discoveredScope) ? SharedScope.Users : discoveredScope;
                }
            }

            facet.Scope = discoveredScope.ToString().ToLower();
            return facet;
        }

        private enum SharedScope
        {
            None = 0,
            Users = 1,
            Tenant = 2,
            Anonymous = 3
        }
    }


}