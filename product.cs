using System;
using System.Collections.Generic;

namespace WFClassDef
{
    [Flags]
    public enum QtyType
    {
        None = 0,
        HalfUnit = 1,
        Unit = 2,
        FatHalf = 4,
        FatQuater = 8,
        Item = 16,
    }

    [Flags]
    public enum GeneratedImages
    {
        None = 0,
        Ruler = 1, 
        Cutouts = 2,
        Models = 4,
        ColorMatcher = 8,
    }

    public enum TagRelationShipTypes
    {
        Parent,
        Child,
        Sibling,
        Cousin,
        Other
    }

    public enum PhysicalPropertyTypes
    {
        Length,
        Height,
        Width,
        WeightU,
        Thickness,
        WeightSqm,
        VolU,
        VolSqM,
        CarefulFolding,
        MustBeRolled,
    }

    public enum StretchType
    {
        none,
        TwoWay,
        FourWay
    }

    public enum CutStatus
    {
        WaitingForStock,
        PreCut,
        InStorage,
        UnCut,
        OnStagingTable,
        InBin,
        InCubby,
        InShipment
    }

    public enum RollStatus
    {
        PreOrder,
        Ordered,
        InProduction,
        Shipping,
        UnPacking,
        Cutting,
        Complete
    }



    public enum SupplierOrderStatus
    {
        PreOrder,
        Ordered,
        InProduction,
        Shipping,
        UnPacking,
        AvailableForCutting,
    }



    public enum MaterialTypes
    {
        Unknown = 0,
        Cotton = 100,
        Bamboo = 101,
        Ramie = 102,
        Linen = 103,
        Leather = 104,
        Tencel = 105,
        Wool = 106,
        Cork = 107,
        Lycra = 200,
        Polyester = 201,
        Rayon = 202,
        Viscose = 203,
        Nylon = 204,
        Vinyl = 300,
    }

    
    
    public enum ShipmentStatus
    {
        Undefined0 = 0,
        Undefined1 = 1,
        Undefined2 = 2,
        Undefined3 = 3,
        Undefined4 = 4,
        Undefined5 = 5,
        Undefined6 = 6,
        Undefined7 = 7,
        Undefined8 = 8,
        Undefined9 = 9,
        Undefined10 = 10,
        Undefined11 = 11,
        Undefined12 = 12,
        Undefined13 = 13,
        Undefined14 = 14,
        Undefined15 = 15,
        Undefined16 = 16,
        Undefined17 = 17,
        ImportedFromPP = 50,
        PreOrder = 110,
        WaitingForPayment = 120,
        WaitingForDelivery = 130,
        Unpacking = 140,
        InCutQueue = 141,
        Cutting = 150,
        CutCompleteWaitingForShipment = 151,
        ShippmentAssigned = 152,
        BinEmpty = 153,
        ShippmentReadyForBining = 154,
        BinAssigned = 155,
        HoldingInCubbie = 160,
        StagingInBin = 170,
        StagingInSharedBin = 175,
        Packing = 180,
        AddressError = 181,
        DimensionalError = 182,
        Packed = 183,
        RatingError = 184,
        PackedCanadaPostSelected = 185,
        PackedNetParcelSelected = 186,
        LabelCreated = 187,
        LabelPrinted = 188,
        Shipping = 190,
        ReadyForLocalPU = 191,
        ReadyToMail = 192,
        InCubby = 193,
        ShippingInSharedPackage = 195,
        Complete = 200,
        PackedIntoSharedShipment,
        AddressVerified,
        PackageRated,
        ShipperSelected,
        ReadyToShip,
        Shipped,
    }



