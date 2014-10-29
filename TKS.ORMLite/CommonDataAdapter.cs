using System;
using System.Collections.Generic;
using System.Text;

namespace TKS.ORMLite
{
    /// <summary>
    ///     数据索引器工厂
    /// </summary>
    internal class CommonDataAdapter
    {
        /// <summary>
        ///     最终要加载的数据，包含列名与实体描述信息
        /// </summary>
        public object UserData { get; set; }
        private IDataRow commonDataRow;
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="row">DataRow对象</param>
        public CommonDataAdapter(System.Data.DataRow row)
        {
            commonDataRow = new DbDataRow(row);
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="reader">IDataReader对象</param>
        public CommonDataAdapter(System.Data.IDataReader reader)
        {
            commonDataRow = new DbDataReader(reader);
        }

        /// <summary>
        ///     获取列名数组
        /// </summary>
        /// <param name="toUpper">是否转成大写</param>
        /// <returns></returns>
        public string[] GetColumnNames(bool toUpper)
        {
            string[] strArray = commonDataRow.GetColumnNames();
            if (strArray != null && toUpper)
            {
                for (int i = 0; i < strArray.Length; i++)
                {
                    strArray[i] = strArray[i].ToUpper();
                }
            }
            return strArray;
        }

        public void SetCurrentRow(object row)
        {
            this.commonDataRow.SetNewRow(row);
        }

        /// <summary>
        ///     根据字段名，获取值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public object this[string fieldName]
        {
            get
            {
                return this.commonDataRow .GetValue(fieldName);
            }
        }

        private interface IDataRow
        {
            /// <summary>
            ///     获取列名
            /// </summary>
            /// <returns>列名数组</returns>
            string[] GetColumnNames();
            /// <summary>
            ///     根据字段名取值
            /// </summary>
            /// <param name="fieldName">字段名</param>
            /// <returns></returns>
            object GetValue(string fieldName);
            /// <summary>
            ///     对于DataTable执行foreach操作时，没有必要为每个DataRow创建一个CommonDataAdapter对象，可以调用这个方法设置“当前行”
            ///     如果是对于同一个DbDataReader，执行这个调用就没有意义了。
            ///     注意：新行的【字段列表】一定要和“前一行”是一样的。（此处代码不检查，但可能会引发异常）     
            /// </summary>
            /// <param name="row">参数的类型只能是：DataRow 或 DbDataReader，否则会抛出异常</param>
            void SetNewRow(object row);
        }

        private sealed class DbDataRow:IDataRow
        {
            private System.Data.DataRow _row;
            public DbDataRow(System.Data.DataRow row)
            {
                if (row==null )
                {
                    throw new ArgumentNullException("row");
                }
                _row = row;
            }
            #region IDataRow Members

            public string[] GetColumnNames()
            {
                string[] strArray = new string[this._row.Table.Columns.Count];
                for (int i = 0; i < this._row.Table.Columns.Count; i++)
                {
                    strArray[i] = this._row.Table.Columns[i].ColumnName;
                }
                return strArray;
            }

            public object GetValue(string fieldName)
            {
                return this._row[fieldName];
            }

            public void SetNewRow(object row)
            {
                this._row = (System.Data.DataRow)row;
            }

            #endregion
        }

        private sealed class DbDataReader : IDataRow
        {
            private System.Data.IDataReader _reader;
            public DbDataReader(System.Data.IDataReader reader)
            {
                if (reader == null)
                {
                    throw new ArgumentNullException("reader");
                }
                _reader = reader;
            }
            #region IDataRow Members

            public string[] GetColumnNames()
            {
                int fieldCount = this._reader.FieldCount;
                string[] strArray = new string[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    strArray[i] = this._reader.GetName(i);
                }
                return strArray;
            }

            public object GetValue(string fieldName)
            {
                return this._reader[fieldName];
            }

            public void SetNewRow(object row)
            {
                
            }

            #endregion
        }

    }
}
