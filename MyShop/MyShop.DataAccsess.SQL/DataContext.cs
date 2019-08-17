using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccsess.SQL
{
    public class DataContext : DbContext
    {
        public DataContext()
            :base("DefaultConnection") // looks in web.config for the inserted string
                {

            }
    }
    
}
