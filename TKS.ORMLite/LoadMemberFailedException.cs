using System;
using System.Collections.Generic;
using System.Text;

namespace TKS.ORMLite
{

    public class LoadMemberFailedException:ApplicationException 
    {
        public LoadMemberFailedException(Type T, string itemField)
            : base("when load the instance of [" + T.ToString() + "]，some members load failed，name： [" + itemField  + "]")
        {

        }
    }
}
