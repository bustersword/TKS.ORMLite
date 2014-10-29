using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using TKS.ORMLite.Attrs;
using System.Threading;

namespace TKS.ORMLite
{
    /// <summary>
    ///     实体类成员描述，并提供获取、设置成员值的方法
    /// </summary>
    internal  class EntityMemberDescription
    {
       
        /// <summary>
        ///     构造函数
        /// </summary>
        public EntityMemberDescription()
        {
            IsPK = false;
        }

        /// <summary>
        ///     获取该成员的值
        /// </summary>
        /// <param name="item">将要返回成员值的对象</param>
        /// <returns>成员的值</returns>
        public object GetValue(object item)
        {
            if (this.IsProperty)
            {
                return ((PropertyInfo) this.MemberInfo).GetValue(item, null);
            }
            return ((FieldInfo) this.MemberInfo).GetValue(item);
        }
        /// <summary>
        /// 获取默认值
        /// </summary>
        /// <returns></returns>
        public object GetDefaultValue()
        {
            if (this.IsProperty)
            {
                return TypeList.GetDefaultValue(this.MemberType); 
            }
            return TypeList.GetDefaultValue(this.MemberType);
        }
      
        /// <summary>
        ///     设置成员的值
        /// </summary>
        /// <param name="item">将要设置成员值的对象</param>
        /// <param name="val">赋予成员的值</param>
        public void SetValue(object item, object val)
        {
            if (this.IsProperty)
            {
                ((PropertyInfo) this.MemberInfo).SetValue(item, val, null);
            }
            else
            {
                ((FieldInfo) this.MemberInfo).SetValue(item, val);
            }
        }

        /// <summary>
        ///     此成员是否为“属性”，否则为“字段”
        /// </summary>
        public bool IsProperty
        {
            get
            {
                return (this.MemberInfo.MemberType == MemberTypes.Property);
            }
        }

        /// <summary>
        ///  是否为主键
        /// </summary>
        public bool IsPK
        {

            get;
            set;
        }

        /// <summary>
        ///     ItemFieldAttribute形式的描述信息 
        /// </summary>
        public ItemFieldAttribute MemberAttr
        {
            get;
            set;

        }

        /// <summary>
        ///     反射信息
        /// </summary>
        public System.Reflection.MemberInfo MemberInfo
        {
            get;
            set;

        }

        /// <summary>
        ///     此成员（字段或属性）的类型 
        /// </summary>
        public Type MemberType
        {
            get
            {
                if (!this.IsProperty)
                {
                    return ((FieldInfo) this.MemberInfo).FieldType;
                }
                return ((PropertyInfo) this.MemberInfo).PropertyType;
            }
        }
    }
}
