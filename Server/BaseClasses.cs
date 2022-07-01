using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Person
    {
        public string Name { get; }
        public string PhoneNumber { get; }
        public string Address { get; }
        
        public Person(string Name, string PhoneNumber, string Address)
        {
            this.Name = Name;
            this.PhoneNumber = PhoneNumber;
            this.Address = Address;
        }
    }

    public class Clerk: Person, ISendAble, IClerk
    {
        public long Id { get; set; }
        public bool IsNew { get; set; }
        public bool Edited { get; set; }
        public bool Removed { get; set; }
        public bool Select { get; set; }
        public string UserName { get; }
        public string Password { get; }
        public bool IsAdmin { get; set; }
        public Clerk(string Name, string PhoneNumber, string Address, string UserName, string Password, bool IsAdmin)
            :base(Name, PhoneNumber, Address)
        {
            this.UserName = UserName;
            this.Password = Password;
            this.IsAdmin = IsAdmin;
        }
    }

    public class Customer : Person, ISendAble, ICustomer
    {
        public long Id { get; set; }
        public bool IsNew { get; set; }
        public bool Edited { get; set; }
        public bool Removed { get; set; }
        public bool Select { get; set; }

        public long Balance { get; set; }
        public long OrderCountRecive { get; set; }
        public long OrderCountRemove { get; set; }
        public string SubscribeCode { get; }

        public Customer(string Name, string PhoneNumber, string Address, string SubscribeCode)
            : base(Name, PhoneNumber, Address)
        {
            this.SubscribeCode = SubscribeCode;
            OrderCountRecive = 0;
            OrderCountRemove = 0;
            Balance = 0;
        }
    }

    class Cake: IProduct, ISendAble
    {
        public long Id { get; set; }
        public bool IsNew { get; set; }
        public bool Edited { get; set; }
        public bool Removed { get; set; }
        public bool Select { get; set; }

        public long Price { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }

        public Cake(long Price, string Name, string Description, string Code)
        {
            this.Price = Price;
            this.Name = Name;
            this.Description = Description;
            this.Code = Code;
        }
    }
}
