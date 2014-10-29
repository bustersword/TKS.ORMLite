namespace TKS.ORMLite
{
    using System;
    using System.Data;
    using System.Collections.Generic;
    using TKS.ORMLite.Driver;
    using System.Text.RegularExpressions;
    using System.Text;
    /// <summary>
    ///     数据库访问类
    /// </summary>
    public class DBHelper
    {

        /// <summary>
        ///     构造函数，会自动根据配置信息使用相应的数据驱动，
        ///     IsAutoOpenClose=true,连接自动动关闭，
        ///     如果要手动打开、关闭，设置IsAutoOpenClose=false，
        ///     IsAutoResetCommand=true，Command执行完自动被重置，
        ///     如需不被重置，设置IsAutoResetCommand=false
        /// </summary>
        public DBHelper()
        {
            try
            {
                DriverType = DriverTypeFactory.GetDriverType();
                DBDSN = DriverType.DBDSN;
                this.Connection = DriverType.Connection;
                IsAutoOpenClose = true;
                IsAutoResetCommand = true;
                this.Connection.Open();
            }
            catch (Exception ex)
            {
                ProcessInitException(ex.Message);
            }
        }

        /// <summary>
        ///     构造函数，会自动根据配置信息使用相应的数据驱动，
        ///     IsAutoOpenClose=true,连接自动动关闭，
        ///     如果要手动打开、关闭，设置IsAutoOpenClose=false，
        ///     IsAutoResetCommand=true，Command执行完自动被重置，
        ///     如需不被重置，设置IsAutoResetCommand=false
        /// </summary>
        /// <param name="driver">数据库驱动</param>
        public DBHelper(IDriverType driver)
        {
            try
            {
                DriverType = driver;
                DBDSN = DriverType.DBDSN;
                this.Connection = DriverType.Connection;
                IsAutoOpenClose = true;
                IsAutoResetCommand = true;
                this.Connection.Open();
            }
            catch (Exception ex)
            {
                ProcessInitException(ex.Message);
            }
        }

        /// <summary>
        ///     执行命令时候是否发生异常
        /// </summary>
        private bool isOccurException = false;

        #region Events

        /// <summary>
        ///     命令执行前后触发事件
        /// </summary>
        public Action<ExecuteEventArgs> Execute;

        /// <summary>
        ///     命令执行时，有异常触发
        /// </summary>
        public Action<ExecuteLogEventArgs> ExecuteException;
        #endregion

        #region properties

        /// <summary>
        ///     日志文件名
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        ///     设备对象
        /// </summary>
        private IDriverType DriverType;

        /// <summary>
        /// 数据库标识：mssql,oracle,mysql
        /// </summary>
        public string DBDSN
        {
            get;
            private set;
        }

        /// <summary>
        ///     获取数据连接对象
        /// </summary>
        public IDbConnection Connection
        {
            get;
            private set;
        }

        /// <summary>
        ///     获取当前事务对象
        /// </summary>
        public IDbTransaction Transaction
        {
            get;
            private set;
        }

        /// <summary>
        ///     当前的命令对象，每当执行ExecteuXXXXXXXXX时，都会在这个对象上执行。
        /// </summary>
        public IDbCommand Command
        {
            get;
            private set;
        }

        /// <summary>
        ///     获取或设置连接对象是否为自动打开、关闭连接，
        ///     默认为true，如果要手动操作前打开连接，操作后关闭连接，请设置为false
        /// </summary>
        public bool IsAutoOpenClose
        {
            get;
            set;
        }

        /// <summary>
        ///     获取或设置Command对象是否自动重置，
        ///     默认为true，每次执行完命令，Command对象都会被重置，
        ///     重置方式，清除CommandText, Parameters.Clear() 
        /// </summary>
        public bool IsAutoResetCommand
        {
            get;
            set;
        }

        /// <summary>
        ///     获取或设置发生异常是否写入日志，默认为false
        /// </summary>
        public bool IsWriteLog
        {
            get;
            set;
        }

        #endregion

        #region Public Methods
        /// <summary>
        ///     执行命令,并返回影响的行数
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            MakeSureCommandNotNull();
            int result = 0;
            this.TriggerBeforeExecute();
            try
            {
                result = this.Command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                ProcessException(new ExecuteLogEventArgs("ExecuteNonQuery", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return result;
        }

        /// <summary>
        ///     执行命令对象,并返回第一条记录第一列的值
        /// </summary>
        /// <returns></returns>
        public object ExecuteScalar()
        {
            MakeSureCommandNotNull();
            TriggerBeforeExecute();
            object value = null;
            try
            {
                value = this.Command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                ProcessException(new ExecuteLogEventArgs("ExecuteScalar", Command.CommandText, ex));
            }

            TriggerAfterExecute();

            return value;
        }

        /// <summary>
        ///     执行命令,并返回查询的记录集
        /// </summary>
        /// <param name="tableCount">数据集中表的数量</param>
        /// <returns>包含一或多个表的数据集[Table1,Table2....]</returns>
        public DataSet ExecuteDataSet(int tableCount)
        {
            string[] tableNames = new string[tableCount];
            for (int i = 1; i <= tableCount; i++)
            {
                tableNames[i - 1] = "Table" + i.ToString();
            }
            return this.ExecuteDataSet(tableNames);
        }

        /// <summary>
        ///     执行命令返回查询的记录集
        /// </summary>
        /// <param name="tableNames">数据集中表的名称</param>
        /// <returns>包含一或多个表的数据集</returns>
        public DataSet ExecuteDataSet(string[] tableNames)
        {
            this.MakeSureCommandNotNull();
            this.TriggerBeforeExecute();
            DataSet set = null;
            try
            {
                using (IDataReader reader = this.Command.ExecuteReader())
                {
                    set = new DataSet();
                    set.Load(reader, LoadOption.PreserveChanges, tableNames);
                    reader.Close();
                }
                return set;
            }
            catch (Exception ex)
            {
                this.ProcessException(new ExecuteLogEventArgs("ExecuteDataSet", Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return set;
        }



        /// <summary>
        /// 执行命令，返回查询的数据表
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public DataTable ExecuteDataTable()
        {
            DataTable table = null;
            MakeSureCommandNotNull();
            TriggerBeforeExecute();
            try
            {
                using (IDataReader reader = this.Command.ExecuteReader())
                {
                    table = new DataTable();
                    table.Load(reader);
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                ProcessException(new ExecuteLogEventArgs("ExecuteDataTable", Command.CommandText, ex));
            }

            TriggerAfterExecute();

            return table;
        }



        /// <summary>
        ///     打开数据库连接
        /// </summary>
        public void Open()
        {
            MakeSureConnectionNotNull();
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        /// <summary>
        ///     关闭数据库连接
        /// </summary>
        public void Close()
        {
            if (Connection != null)
                if (Connection.State == ConnectionState.Open)
                {
                    Connection.Close();
                }
            if (Transaction != null)
            {
                Transaction.Dispose();
            }
        }

        /// <summary>
        ///     开始事务,数据库连接将保持连接状态
        /// </summary>
        public void BeginTran()
        {
            IsAutoOpenClose = false;
            Open();
            Transaction = Connection.BeginTransaction();
        }

        /// <summary>
        ///     事务回滚
        /// </summary>
        public void Rollback()
        {
            if (Transaction != null)
            {
                Transaction.Rollback();
            }
        }

        /// <summary>
        ///     提交事务
        /// </summary>
        public void CommitTran()
        {
            IsAutoOpenClose = true;
            if (this.Transaction == null)
            {
                throw new InvalidOperationException("Transaction is null.");
            }
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
            Close();
        }

        /// <summary>
        /// 参数格式化
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        string FiltPara(string sql, string para)
        {
            Regex re = new Regex("@" + para, RegexOptions.IgnoreCase);
            string formatedSql = re.Replace(sql, DriverType.NamedPrefix + para);
            return formatedSql;
        }

        /// <summary>
        ///     获取sql查询cmd
        /// </summary>
        /// <param name="strSql">sql语句</param>
        /// <returns></returns>
        public IDbCommand GetSqlStringCommond(string strSql)
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = strSql;
            cmd.CommandType = CommandType.Text;
            this.Command = cmd;
            SetCommandState();
            return Command;
        }

        IDbCommand GetCommand()
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandType = CommandType.Text;
            this.Command = cmd;
            SetCommandState();
            return Command;
        }

        /// <summary>
        ///     获取存储过程cmd
        /// </summary>
        /// <param name="strProcName">存储过程名</param>
        /// <returns></returns>
        public IDbCommand GetProcedureCommond(string strProcName)
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = strProcName;
            cmd.CommandType = CommandType.StoredProcedure;
            this.Command = cmd;
            SetCommandState();
            return Command;
        }

        /// <summary>
        ///     替换命令对象中的参数
        /// </summary>
        /// <param name="paraName">参数名</param>
        /// <param name="paraValue">参数值</param>
        public void AddInParameter(string paraName, object paraValue)
        {
            AddInParameter(paraName, null, null, paraValue);
        }
        /// <summary>
        ///     替换command中的input类型参数
        /// </summary>
        /// <param name="paraName">参数名</param>
        /// <param name="paraType">参数类型</param>
        /// <param name="size">大小</param>
        /// <param name="paraValue">参数值</param>
        public void AddInParameter(string paraName, DbType? paraType, int? size, object paraValue)
        {
            MakeSureCommandNotNull();
            IDbDataParameter parameter = GetParameter(paraName, paraType, size, paraValue);
            parameter.Direction = ParameterDirection.Input;
            this.Command.CommandText = FiltPara(this.Command.CommandText, paraName);
            this.Command.Parameters.Add(parameter);
        }

        /// <summary>
        ///     替换命令对象中的参数
        /// </summary>
        /// <param name="paraName">参数名</param>
        /// <param name="paraValue">参数值</param>
        public void AddOutParameter(string paraName, object paraValue)
        {
            AddOutParameter(paraName, null, null, paraValue);
        }

        /// <summary>
        ///     替换command中的output类型参数
        /// </summary>
        /// <param name="paraName">参数名</param>
        /// <param name="paraType">参数类型</param>
        /// <param name="size">大小</param>
        /// <param name="paraValue">参数值</param>
        public void AddOutParameter(string paraName, DbType? paraType, int? size, object paraValue)
        {
            MakeSureCommandNotNull();
            IDbDataParameter parameter = GetParameter(paraName, paraType, size, paraValue);
            parameter.Direction = ParameterDirection.Output;

            this.Command.CommandText = FiltPara(this.Command.CommandText, paraName);
            this.Command.Parameters.Add(parameter);
        }

        /// <summary>
        ///     替换command中的ReturnValue类型参数
        /// </summary>
        /// <param name="paraName">参数名</param>
        /// <param name="paraValue">参数值</param>
        public void AddReturnParameter(string paraName, object paraValue)
        {
            AddReturnParameter(paraName, null, null, paraValue);
        }
        /// <summary>
        ///     替换command中的ReturnValue类型参数
        /// </summary>
        /// <param name="paraName">参数名</param>
        /// <param name="paraType">参数类型</param>
        /// <param name="size">大小</param>
        /// <param name="paraValue">参数值</param>
        public void AddReturnParameter(string paraName, DbType? paraType, int? size, object paraValue)
        {
            MakeSureCommandNotNull();
            IDbDataParameter parameter = GetParameter(paraName, paraType, size, paraValue);
            parameter.Direction = ParameterDirection.ReturnValue;
            this.Command.CommandText = FiltPara(this.Command.CommandText, paraName);
            this.Command.Parameters.Add(parameter);
        }



        /// <summary>
        ///     获取command中的指定parameter
        /// </summary>
        /// <param name="cmd">命令对象</param>
        /// <param name="parameterName">参数名</param>
        /// <returns></returns>
        public IDbDataParameter GetCmdParameter(string parameterName)
        {
            return (IDbDataParameter)this.Command.Parameters[parameterName];
        }

        string GetCurrentSql(string sql)
        {
            string currentSql = sql;
            IDataParameterCollection lsp = this.Command.Parameters;
            foreach (IDataParameter item in lsp)
            {
                currentSql = currentSql.Replace(item.ParameterName, item.Value.ToString());
            }
            return currentSql;
        }

        #region 获取JSON
        /// <summary>
        /// 获取Json数据
        /// </summary>
        /// <returns></returns>
        public string ExecuteSelectCmdGetJson()
        {
            string result = string.Empty;
            this.MakeSureCommandNotNull();
            this.TriggerBeforeExecute();
            try
            {
                result = ToJson(this.Command.ExecuteReader(CommandBehavior.SingleRow));
            }
            catch (Exception ex)
            {
                ProcessException(new ExecuteLogEventArgs("ExecuteSelectCmdGetJson", Command.CommandText, ex));
            }
            this.TriggerAfterExecute();
            return result;
        }

        string ToJson(IDataReader dataReader)
        {
            try
            {
                StringBuilder jsonString = new StringBuilder();
                jsonString.Append("[");

                while (dataReader.Read())
                {
                    jsonString.Append("{");
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        Type type = dataReader.GetFieldType(i);
                        string strKey = dataReader.GetName(i);
                        object strValue = dataReader[i];
                        jsonString.Append("\"" + strKey + "\":");
                        strValue = ConvertValue(strValue);
                        if (i < dataReader.FieldCount - 1)
                        {
                            jsonString.Append(strValue + ",");
                        }
                        else
                        {
                            jsonString.Append(strValue);
                        }
                    }
                    jsonString.Append("},");
                }
                if (!dataReader.IsClosed)
                {
                    dataReader.Close();
                }
                jsonString.Remove(jsonString.Length - 1, 1);
                jsonString.Append("]");
                if (jsonString.Length == 1)
                {
                    return "[]";
                }
                return jsonString.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        string ConvertValue(object value)
        {
            Type T = value.GetType();
            string result = string.Empty;
            switch (T.Name)
            {
                case Types._int16:
                case Types._int32:
                case Types._int64:
                case Types._float:
                case Types._double:
                case Types._decimal:
                    result = value.ToString();
                    break;
                case Types._bool:
                    result = value.ToString().ToLower();
                    break;
                default:
                    result = "\"" + String2Json(value.ToString()) + "\"";
                    break;
            }

            return result;
        }

        class Types
        {
            public const string _int16 = "Int16";
            public const string _int32 = "Int32";
            public const string _int64 = "Int64";
            public const string _float = "Single";
            public const string _double = "Double";
            public const string _decimal = "Decimal";
            public const string _bool = "Boolean";

        }

        /// <summary>  
        /// 过滤特殊字符  
        /// </summary>  
        /// <param name="s"></param>  
        /// <returns></returns>  
        string String2Json(String s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToCharArray()[i];
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\""); break;
                    case '\\':
                        sb.Append("\\\\"); break;
                    case '/':
                        sb.Append("\\/"); break;
                    case '\b':
                        sb.Append("\\b"); break;
                    case '\f':
                        sb.Append("\\f"); break;
                    case '\n':
                        sb.Append("\\n"); break;
                    case '\r':
                        sb.Append("\\r"); break;
                    case '\t':
                        sb.Append("\\t"); break;
                    case '\v':
                        sb.Append("\\v"); break;
                    case '\0':
                        sb.Append("\\0"); break;
                    default:
                        sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
        #endregion

        #region 获取实体

        /// <summary>
        ///     执行当前命令，获取一个业务实体对象 
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <returns></returns>
        public T ExecuteSelectCmdGetBllObject<T>() where T : class,new()
        {
            return this.ExecuteSelectCmdGetBllObject<T>(true, true);
        }
        /// <summary>
        ///     执行当前命令，获取一个业务实体对象 
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="loadAllField">是否加载业务实体全部字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns>数据实体类型</returns>
        public T ExecuteSelectCmdGetBllObject<T>(bool loadAllField, bool checkSucessCount) where T : class, new()
        {

            EntityHelper.EnsureIsDataItemType(typeof(T));
            this.MakeSureCommandNotNull();
            this.TriggerBeforeExecute();
            T entity = default(T);
            try
            {
                using (IDataReader reader = this.Command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    CommonDataAdapter row = new CommonDataAdapter(reader);
                    if (reader.Read())
                    {
                        entity = Activator.CreateInstance(typeof(T)) as T;
                        EntityHelper.LoadItemValuesFormDbRow(row, entity, loadAllField, checkSucessCount);
                    }
                    reader.Close();
                }
                return entity;
            }
            catch (Exception ex)
            {
                this.ProcessException(new ExecuteLogEventArgs("ExecuteSelectCommandGetBllObject<" + typeof(T).Name + ">", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return entity;
        }

        /// <summary>
        ///     执行当前命令并返回一个泛型的Dictionary，表中关键字为ID整型
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个int的值</param>
        /// <returns></returns>
        public Dictionary<int, T> ExecuteSelectCmdToDicKeyInt<T>(string fieldName) where T : class,new()
        {
            return this.ExecuteSelectCmdToDicKeyInt<T>(fieldName, true, true);
        }

        /// <summary>
        ///     执行当前命令并返回一个泛型的Dictionary，表中关键字为ID整型
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个int的值</param>
        /// <param name="loadAllField">是否加载所有字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns></returns>
        public Dictionary<int, T> ExecuteSelectCmdToDicKeyInt<T>(string fieldName, bool loadAllField, bool checkSucessCount) where T : class, new()
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }

            EntityHelper.EnsureIsDataItemType(typeof(T));
            this.MakeSureCommandNotNull();
            this.TriggerBeforeExecute();
            Dictionary<int, T> dictionary = new Dictionary<int, T>();
            try
            {
                using (IDataReader reader = this.Command.ExecuteReader())
                {
                    CommonDataAdapter row = new CommonDataAdapter(reader);
                    while (reader.Read())
                    {
                        T item = Activator.CreateInstance(typeof(T)) as T;
                        EntityHelper.LoadItemValuesFormDbRow(row, item, loadAllField, checkSucessCount);
                        dictionary.Add(Convert.ToInt32(reader[fieldName]), item);
                    }
                    reader.Close();
                }
                return dictionary;
            }
            catch (Exception ex)
            {
                this.ProcessException(new ExecuteLogEventArgs("ExecuteSelectCommandToDictionary<" + typeof(T).Name + ">", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return dictionary;
        }

        /// <summary>
        ///     执行当前命令并返回一个泛型的Dictionary，表中关键字为string
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个string的值</param>
        /// <returns></returns>
        public Dictionary<string, T> ExecuteSelectCmdToDicKeyString<T>(string fieldName) where T : class ,new()
        {
            return this.ExecuteSelectCmdToDicKeyString<T>(fieldName, true, true);
        }
        /// <summary>
        ///     执行当前命令并返回一个泛型的Dictionary，表中关键字为string
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="fieldName">做为字典KEY的字段名，它应该包含一个string的值</param>
        /// <param name="loadAllField">是否加载所有字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns></returns>
        public Dictionary<string, T> ExecuteSelectCmdToDicKeyString<T>(string fieldName, bool loadAllField, bool checkSucessCount) where T : class, new()
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentNullException("fieldName");
            }

            EntityHelper.EnsureIsDataItemType(typeof(T));
            this.MakeSureCommandNotNull();
            this.TriggerBeforeExecute();
            Dictionary<string, T> dictionary = new Dictionary<string, T>();
            try
            {
                using (IDataReader reader = this.Command.ExecuteReader())
                {
                    CommonDataAdapter row = new CommonDataAdapter(reader);
                    while (reader.Read())
                    {
                        T item = Activator.CreateInstance(typeof(T)) as T;
                        EntityHelper.LoadItemValuesFormDbRow(row, item, loadAllField, checkSucessCount);
                        dictionary.Add(reader[fieldName].ToString(), item);
                    }
                    reader.Close();
                }
                return dictionary;
            }
            catch (Exception ex)
            {
                this.ProcessException(new ExecuteLogEventArgs("ExecuteSelectCommandToDictionary<" + typeof(T).Name + ">", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return dictionary;
        }

        /// <summary>
        ///     执行当前命令，并返回一个泛型的list
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <returns></returns>
        public List<T> ExecuteSelectCommandToList<T>() where T : class ,new()
        {
            return this.ExecuteSelectCommandToList<T>(true, true);
        }
        /// <summary>
        ///     执行当前命令，并返回一个泛型的list
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="loadAllField">是否加载所有字段</param>
        /// <param name="checkSucessCount">加载完成后，是否要检查成功的数量，确保每次的加载都是成功的</param>
        /// <returns></returns>
        public List<T> ExecuteSelectCommandToList<T>(bool loadAllField, bool checkSucessCount) where T : class ,new()
        {
            EntityHelper.EnsureIsDataItemType(typeof(T));
            this.MakeSureCommandNotNull();
            this.TriggerBeforeExecute();
            List<T> list = new List<T>();
            try
            {
                using (IDataReader reader = this.Command.ExecuteReader())
                {
                    CommonDataAdapter row = new CommonDataAdapter(reader);
                    while (reader.Read())
                    {
                        T item = Activator.CreateInstance(typeof(T)) as T;
                        EntityHelper.LoadItemValuesFormDbRow(row, item, loadAllField, checkSucessCount);
                        list.Add(item);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                ProcessException(new ExecuteLogEventArgs("ExecuteSelectCommandToList<" + typeof(T).Name + ">", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return list;
        }

        #endregion

        #endregion

        #region WriteExtension

      
        /// <summary>
        ///   插入一行数据 E.g
        /// <para>db.Insert(new Person { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="obj">实体</param>
        /// <returns>受影响行数</returns>
        public int Insert<T>(T obj)
        {
            Type entityType = typeof(T);
            EntityHelper.EnsureIsDataItemType(entityType);
            int res = 0;
            this.TriggerBeforeExecute();
            try
            {
                List<EntityHelper.ParameterValue> description = new List<EntityHelper.ParameterValue>();
                description = EntityHelper.GetInsertDescription<T>(typeof(T), obj);
                StringBuilder columns = new StringBuilder();
                StringBuilder parameters = new StringBuilder();
                int total = description.Count;
                GetCommand();
                for (int i = 0; i < total; i++)
                {
                    if (columns.Length > 0)
                    {
                        columns.Append(",");
                        parameters.Append(",");
                    }
                    columns.Append(description[i].Parameter);
                    parameters.Append(FiltPara("@" + description[i].Parameter, description[i].Parameter));
                    AddInParameter(description[i].Parameter, description[i].Value);
                }
                string sql = string.Format("insert into {0} ({1}) values({2})", entityType.Name,
                    columns.ToString(), parameters.ToString());

                Command.CommandText = sql;

                res=  ExecuteNonQuery();


            }
            catch (Exception ex)
            {
                this.ProcessException(new ExecuteLogEventArgs("Insert<" + typeof(T).Name + ">", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return res;
        }

        /// <summary>
        /// 删除一或者多行数据，只使用（不是默认值）属性的值作为筛选条件  E.g:
        /// <para>db.DeleteNonDefaults(new Person { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>受影响的行数</returns>
        public int DeleteNonDefults<T>(T obj)
        {
            Type entityType = typeof(T);
            EntityHelper.EnsureIsDataItemType(entityType);
            int res = 0;
            this.TriggerBeforeExecute();
            try
            {
                EntityHelper.SqlStatement _description = EntityHelper.GetDeleteNonDefultsStatement<T>(entityType, obj);
                GetCommand();
                this.Command.CommandText = _description.Sql;
                foreach (var item in _description.Parameters)
                {
                    AddInParameter(item.Parameter, item.Value);
                }
                res = ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                this.ProcessException(new ExecuteLogEventArgs("DeleteNonDefults<" + typeof(T).Name + ">", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return res;
        }

        /// <summary>
        /// 删除一或者多行数据，使用匿名类属性的值作为筛选条件  E.g:
        /// <para>db.Delete(new Person { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="anonType"></param>
        /// <returns>受影响的行数</returns>
        public int Delete<T>(object anonType)
        {

            Type entityType = typeof(T);
            EntityHelper.EnsureIsDataItemType(entityType);
            int res = 0;
            this.TriggerBeforeExecute();
            try
            {
                EntityHelper.SqlStatement _description = EntityHelper.GetDeleteStatement(entityType, anonType);
                GetCommand();
                this.Command.CommandText = _description.Sql;
                foreach (var item in _description.Parameters)
                {
                    AddInParameter(item.Parameter, item.Value);
                }
                res = ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                this.ProcessException(new ExecuteLogEventArgs("Delete<" + typeof(T).Name + ">", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return res;
        }

        /// <summary>
        /// 更新一或者多行数据,使用主键作为筛选条件  E.g
        /// <para>db.Update(new Person { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="updateAllFields">是否更新所有字段</param>
        /// <returns>受影响的行数</returns>
        public int Update<T>(T obj, bool updateAllFields = false)
        {
            Type entityType = typeof(T);
            EntityHelper.EnsureIsDataItemType(entityType);
            int res = 0;
            this.TriggerBeforeExecute();
            try
            {
                EntityHelper.SqlStatement _description = EntityHelper.GetUpdateStatement<T>(entityType, obj, updateAllFields);
                GetCommand();
                this.Command.CommandText = _description.Sql;
                foreach (var item in _description.Parameters)
                {
                    AddInParameter(item.Parameter, item.Value);
                }
                res = ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                this.ProcessException(new ExecuteLogEventArgs("Update<" + typeof(T).Name + ">", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return res;
        }

        #endregion

        #region ReadExtension

        /// <summary>
        /// 查询一行或者多行数据，只使用（不是默认值）属性的值作为筛选条件  E.g:
        /// <para>db.Select(new Person { FirstName = "Jimi", Age = 27 })</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>集合</returns>
        public List<T> Select<T>(T obj)where T:class,new()
        {
            Type entityType = typeof(T);
            EntityHelper.EnsureIsDataItemType(entityType);
            List<T> res=new List<T>();
            this.TriggerBeforeExecute();
            try
            {
                EntityHelper.SqlStatement _description = EntityHelper.GetSelectStatement<T>(entityType, obj);
                GetCommand();
                this.Command.CommandText = _description.Sql;
                foreach (var item in _description.Parameters)
                {
                    AddInParameter(item.Parameter, item.Value);
                }
                res = ExecuteSelectCommandToList<T>(true, false);
            }
            catch (Exception ex)
            {
                this.ProcessException(new ExecuteLogEventArgs("Select<" + typeof(T).Name + ">", this.Command.CommandText, ex));
            }

            this.TriggerAfterExecute();

            return res;
        }

        #endregion
        #region Private Methods

        /// <summary>
        ///     处理运行时异常
        /// </summary>
        /// <param name="args"></param>
        public void ProcessException(ExecuteLogEventArgs args)
        {
            args.ExecuteText = GetCurrentSql(args.ExecuteText);
            Rollback();
            Dispose();
            this.isOccurException = true;

            if (IsWriteLog)
            {
                LogForDB.WriteLog(args.ExecuteMethod + ":" + args.ExecuteText + ">>>" + args._Exception.Message);
            }
            if (ExecuteException != null)
            {
                ExecuteException(args);
            }
            else
            {
                throw args._Exception; //异常需抛出到上层
            }
        }

        void ProcessInitException(string errorMsg)
        {
            if (IsWriteLog)
            {
                LogForDB.WriteLog("数据处理层初始化异常>>>" + errorMsg);
            }
            throw new Exception("数据处理层初始化异常>>>" + errorMsg);


        }

        /// <summary>
        ///     执行前触发函数
        /// </summary>
        private void TriggerBeforeExecute()
        {
            if (IsAutoOpenClose)
            {
                Open();
            }

            if (Execute != null)
            {
                Execute(new ExecuteEventArgs(ExecuteTime.BeforeExecute));
            }
        }

        /// <summary>
        ///     执行后触发函数
        /// </summary>
        private void TriggerAfterExecute()
        {
            if (IsAutoOpenClose)
            {
                Close();
            }
            if (!isOccurException)
            {
                if (Execute != null)
                {
                    Execute(new ExecuteEventArgs(ExecuteTime.AfterExecute));
                }
            }
            if (IsAutoResetCommand)
            {
                ClearCommandStatus();
            }
        }

        /// <summary>
        ///     确认命令不为空
        /// </summary>
        private void MakeSureCommandNotNull()
        {
            if (this.Command == null)
            {
                throw new InvalidOperationException("Command is null.");
            }
        }

        /// <summary>
        ///     确认连接对象不为空
        /// </summary>
        private void MakeSureConnectionNotNull()
        {
            if (this.Connection == null)
            {
                throw new InvalidOperationException("Connection is null.");
            }
        }

        /// <summary>
        ///     设置命令对象状态
        /// </summary>
        private void SetCommandState()
        {
            if (Transaction != null)
            {
                this.Command.Transaction = Transaction;
            }
        }

        /// <summary>
        ///     清理当前命令对象，处理方式：重置CommandText, Parameters.Clear() 
        /// </summary>
        private void ClearCommandStatus()
        {
            this.MakeSureCommandNotNull();
            this.Command.CommandText = null;
            this.Command.Parameters.Clear();
        }

        /// <summary>
        ///     获取参数，不指定类型
        /// </summary>
        /// <param name="paraName">参数名</param>
        /// <param name="paraType">参数类型</param>
        /// <param name="size">大小</param>
        /// <param name="paraValue">参数值</param>
        /// <returns>IDbDataParameter</returns>
        private IDbDataParameter GetParameter(string paraName, DbType? paraType, int? size, object paraValue)
        {
            IDbDataParameter parameter = this.Command.CreateParameter();
            if (paraType.HasValue)
            {
                parameter.DbType = paraType.Value;
            }
            parameter.ParameterName = DriverType.FormatNameForParameter(paraName);
            if (size.HasValue)
            {
                parameter.Size = (int)size;
            }
            parameter.Value = paraValue == null ? DBNull.Value : paraValue;
            return parameter;
        }

        #endregion

        #region IDisposable Members


        /// <summary>
        ///     释放放当前数据处理环境占用的资源
        /// </summary>
        public void Dispose()
        {
            CDispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool disposed = false;

        protected void CDispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if (Connection != null)
                    {
                        if (this.Connection.State != System.Data.ConnectionState.Closed)
                        {
                            this.Connection.Close();
                        }
                        this.Connection.Dispose();
                        this.Connection = null;
                    }
                    if (Command != null)
                    {
                        Command.Dispose();
                    }
                    if (Transaction != null)
                    {
                        Transaction.Dispose();
                    }
                }

            }
            disposed = true;
        }

        /// <summary>
        ///     释放当前对象资源
        /// </summary>
        ~DBHelper()
        {
            CDispose(false);
        }

        #endregion
    }
}

