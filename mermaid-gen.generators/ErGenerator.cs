using System;
using System.Collections.Generic;
using System.Text;

namespace mermaid_gen.generators
{
    public class ErGenerator
    {
        public string ErDiagram
        {
            get
            {
                return _erDiagram.ToString();
            }
        }

        private StringBuilder _erDiagram { get; set; }

        private readonly List<Type> _assemblyTypes;

        public ErGenerator(List<Type> assemblyTypes)
        {
            _assemblyTypes = assemblyTypes;
            _erDiagram = new StringBuilder("erDiagram\n");
        }

        public void Generate()
        {
            foreach (var entity in _assemblyTypes)
            {
                _erDiagram.Append(GenerateRelationshipsSection(entity));
                _erDiagram.Append(GenerateEntitySection(entity));
            }
        }

        private string GenerateRelationshipsSection(Type entity)
        {
            var ret = "";
            return ret;
        }

        private string GenerateEntitySection(Type entity)
        {
            var ret = $"\t{entity.Name.ToUpper()}{{\n";
            foreach (var prop in entity.GetProperties())
            {
                ret += $"\t\t{prop.PropertyType.Name} {prop.Name}\n";
            }
            ret += "\t}";
            return ret;
        }
    }
}