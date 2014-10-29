using System;
using System.Collections.Generic;
using System.Text;

namespace TKS.ORMLite
{
    /// <summary>
    ///     实体类描述
    /// </summary>
    internal class EntityDescription
    {   
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="type">实体类类型</param>
        /// <param name="memberCapacity">成员数量</param>
        public EntityDescription(Type type, int memberCapacity)
        {
            ItemType = type;
            Dict = new Dictionary<string, EntityMemberDescription>(memberCapacity, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     根据一个成员的名称，获取相应的EntityMemberDescription的实例描述信息。 
        /// </summary>
        /// <param name="name">成员的属性名或映射的数据库的字段名</param>
        /// <param name="info">EntityMemberDescription对象</param>
        /// <returns>是否成功获取到指定的成员信息</returns>
        public bool TryGetValue(string name, out EntityMemberDescription info)
        {
            if (this.Dict.TryGetValue(name, out info))
            {
                return true;
            }
            foreach (KeyValuePair<string, EntityMemberDescription> pair in this.Dict)
            {
                if (pair.Value.MemberAttr.DbFieldName == name)
                {
                    info = pair.Value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     设置实体类的期望加载成员的成功数量
        /// </summary>
        /// <param name="loadAllcount">加载全部时的数量</param>
        /// <param name="loadPartCount">加载部分时的数量</param>
        public void SetSuccessCount(int loadAllcount,int loadPartCount)
        {
            this.LoadALLValues_ExpectSuccessCount = loadAllcount;
            this.LoadPartialValues_ExpectSuccessCount = loadPartCount;
        }

        /// <summary>
        ///     实体类的成员描述,key：成员名，value：成员描述
        /// </summary>
        public Dictionary<string, EntityMemberDescription> Dict
        {
            get;
            private set;
        }
        /// <summary>
        ///     实体类的类型
        /// </summary>
        public Type ItemType
        {
            get;
            private set;
        }

        /// <summary>
        ///     当实体类成员有设置OnlyLoadAll为true时候，全部加载和部分加载才会有区分，
        ///     否则全部加载和部分加载没有区分，返回【全部加载时】期望的成功加载数据成员数量
        /// </summary>
        public int LoadALLValues_ExpectSuccessCount
        {
            get;
            private set;
        }

        /// <summary>
        ///     当实体类成员有设置OnlyLoadAll为true时候，全部加载和部分加载才会有区分，
        ///     否则全部加载和部分加载没有区分，返回【部分加载时】期望的成功加载数据成员数量 
        /// </summary>
        public int LoadPartialValues_ExpectSuccessCount
        {
            get;
            private set;
        }
    }
}
