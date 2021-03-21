using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
                var compilerGenerated = entity.GetCustomAttributes().Any(a => a is CompilerGeneratedAttribute);
                var abstractEntity = entity.IsAbstract;
                if (!compilerGenerated && !abstractEntity)
                {
                    GenerateRelationships(entity);
                    _erDiagram.AppendLine(GenerateEntitySection(entity));
                }
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
                    };
                    // If the property is nullable, by convention that means it's optional
                    if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        relationship.primarySideRelationship = "|o";
                        relationship.label = "may have";

                        relationship.secondarySideRelationship = "o|";
                        relationship.isIdentifying = false;
                    }
                    else
                    {
                        // One to one relationship
                        // TODO: Refine this check; need the secondary types foreign key property to match entity's ID type, for example
                        if (prop.PropertyType.GetProperties().Any(p => p.PropertyType == entity) && prop.PropertyType.GetProperties().Any(p => p.Name.StartsWith(entity.Name)))
                        {
                            relationship.primarySideRelationship = "||";
                            relationship.label = "has";
                            relationship.secondarySideRelationship = "||";
                            relationship.isIdentifying = true;
                        }
                        // many to one from secondary
                        else if (prop.PropertyType.GetProperties().Any(p =>
                            typeof(IEnumerable).IsAssignableFrom(p.PropertyType) && p.PropertyType != typeof(string) && p.PropertyType.GetEnumeratedType() == entity)
                        )
                        {
                            relationship.primarySideRelationship = "}o";
                            relationship.label = "has";
                            relationship.secondarySideRelationship = "||";
                            relationship.isIdentifying = true;
                        }
                        // optional foreign key
                        else
                        {
                            // TODO: probably do some additional checking here
                            relationship.primarySideRelationship = "||";
                            relationship.label = "has";
                            relationship.secondarySideRelationship = "o|";
                            relationship.isIdentifying = false;
                        }
                    }
                    if (!_recognizedRelationships.RelationshipExists(relationship))
                    {
                        _recognizedRelationships.Add(relationship);
                    }
                }
                else if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string) && _assemblyTypes.Any(t => t.Name == prop.PropertyType.GetEnumeratedType().Name)) // Found a one-to-many relationship by convention
                {
                    // TODO: Handle arrays here
                    var relationship = new RecognizedRelationship
                    {
                        primary = entity,
                        secondary = prop.PropertyType.GetEnumeratedType(),
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
                    if (!_recognizedRelationships.RelationshipExists(relationship))
                    {
                        _recognizedRelationships.Add(relationship);
                    }
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
                                var type = prop.PropertyType.GetEnumeratedType();
                                if (prop.PropertyType.Name.IndexOf('`') > 0) // Colleciton
                                {
                                    ret += $"\t\t{prop.PropertyType.Name.ToLower().Remove(prop.PropertyType.Name.IndexOf('`'))}Of{type.Name.ToUpper()}s {prop.Name}\n";
                                }
                                else if(prop.PropertyType.Name.IndexOf('[') > 0) // Array
                                {
                                    ret += $"\t\tarrayOf{type.Name.ToUpper()}s {prop.Name}\n";
                                }

                            }
                            else if(prop.PropertyType.IsNullable())
                            {
                                var propTypeName = Nullable.GetUnderlyingType(prop.PropertyType).Name;
                                ret += $"\t\tnullable{Char.ToUpperInvariant(propTypeName[0]) + propTypeName.ToLower().Substring(1)} {prop.Name}\n";
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