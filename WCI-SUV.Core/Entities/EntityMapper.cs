using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCI_SUV.Core.Entities
{
    public class EntityMapper
    {

        private Dictionary<string, Type> _entityMap = new Dictionary<string, Type>
        {
            {"Conveyor", typeof(Conveyor) },
            {"Customer", typeof(Conveyor) }
        };

        public Type? GetEntityType(string entityName)
        {
            try
            {
                if (_entityMap.ContainsKey(entityName))
                {
                    return _entityMap[entityName];
                }
                else
                {
                    return null;
                }
            } catch (Exception ex)
            {
                Console.WriteLine($"Exception occured: {ex.Message}");
                throw;
            }
            
        }



    }
}
