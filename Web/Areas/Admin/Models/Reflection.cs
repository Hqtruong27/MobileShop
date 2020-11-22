using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Web.Areas.Admin.Models
{
    public  class Reflection
    {
        public static List<Type> GetAllController(string namespaces)
        {
         Assembly asm = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types = asm.GetTypes()
            .Where(type => typeof(Controller).IsAssignableFrom(type) && type.Namespace.Contains(namespaces)) //filter controllers
            .OrderBy(x => x.Name);
            return types.ToList();
        }
    }
}