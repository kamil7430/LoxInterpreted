namespace Lox.Statements;

public class Class : Stmt
{
    public Class(Token name, List<Function> methods)
    {
        Name = name;
        Methods = methods;
    }

    public Token Name { get; set; }
    public List<Function> Methods { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}