    public enum ItemStatus
    {
        Undefined0 = 0,
        Undefined1 = 1,
        Undefined2 = 2,
        Undefined3 = 3,
        Undefined4 = 4,
        Undefined5 = 5,
        Undefined6 = 6,
        Undefined7 = 7,
        Undefined8 = 8,
        Undefined9 = 9,
        Undefined10 = 10,
        Undefined11 = 11,
        Undefined12 = 12,
        Undefined13 = 13,
        Undefined14 = 14,
        Undefined15 = 15,
        Undefined16 = 16,
        Undefined17 = 17,
        ImportedFromPP = 50,
        PreOrder = 110,
        WaitingForPayment = 120,
        WaitingForDelivery = 130,
        Unpacking = 140,
        InCutQueue = 141,
        Cutting = 150,
        CutCompleteWaitingForShipment = 151,
        ShippmentAssigned = 152,
        BinEmpty = 153,
        ShippmentReadyForBining = 154,
        BinAssigned = 155,
        HoldingInCubbie = 160,
        StagingInBin = 170,
        StagingInSharedBin = 175,
        Packing = 180,
        AddressError = 181,
        DimensionalError = 182,
        Packed = 183,
        RatingError = 184,
        PackedCanadaPostSelected = 185,
        PackedNetParcelSelected = 186,
        LabelCreated = 187,
        LabelPrinted = 188,
        Shipping = 190,
        ReadyForLocalPU = 191,
        ReadyToMail = 192,
        InCubby = 193,
        ShippingInSharedPackage = 195,
        Complete = 200
    }

   public enum Status
    {
        Undefined0 = 0,
        Undefined1 = 1,
        Undefined2 = 2,
        Undefined3 = 3,
        Undefined4 = 4,
        Undefined5 = 5,
        Undefined6 = 6,
        Undefined7 = 7,
        Undefined8 = 8,
        Undefined9 = 9,
        Undefined10 = 10,
        Undefined11 = 11,
        Undefined12 = 12,
        Undefined13 = 13,
        Undefined14 = 14,
        Undefined15 = 15,
        Undefined16 = 16,
        Undefined17 = 17,
        ImportedFromPP = 50,
        PreOrder = 110,
        WaitingForPayment = 120,
        WaitingForDelivery = 130,
        Unpacking = 140,
        InCutQueue = 141,
        Cutting = 150,
        CutCompleteWaitingForShipment = 151,
        ShippmentAssigned = 152,
        BinEmpty = 153,
        ShippmentReadyForBining = 154,
        BinAssigned = 155,
        HoldingInCubbie = 160,
        StagingInBin = 170,
        StagingInSharedBin = 175,
        Packing = 180,
        AddressError = 181,
        DimensionalError = 182,
        Packed = 183,
        RatingError = 184,
        PackedCanadaPostSelected = 185,
        PackedNetParcelSelected = 186,
        LabelCreated = 187,
        LabelPrinted = 188,
        Shipping = 190,
        ReadyForLocalPU = 191,
        ReadyToMail = 192,
        InCubby = 193,
        ShippingInSharedPackage = 195,
        Complete = 200
    }

    public class ImageEdit
    {
        public string OriginalFileLocation;
        public string EditedFileLocation;
        public string ThumbnailFileLocation;
        
        public int TLX;
        public int TLY;
        public int BRX;
        public int BRY;

        public string EffectData;

        public double PrintedSize;

        public string Moniker;
        public string Descirption;
        public string PhotographerName;
        public string CopyRightOwner;

        public bool RequiresPublish;

        public string ClickLink;

        //public bool MenuImage;
    }

    public class Product
    {
        public string OrgTreeLocation;

        public bool Enabled;
        
        public string Moniker;
        public string Description;
        public string IdBuilderTag;
        public string Notes;

        public DateTime FirstDateOffered;
        public string DefCustomsMoniker;
        public double DefCustomsPrice;

        public List<string> Catagories;
        
        //public double CurrentPrice;
        //public DateTime DateCurrentPriceSet;
       
        //public List<double> PriceHistory;
        //public List<DateTime> PriceHistoryDateChange;
        
        //public List<PhysicalPropertyTypes> PhysicalProperties;
        //public List<double> PhysicalPropertyValues;

        //public List<MaterialTypes> MaterialContents;
        //public List<double> MaterialContentValues;

        public Units DefSellUnits;
        public QtyType DefAllowedQtyType;

        public int LastImageID;

        public string InternalMenuImageId;
    }

    public class ProductVariationTag
    {
        public double GroupDisplayOrder;
        public string GroupTag;
        public string GroupMoniker;
        public string GroupDescription;
        public string OptionTag;
        public string OptionMoniker;
        public string OptionDescription;
    }

    public class ProductSku
    {
        public string SupplierId;
        public string SupplierOrderId;
        public string SupplierOrderLineItemId;

