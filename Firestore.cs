//<!--  -Copyright 2021 Alexandra Dorey / Wellington Fabrics  -->﻿﻿
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using static WFInventory.Util;
using Google.Cloud.Firestore;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WFInventory.ViewModels;
using FSCommon;
using System.Windows;
using static FSCommon.FireStoreAccess;

namespace WFInventory.Cloud
{

    public static class FS
    {

//        static public ConcurrentQueue<FirestoreNode> CloudUpdateRequired = new ConcurrentQueue<FirestoreNode>();

        static public int ListenersInitialized = 0;

        static public ConcurrentQueue<CancellationTokenSource> listenerTokens = new ConcurrentQueue<CancellationTokenSource>();


        public static observableConcurrentDataSourceGroup<CT> GetTypeTable<CT>() where CT : FirestoreNode
        {
            if (!TypeTables.ContainsKey(typeof(CT)))
            {
                TypeTables.Add(typeof(CT), new observableConcurrentDataSourceGroup<CT>(typeof(CT)));
            }
            return TypeTables[typeof(CT)];
        }

        public static object CreateTypeTable(Type t)
        {
            var accesstype = typeof(FS);
            var mi = accesstype.GetMethod("GetTypeTable", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            var mi2 = mi.MakeGenericMethod(t);
            return mi2.Invoke(null, null); 
        }


        //public static CT GetNode<CT>(string id) where CT: FirestoreNode
        //{
        //    return (CT)GetNodes(typeof(CT))[id];
        //}

        public static CT GetVM<CT, NODET>(string id) where CT : iDocumentViewModel where NODET : FirestoreNode
        {
            return (CT)GetVM(GetNode<NODET>(id));
        }

        public static iDocumentViewModel GetVM(FirestoreNode node)
        {
            if (node != null)
            {
                if (node.IsRoot) return (iDocumentViewModel)RootVM;

                Type closed = typeof(observableConcurrentDataSourceGroup<>).MakeGenericType(node.GetType());
                
                return TypeTables[node.GetType()].NodeToVMMap[node];
            }
            return null;
        }

        static FS()
        {
            createClassFactories();

            Root = new FirestoreNode
            {
                DocRef = null,
                Id = "FireStoreRoot",
                IsRoot = true
            };

            RootVM = new DocumentViewModel(Root);
            RootVM.isRoot = true;
            FSCommon.FireStoreAccess.Root = Root;
            FSCommon.FireStoreAccess.FireStore = CloudCommon.FireStore;

            List<string> collectionstoload = new List<string>
                {
                    "wf_Shipment",
                    "wf_WebContent",
                    "wf_Discount",
                    "wf_WebInfoPage",
                    //"wf_WebNewsPost",
                    "wf_OrderLineItem",
                    "wf_Order",
                    "wf_FabricCut",
                    "wf_Customer",
                    "wf_CustomerGroup",
                    //"wf_CustomerCreditAdjustment",
                    "wf_Product",
                    //"wf_ProductVariationTag",
                    "wf_ProductSku",
                    "wf_ProductCatagorie",
                    "wf_Supplier",
                    "wf_SupplierOrder",
                    "wf_SupplierOrderLineItem",
                    "wf_Roll",
                    "wf_AlwaysLoadData",
                    "wf_Address",
                    "wf_ImageEdit",
                    "wf_Country",
                    "wf_Area",
                    "wf_ContentSection",
                    "wf_JsonPayload",
                    //"wf_Account",
                    "wf_Expense",
                    //"wf_AccountEntry",
                    //"wf_RollAdjustment",
                    "wf_User",
                };

            Type t = typeof(wf_Product);
            string namepath = t.FullName.Substring(0, t.FullName.Length - 10);
            Assembly dcAssembly = Assembly.GetAssembly(typeof(wf_Product));
            foreach (string typename in collectionstoload)
            {
                string name = namepath + typename;
                t = dcAssembly.GetType(name);
                if (t == null)
                {
                    StdOut.WriteLine("CLoudInit", $"Count not find type: {name}", MessageType.MinorError);
                }
                else
                {
                    dynamic ocol = CreateTypeTable(t);
                    FireStoreAccess.FlatNodes.Add(t, ocol.nodes);
                }
            }

            CancellationToken token = _cts.Token;
            StdOut.WriteLine("Cloud Commmon", "FireStore Push Deamon Started");
            Task.Run(() => FireStoreUpdateDeamon(token));
            StdOut.WriteLine("Cloud Commmon", "FireStore Pull Deamon Started");
            Task.Run(() => ListenerUpdater(token));

        }

