﻿using System;
using System.Data;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.IO;
using System.Text;


namespace MsSqlClrUtility
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/sql/relational-databases/clr-integration-database-objects-user-defined-functions/clr-user-defined-aggregate-invoking-functions?view=sql-server-ver15
    /// </summary>
    [Serializable]
    [SqlUserDefinedAggregate(
        Format.UserDefined, //use clr serialization to serialize the intermediate result  
        IsInvariantToNulls = true, //optimizer property  
        IsInvariantToDuplicates = false, //optimizer property  
        IsInvariantToOrder = false, //optimizer property  
        MaxByteSize = 8000) //maximum size in bytes of persisted value  
    ]
    public class Concatenate : IBinarySerialize
    {
        /// <summary>  
        /// The variable that holds the intermediate result of the concatenation  
        /// </summary>  
        public StringBuilder intermediateResult;

        /// <summary>  
        /// Initialize the internal data structures  
        /// </summary>  
        public void Init()
        {
            this.intermediateResult = new StringBuilder();
        }

        /// <summary>  
        /// Accumulate the next value, not if the value is null  
        /// </summary>  
        /// <param name="value"></param>  
        public void Accumulate(SqlString value)
        {
            if (value.IsNull)
            {
                return;
            }

            this.intermediateResult.Append(value.Value).Append(',');
        }

        /// <summary>  
        /// Merge the partially computed aggregate with this aggregate.  
        /// </summary>  
        /// <param name="other"></param>  
        public void Merge(Concatenate other)
        {
            this.intermediateResult.Append(other.intermediateResult);
        }

        /// <summary>  
        /// Called at the end of aggregation, to return the results of the aggregation.  
        /// </summary>  
        /// <returns></returns>  
        public SqlString Terminate()
        {
            string output = string.Empty;
            //delete the trailing comma, if any  
            if (this.intermediateResult != null
                && this.intermediateResult.Length > 0)
            {
                output = this.intermediateResult.ToString(0, this.intermediateResult.Length - 1);
            }

            return new SqlString(output);
        }

        public void Read(BinaryReader r)
        {
            intermediateResult = new StringBuilder(r.ReadString());
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.intermediateResult.ToString());
        }
    }  
}
