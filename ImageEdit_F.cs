using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;
using static WFInventory.Util;
using Google.Type;

namespace FSCommon
{


public partial class wf_ImageEdit: FirestoreNode
{//this file is NOT safe to edit...

//AutoGenerated Clone Function...
public wf_ImageEdit Clone()
{
wf_ImageEdit clone = new wf_ImageEdit();
clone.OriginalFileLocation = this.OriginalFileLocation;
clone.EditedFileLocation = this.EditedFileLocation;
clone.ThumbnailFileLocation = this.ThumbnailFileLocation;
clone.TLX = this.TLX;
clone.TLY = this.TLY;
clone.BRX = this.BRX;
clone.BRY = this.BRY;
clone.EffectData = this.EffectData;
clone.PrintedSize = this.PrintedSize;
clone.Moniker = this.Moniker;
clone.Descirption = this.Descirption;
clone.PhotographerName = this.PhotographerName;
clone.CopyRightOwner = this.CopyRightOwner;
clone.RequiresPublish = this.RequiresPublish;
clone.ClickLink = this.ClickLink;
return clone;}


}
}