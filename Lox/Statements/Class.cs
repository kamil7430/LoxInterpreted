namespace Lox.Statements;

public class Class : Stmt
{
    public Class(Token name, List<Function> methods, List<Function> staticMethods)
    {
        Name = name;
        Methods = methods;
        StaticMethods = staticMethods;
    }

    public Token Name { get; set; }
    public List<Function> Methods { get; set; }
    public List<Function> StaticMethods { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}