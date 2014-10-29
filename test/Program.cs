using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TKS.ORMLite.Driver;
using TKS.ORMLite;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;

namespace test
{

    class Program
    {
        /// <summary>
        /// 测试方法运行时间
        /// </summary>
        /// <param name="test">传入方法名</param>
        public static void TestMethodProcessTime(Action test)
        {
            Stopwatch process = new Stopwatch();
            process.Start();
            test();
            process.Stop();
            Console.WriteLine("总运行时间：" + process.Elapsed);
            Console.WriteLine("测量实例得出的总运行时间（毫秒为单位）：" + process.ElapsedMilliseconds);
            Console.WriteLine("总运行时间(计时器刻度标识)：" + process.ElapsedTicks);
            Console.WriteLine("计时器是否运行：" + process.IsRunning.ToString());
            Console.ReadLine();
        }
        static void Main(string[] args)
        {
            //for (int i = 0; i < 10;i++ )
            //    InsertTest();
            Console.ReadLine();
            for (int i = 0; i < 10; i++)
            {
                TestMethodProcessTime(InsertTest);
                TestMethodProcessTime(UpdateTest);
                TestMethodProcessTime(SelectTest);
            }


        }

       



      
        static void testException()
        {
            DBHelper db;
            try
            {
                //用数据驱动设备构造DBHelper
                db = new DBHelper();
                db.IsWriteLog = true;
                db.IsAutoOpenClose = false;
                db.IsAutoResetCommand = false;
                //可添加异常事件触发处理
                db.ExecuteException = (e) => { throw new Exception(e.ExecuteMethod + ">>>" + e.ExecuteText + ">>>" + e._Exception.Message); };

                //可添加命令执行前后触发事件
                db.Execute = (e) => { };
                string sql = "select * from wx_areas where (app_id=123 and areaid=002)";

                //获取处理命令
                db.GetSqlStringCommond(sql);

                //对命令添加参数
                // db.AddInParameter("fk_node", 10);

                int json = db.ExecuteNonQuery();
                //获取实体
                List<IC_CAR_APPLY> lis = db.ExecuteSelectCommandToList<IC_CAR_APPLY>(true,false);

                //foreach (KeyValuePair<int, Base_Businesscard> pair in dic)
                //{
                //    Console.WriteLine(pair.Key);
                //    foreach (PropertyInfo item in pair.Value.GetType().GetProperties())
                //    {
                //        Console.Write("----"+item.GetValue(pair.Value ,null ));
                //    }
                //    Console.WriteLine();
                //}

                Console.WriteLine(json);
                foreach (IC_CAR_APPLY item in lis)
                {
                    
                    Console.WriteLine(item.STATUS_NAME + ":" + item.PLAN_START_TIME);
                }

                db.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        static void test()
        {
            string sql = "insert into wf_track (fid,wid,msg)values(@fid,@wid,@msg)";

            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    DBHelper DB = new DBHelper();

                    string s = "select * from wf_nodestation";
                    DB.GetSqlStringCommond(s);
                    var ss = DB.ExecuteScalar();
                    Console.WriteLine(ss.ToString());

                    DB.BeginTran();
                    DB.GetSqlStringCommond(sql);
                    DB.AddInParameter("fid", i.ToString());
                    DB.AddInParameter("wid", i.ToString());
                    DB.AddInParameter("msg", "test");
                    DB.ExecuteNonQuery();

                    DB.GetSqlStringCommond(sql);
                    // DB.AddInParameter("fid", i.ToString());
                    DB.AddInParameter("wid", i.ToString());
                    DB.AddInParameter("msg", "test");
                    DB.ExecuteNonQuery();

                    DB.CommitTran();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "test" + i.ToString());
                    // Console.ReadLine();
                    continue;

                }
            }
        }

        public static  void InsertTest()
        {
            DBHelper db = new DBHelper();
            try
            {
                var res = db.Insert<WF_NodeStation>(new WF_NodeStation { 
                    FK_Flow = "111", FK_NODE = 001, FK_Station = "123", StationName="测试的", FK_Dept="dept" });

                Console.WriteLine(res.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void InsertTest2()
        {
            string sql = "insert into WF_NodeStation(fk_flow,fk_node,fk_Station)values(@fk_flow,@fk_node,@fk_station)";
            DBHelper db = new DBHelper();
            db.GetSqlStringCommond(sql);
            db.AddInParameter("fk_flow", "999");
            db.AddInParameter("fk_node",123);
            db.AddInParameter("fk_station","222");
           var res= db.ExecuteNonQuery();
           Console.WriteLine(res.ToString());
        }

        public static void UpdateTest()
        {
            DBHelper db = new DBHelper();
            try
            {
                var res = db.Update<WF_NodeStation>(new WF_NodeStation { FK_Flow = "111", FK_NODE = 002, FK_Station ="222" },true);
                Console.WriteLine(res.ToString ());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void DeleteTest()
        {
            DBHelper db = new DBHelper();
            db.ExecuteException = (e) => { throw new Exception(e.ExecuteMethod + ">>>" + e.ExecuteText + ">>>" + e._Exception.Message); };

            try
            {
                var res = db.DeleteNonDefults<WF_NodeStation>(new WF_NodeStation { FK_Flow = "999" });
                Console.WriteLine(res.ToString());

                var r = db.Delete<WF_NodeStation>(new { fkFlow="111" });
                Console.WriteLine(r.ToString ());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void SelectTest()
        {
            DBHelper db = new DBHelper();
            db.ExecuteException = (e) => { throw new Exception(e.ExecuteMethod + ">>>" + e.ExecuteText + ">>>" + e._Exception.Message); };

            try
            {
                var res = db.Select<WF_NodeStation>(new WF_NodeStation { FK_Flow = "111" });
                foreach (var item in res)
                {
                    Console.WriteLine(item.FK_Dept + item.FK_Flow);
                }
             }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static Type GetRealType(Type testType)
        {
            if (testType.IsGenericType && (testType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return testType.GetGenericArguments()[0];
            }
            return testType;
        }
    }
}
