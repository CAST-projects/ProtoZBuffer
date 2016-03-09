namespace protozbuffer
{
    public interface IAstNode
    {
        void Accept(IAstNodeVisitor visitor);
    }

    public interface IAstNodeVisitor
    {
        void Visit(enumElementType node);
        void Visit(enumType node);
        void Visit(fieldType node);
        void Visit(indexType node);
        void Visit(messageType node);
        void Visit(protozbuffType node);
    }

    // ReSharper disable once InconsistentNaming
    public partial class enumElementType : IAstNode
    {
        public void Accept(IAstNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // ReSharper disable once InconsistentNaming
    public partial class enumType : IAstNode
    {
        public void Accept(IAstNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // ReSharper disable once InconsistentNaming
    public partial class fieldType : IAstNode
    {
        public void Accept(IAstNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // ReSharper disable once InconsistentNaming
    public partial class indexType : IAstNode
    {
        public void Accept(IAstNodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        // reference field that will be generated in the .proto file
        public fieldType ReferenceField { get; set; }

        // type of the field we want to sort by
        public fieldType SortingField { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    public partial class messageType : IAstNode
    {
        public void Accept(IAstNodeVisitor visitor)
        {
            visitor.Visit(this);
        }

        public bool IsRoot { get; set; }
    }

    // ReSharper disable once InconsistentNaming
    public partial class protozbuffType : IAstNode
    {
        public void Accept(IAstNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}