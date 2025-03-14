﻿using Microsoft.SharePoint.Client;
using PnP.Framework.Diagnostics;
using PnP.Framework.Provisioning.Model;
using PnP.Framework.Utilities;
using System;

namespace PnP.Framework.Provisioning.ObjectHandlers
{
    internal class ObjectSiteHeaderSettings : ObjectHandlerBase
    {
        public override string Name
        {
            get { return "Site Header"; }
        }

        public override string InternalName => "SiteHeader";

        public override ProvisioningTemplate ExtractObjects(Web web, ProvisioningTemplate template, ProvisioningTemplateCreationInformation creationInfo)
        {
            using (var scope = new PnPMonitoredScope(this.Name))
            {
                web.EnsureProperties(w => w.HeaderEmphasis, w => w.HeaderLayout, w => w.MegaMenuEnabled);
                var header = new SiteHeader
                {
                    MenuStyle = web.MegaMenuEnabled ? SiteHeaderMenuStyle.MegaMenu : SiteHeaderMenuStyle.Cascading
                };
                switch (web.HeaderLayout)
                {
                    case HeaderLayoutType.Compact:
                        {
                            header.Layout = SiteHeaderLayout.Compact;
                            break;
                        }
                    case HeaderLayoutType.Minimal:
                        {
                            header.Layout = SiteHeaderLayout.Minimal;
                            break;
                        }
                    case HeaderLayoutType.Extended:
                        {
                            header.Layout = SiteHeaderLayout.Extended;
                            break;
                        }
                    case HeaderLayoutType.Standard:
                    default:
                        {
                            header.Layout = SiteHeaderLayout.Standard;
                            break;
                        }
                }

                if (Enum.TryParse<Emphasis>(web.HeaderEmphasis.ToString(), out Emphasis backgroundEmphasis))
                {
                    header.BackgroundEmphasis = backgroundEmphasis;
                }

                template.Header = header;
            }
            return template;
        }

        public override TokenParser ProvisionObjects(Web web, ProvisioningTemplate template, TokenParser parser, ProvisioningTemplateApplyingInformation applyingInformation)
        {
            using (var scope = new PnPMonitoredScope(this.Name))
            {
                web.EnsureProperties(w => w.Url);

                if (template.Header != null)
                {
                    switch (template.Header.Layout)
                    {
                        case SiteHeaderLayout.Compact:
                            {
                                web.HeaderLayout = HeaderLayoutType.Compact;
                                break;
                            }
                        case SiteHeaderLayout.Standard:
                            {
                                web.HeaderLayout = HeaderLayoutType.Standard;
                                break;
                            }
                    }
                    web.HeaderEmphasis = (SPVariantThemeType)Enum.Parse(typeof(SPVariantThemeType), template.Header.BackgroundEmphasis.ToString());
                    web.MegaMenuEnabled = template.Header.MenuStyle == SiteHeaderMenuStyle.MegaMenu;

                    var jsonRequest = new
                    {
                        headerLayout = web.HeaderLayout,
                        headerEmphasis = web.HeaderEmphasis,
                        megaMenuEnabled = web.MegaMenuEnabled,
                    };

                    web.ExecutePostAsync("/_api/web/SetChromeOptions", System.Text.Json.JsonSerializer.Serialize(jsonRequest)).GetAwaiter().GetResult();

                }
            }
            return parser;
        }

        public override bool WillExtract(Web web, ProvisioningTemplate template, ProvisioningTemplateCreationInformation creationInfo)
        {
            return true;
        }

        public override bool WillProvision(Web web, ProvisioningTemplate template, ProvisioningTemplateApplyingInformation applyingInformation)
        {
            return template.Header != null;
        }
    }
}
