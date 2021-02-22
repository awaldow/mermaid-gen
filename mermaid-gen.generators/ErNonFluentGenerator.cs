using System;
using System.Collections;
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

        private readonly List<RecognizedRelationship> _recognizedRelationships;

        public ErNonFluentGenerator(List<Type> assemblyTypes)
        {
            _assemblyTypes = assemblyTypes;
            _erDiagram = new StringBuilder("erDiagram\n");
            _recognizedRelationships = new List<RecognizedRelationship>();
        }

        public void Generate()
        {
            foreach (var entity in _assemblyTypes)
            {
                GenerateRelationships(entity);
                _erDiagram.AppendLine(GenerateEntitySection(entity));
            }
            foreach (var relationship in _recognizedRelationships)
            {
                _erDiagram.AppendLine(GenerateRelationshipSection(relationship));
            }
        }

        private string GenerateRelationshipSection(RecognizedRelationship relationship)
        {
            var ret = $"\t{relationship.primary.Name.ToUpper()} {relationship.primarySideRelationship}";
            if (relationship.isIdentifying) ret += $"--";
            else ret += $"..";
            if (relationship.secondary.IsEnum)
            {
                ret += $"{relationship.secondarySideRelationship} {relationship.secondary.Name.ToUpper()}_ENUM : {relationship.label}";
            }
            else
            {
                ret += $"{relationship.secondarySideRelationship} {relationship.secondary.Name.ToUpper()} : {relationship.label}";
            }
            return ret;
        }

        private void GenerateRelationships(Type entity)
        {
            foreach (var prop in entity.GetProperties())
            {
                if (_assemblyTypes.Any(t => t.Name.ToUpper() == prop.PropertyType.Name.ToUpper())) // Possible relationship found
                {
                    var relationship = new RecognizedRelationship
                    {
                        primary = entity,
                        secondary = prop.PropertyType,
                        secondaryDefined = false
                    };
                    // If the property is nullable, by convention that means it's optional
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        relationship.primarySideRelationship = "|o";
                        relationship.label = "may have";

                        relationship.secondarySideRelationship = "o|";
                        relationship.secondaryDefined = false;
                    }
                    else
                    {
                        relationship.primarySideRelationship = "||";
                        relationship.label = "has";
                        // Now check other side for defined relationship
                        // By convention, if there's an inverse navigation property and an explicit Id column on the secondary entity, we have a defined one-to-one relationship
                        if (prop.PropertyType.GetProperties().Any(p => p.PropertyType == entity) && prop.PropertyType.GetProperties().Any(p => p.Name.StartsWith(entity.Name)))
                        {
                            relationship.secondarySideRelationship = "||";
                            relationship.secondaryDefined = true;
                            relationship.isIdentifying = true;
                        }
                        else
                        {
                            relationship.secondarySideRelationship = "o|";
                            relationship.secondaryDefined = false;
                        }
                    }

                    _recognizedRelationships.Add(relationship);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string)) // Found a one-to-many relationship by convention
                {
                    var relationship = new RecognizedRelationship
                    {
                        primary = entity,
                        secondary = prop.PropertyType.GenericTypeArguments[0],
                        secondaryDefined = false,
                        primarySideRelationship = "||",
                        label = "has",
                        secondarySideRelationship = "o{"
                    };

                    if (relationship.secondary.GetProperties().Any(p => p.Name.StartsWith(entity.Name)) || relationship.secondary.GetProperties().Any(p => p.PropertyType == entity))
                    {

                        relationship.isIdentifying = true;
                    }
                    else
                    {
                        relationship.isIdentifying = false;
                    }
                    _recognizedRelationships.Add(relationship);
                }
            }
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
                            if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                            {
                                ret += $"\t\t{prop.PropertyType.Name.ToLower().Remove(prop.PropertyType.Name.IndexOf('`'))}Of{prop.PropertyType.GetGenericArguments()[0].Name.ToUpper()}s {prop.Name}\n";
                            }
                            else
                            {
                                ret += $"\t\t{prop.PropertyType.Name.ToLower()} {prop.Name}\n";
                            }
                        }
                    }
                }
                ret += "\t}";
                return ret;
            }
        }
    }
}