using System;
using System.Collections.Generic;

namespace mermaid_gen.generators
{
    public class RecognizedRelationship
    {
        public Type primary { get; set; }
        public Type secondary { get; set; }
        public string primarySideRelationship { get; set; }
        public string secondarySideRelationship { get; set; }
        public bool isIdentifying { get; set; }
        public string label { get; set; }

        public RecognizedRelationship()
        {
            isIdentifying = false;
        }

        public RecognizedRelationship Invert()
        {
            var ret = new RecognizedRelationship
            {
                primary = secondary,
                secondary = primary,
                isIdentifying = isIdentifying,
                label = label
            };
            switch (primarySideRelationship)
            {
                case "}o": ret.secondarySideRelationship = "o{"; break;
                case "|o": ret.secondarySideRelationship = "o|"; break;
                default: ret.secondarySideRelationship = primarySideRelationship; break;
            }
            switch (secondarySideRelationship)
            {
                case "o{": ret.primarySideRelationship = "}o"; break;
                case "o|": ret.primarySideRelationship = "|o"; break;
                default: ret.primarySideRelationship = secondarySideRelationship; break;
            }
            return ret;
        }

        public override bool Equals(object obj)
        {
            if(obj is RecognizedRelationship)
            {
                var equals = primary == ((RecognizedRelationship)obj).primary 
                && primary == ((RecognizedRelationship)obj).primary
                && primarySideRelationship == ((RecognizedRelationship)obj).primarySideRelationship
                && secondarySideRelationship == ((RecognizedRelationship)obj).secondarySideRelationship
                && secondary == ((RecognizedRelationship)obj).secondary
                && label == ((RecognizedRelationship)obj).label
                && isIdentifying == ((RecognizedRelationship)obj).isIdentifying;
                
                return equals;
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(primary.Name, secondary.Name, primarySideRelationship, secondarySideRelationship).GetHashCode();
        }
    }

    public static class RecognizedRelationshipExtensions
    {
        public static bool RelationshipExists(this List<RecognizedRelationship> relationships, RecognizedRelationship relationship)
        {
            var invert = relationship.Invert();
            var found = relationships.Contains(invert);
            return found;
        }
    }
}