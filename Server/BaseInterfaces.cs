using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    interface Itransfer
    {
        string Read();
        string Write(string json);
    }
    interface ISendAble
    {
        long Id { get;}
        bool IsNew { get; }
        bool Edited { get;}
        bool Removed { get;}
        bool Select { get; }
    }
    interface IProduct
    {
        long Price { get;}
        string Name { get;}
        string Description { get;}
        string Code { get; }
    }
    
    interface ICustomer
    {
        long Balance { get; set; }
        long OrderCountRecive { get; set; }
        long OrderCountRemove { get; set; }
        string SubscribeCode { get; }
    }

    interface IClerk
    {
        string UserName { get; }
        string Password { get; }
        bool IsAdmin { get; set; }
    }
}
