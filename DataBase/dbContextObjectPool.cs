//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.ObjectPool;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace DarlingBotNet.DataBase
//{
//    public class dbContextObjectPool : IPooledObjectPolicy<DbContext>
//    {
//        public static DbContext Create()
//        {
//            return new DBcontext();
//        }

//        public static DbContext Return(DbContext db)
//        {
//            return new DBcontext();
//        }
//    }
//}
