﻿using System;
using MySpace.Common.IO;
using MySpace.Common;
using MySpace.DataRelay.Interfaces.Query.IndexCacheV3;

namespace MySpace.DataRelay.Common.Interfaces.Query.IndexCacheV3
{
    public class GetRangeQuery : IRelayMessageQuery, IPrimaryQueryId
    {
        #region Data Members
        private byte[] indexId;
        public byte[] IndexId
        {
            get
            {
                return indexId;
            }
            set
            {
                indexId = value;
            }
        }

        private int offset;
        public int Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
            }
        }

        private int itemNum;
        public int ItemNum
        {
            get
            {
                return itemNum;
            }
            set
            {
                itemNum = value;
            }
        }

        private string targetIndexName;
        public string TargetIndexName
        {
            get
            {
                return targetIndexName;
            }
            set
            {
                targetIndexName = value;
            }
        }

        private bool excludeData;
        public bool ExcludeData
        {
            get
            {
                return excludeData;
            }
            set
            {
                excludeData = value;
            }
        }

        private bool getMetadata;
        public bool GetMetadata
        {
            get
            {
                return getMetadata;
            }
            set
            {
                getMetadata = value;
            }
        }

        private Filter filter;
        public Filter Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
            }
        }

        private FullDataIdInfo fullDataIdInfo;
        public FullDataIdInfo FullDataIdInfo
        {
            get
            {
                return fullDataIdInfo;
            }
            set
            {
                fullDataIdInfo = value;
            }
        }
        #endregion

        #region Ctors
        public GetRangeQuery()
        {
            Init(null, -1, -1, null, false, false, null);
        }

        public GetRangeQuery(byte[] indexId, int offset, int itemNum, string targetIndexName)
        {
            Init(indexId, offset, itemNum, targetIndexName, false, false, null);
        }

        [Obsolete("This constructor is obsolete; use object initializer instead")]
        public GetRangeQuery(byte[] indexId, int offset, int itemNum, string targetIndexName, CriterionList criterionList)
        {
            Init(indexId, offset, itemNum, targetIndexName, false, false, null);
        }

        [Obsolete("This constructor is obsolete; use object initializer instead")]
        public GetRangeQuery(byte[] indexId, int offset, int itemNum, string targetIndexName, CriterionList criterionList, bool excludeData, bool getMetadata)
        {
            Init(indexId, offset, itemNum, targetIndexName, excludeData, getMetadata, null);
        }

        private void Init(byte[] indexId, int offset, int itemNum, string targetIndexName, bool excludeData, bool getMetadata, FullDataIdInfo fullDataIdInfo)
        {
            this.indexId = indexId;
            this.offset = offset;
            this.itemNum = itemNum;
            this.targetIndexName = targetIndexName;
            this.excludeData = excludeData;
            this.getMetadata = getMetadata;
            this.fullDataIdInfo = fullDataIdInfo;
        }
        #endregion

        #region IRelayMessageQuery Members
        public byte QueryId
        {
            get
            {
                return (byte)QueryTypes.GetRangeQuery;
            }
        }
        #endregion

        #region IPrimaryQueryId Members
        private int primaryId;
        public int PrimaryId
        {
            get
            {
                return primaryId > 0 ? primaryId : IndexCacheUtils.GeneratePrimaryId(indexId);
            }
            set
            {
                primaryId = value;
            }
        }
        #endregion

        #region IVersionSerializable Members
        public void Serialize(IPrimitiveWriter writer)
        {
            //IndexId
            if (indexId == null || indexId.Length == 0)
            {
                writer.Write((ushort)0);
            }
            else
            {
                writer.Write((ushort)indexId.Length);
                writer.Write(indexId);
            }

            //Offset
            writer.Write(offset);

            //ItemNum
            writer.Write(itemNum);

            //TargetIndexName
            writer.Write(targetIndexName);

            //Write a byte to account for deprecated CriterionList
            writer.Write((byte)0);

            //ExcludeData
            writer.Write(excludeData);

            //GetMetadata
            writer.Write(getMetadata);

            //Filter
            if (filter == null)
            {
                writer.Write((byte)0);
            }
            else
            {
                writer.Write((byte)filter.FilterType);
                Serializer.Serialize(writer.BaseStream, filter);
            }

            //FullDataIdInfo
            Serializer.Serialize(writer.BaseStream, fullDataIdInfo);
        }

        public void Deserialize(IPrimitiveReader reader, int version)
        {
            //IndexId
            ushort len = reader.ReadUInt16();
            if (len > 0)
            {
                indexId = reader.ReadBytes(len);
            }

            //Offset
            offset = reader.ReadInt32();

            //ItemNum
            itemNum = reader.ReadInt32();

            //TargetIndexName
            targetIndexName = reader.ReadString();

            //Read a byte to account for deprecated CriterionList
            reader.ReadByte();

            //ExcludeData
            excludeData = reader.ReadBoolean();

            //GetMetadata
            getMetadata = reader.ReadBoolean();

            if (version >= 2)
            {
                //Filter
                byte b = reader.ReadByte();
                if (b != 0)
                {
                    FilterType filterType = (FilterType)b;
                    filter = FilterFactory.CreateFilter(reader, filterType);
                }
            }

            if(version >= 3)
            {
                //FullDataIdInfo
                fullDataIdInfo = new FullDataIdInfo();
                Serializer.Deserialize(reader.BaseStream, fullDataIdInfo);
            }
        }

        private const int CURRENT_VERSION = 3;
        public int CurrentVersion
        {
            get
            {
                return CURRENT_VERSION;
            }
        }

        public bool Volatile
        {
            get
            {
                return false;
            }
        }
        #endregion

        #region ICustomSerializable Members
        public void Deserialize(IPrimitiveReader reader)
        {
            reader.Response = SerializationResponse.Unhandled;
        }
        #endregion
    }
}