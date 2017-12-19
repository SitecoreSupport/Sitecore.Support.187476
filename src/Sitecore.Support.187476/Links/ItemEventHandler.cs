namespace Sitecore.Support.Links
{
    using Sitecore.Configuration;
    using Sitecore.Data.DataProviders.Sql;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Diagnostics;
    using Sitecore.Events;
    using Sitecore.Globalization;
    using Sitecore.Links;
    using Sitecore.Publishing;
    using Sitecore.SecurityModel;
    using System;
    using System.Linq;

    public class ItemEventHandler
    {
        /// <summary>
        /// Gets the link database.
        /// </summary>
        /// <value>The link database.</value>
        private static Sitecore.Links.LinkDatabase LinkDatabase
        {
            get
            {
                return Globals.LinkDatabase;
            }
        }

        /// <summary>
        /// Called when the item has been deleted.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        /// <contract>
        ///   <requires name="sender" condition="none" />
        ///   <requires name="args" condition="none" />
        /// </contract>
        protected void OnItemDeleted(object sender, System.EventArgs args)
        {
            if (args == null)
            {
                return;
            }
            if (!Settings.LinkDatabase.UpdateDuringPublish && PublishHelper.IsPublishing())
            {
                return;
            }
            Item item = Event.ExtractParameter(args, 0) as Item;
            Assert.IsNotNull(item, "No item in parameters");
            Sitecore.Links.LinkDatabase linkDatabase = ItemEventHandler.LinkDatabase;
            if (linkDatabase != null)
            {
                linkDatabase.RemoveReferences(item);
            }
        }

        /// <summary>
        /// Called when the item has copied.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        protected void OnItemCopied(object sender, System.EventArgs args)
        {
            if (args == null)
            {
                return;
            }
            if (!Settings.LinkDatabase.UpdateDuringPublish && PublishHelper.IsPublishing())
            {
                return;
            }
            Item item = Event.ExtractParameter(args, 1) as Item;
            Assert.IsNotNull(item, "No item in parameters");
            if (ItemEventHandler.LinkDatabase != null)
            {
                ItemEventHandler.LinkDatabase.UpdateReferences(item);
            }
        }

        /// <summary>
        /// Called when the version has been removed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        /// <contract>
        ///   <requires name="sender" condition="none" />
        ///   <requires name="args" condition="none" />
        /// </contract>
        protected void OnVersionRemoved(object sender, System.EventArgs args)
        {
            if (args == null)
            {
                return;
            }
            if (!Settings.LinkDatabase.UpdateDuringPublish && PublishHelper.IsPublishing())
            {
                return;
            }
            Item item = Event.ExtractParameter(args, 0) as Item;
            Assert.IsNotNull(item, "No item in parameters");
            Sitecore.Links.LinkDatabase linkDatabase = ItemEventHandler.LinkDatabase;
            if (linkDatabase != null)
            {
                //linkDatabase.UpdateItemVersionReferences(item);
                //sitecore.support.187476
                ItemLink[] allReferences = linkDatabase.GetReferences(item);
                foreach (var reference in allReferences)
                {
                    if (reference.SourceItemLanguage == item.Language)
                    {
                        RemoveLink(item, reference);
                    }
                }
                linkDatabase.UpdateItemVersionReferences(item);
                //end of sitecore.support.187476   
            }
        }
        //sitecore.support.187476
        /// <summary>
        /// Manually removes the link of the current version in the current language from the database
        /// </summary>
        /// <param name="version"></param>
        /// <param name="itemLink"></param>
        private static void RemoveLink(Item version, ItemLink itemLink)
        {
            Assert.ArgumentNotNull(version, "version");
            Assert.ArgumentNotNull(itemLink, "itemLink");
            Field field = version.Fields[itemLink.SourceFieldID];
            CustomField field2 = FieldTypeManager.GetField(field);
            if (field2 == null)
            {
                return;
            }
            using (new SecurityDisabler())
            {
                version.Editing.BeginEdit();
                field2.RemoveLink(itemLink);
                version.Editing.EndEdit(true);
            }
        }
        //end of sitecore.support.187476

        /// <summary>
        /// Called when the item has saved.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        /// <contract>
        ///   <requires name="sender" condition="none" />
        ///   <requires name="args" condition="none" />
        /// </contract>
        protected void OnItemSaved(object sender, System.EventArgs args)
        {
            if (args == null)
            {
                return;
            }
            if (LinkDisabler.IsActive)
            {
                return;
            }
            if (!Settings.LinkDatabase.UpdateDuringPublish && PublishHelper.IsPublishing())
            {
                return;
            }
            Item item = Event.ExtractParameter(args, 0) as Item;
            Assert.IsNotNull(item, "No item in parameters");
            Sitecore.Links.LinkDatabase linkDatabase = ItemEventHandler.LinkDatabase;
            if (linkDatabase != null)
            {
                linkDatabase.UpdateItemVersionReferences(item);
            }
        }
    }
}