        static CancellationTokenSource _cts = new CancellationTokenSource();

        private static Dictionary<Type, dynamic> TypeTables = new Dictionary<Type, dynamic>();

        public static FirestoreNode Root = null;
        public static DocumentViewModel RootVM = null;

        private static dataClassActivator defaultDocumentDC;
        private static ConcurrentDictionary<Type, dataClassActivator> documentDC = new ConcurrentDictionary<Type, dataClassActivator>();

        private static void createClassFactories()
        {
            List<string> class_names_dc = new List<string>();
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in ass.GetTypes())
                {
                    if (type.GetCustomAttributes<FirestoreDataAttribute>().Any())
                    {
                        //WL("Added to docDC Factory:" + type.FullName);
                        foreach (var ctor in type.GetConstructors())
                        {
                            if (ctor.GetParameters().Length == 0)
                            {
                                documentDC[type] = getDataclassActivator(ctor);
                                break;
                            }
                        }
                    }
                }
            }

            ConstructorInfo ctor2 = typeof(FirestoreNode).GetConstructors().First();
            defaultDocumentDC = getDataclassActivator(ctor2);
        }

        private static dataClassActivator documentDCFactory(Type t)
        {
            if (documentDC.ContainsKey(t))
            {
                return documentDC[t];
            }
            return defaultDocumentDC;
        }

        private static dataClassActivator getDataclassActivator(ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] paramsInfo = ctor.GetParameters();

            Debug.WriteLine($"trying to init: {type.Name}");
            NewExpression newExp = System.Linq.Expressions.Expression.New(ctor);
            
            LambdaExpression lambda = System.Linq.Expressions.Expression.Lambda(typeof(dataClassActivator), newExp);

