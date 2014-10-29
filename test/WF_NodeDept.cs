using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TKS.ORMLite.Attrs;

namespace test
{
    /// <summary>
    /// 节点部门表
    /// </summary>
    public class WF_NodeStation
    {


        /// <summary>
        /// 节点
        /// </summary>
        public int FK_NODE
        {
            get;
            set;
        }


        /// <summary>
        /// 岗位
        /// </summary>
        public string FK_Station
        {
            get;
            set;
        }
        [PrimaryKey]
        public string FK_Flow
        {
            get;
            set;
        }

        public string StationName
        {
            get;
            set;
        }

        public string FK_Dept
        {
            get;
            set;
        }
    }


    public class IC_CAR_APPLY
    {
        public string STATUS_NAME
        {
            get;
            set;
        }

        public float MAN_NUM
        {
            get;
            set;
        }

        public DateTime PLAN_START_TIME
        {
            get;
            set;
        }
    }
}