        public bool Enabled;
        public string Moniker; //Human ReadableName
        public string Sku; //SKU text Version
        public bool ShowSkuTagInDescription;
        public string Description; //Human Readable Description...

        public string SkuImageEditId;

        public Status Status;

        public double InventoryAdjustment;

        public List<string> ProductVariationGroupTags;
        public List<string> ProductVariationOptionTags;

        public string PRT; //hidden for internal usages

        public string TYP;
        public string TYPName;
        public string TYPDesc;

        public string FB;
        public string FBName;
        public string FBDesc;

        public string SZ;
        public string SZName;
        public string SZDesc;

        public string C;
        public string CName;
        public string CDesc;

        public string D;
        public string DName;
        public string DDesc;

        public string CustomsMoniker;
        public string CustomsCode;
        public double CustomsPrice;
        

        public Units Units;
        public QtyType AllowedQtyType;
        public double FatHalfAdder;
        public double FatQuaterAdder;

        public bool Bundle;
        public List<string> BundledIds;
        public List<double> QtyRatio;

        public double CurrentCostPerSellUnit;

        public List<PhysicalPropertyTypes> PhysicalProperties;
        public List<double> PhysicalPropertyValues;

        public List<MaterialTypes> MaterialContents;
        public List<double> MaterialContentValues;

        public double CurrentPrice;
        public DateTime CurrentPriceDatetime;

        public List<double> PriceHistory;
        public List<DateTime> PriceHistoryDateChange;

        public double CurrentRollQty;
        public double CurrentPreCutItemsQty;

        //legacy delete after current code is workign and fix.
        public string SkuMoniker;
        public double TotalQtyU;
        public double TotalQtyAcutalU; 
        public double QtyCutU;
        public double InitalPrice;

        public List<string> SlideShowImageEditIds;
        public string MenuImageEditId;
        public string SizeImageEditId;
        public string ColourImageEditId;

        public double SizeScaleFactor;

        public PackingCultures PackingCulture;

        public bool PreOrderActive;
        public double PreOrderMaxQty;
        public DateTime PreOrderEstShipDate;

        public string InventoryAccountId;
    }

    public class Roll
    {
        //where this particular roll came from!
        public string SupplierId;
        public string SupplierOrderId;
        public string SupplierOrderLineId;

        //qty stuff.... hrm?

        public Units Units;
        public double Qty;
        public double UnitCostAtPurchase;

        public RollStatus RollStatus;

        public StretchType StretchType;

        public List<MaterialTypes> MaterialTypes;
        public List<double> MaterialValues;

        public double RollWidth;
        public double WeightSqUnit;
        public bool MustBeRoll;

        //Notes
        public string Notes;

        //Location Info
        public string RackId;
        public string ShelfId;
        public string CabinetId;
        public string DrawerId;
        public string StorageBinId;
        public string InventoryEntryId;
    }

    public class RollAdjustment
    {
        public DateTime AdjustmentDate;
        public string Notes;
        public Units Units;
        public double Qty;
        public double UnitCostAtAdjustment;
        public string InventoryEntryId;
    }

    public class FabricCut
    {
        public Status Status;
        public CutStatus CurrentStatus;
        public DateTime CurrentStatusDateTime;

        public List<DateTime> StatusChangeDateTimes;
        public List<CutStatus> StatusChangeType;

        public Units Units;
        public double Qty;
        public QtyType QtyType;

        public bool UseCustomSize;
        public double CustomWidth;
        public double CustomHeight;
        public string CustomCutNote;
        public string CustomCutImageId;
        public double CustomCutPercentDiscount;

        public string RollId;

        public double UnitCostWhenSold;

        public double BasePrice;
        public double ActualPrice;
        public double Total;
        public double DiscountedPrice;
        public double CustomCutBasePrice;

        //find my order info
        public string CustomerId;
        public string CustomerOrderId;
        public string CustomerOrderLineItemId;
        public string ShipmentId;

        public string InventoryEntryId;

        public string PayPalRef1;
        public string PayPalRef2;
        public string PayPalRef3;

    }

    public class Tag
    {
        public string Name;
    }

    public class TagRelation
    {
        public string RelatedTag;
        public TagRelationShipTypes Relationship;
        public string SearchTag;
        public string firebaseId;
    }
}