            dataClassActivator compiled = (dataClassActivator)lambda.Compile();
            return compiled;
        }



        private static async Task ListenerUpdater(CancellationToken token)
        {
            try
            {

                while (!token.IsCancellationRequested)
                {
                    int tcount = 0;
                    foreach (var source2 in TypeTables.Values)
                    {
                        tcount += source2.ChangesFromListener.Count;
                    }
                    foreach (var source in TypeTables.Values)
                    {
                        StdOut.StatusUpdate($"ListenerQueue", $"Total Changes: {tcount} Processing: {source.TypeName} {source.ChangesFromListener.Count}");
                        source.processQueue();
                        //await Task.Delay(500);
                    }
                    await Task.Delay(2000);
                }
            }
            catch (Exception e)
            {
               w("ListenerUpdater", e);
            }

            foreach( var source in TypeTables.Values)
            {
                source.StopListening();
            }

            StdOut.WriteLine("ListenerUpdater", "Stopped Listenting for changes...");
        }

        public static void Stop()
        {
            _cts.Cancel();
        }

        private static async Task FireStoreUpdateDeamon(CancellationToken token)
        {
            try
            {
                int TotalUnverifiedUpdates = 0;
                Dictionary<FirestoreNode, DateTime> DirtyNodeList = new Dictionary<FirestoreNode, DateTime>();
                TimeSpan MinUpdateLag = TimeSpan.FromSeconds(30); //after the last change to an object how long until we upload the change...
                TimeSpan TimeBetweenUpdates = TimeSpan.FromSeconds(60); //Aggradate updates every...
                List<FirestoreNode> CurrentUpdateList = new List<FirestoreNode>();
                DateTime LastUpdate = DateTime.UtcNow;
                Dictionary<FirestoreNode, Dictionary<string, object>> CurrentUpdateCommand = new Dictionary<FirestoreNode, Dictionary<string, object>>();

                while (!token.IsCancellationRequested)
                {
                    List<FirestoreNode> updatelist = new List<FirestoreNode>();
                    FirestoreNode NodeToUpdate;
                    bool result = FSCommon.FireStoreAccess.CloudUpdateRequired.TryDequeue(out NodeToUpdate);
                    while (result)
                    {
                        if (DirtyNodeList.ContainsKey(NodeToUpdate))
                        {
                            DirtyNodeList[NodeToUpdate] = DateTime.UtcNow;
                        }
                        else
                        {
                            DirtyNodeList.Add(NodeToUpdate, DateTime.UtcNow);
                        }
                        result = FSCommon.FireStoreAccess.CloudUpdateRequired.TryDequeue(out NodeToUpdate);
                    }

                    TimeSpan TimeUntilNextUpdate = TimeBetweenUpdates - (DateTime.UtcNow - LastUpdate);
                    
                    foreach (var kvp in DirtyNodeList)
                    {
                        if(DateTime.UtcNow - kvp.Value <= MinUpdateLag)
                        {
                            if(CurrentUpdateList.Contains(kvp.Key))
                            {
                                CurrentUpdateList.Remove(kvp.Key);
                            }
                        }
                        else
                        {
                            if(!CurrentUpdateList.Contains(kvp.Key))
                            {
                                CurrentUpdateList.Add(kvp.Key);
                            }
                        }
                    }

                    if (DirtyNodeList.Count == 0 && CurrentUpdateList.Count == 0)
                    {
                        LastUpdate = DateTime.UtcNow;
                        StdOut.StatusUpdate("FireStore Deamon", $"Waiting on Changes...");
                    }
                    else
                    {
                        StdOut.StatusUpdate("FireStore Deamon", $"Nodes With Changes: {DirtyNodeList.Count} Nodes Ready: {CurrentUpdateList.Count} NextUpdate: {TimeUntilNextUpdate.TotalSeconds:F0} s");
                    }

                    if (DateTime.UtcNow - LastUpdate > TimeBetweenUpdates)
                    {
                        StdOut.StatusUpdate("FireStore Deamon", $"Starting Commit...");
                        foreach (FirestoreNode node in CurrentUpdateList)
                        {
                            if(DirtyNodeList.ContainsKey(node))DirtyNodeList.Remove(node);
                            if (node.Deleted) continue;
                            CurrentUpdateCommand.Add(node, new Dictionary<string, object>());
                            List<string> Properties = new List<string>();
                            bool result2 = true;
                            while (result2)
                            {
                                string prop;
                                result2 = node.DirtyProperties.TryDequeue(out prop);
                                if (result2)
                                {
                                    if (!Properties.Contains(prop)) 
                                    Properties.Add(prop);
                                }
                            }
                            
                            foreach (string p in Properties)
                            {
                                var pi = node.GetType().GetProperty(p);
                                CurrentUpdateCommand[node].Add(p, pi.GetValue(node));
                            }

                            if(CurrentUpdateCommand[node].Count == 0)
                            {
                                CurrentUpdateCommand.Remove(node);
                            }
                        }
                        CurrentUpdateList.Clear();

                        if (CurrentUpdateCommand.Count > 0)
                        {
                            bool batchcommitfailed = true;
                            try
                            {
                                WriteBatch batch = CloudCommon.FireStore.StartBatch();
                                int NodesProcessed = 0;
                                int BatchesProcessed = 0;
                                foreach (var kvp in CurrentUpdateCommand)
                                {
                                    NodesProcessed++;
                                    batch.Update(kvp.Key.DocRef, kvp.Value);
                                    if (NodesProcessed % 400 == 0)
                                    {
                                        BatchesProcessed++;
                                        StdOut.StatusUpdate("FireStore Deamon", $"Commiting Batch {BatchesProcessed}....(400 Nodes)\n");
                                        await batch.CommitAsync();
                                        batch = CloudCommon.FireStore.StartBatch();
                                    }
                                }
                                int NodesLeft = CurrentUpdateCommand.Count - BatchesProcessed * 400;
                                BatchesProcessed++;
                                StdOut.StatusUpdate("FireStore Deamon", $"Commiting Batch {BatchesProcessed}....({NodesLeft} Nodes)\n");
                                await batch.CommitAsync();
                                batchcommitfailed = false;
                            }
                            catch(Exception e)
                            {
                                StdOut.StatusUpdate("FireStore Commit Error", "Batch commit failed... :(");
                                Debug.WriteLine($"{e.Message}");
                            }

                            if(batchcommitfailed)
                            {
                                int currentitem = 0;
                                foreach (var kvp in CurrentUpdateCommand)
                                {
                                    currentitem++;
                                    try
                                    {
                                        StdOut.StatusUpdate("FireStore Slow Commit", $"Working on item: {currentitem} of {CurrentUpdateCommand.Count}");
                                        await kvp.Key.DocRef.UpdateAsync(kvp.Value);
                                        await Task.Delay(50);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine($"{e.Message}");
                                        Debug.WriteLine($"{kvp.Value}");
                                    }
                                }
                            }
                        }

                        TimeSpan TimeoutForManualCheck = TimeSpan.FromSeconds(10.0 + CurrentUpdateCommand.Count / 10); //Aggradate updates every...
                        DateTime CommitCheckStartTime = DateTime.UtcNow;
                        while (CurrentUpdateCommand.Count > 0 && (DateTime.UtcNow - CommitCheckStartTime) < TimeoutForManualCheck)
                        {
                            TimeSpan TimeUntilDone = TimeoutForManualCheck - (DateTime.UtcNow - CommitCheckStartTime);
                            StdOut.StatusUpdate("FireStore Deamon", $"Confirming Commits...Waiting on {CurrentUpdateCommand.Count} for Max {TimeUntilDone.TotalSeconds} s");
                            await Task.Delay(1000);
                            FirestoreNode UpdatedNode;
                            bool result2 = FSCommon.FireStoreAccess.ChangeFromCloud.TryDequeue(out UpdatedNode);
                            while(result2)
                            {
                                if(CurrentUpdateCommand.ContainsKey(UpdatedNode))
                                {
                                    CurrentUpdateCommand.Remove(UpdatedNode);
                                }
                                result2 = FSCommon.FireStoreAccess.ChangeFromCloud.TryDequeue(out UpdatedNode);
                            }
                        }
                        
                        if(CurrentUpdateCommand.Count > 0)
                        {
                            TotalUnverifiedUpdates += CurrentUpdateCommand.Count;
                         //   StdOut.StatusUpdate("FireStore Deamon Error", $"There Have been {TotalUnverifiedUpdates} Unverified Updates");
                            CurrentUpdateCommand.Clear();
                        }

                        LastUpdate = DateTime.UtcNow;
                    }
                    else
                    {
                        if (CloudUpdateRequired.Count > 0) continue;
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception e)
            {
                wl(e);
            }
        }


        public delegate FirestoreNode dataClassActivator();

        public static List<string> GetFireStoreProperties(this Type t)
        {
            List<string> fsprops = new List<string>();
            PropertyInfo[] allprops = t.GetProperties();
            foreach (PropertyInfo pi in allprops)
            {
                if (pi.GetCustomAttributes<FirestorePropertyAttribute>().Any())
                {
                    fsprops.Add(pi.Name);
                }
            }
            return fsprops;
        }
    }
}
