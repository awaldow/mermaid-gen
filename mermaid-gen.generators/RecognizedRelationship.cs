using System;

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
        public bool secondaryDefined { get; set; }

        public RecognizedRelationship()
        {
            isIdentifying = false;
        }
    }
}