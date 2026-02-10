namespace Lox.Statements;

public class Function : Stmt
{
    public Function(Token name, List<Token> @params, List<Stmt> body)
    {
        Name = name;
        Params = @params;
        Body = body;
    }

    public Token Name { get; set; }
    public List<Token> Params { get; set; }
    public List<Stmt> Body { get; set; }
    
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}