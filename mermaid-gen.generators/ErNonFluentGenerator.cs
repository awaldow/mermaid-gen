using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace mermaid_gen.generators
{
    public class ErNonFluentGenerator
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

        private readonly List<RecognizedRelationship> _recognizedRelationship;

        public ErNonFluentGenerator(List<Type> assemblyTypes)
        {
            _assemblyTypes = assemblyTypes;
            _erDiagram = new StringBuilder("erDiagram\n");
            _recognizedRelationship = new List<RecognizedRelationship>();
        }

        public void Generate()
        {
            foreach (var entity in _assemblyTypes)
            {
                _erDiagram.AppendLine(GenerateRelationshipsSection(entity));
                _erDiagram.AppendLine(GenerateEntitySection(entity));
            }
        }

        private string GenerateRelationshipsSection(Type entity)
        {
            var ret = "";
            foreach (var prop in entity.GetProperties())
            {
                if (prop.GetGetMethod().IsVirtual && _assemblyTypes.Any(t => t.Name.ToUpper() == prop.PropertyType.Name.ToUpper())) // Found a relationship by convention
                {
                    if (!_recognizedRelationship.Any(rr => (rr.primary == entity && rr.secondary == prop.PropertyType) || (rr.primary == prop.PropertyType && rr.secondary == entity)))
                    {
                        if (prop.GetCustomAttributes().Any(a => a.GetType().Name == "RequiredAttribute"))
                        {
                            ret += $"\t{entity.Name.ToUpper()} ||--|| {prop.PropertyType.Name.ToUpper()}";
                        }
                        else
                        {
                            ret += $"\t{entity.Name.ToUpper()} |o--o| {prop.PropertyType.Name.ToUpper()}";
                        }
                    }
                }
            }
            return ret;
        }

        private string GenerateEntitySection(Type entity)
        {
            if (entity.IsEnum)
            {
                var ret = $"\t{entity.Name.ToUpper()}_ENUM {{\n";
                foreach (var prop in entity.GetEnumValues())
                {
                    ret += $"\t\tenumValue {prop.ToString()}\n";
                }
                ret += "\t}";
                return ret;
            }
            else
            {
                var ret = $"\t{entity.Name.ToUpper()} {{\n";
                foreach (var prop in entity.GetProperties())
                {
                    if (prop.PropertyType.IsEnum)
                    {
                        ret += $"\t\t{prop.Name.ToUpper()}_ENUM {prop.Name}\n";
                    }
                    else
                    {
                        if (_assemblyTypes.Any(t => t.Name.ToUpper() == prop.PropertyType.Name.ToUpper()))
                        {
                            ret += $"\t\t{prop.PropertyType.Name.ToUpper()} {prop.Name}\n";
                        }
                        else
                        {
                            ret += $"\t\t{prop.PropertyType.Name.ToLower()} {prop.Name}\n";
                        }
                    }
                }
                ret += "\t}";
                return ret;
            }
        }
    }
}