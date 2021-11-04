using System;
using System.Collections.Generic;
using Google.Cloud.Firestore;
using static WFInventory.Util;
using Google.Type;

namespace FSCommon
{

[FirestoreData]
public partial class wf_ImageEdit: FirestoreNode
{

private string _OriginalFileLocation = "";
[FirestoreProperty]
public string OriginalFileLocation
{
get => _OriginalFileLocation;
set { if (_OriginalFileLocation != value && value != null) { _OriginalFileLocation = value; UpdateProperty("OriginalFileLocation"); } }
}
private string _EditedFileLocation = "";
[FirestoreProperty]
public string EditedFileLocation
{
get => _EditedFileLocation;
set { if (_EditedFileLocation != value && value != null) { _EditedFileLocation = value; UpdateProperty("EditedFileLocation"); } }
}
private string _ThumbnailFileLocation = "";
[FirestoreProperty]
public string ThumbnailFileLocation
{
get => _ThumbnailFileLocation;
set { if (_ThumbnailFileLocation != value && value != null) { _ThumbnailFileLocation = value; UpdateProperty("ThumbnailFileLocation"); } }
}
private int _TLX = 0;
[FirestoreProperty]
public int TLX
{
get => _TLX;
set { if (_TLX != value && value != null) { _TLX = value; UpdateProperty("TLX"); } }
}
private int _TLY = 0;
[FirestoreProperty]
public int TLY
{
get => _TLY;
set { if (_TLY != value && value != null) { _TLY = value; UpdateProperty("TLY"); } }
}
private int _BRX = 0;
[FirestoreProperty]
public int BRX
{
get => _BRX;
set { if (_BRX != value && value != null) { _BRX = value; UpdateProperty("BRX"); } }
}
private int _BRY = 0;
[FirestoreProperty]
public int BRY
{
get => _BRY;
set { if (_BRY != value && value != null) { _BRY = value; UpdateProperty("BRY"); } }
}
private string _EffectData = "";
[FirestoreProperty]
public string EffectData
{
get => _EffectData;
set { if (_EffectData != value && value != null) { _EffectData = value; UpdateProperty("EffectData"); } }
}
private double _PrintedSize = 0.0;
[FirestoreProperty]
public double PrintedSize
{
get => _PrintedSize;
set { if (_PrintedSize != value && value != null) { _PrintedSize = value; UpdateProperty("PrintedSize"); } }
}
private string _Moniker = "";
[FirestoreProperty]
public string Moniker
{
get => _Moniker;
set { if (_Moniker != value && value != null) { _Moniker = value; UpdateProperty("Moniker"); } }
}
private string _Descirption = "";
[FirestoreProperty]
public string Descirption
{
get => _Descirption;
set { if (_Descirption != value && value != null) { _Descirption = value; UpdateProperty("Descirption"); } }
}
private string _PhotographerName = "";
[FirestoreProperty]
public string PhotographerName
{
get => _PhotographerName;
set { if (_PhotographerName != value && value != null) { _PhotographerName = value; UpdateProperty("PhotographerName"); } }
}
private string _CopyRightOwner = "";
[FirestoreProperty]
public string CopyRightOwner
{
get => _CopyRightOwner;
set { if (_CopyRightOwner != value && value != null) { _CopyRightOwner = value; UpdateProperty("CopyRightOwner"); } }
}
private bool _RequiresPublish = false;
[FirestoreProperty]
public bool RequiresPublish
{
get => _RequiresPublish;
set { if (_RequiresPublish != value && value != null) { _RequiresPublish = value; UpdateProperty("RequiresPublish"); } }
}
private string _ClickLink = "";
[FirestoreProperty]
public string ClickLink
{
get => _ClickLink;
set { if (_ClickLink != value && value != null) { _ClickLink = value; UpdateProperty("ClickLink"); } }
}

}
}